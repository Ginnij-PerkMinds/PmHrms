using System.Data;
using System.Text.Json;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using log4net;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using PmHrmsAPI.PmHrmsBAL.Helpers;
using PmHrmsAPI.PmHrmsDAL.Repositories;
using PmHrmsAPI.PmHrmsBAL.Models;
using PmHrmsAPI.PmHrmsBAL.ResponseModel;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.Jobs
{
    public class PayrollJob
    {
        private const string PendingStatus = "Pending";
        private const string RunningStatus = "Running";
        private const string CompletedStatus = "Completed";
        private const string FailedStatus = "Failed";
        private const string RegularRunType = "Regular";
        private const string ScheduledTrigger = "Scheduled";
        private const string ManualTrigger = "Manual";
        private const string CalculatedEmployeeStatus = "Calculated";
        private const string PendingPaymentStatus = "Pending";

        

        private readonly PmHrmsContext _context;
        private readonly IPayrollCalculator _calculator;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly ILogger<PayrollJob> _log;
        private readonly EmployeeDAL _employeeDal;
        private readonly SalaryStructureDAL _salaryDal;
        private readonly AttendanceDAL _attendanceDal;
        private readonly EmployeeBankAccountDAL _bankDal;
        private readonly HolidayDAL _holidayDal; 


        public PayrollJob(
            PmHrmsContext context,
            IPayrollCalculator calculator,
            IBackgroundJobClient backgroundJobs,
            ILogger<PayrollJob> log,
            EmployeeDAL employeeDal,
            SalaryStructureDAL salaryDal,
            AttendanceDAL attendanceDal,
            EmployeeBankAccountDAL bankDal,
             HolidayDAL holidayDal) 
        {
            _context = context;
            _calculator = calculator;
            _backgroundJobs = backgroundJobs;
            _log = log;
            _employeeDal = employeeDal;
            _salaryDal = salaryDal;
            _attendanceDal = attendanceDal;
            _bankDal = bankDal;
             _holidayDal = holidayDal; 
        }

        [AutomaticRetry(Attempts = 1)]
        public async Task RunPayrollAsync(int payrollRunId)
        {
            PayrollRunScope runScope;

            try
            {
                runScope = await ClaimPayrollRunAsync(payrollRunId);
            }
            catch (InvalidOperationException ex)
            {
                _log.LogWarning(ex, "Payroll run {PayrollRunId} was not started.", payrollRunId);
                return;
            }

            _log.LogInformation($"RunPayrollAsync started for payrollRunId={payrollRunId}, OrgId={runScope.OrgId}, Period={runScope.StartDate}-{runScope.EndDate}");

            var config = await GetPayrollConfigAsync(runScope.OrgId);

            var employees = await _context.Employees
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(employee => employee.OrganizationId == runScope.OrgId && employee.IsActive)
                .OrderBy(employee => employee.EmployeeId)
                .ToListAsync();


                var allHolidayGroups = await _holidayDal
        .GetActiveGroupsWithDetailsAsync(runScope.OrgId, runScope.StartDate.Year);


            var successCount = 0;
            var failedCount = 0;
            var totalNetPayable = 0m;

            _log.LogInformation($"Processing {employees.Count} employees for payroll run {runScope.Id}.");

            foreach (var employee in employees)
            {
                 var result = await ProcessEmployeeAsync(runScope, config, employee, allHolidayGroups);

                if (result.IsSuccess)
                {
                    successCount++;
                    totalNetPayable += result.NetPayable;
                }
                else
                {
                    failedCount++;
                }
            }

            await CompletePayrollRunAsync(
                runScope.Id,
                employees.Count,
                totalNetPayable,
                successCount,
                failedCount);

            _log.LogInformation($"Payroll run {runScope.Id} finished. Success={successCount}, Failed={failedCount}, TotalNetPayable={totalNetPayable}");
        }

        [AutomaticRetry(Attempts = 1)]
        public async Task EnqueueDuePayrollRunsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var configs = await _context.PayrollConfigs
                .AsNoTracking()
                .Where(config => config.IsAutoRunEnabled && config.AutoRunDayOfMonth == today.Day)
                .ToListAsync();

            foreach (var config in configs)
            {
                var runId = await GetOrCreateScheduledPayrollRunAsync(config, today);
                if (runId.HasValue)
                {
                    _log.LogInformation($"Enqueuing scheduled payroll run {runId.Value} for Org {config.OrgId}");
                    _backgroundJobs.Enqueue<PayrollJob>(job => job.RunPayrollAsync(runId.Value));
                }
            }
        }

        private async Task<PayrollRunScope> ClaimPayrollRunAsync(int payrollRunId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var payrollRun = await _context.PayrollRuns
            
                .FirstOrDefaultAsync(run => run.Id == payrollRunId);

            if (payrollRun is null)
            {
                throw new InvalidOperationException($"Payroll run {payrollRunId} was not found.");
            }

            if (payrollRun.IsLocked)
            {
                throw new InvalidOperationException($"Payroll run {payrollRunId} is locked.");
            }

            if (payrollRun.Status == RunningStatus)
            {
                throw new InvalidOperationException($"Payroll run {payrollRunId} is already running.");
            }

            if (payrollRun.Status == CompletedStatus)
            {
                throw new InvalidOperationException($"Payroll run {payrollRunId} is already completed.");
            }

            var now = DateTime.UtcNow;
            payrollRun.Status = RunningStatus;
            payrollRun.ActualRunStart ??= now;
            payrollRun.ActualRunEnd = null;
            payrollRun.ErrorMessage = null;
            payrollRun.UpdatedAt = now;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _log.LogInformation($"Claimed payroll run {payrollRun.Id} for Org {payrollRun.OrgId}");

            return new PayrollRunScope(
                payrollRun.Id,
                payrollRun.OrgId,
                payrollRun.PayrollMonth,
                payrollRun.PayrollYear,
                payrollRun.StartDate,
                payrollRun.EndDate, 
                payrollRun.IsRecalculated);
        }

        private async Task<PayrollConfig> GetPayrollConfigAsync(int orgId)
        {
            var config = await _context.PayrollConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.OrgId == orgId);

            return config ?? new PayrollConfig
            {
                OrgId = orgId,
                AutoRunDayOfMonth = 5,
                IsAutoRunEnabled = false,
                PayrollCutoffDay = 25,
                PayslipGenerateDay = 7,
                Currency = "INR",
                RoundingRule = "Round"
            };
        }

        private async Task<PayrollEmployeeProcessResult> ProcessEmployeeAsync(
            PayrollRunScope runScope,
            PayrollConfig config,
            Employee employee , 
            List<HolidayGroup> allHolidayGroups)
        {
            try
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var existingPayroll = await _context.EmployeePayrolls
                    .Include(payroll => payroll.Components)
                    .FirstOrDefaultAsync(payroll =>
                        payroll.PayrollRunId == runScope.Id &&
                        payroll.EmployeeId == employee.EmployeeId);

               if (existingPayroll?.Status == CalculatedEmployeeStatus && !runScope.IsRecalculated)
                {
                    await transaction.CommitAsync();
                    return new PayrollEmployeeProcessResult(true, existingPayroll.NetPayable);
                }

                var payrollRun = await _context.PayrollRuns
                    .FirstAsync(run => run.Id == runScope.Id);

                var attendance = await GetAttendanceSummaryAsync(runScope, employee.EmployeeId, allHolidayGroups);


                var salaryStructure = await _salaryDal.GetEmployeeSalary(employee.EmployeeId, runScope.OrgId);
                if (salaryStructure is null)
                {
                    throw new InvalidOperationException($"Salary structure is not assigned for employee {employee.EmployeeId}.");
                }

                var salaryComponents = salaryStructure.Components ?? new List<SalaryComponent>();

                var grossSalary = salaryComponents
                    .Where(component => component.IsEarning)
                    .Sum(component => component.Amount);

                if (grossSalary <= 0)
                {
                    throw new InvalidOperationException($"Gross salary is not configured for employee {employee.EmployeeId}.");
                }

                var totalWorkingDays = attendance?.TotalWorkingDays > 0
                    ? attendance.TotalWorkingDays
                    : GetFallbackWorkingDays(runScope.StartDate, runScope.EndDate);

                var payrollContext = new PayrollContext
                {
                    EmployeeId = employee.EmployeeId,
                    GrossSalary = grossSalary,
                    TotalWorkingDays = totalWorkingDays,
                    PresentDays = attendance?.PresentDays ?? 0m,
                    UnpaidLeaveDays = attendance?.UnpaidLeaveDays ?? 0m,
                    IsPfApplicable = config.IsPfApplicable,
                    PfWageLimit = config.PfWageLimit,
                    PfEmployeePercentage = config.PfEmployeePercentage,
                    PfEmployerPercentage = config.PfEmployerPercentage,
                    IsEsiApplicable = config.IsEsiApplicable,
                    EsiEmployeePercentage = config.EsiEmployeePercentage,
                    EsiEmployerPercentage = config.EsiEmployerPercentage,
                    IsTdsApplicable = config.IsTdsApplicable,
                    TdsPercentage = config.TdsPercentage,
                    RoundingRule = string.IsNullOrWhiteSpace(config.RoundingRule) ? "Round" : config.RoundingRule,
                    SalaryComponents     = salaryComponents.ToList()

                };

                var calculationResult = _calculator.Calculate(payrollContext);
                var now = DateTime.UtcNow;
                var employeePayroll = existingPayroll ?? new EmployeePayroll
                {
                    PayrollRunId = runScope.Id,
                    PayrollRun = payrollRun,
                    EmployeeId = employee.EmployeeId,
                    CreatedAt = now
                };

                employeePayroll.TotalWorkingDays = totalWorkingDays;
                employeePayroll.PresentDays = attendance?.PresentDays ?? 0m;
                employeePayroll.AbsentDays = attendance?.AbsentDays ?? 0m;
                employeePayroll.LeaveDays = attendance?.LeaveDays ?? 0m;
                employeePayroll.LopDays = attendance?.UnpaidLeaveDays ?? 0m;
                employeePayroll.GrossSalary = grossSalary;
                employeePayroll.TotalEarnings = calculationResult.TotalEarnings;
                employeePayroll.TotalDeductions = calculationResult.TotalDeductions;
                employeePayroll.NetPayable = calculationResult.NetPayable;
                employeePayroll.ArrearsAmount = 0m;
                employeePayroll.RoundingAdjustment = calculationResult.RoundingAdjustment;
                employeePayroll.SalaryStructureSnapshot = BuildSalaryStructureSnapshot(salaryComponents);
                employeePayroll.BankAccountSnapshot = await BuildBankAccountSnapshotAsync(runScope.OrgId, employee.EmployeeId);
                employeePayroll.PaymentStatus = string.IsNullOrWhiteSpace(employeePayroll.PaymentStatus)
                    ? PendingPaymentStatus
                    : employeePayroll.PaymentStatus;
                employeePayroll.Status = CalculatedEmployeeStatus;
                employeePayroll.Remarks = null;
                employeePayroll.UpdatedAt = now;

                if (existingPayroll is null)
                {
                    _context.EmployeePayrolls.Add(employeePayroll);
                }

                var oldComponents = await _context.EmployeePayrollComponents
                .Where(c => c.EmployeePayrollId == employeePayroll.Id)
                .ToListAsync();

             if (oldComponents.Count > 0)
                _context.EmployeePayrollComponents.RemoveRange(oldComponents);


                    foreach (var component in calculationResult.Components)
                    {
                        _context.EmployeePayrollComponents.Add(new EmployeePayrollComponent
                        {
                            EmployeePayroll = employeePayroll,
                            ComponentId = 0,
                            ComponentName = component.Name,
                            Amount = component.Amount,
                            Type = component.Type,
                            IsStatutory = IsStatutoryComponent(component.Name),
                            IsTaxExempt = false,
                            CalculationBasis = "Fixed",
                            CreatedAt = now
                        });
                    }
                

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new PayrollEmployeeProcessResult(true, calculationResult.NetPayable);
            }
            catch (Exception ex)
            {
                _log.LogError(
                    ex,
                    "Payroll calculation failed for employee {EmployeeId} in payroll run {PayrollRunId}.",
                    employee.EmployeeId,
                    runScope.Id);

                _log.LogError($"Payroll calculation failed for employee {employee.EmployeeId} in payroll run {runScope.Id}.", ex);

                await SaveFailedEmployeePayrollAsync(runScope, employee.EmployeeId, ex.Message);
                return new PayrollEmployeeProcessResult(false, 0m);
            }
        }

     private async Task<EmployeeAttendanceSummary?> GetAttendanceSummaryAsync(PayrollRunScope runScope, int employeeId  ,List<HolidayGroup> allHolidayGroups)
        {

          

            var existingSummary = await _context.EmployeeAttendanceSummaries
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(summary =>
                    summary.EmployeeId == employeeId &&
                    summary.Month == runScope.PayrollMonth &&
                    summary.Year == runScope.PayrollYear);

                      _log.LogWarning(
            "Attendance summary FOUND? {Exists} for EmployeeId={EmployeeId}",
            existingSummary != null,
            employeeId);

            // Step 1: Get employee details with policy and holiday info
            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Include(e => e.Policy)
                    .ThenInclude(p => p.WeekOffs)
                .Include(e => e.HolidayGroup)
                    .ThenInclude(g => g.GroupHolidays)
                    .ThenInclude(gh => gh.SystemHoliday)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

                

           if (employee == null)
                {
                    return existingSummary;
                }
             
            // Step 2: Get all attendance records for the period
            var rawRecords = await _attendanceDal.GetAttendanceByDateRangeAsync(               
                employeeId, 
                runScope.StartDate,      
                runScope.EndDate);
          
            // Step 3: Get approved leaves for this period (Status == 2 generally means Approved)
            var approvedLeaves = await  _context.LeaveRequests                                                          
                .AsNoTracking()                                                                                                                   
                .Where(l =>      
                    l.EmployeeId == employeeId &&                                                           
                    l.Status == 2 && // Approved Status  
                    l.FromDate <= runScope.EndDate &&                             
                    l.ToDate >= runScope.StartDate)         
                .ToListAsync();                                                                                                                    

            // Step 4: Build lookup dictionaries
            var attendanceByDate = rawRecords .GroupBy(a => a.AttendanceDate).ToDictionary(g => g.Key, g => g.First());
            var leavesByDate = BuildLeaveLookup(approvedLeaves, runScope.StartDate, runScope.EndDate);

            var year = runScope.StartDate.Year;
            var (resolvedGroup, _) = HolidayResolutionHelper.ResolveGroup(employee, allHolidayGroups);
            var holidayLookup = BuildHolidayLookup(resolvedGroup, year);
            var weekOffDays = employee.Policy?.WeekOffs?.Select(w => w.DayOfWeek).ToHashSet() 
                ?? new HashSet<DayOfWeek>();

            // Step 5: Initialize counters
            decimal presentDays = 0m;
            decimal absentDays = 0m;
            decimal leaveDays = 0m;
            int totalWorkingDays = 0;

            _log.LogInformation($"--- STARTING ATTENDANCE CALCULATION FOR EMPLOYEE ID: {employeeId} ---");

            // Step 6: Iterate through each day in the payroll period
            for (var date = runScope.StartDate; date <= runScope.EndDate; date = date.AddDays(1))
            {
                // Skip if before employee's joining date
                if (employee.DateOfJoining != DateOnly.MinValue && date < employee.DateOfJoining)
                {_log.LogInformation($"[Emp {employeeId}] Date: {date} -> Skipped (Before Joining Date)");
                    continue;
                }

                // Check if it's a holiday or week off
                bool isHoliday = holidayLookup.ContainsKey(date);
                bool isWeekOff = weekOffDays.Contains(date.DayOfWeek);
                bool isOnLeave = leavesByDate.ContainsKey(date);

                if (isHoliday)
                {
                    _log.LogInformation($"[Emp {employeeId}] Date: {date} -> Skipped (Holiday)");
                    continue; 
                }

                if (isWeekOff)
                {
                    _log.LogInformation($"[Emp {employeeId}] Date: {date} -> Skipped (WeekOff)");
                    continue; 
                }

                // Increment total working days (excluding holidays and weekoffs)
                totalWorkingDays++;

                

                if (isOnLeave)
                {
                    leaveDays += 1.0m;
                    presentDays += 1.0m; // Leave counts as Present
                    _log.LogInformation($"[Emp {employeeId}] Date: {date} -> Approved Leave -> Added 1.0 Present");
                    continue; // Move to the next day
                }


                // Handle attendance records
                if (attendanceByDate.TryGetValue(date, out var attendance))
                {
                    // Check if attendance has a valid check-in
                    if (attendance.CheckInTime == null)
                    {
                        // No check-in = Absent
                        absentDays += 1.0m;
                        _log.LogInformation($"[Emp {employeeId}] Date: {date} -> Record found but No Check-in -> Added 1.0 Absent");
                        continue;
                    }

                    // Process based on attendance status Enum
                    switch (attendance.Status)
                    {
                        case AttendanceStatus.Present:
                        case AttendanceStatus.Late:
                        case AttendanceStatus.Working:
                            presentDays += 1.0m;
                            _log.LogInformation($"[Emp {employeeId}] Date: {date} -> Status: {attendance.Status} -> Added 1.0 Present");
                            break;

                        case AttendanceStatus.MissedCheckOut:
                            presentDays += 0.8m;
                            absentDays += 0.2m;
                            _log.LogInformation($"[Emp {employeeId}] Date: {date} -> Status: MissedCheckOut -> Added 0.8 Present, 0.2 Absent");
                            break;

                        case AttendanceStatus.HalfDay:
                            presentDays += 0.5m;
                            absentDays += 0.5m;
                            _log.LogInformation($"[Emp {employeeId}] Date: {date} -> Status: HalfDay -> Added 0.5 Present, 0.5 Absent");
                            break;

                        case AttendanceStatus.Absent:
                        default:
                            absentDays += 1.0m;
                            _log.LogInformation($"[Emp {employeeId}] Date: {date} -> Status: {attendance.Status} -> Added 1.0 Absent");
                            break;
                    }
                    continue;
                }

                 else
                {
                    // 3. Fallback: No attendance record AND not on leave = Absent
                    absentDays += 1.0m;
                    _log.LogInformation($"[Emp {employeeId}] Date: {date} -> NO DATABASE RECORD FOUND -> Added 1.0 Absent");
                }
           
                       
         }
              
            

            _log.LogInformation($"--- FINAL COUNT EMP {employeeId} | WorkingDays: {totalWorkingDays} | Present: {presentDays} | Absent: {absentDays} | PaidLeaves: {leaveDays} ---");

            // Step 7: Update or create summary
            if (existingSummary != null)
            {
                existingSummary.TotalWorkingDays = (byte)totalWorkingDays;
                existingSummary.PresentDays = presentDays;
                existingSummary.AbsentDays = absentDays;
                existingSummary.LeaveDays = leaveDays;
                existingSummary.UnpaidLeaveDays = absentDays; // all absents are unpaid leaves for summary purposes
                existingSummary.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                existingSummary = new EmployeeAttendanceSummary
                {
                    EmployeeId = employeeId,
                    Month = runScope.PayrollMonth,
                    Year = runScope.PayrollYear,
                    TotalWorkingDays = (byte)totalWorkingDays,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    LeaveDays = leaveDays,
                    UnpaidLeaveDays = absentDays, // all absents are unpaid leaves for summary purposes
                    DataSource = "Auto",
                    IsApproved = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                _context.EmployeeAttendanceSummaries.Add(existingSummary);
            }

            await _context.SaveChangesAsync();

            return existingSummary;
        }

        // --- HELPER METHODS ---

        private Dictionary<DateOnly, LeaveInfo> BuildLeaveLookup(
            List<LeaveRequest> leaves, 
            DateOnly startDate, 
            DateOnly endDate)
        {
            var leaveLookup = new Dictionary<DateOnly, LeaveInfo>();

            foreach (var leave in leaves)
            {
                var dayStart = leave.FromDate > startDate ? leave.FromDate : startDate;
                var dayEnd = leave.ToDate < endDate ? leave.ToDate : endDate;

                for (var date = dayStart; date <= dayEnd; date = date.AddDays(1))
                {
                    if (!leaveLookup.ContainsKey(date))
                    {
                        leaveLookup[date] = new LeaveInfo(leave.LeaveTypeId, leave.LeaveType?.LeaveTypeName);
                    }
                }
            }

            return leaveLookup;
        }


        private Dictionary<DateOnly, string> BuildHolidayLookup(HolidayGroup? group, int year)
        {
            if (group == null || !group.IsActive || group.Year != year)
            {
                return new Dictionary<DateOnly, string>();
            }

            return group.GroupHolidays
                .Where(mapping => mapping.SystemHoliday != null)
                .GroupBy(mapping => mapping.SystemHoliday.HolidayDate)
                .ToDictionary(
                    g => g.Key,
                    g => string.Join(", ", g
                        .Select(mapping => mapping.SystemHoliday.HolidayName)
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct()));
        }



        private static string BuildSalaryStructureSnapshot(IEnumerable<SalaryComponent> salaryComponents)
        {
            var snapshot = salaryComponents.Select(component => new
            {
                component.SalaryComponentId,
                component.ComponentMasterId,
                component.ComponentName,
                component.Amount,
                component.IsEarning
            });

            return JsonSerializer.Serialize(snapshot);
        }

        private async Task<string?> BuildBankAccountSnapshotAsync(int orgId, int employeeId)
        {
            var bankAccount = await _bankDal.GetPrimaryBankAccountByEmployeeIdAsync(employeeId);

            if (bankAccount is null) return null;

            var obj = new
            {
                bankAccount.BankAccountId,
                bankAccount.AccountHolderName,
                bankAccount.AccountNumber,
                bankAccount.IFSCCode,
                bankAccount.BankName,
                bankAccount.BranchName,
                bankAccount.IsPrimary
            };

            return JsonSerializer.Serialize(obj);
        }

        private async Task SaveFailedEmployeePayrollAsync(
            PayrollRunScope runScope,
            int employeeId,
            string errorMessage)
        {
            _context.ChangeTracker.Clear();

            var payrollRun = await _context.PayrollRuns
                .FirstAsync(run => run.Id == runScope.Id);

            var existingPayroll = await _context.EmployeePayrolls
                .FirstOrDefaultAsync(payroll =>
                    payroll.PayrollRunId == runScope.Id &&
                    payroll.EmployeeId == employeeId);

            var now = DateTime.UtcNow;
            var fallbackWorkingDays = GetFallbackWorkingDays(runScope.StartDate, runScope.EndDate);

            if (existingPayroll is null)
            {
                _context.EmployeePayrolls.Add(new EmployeePayroll
                {
                    PayrollRunId = runScope.Id,
                    PayrollRun = payrollRun,
                    EmployeeId = employeeId,
                    TotalWorkingDays = fallbackWorkingDays,
                    PresentDays = 0m,
                    AbsentDays = 0m,
                    LeaveDays = 0m,
                    LopDays = 0m,
                    GrossSalary = 0m,
                    TotalEarnings = 0m,
                    TotalDeductions = 0m,
                    NetPayable = 0m,
                    ArrearsAmount = 0m,
                    RoundingAdjustment = 0m,
                    PaymentStatus = PendingPaymentStatus,
                    Status = FailedStatus,
                    Remarks = Truncate(errorMessage, 500),
                    CreatedAt = now,
                    UpdatedAt = now
                });
            }
            else if (existingPayroll.Status != CalculatedEmployeeStatus)
            {
                existingPayroll.Status = FailedStatus;
                existingPayroll.Remarks = Truncate(errorMessage, 500);
                existingPayroll.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            _log.LogInformation($"Saved failed payroll record for employee {employeeId} in run {runScope.Id}");
        }

        private async Task CompletePayrollRunAsync(
            int payrollRunId,
            int totalEmployees,
            decimal totalNetPayable,
            int successCount,
            int failedCount)
        {
            var payrollRun = await _context.PayrollRuns
                .FirstAsync(run => run.Id == payrollRunId);

            var hasFailures = failedCount > 0 || successCount == 0;
            payrollRun.Status = hasFailures ? FailedStatus : CompletedStatus;
            payrollRun.TotalEmployees = totalEmployees;
            payrollRun.TotalNetPayable = totalNetPayable;
            payrollRun.ActualRunEnd = DateTime.UtcNow;
            payrollRun.ErrorMessage = hasFailures
                ? BuildPayrollRunErrorMessage(totalEmployees, failedCount)
                : null;
            payrollRun.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _log.LogInformation($"Payroll run {payrollRunId} marked {(hasFailures ? "Failed" : "Completed")}. Success={successCount}, Failed={failedCount}");
        }

        private async Task<int?> GetOrCreateScheduledPayrollRunAsync(PayrollConfig config, DateOnly today)
        {
            var periodDate = DateTime.Today.AddMonths(-1);
            var startDate = new DateOnly(periodDate.Year, periodDate.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var existingRun = await _context.PayrollRuns
                .FirstOrDefaultAsync(run =>
                    run.OrgId == config.OrgId &&
                    run.PayrollMonth == startDate.Month &&
                    run.PayrollYear == startDate.Year &&
                    run.RunType == PayrollRunType.Regular);

            if (existingRun is not null)
            {
                await transaction.CommitAsync();
                return existingRun.Status is PendingStatus or FailedStatus ? existingRun.Id : null;
            }

            var now = DateTime.UtcNow;
            var payrollRun = new PayrollRun
            {
                OrgId = config.OrgId,
                PayrollMonth = (byte)startDate.Month,
                PayrollYear = (short)startDate.Year,
                StartDate = startDate,
                EndDate = endDate,
                Status = PendingStatus,
                RunType = PayrollRunType.Regular,
                IsLocked = false,
                ApprovalStatus = PendingStatus,
                TriggeredBy = ScheduledTrigger,
                ScheduledRunDate = today,
                IsRecalculated = false,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.PayrollRuns.Add(payrollRun);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return payrollRun.Id;
        }

        private static byte GetFallbackWorkingDays(DateOnly startDate, DateOnly endDate)
        {
            var totalDays = Math.Max(1, endDate.DayNumber - startDate.DayNumber + 1);
            return (byte)Math.Min(totalDays, byte.MaxValue);
        }

        private static bool IsStatutoryComponent(string componentName)
        {
            return componentName.StartsWith("PF", StringComparison.OrdinalIgnoreCase) ||
                   componentName.StartsWith("ESI", StringComparison.OrdinalIgnoreCase) ||
                   componentName.StartsWith("TDS", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildPayrollRunErrorMessage(int totalEmployees, int failedCount)
        {
            if (totalEmployees == 0)
            {
                return "No active employees found for payroll run.";
            }

            return Truncate($"{failedCount} employee payroll calculation(s) failed.", 4000);
        }

        private static string Truncate(string value, int maxLength)
        {
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private sealed record PayrollRunScope(
            int Id,
            int OrgId,
            byte PayrollMonth,
            short PayrollYear,
            DateOnly StartDate,
            DateOnly EndDate,
            bool IsRecalculated);

        private sealed record PayrollEmployeeProcessResult(bool IsSuccess, decimal NetPayable);

        private record LeaveInfo(int? LeaveTypeId, string? LeaveTypeName);
    }
}
