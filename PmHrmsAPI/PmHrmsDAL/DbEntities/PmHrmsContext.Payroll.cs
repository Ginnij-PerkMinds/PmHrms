using Microsoft.EntityFrameworkCore;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class PmHrmsContext
{
    public virtual DbSet<PayrollConfig> PayrollConfigs { get; set; }
    public virtual DbSet<PayrollRun> PayrollRuns { get; set; }
    public virtual DbSet<SalaryRevisionLog> SalaryRevisionLogs { get; set; }
    public virtual DbSet<EmployeeAttendanceSummary> EmployeeAttendanceSummaries { get; set; }
    public virtual DbSet<EmployeePayroll> EmployeePayrolls { get; set; }
    public virtual DbSet<EmployeePayrollComponent> EmployeePayrollComponents { get; set; }
    public virtual DbSet<EmployeePayrollHistory> EmployeePayrollHistories { get; set; }
    public virtual DbSet<PayrollAuditLog> PayrollAuditLogs { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PayrollConfig>(entity =>
        {
            entity.ToTable("payroll_config");

            entity.HasKey(e => e.Id).HasName("pk_payroll_config");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.OrgId)
                .HasColumnName("org_id");

            entity.Property(e => e.AutoRunDayOfMonth)
                .HasColumnName("auto_run_day_of_month")
                .HasDefaultValue((byte)5);

            entity.Property(e => e.IsAutoRunEnabled)
                .HasColumnName("is_auto_run_enabled")
                .HasDefaultValue(false);

            entity.Property(e => e.PayrollCutoffDay)
                .HasColumnName("payroll_cutoff_day")
                .HasDefaultValue((byte)25);

            entity.Property(e => e.PayslipGenerateDay)
                .HasColumnName("payslip_generate_day")
                .HasDefaultValue((byte)7);

            entity.Property(e => e.Currency)
                .HasColumnName("currency")
                .HasColumnType("char(3)")
                .IsRequired()
                .HasDefaultValue("INR");

            entity.Property(e => e.RoundingRule)
                .HasColumnName("rounding_rule")
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired()
                .HasDefaultValue("Round");

            entity.Property(e => e.IsPfApplicable)
                .HasColumnName("is_pf_applicable")
                .HasDefaultValue(false);

            entity.Property(e => e.PfWageLimit)
                .HasColumnName("pf_wage_limit")
                .HasPrecision(12, 2);

            entity.Property(e => e.PfEmployeePercentage)
                .HasColumnName("pf_employee_percentage")
                .HasPrecision(5, 2);

            entity.Property(e => e.PfEmployerPercentage)
                .HasColumnName("pf_employer_percentage")
                .HasPrecision(5, 2);

            entity.Property(e => e.IsEsiApplicable)
                .HasColumnName("is_esi_applicable")
                .HasDefaultValue(false);

            entity.Property(e => e.EsiEmployeePercentage)
                .HasColumnName("esi_employee_percentage")
                .HasPrecision(5, 2);

            entity.Property(e => e.EsiEmployerPercentage)
                .HasColumnName("esi_employer_percentage")
                .HasPrecision(5, 2);

            entity.Property(e => e.IsTdsApplicable)
                .HasColumnName("is_tds_applicable")
                .HasDefaultValue(false);

            entity.Property(e => e.TdsPercentage)
                .HasColumnName("tds_percentage")
                .HasPrecision(5, 2);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.OrgId)
                .IsUnique()
                .HasDatabaseName("uq_payroll_config_org");

            entity.HasCheckConstraint("chk_rounding_rule", "rounding_rule IN ('Floor', 'Ceil', 'Round')");
        });

        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.ToTable("payroll_run");

            entity.HasKey(e => e.Id).HasName("pk_payroll_run");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.OrgId)
                .HasColumnName("org_id");

            entity.Property(e => e.PayrollMonth)
                .HasColumnName("payroll_month");

            entity.Property(e => e.PayrollYear)
                .HasColumnName("payroll_year");

            entity.Property(e => e.StartDate)
                .HasColumnName("start_date")
                .HasColumnType("date");

            entity.Property(e => e.EndDate)
                .HasColumnName("end_date")
                .HasColumnType("date");

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired()
                .HasDefaultValue("Pending");

            entity.Property(e => e.RunType)
                .HasColumnName("run_type")
                .HasMaxLength(20)
                .IsUnicode(false)
                .IsRequired()
               .HasConversion<string>() 
               .HasDefaultValue(PayrollRunType.Regular);

            entity.Property(e => e.IsLocked)
                .HasColumnName("is_locked")
                .HasDefaultValue(false);

            entity.Property(e => e.LockedAt)
                .HasColumnName("locked_at")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.LockedBy)
                .HasColumnName("locked_by");

            entity.Property(e => e.ApprovalStatus)
                .HasColumnName("approval_status")
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsRequired()
                .HasDefaultValue("Pending");

            entity.Property(e => e.TriggeredBy)
                .HasColumnName("triggered_by")
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired()
                .HasDefaultValue("Manual");

            entity.Property(e => e.ScheduledRunDate)
                .HasColumnName("scheduled_run_date")
                .HasColumnType("date");

            entity.Property(e => e.ActualRunStart)
                .HasColumnName("actual_run_start")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.ActualRunEnd)
                .HasColumnName("actual_run_end")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.TotalEmployees)
                .HasColumnName("total_employees");

            entity.Property(e => e.TotalNetPayable)
                .HasColumnName("total_net_payable")
                .HasPrecision(16, 2);

            entity.Property(e => e.IsRecalculated)
                .HasColumnName("is_recalculated")
                .HasDefaultValue(false);

            entity.Property(e => e.ParentPayrollRunId)
                .HasColumnName("parent_payroll_run_id");

            entity.Property(e => e.ApprovedBy)
                .HasColumnName("approved_by");

            entity.Property(e => e.ApprovedAt)
                .HasColumnName("approved_at")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.ErrorMessage)
                .HasColumnName("error_message")
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.OrgId, e.PayrollMonth, e.PayrollYear, e.RunType })
                .IsUnique()
                .HasDatabaseName("uq_payroll_run_org_month_year_type");

            entity.HasIndex(e => new { e.OrgId, e.PayrollYear, e.PayrollMonth })
                .HasDatabaseName("ix_payroll_run_org_year_month");

            entity.HasCheckConstraint("chk_payroll_run_month", "payroll_month BETWEEN 1 AND 12");
            entity.HasCheckConstraint("chk_payroll_run_status", "status IN ('Pending', 'Running', 'Completed', 'Failed')");
            entity.HasCheckConstraint("chk_payroll_run_type", "run_type IN ('Regular', 'Supplementary', 'Revised')");
            entity.HasCheckConstraint("chk_payroll_run_approval", "approval_status IN ('Pending', 'Approved', 'Rejected')");
            entity.HasCheckConstraint("chk_payroll_run_trigger", "triggered_by IN ('Manual', 'Scheduled')");

            entity.HasOne(e => e.ParentPayrollRun)
                .WithMany(e => e.ReRuns)
                .HasForeignKey(e => e.ParentPayrollRunId)
                .HasConstraintName("fk_payroll_run_parent")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SalaryRevisionLog>(entity =>
        {
            entity.ToTable("salary_revision_log");

            entity.HasKey(e => e.Id).HasName("pk_salary_revision_log");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.EmployeeId)
                .HasColumnName("employee_id");

            entity.Property(e => e.EffectiveFrom)
                .HasColumnName("effective_from")
                .HasColumnType("date");

            entity.Property(e => e.OldGross)
                .HasColumnName("old_gross")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.NewGross)
                .HasColumnName("new_gross")
                .HasPrecision(14, 2);

            entity.Property(e => e.RevisedBy)
                .HasColumnName("revised_by");

            entity.Property(e => e.Reason)
                .HasColumnName("reason")
                .HasMaxLength(500);

            entity.Property(e => e.PayrollRunId)
                .HasColumnName("payroll_run_id");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.EmployeeId, e.EffectiveFrom })
                .IsDescending(false, true)
                .HasDatabaseName("ix_srl_employee_effective");

            entity.HasOne(e => e.PayrollRun)
                .WithMany(e => e.SalaryRevisions)
                .HasForeignKey(e => e.PayrollRunId)
                .HasConstraintName("fk_salary_revision_payroll_run")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeeAttendanceSummary>(entity =>
        {
            entity.ToTable("employee_attendance_summary");

            entity.HasKey(e => e.Id).HasName("pk_employee_attendance_summary");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.EmployeeId)
                .HasColumnName("employee_id");

            entity.Property(e => e.Month)
                .HasColumnName("month");

            entity.Property(e => e.Year)
                .HasColumnName("year");

            entity.Property(e => e.TotalWorkingDays)
                .HasColumnName("total_working_days")
                .HasDefaultValue((byte)0);

            entity.Property(e => e.PresentDays)
                .HasColumnName("present_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.AbsentDays)
                .HasColumnName("absent_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.LeaveDays)
                .HasColumnName("leave_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.PaidLeaveDays)
                .HasColumnName("paid_leave_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.UnpaidLeaveDays)
                .HasColumnName("unpaid_leave_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.HalfDays)
                .HasColumnName("half_days")
                .HasDefaultValue((byte)0);

            entity.Property(e => e.WeekOffs)
                .HasColumnName("week_offs")
                .HasDefaultValue((byte)0);

            entity.Property(e => e.Holidays)
                .HasColumnName("holidays")
                .HasDefaultValue((byte)0);

            entity.Property(e => e.OvertimeHours)
                .HasColumnName("overtime_hours")
                .HasPrecision(6, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.DataSource)
                .HasColumnName("data_source")
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsRequired()
                .HasDefaultValue("Auto");

            entity.Property(e => e.IsApproved)
                .HasColumnName("is_approved")
                .HasDefaultValue(false);

            entity.Property(e => e.ApprovedBy)
                .HasColumnName("approved_by");

            entity.Property(e => e.ApprovedAt)
                .HasColumnName("approved_at")
                .HasColumnType("datetime2(0)");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.EmployeeId, e.Month, e.Year })
                .IsUnique()
                .HasDatabaseName("uq_attendance_emp_month_year");

            entity.HasCheckConstraint("chk_attendance_month", "month BETWEEN 1 AND 12");
            entity.HasCheckConstraint("chk_attendance_source", "data_source IN ('Manual', 'Biometric', 'Auto')");
        });

        modelBuilder.Entity<EmployeePayroll>(entity =>
        {
            entity.ToTable("employee_payroll");

            entity.HasKey(e => e.Id).HasName("pk_employee_payroll");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.PayrollRunId)
                .HasColumnName("payroll_run_id");

            entity.Property(e => e.EmployeeId)
                .HasColumnName("employee_id");

            entity.Property(e => e.TotalWorkingDays)
                .HasColumnName("total_working_days")
                .HasDefaultValue((byte)0);

            entity.Property(e => e.PresentDays)
                .HasColumnName("present_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.AbsentDays)
                .HasColumnName("absent_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.LeaveDays)
                .HasColumnName("leave_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.LopDays)
                .HasColumnName("lop_days")
                .HasPrecision(5, 1)
                .HasDefaultValue(0m);

            entity.Property(e => e.GrossSalary)
                .HasColumnName("gross_salary")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.TotalEarnings)
                .HasColumnName("total_earnings")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.TotalDeductions)
                .HasColumnName("total_deductions")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.NetPayable)
                .HasColumnName("net_payable")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.ArrearsAmount)
                .HasColumnName("arrears_amount")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.RoundingAdjustment)
                .HasColumnName("rounding_adjustment")
                .HasPrecision(10, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.SalaryStructureSnapshot)
                .HasColumnName("salary_structure_snapshot")
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.BankAccountSnapshot)
                .HasColumnName("bank_account_snapshot")
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.PaymentStatus)
                .HasColumnName("payment_status")
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsRequired()
                .HasDefaultValue("Pending");

            entity.Property(e => e.PaymentDate)
                .HasColumnName("payment_date")
                .HasColumnType("date");

            entity.Property(e => e.PaymentMode)
                .HasColumnName("payment_mode")
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(15)
                .IsUnicode(false)
                .IsRequired()
                .HasDefaultValue("Calculated");

            entity.Property(e => e.Remarks)
                .HasColumnName("remarks")
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.PayrollRunId, e.EmployeeId })
                .IsUnique()
                .HasDatabaseName("uq_employee_payroll_run_emp");

            entity.HasCheckConstraint("chk_ep_payment_status", "payment_status IN ('Pending', 'Paid', 'OnHold', 'Reversed')");
            entity.HasCheckConstraint("chk_ep_payment_mode", "payment_mode IN ('Bank', 'Cash', 'Cheque')");
            entity.HasCheckConstraint("chk_ep_status", "status IN ('Calculated', 'Failed')");

            entity.HasOne(e => e.PayrollRun)
                .WithMany(e => e.EmployeePayrolls)
                .HasForeignKey(e => e.PayrollRunId)
                .HasConstraintName("fk_employee_payroll_run")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeePayrollComponent>(entity =>
        {
            entity.ToTable("employee_payroll_component");

            entity.HasKey(e => e.Id).HasName("pk_employee_payroll_component");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.EmployeePayrollId)
                .HasColumnName("employee_payroll_id");

            entity.Property(e => e.ComponentId)
                .HasColumnName("component_id");

            entity.Property(e => e.ComponentName)
                .HasColumnName("component_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.IsStatutory)
                .HasColumnName("is_statutory")
                .HasDefaultValue(false);

            entity.Property(e => e.IsTaxExempt)
                .HasColumnName("is_tax_exempt")
                .HasDefaultValue(false);

            entity.Property(e => e.CalculationBasis)
                .HasColumnName("calculation_basis")
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.Property(e => e.BaseAmount)
                .HasColumnName("base_amount")
                .HasPrecision(14, 2);

            entity.Property(e => e.Rate)
                .HasColumnName("rate")
                .HasPrecision(10, 4);

            entity.Property(e => e.FormulaSnapshot)
                .HasColumnName("formula_snapshot")
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.EmployeePayrollId, e.Type })
                .HasDatabaseName("ix_epc_employee_payroll_type");

            entity.HasCheckConstraint("chk_epc_type", "type IN ('Earning', 'Deduction')");
            entity.HasCheckConstraint("chk_epc_calc_basis", "calculation_basis IN ('Fixed', 'PerDay', 'Percentage')");

            entity.HasOne(e => e.EmployeePayroll)
                .WithMany(e => e.Components)
                .HasForeignKey(e => e.EmployeePayrollId)
                .HasConstraintName("fk_epc_employee_payroll")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<EmployeePayrollHistory>(entity =>
        {
            entity.ToTable("employee_payroll_history");

            entity.HasKey(e => e.Id).HasName("pk_employee_payroll_history");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.EmployeeId)
                .HasColumnName("employee_id");

            entity.Property(e => e.PayrollMonth)
                .HasColumnName("payroll_month");

            entity.Property(e => e.PayrollYear)
                .HasColumnName("payroll_year");

            entity.Property(e => e.PayrollRunId)
                .HasColumnName("payroll_run_id");

            entity.Property(e => e.GrossSalary)
                .HasColumnName("gross_salary")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.NetPayable)
                .HasColumnName("net_payable")
                .HasPrecision(14, 2)
                .HasDefaultValue(0m);

            entity.Property(e => e.JsonSnapshot)
                .HasColumnName("json_snapshot")
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.EmployeeId, e.PayrollMonth, e.PayrollYear, e.PayrollRunId })
                .IsUnique()
                .HasDatabaseName("uq_eph_emp_month_year_run");

            entity.HasIndex(e => new { e.EmployeeId, e.PayrollYear, e.PayrollMonth })
                .HasDatabaseName("ix_eph_employee_year_month");

            entity.HasCheckConstraint("chk_eph_month", "payroll_month BETWEEN 1 AND 12");

            entity.HasOne(e => e.PayrollRun)
                .WithMany(e => e.PayrollHistories)
                .HasForeignKey(e => e.PayrollRunId)
                .HasConstraintName("fk_eph_payroll_run")
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PayrollAuditLog>(entity =>
        {
            entity.ToTable("payroll_audit_log");

            entity.HasKey(e => e.Id).HasName("pk_payroll_audit_log");

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.PayrollRunId)
                .HasColumnName("payroll_run_id");

            entity.Property(e => e.EmployeeId)
                .HasColumnName("employee_id");

            entity.Property(e => e.Action)
                .HasColumnName("action")
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.OldValue)
                .HasColumnName("old_value")
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.NewValue)
                .HasColumnName("new_value")
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.ChangedBy)
                .HasColumnName("changed_by");

            entity.Property(e => e.ChangedAt)
                .HasColumnName("changed_at")
                .HasColumnType("datetime2(0)")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(45)
                .IsUnicode(false);

            entity.HasIndex(e => new { e.PayrollRunId, e.EmployeeId })
                .HasDatabaseName("ix_pal_run_employee");

            entity.HasIndex(e => e.ChangedAt)
                .IsDescending()
                .HasDatabaseName("ix_pal_changed_at");

            entity.HasOne(e => e.PayrollRun)
                .WithMany(e => e.AuditLogs)
                .HasForeignKey(e => e.PayrollRunId)
                .HasConstraintName("fk_pal_payroll_run")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
