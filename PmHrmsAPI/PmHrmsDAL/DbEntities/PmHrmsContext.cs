using Microsoft.EntityFrameworkCore;
using PmHrmsAPI.PmHrmsBAL.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class PmHrmsContext : DbContext
{
    private readonly ITenantService _tenant;

    public DbSet<AttendanceRequest> AttendanceRequests { get; set; }
    public PmHrmsContext()
    {
    }

    public PmHrmsContext(
     DbContextOptions<PmHrmsContext> options,
     ITenantService tenant) : base(options)
    {
        _tenant = tenant;
        CurrentOrgId = tenant.GetOrgId();
    }


   

    public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }
             
    public virtual DbSet<AttendanceLog> AttendanceLogs { get; set; }
     public virtual DbSet<Counter> Counters { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Designation> Designations { get; set; }

    public virtual DbSet<DocumentMaster> DocumentMasters { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EmployeeDetail> EmployeeDetails { get; set; }

    public virtual DbSet<EmployeeDocument> EmployeeDocuments { get; set; }

    public virtual DbSet<EmployeePolicyMapping> EmployeePolicyMappings { get; set; }

    public virtual DbSet<EmployeeRemoteLocation> EmployeeRemoteLocations { get; set; }

    public virtual DbSet<Hash> Hashes { get; set; }

    

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobParameter> JobParameters { get; set; }

    public virtual DbSet<JobQueue> JobQueues { get; set; }

    public virtual DbSet<Layout> Layouts { get; set; }

    public virtual DbSet<LeaveAllocationRule> LeaveAllocationRules { get; set; }

    public virtual DbSet<LeaveRequest> LeaveRequests { get; set; }

    public virtual DbSet<LeaveRuleDesignation> LeaveRuleDesignations { get; set; }

    public virtual DbSet<LeaveType> LeaveTypes { get; set; }
    public virtual DbSet<LeaveBalance> LeaveBalance { get; set; }

    public virtual DbSet<List> Lists { get; set; }

    public virtual DbSet<MigrationConfig> MigrationConfigs { get; set; }

    public virtual DbSet<MigrationJob> MigrationJobs { get; set; }

    public virtual DbSet<MigrationJobRow> MigrationJobRows { get; set; }

    public virtual DbSet<OfficeLocation> OfficeLocations { get; set; }


    public virtual DbSet<OrgRole> OrgRoles { get; set; }

    public virtual DbSet<OrgWorkSchedule> OrgWorkSchedules { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<OrganizationDocumentRequirement> OrganizationDocumentRequirements { get; set; }

    public virtual DbSet<OtpVerification> OtpVerifications { get; set; }

    public virtual DbSet<PermissionMaster> PermissionMasters { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleLayoutAccess> RoleLayoutAccesses { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Schema> Schemas { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<ShiftMaster> ShiftMasters { get; set; }

    public virtual DbSet<SpecialLeaveRequest> SpecialLeaveRequests { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<State1> States1 { get; set; }

    public virtual DbSet<SystemRole> SystemRoles { get; set; }

    public virtual DbSet<WorkPolicy> WorkPolicies { get; set; }
    public virtual DbSet<Post> Posts { get; set; }
    public virtual DbSet<PostTarget> PostTargets { get; set; }
    public virtual DbSet<TaskEntity> Tasks { get; set; }
    public virtual DbSet<TaskAssignment> TaskAssignments { get; set; }
    public virtual DbSet<TaskEmployeeProgress> TaskEmployeeProgresses { get; set; }
    public virtual DbSet<TaskFollowUp> TaskFollowUps { get; set; }
    public virtual DbSet<TaskFollowUpReceipt> TaskFollowUpReceipts { get; set; }
    public virtual DbSet<TaskNote> TaskNotes { get; set; }
    public virtual DbSet<LeaveMaster> LeaveMasters { get; set; }
    public virtual DbSet<ExpenseMaster> ExpenseMasters { get; set; }
    public virtual DbSet<OrganizationExpenseConfig> OrganizationExpenseConfigs { get; set; }
    public virtual DbSet<ExpenseClaim> ExpenseClaims { get; set; }

    public virtual DbSet<SystemMailSetting> SystemMailSettings { get; set; }
    public virtual DbSet<WorkPolicyWeekOff> WorkPolicyWeekOffs { get; set; }
    public virtual DbSet<SystemHoliday> SystemHolidays { get; set; }
    public virtual DbSet<HolidayGroup> HolidayGroups { get; set; }
    public virtual DbSet<HolidayGroupMapping> HolidayGroupMappings { get; set; }
    public virtual DbSet<HolidayGroupEligibility> HolidayGroupEligibilities { get; set; }
    



    public DbSet<DesignationWorkPolicyMapping> DesignationWorkPolicyMappings { get; set; }
    public DbSet<EmployeeBankAccount> EmployeeBankAccounts { get; set; }
    public DbSet<SalaryStructure> SalaryStructures {get; set;}
    public DbSet<SalaryComponent> SalaryComponents {get; set;}
    public DbSet<EmployeeSalaryMapping> EmployeeSalaryMappings {get; set;}
    public DbSet<DesignationSalaryMapping> DesignationSalaryMappings {get; set;}
    public DbSet<SalaryComponentMaster> SalaryComponentMasters {get; set;}


    public int CurrentOrgId { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
     #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=14.192.17.204;User Id=sa-shootbucket;Password=Perk123!@#;Initial Catalog=PerkMindsHRMS;Integrated Security=False;Encrypt=False");


                         




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<DesignationWorkPolicyMapping>()
      .ToTable("DesignationWorkPolicyMapping");  

        modelBuilder.Entity<LeaveRequest>()
    .HasOne(l => l.ApprovedBy)
    .WithMany()
    .HasForeignKey(l => l.ApprovedById)
    .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Department>()
  .HasQueryFilter(d => d.OrganizationId == CurrentOrgId);

        modelBuilder.Entity<Employee>()
            .HasQueryFilter(e => e.OrganizationId == CurrentOrgId);

        modelBuilder.Entity<Attendance>()
    .HasQueryFilter(a => a.OrganizationId == CurrentOrgId);

        modelBuilder.Entity<WorkPolicy>()
            .HasQueryFilter(p => p.OrganizationId == CurrentOrgId);

        modelBuilder.Entity<EmployeePolicyMapping>()
            .HasQueryFilter(m => m.OrganizationId == CurrentOrgId);

        modelBuilder.Entity<AttendanceLog>()
            .HasQueryFilter(l => l.OrganizationId == CurrentOrgId);

        
        modelBuilder.Entity<LeaveRequest>()
    .HasQueryFilter(l => l.OrganizationId == CurrentOrgId);





        modelBuilder.Entity<LeaveMaster>(entity =>
        {
            entity.ToTable("leave_master");
            entity.HasKey(e => e.LeaveMasterId);
            entity.Property(e => e.LeaveMasterId).HasColumnName("leave_master_id");
            entity.Property(e => e.LeaveTypeName).HasColumnName("leave_type_name").HasMaxLength(50).IsRequired();
            entity.Property(e => e.MaxDaysPerApplication).HasColumnName("max_days_per_application");
            entity.Property(e => e.IsBalanceBased).HasColumnName("is_balance_based");
            entity.Property(e => e.IsSpecialPolicy).HasColumnName("is_special_policy");
            entity.Property(e => e.IsSystemDefault).HasColumnName("is_system_default");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.LeaveTypeId).HasName("pk_leave_type");
            entity.ToTable("leave_type");

            entity.Property(e => e.LeaveTypeId).HasColumnName("leave_type_id");
            entity.Property(e => e.LeaveTypeName).HasColumnName("leave_type_name").HasMaxLength(50);
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.LeaveMasterId).HasColumnName("leave_master_id");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.IsBalanceBased).HasColumnName("is_balance_based").HasDefaultValue(true);
            entity.Property(e => e.IsSpecialPolicy).HasColumnName("is_special_policy");
            entity.Property(e => e.MaxDaysPerApplication).HasColumnName("max_days_per_application");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.LeaveMaster)
                  .WithMany()
                  .HasForeignKey(d => d.LeaveMasterId)
                  .HasConstraintName("FK_LeaveType_LeaveMaster");
        });

        modelBuilder.Entity<ExpenseMaster>(entity => {
            entity.ToTable("expense_master");
            entity.Property(e => e.TypeName).HasColumnName("type_name");
            entity.Property(e => e.DefaultMaxLimit).HasColumnName("default_max_limit");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<OrganizationExpenseConfig>(entity => {
            entity.ToTable("organization_expense_config");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.ExpenseTypeId).HasColumnName("expense_type_id");
            entity.Property(e => e.MaxLimit).HasColumnName("max_limit");
            entity.Property(e => e.IsEnabled).HasColumnName("is_enabled");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
        });

        modelBuilder.Entity<ExpenseClaim>(entity => {
            entity.ToTable("expense_claims");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.ExpenseTypeId).HasColumnName("expense_type_id");
            entity.Property(e => e.FilePath).HasColumnName("file_path");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.ReviewedBy).HasColumnName("reviewed_by");
            entity.Property(e => e.ReviewedDate).HasColumnName("reviewed_date");
        });

        modelBuilder.Entity<AggregatedCounter>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PK_HangFire_CounterAggregated");

            entity.ToTable("AggregatedCounter", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_AggregatedCounter_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__app_user__B9BE370F5017959E");

            entity.ToTable("app_users");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsFirstLogin)
                .HasDefaultValue(true)
                .HasColumnName("is_first_login");
            entity.Property(e => e.OrgRoleId).HasColumnName("org_role_id");
            entity.Property(e => e.OtpCode)
                .HasMaxLength(6)
                .HasColumnName("otp_code");
            entity.Property(e => e.OtpExpiry)
                .HasColumnType("datetime")
                .HasColumnName("otp_expiry");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.RoleId)
                .HasDefaultValue(4)
                .HasColumnName("role_id");
            entity.Property(e => e.SystemRoleId).HasColumnName("system_role_id");


            entity.HasOne(d => d.Employee).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_employee");

            entity.HasOne(d => d.Role).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_users_role");

            entity.HasOne(d => d.SystemRole).WithMany(p => p.AppUsers)
                .HasForeignKey(d => d.SystemRoleId)
                .HasConstraintName("FK_users_system_role");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("pk_attendance");

            entity.ToTable("attendance");

            entity.HasIndex(e => e.OrganizationId, "ix_attendance_organization_id");

            entity.HasIndex(e => new { e.EmployeeId, e.AttendanceDate }, "ux_employee_attendance_date").IsUnique();

            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");
            entity.Property(e => e.CheckInIp)
                .HasMaxLength(45)
                .HasColumnName("check_in_ip");
            entity.Property(e => e.CheckInLatitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("check_in_latitude");
            entity.Property(e => e.CheckInLongitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("check_in_longitude");
            entity.Property(e => e.CheckInTime).HasColumnName("check_in_time");
            entity.Property(e => e.CheckOutIp)
                .HasMaxLength(45)
                .HasColumnName("check_out_ip");
            entity.Property(e => e.CheckOutLatitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("check_out_latitude");
            entity.Property(e => e.CheckOutLongitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("check_out_longitude");
            entity.Property(e => e.CheckOutTime).HasColumnName("check_out_time");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsManualEntry).HasColumnName("is_manual_entry");
            entity.Property(e => e.IsVoided).HasColumnName("is_voided");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalWorkingMinutes).HasColumnName("total_working_minutes");
            entity.Property(e => e.VoidReason).HasColumnName("void_reason");
            entity.Property(e => e.VoidedAt).HasColumnName("voided_at");
        });

        modelBuilder.Entity<AttendanceLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("pk_attendance_log");

            entity.ToTable("attendance_log");

            entity.HasIndex(e => new { e.OrganizationId, e.EmployeeId }, "ix_log_org_lookup");

            entity.HasIndex(e => new { e.IsProcessed, e.EmployeeId, e.LogTimestamp }, "ix_unprocessed_logs");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceInfo)
                .HasMaxLength(255)
                .HasColumnName("device_info");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(45)
                .HasColumnName("ip_address");
            entity.Property(e => e.IsProcessed).HasColumnName("is_processed");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.LogTimestamp).HasColumnName("log_timestamp");
            entity.Property(e => e.LogType).HasColumnName("log_type");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
        });

        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_Counter");

            entity.ToTable("Counter", "HangFire");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("PK__countrie__7E8CD055582E6642");

            entity.ToTable("countries");

            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CountryName)
                .HasMaxLength(100)
                .HasColumnName("country_name");
            entity.Property(e => e.IsoCode)
                .HasMaxLength(10)
                .HasColumnName("iso_code");
            entity.Property(e => e.PhoneCode)
                .HasMaxLength(10)
                .HasColumnName("phone_code");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("departments");

            entity.HasIndex(e => new { e.OrganizationId, e.DepartmentName }, "IX_Departments_OrgId_Name");

            entity.HasIndex(e => new { e.OrganizationId, e.DepartmentNameNormalized }, "ux_department_org_name")
                .IsUnique()
                .HasFilter("([is_active]=(1))");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(100)
                .HasColumnName("department_name");
            entity.Property(e => e.DepartmentNameNormalized)
                .HasMaxLength(100)
                .HasComputedColumnSql("(lower(ltrim(rtrim([department_name]))))", false)
                .HasColumnName("department_name_normalized");
            entity.Property(e => e.HeadOfDepartmentId).HasColumnName("head_of_department_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsSystemDefault).HasColumnName("is_system_default");
            entity.Property(e => e.OrganizationId)
                .HasDefaultValue(1)
                .HasColumnName("organization_id");

            entity.HasOne(d => d.HeadOfDepartment).WithMany(p => p.Departments)
                .HasForeignKey(d => d.HeadOfDepartmentId)
                .HasConstraintName("FK_departments_hod");

            entity.HasOne(d => d.Organization).WithMany(p => p.Departments)
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_departments_org");
        });

        modelBuilder.Entity<Designation>(entity =>
        {
            entity.HasKey(e => e.DesignationId).HasName("PK__designat__177649C19DF5CFA5");

            entity.ToTable("designations");

            entity.HasIndex(e => new { e.DepartmentId, e.DesignationName }, "IX_Designations_DeptId_Name");

            entity.Property(e => e.DesignationId).HasColumnName("designation_id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DesignationName)
                .HasMaxLength(100)
                .HasColumnName("designation_name");
            entity.Property(e => e.HierarchyLevel).HasColumnName("hierarchy_level");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsSystemDefault).HasColumnName("is_system_default");

            entity.HasOne(d => d.Department).WithMany(p => p.Designations)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("fk_designations_department");
        });

        modelBuilder.Entity<DocumentMaster>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__document__9666E8ACFF67AF36");

            entity.ToTable("document_master");

            entity.HasIndex(e => e.DocumentKey, "UQ__document__2BF5CD38EFA11500").IsUnique();

            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("display_name");
            entity.Property(e => e.DocumentKey)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("document_key");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsExpiryRequired)
                .HasDefaultValue(false)
                .HasColumnName("is_expiry_required");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("employees");

            entity.HasIndex(e => new { e.OrganizationId, e.OfficialEmail }, "IX_Employees_OrgId_Email");

            entity.HasIndex(e => new { e.OrganizationId, e.EmployeeCode }, "UQ_Employee_Code_Org").IsUnique();

            entity.HasIndex(e => new { e.OrganizationId, e.OfficialEmail }, "UQ_Employee_Email_Org").IsUnique();

            entity.HasIndex(e => new { e.OrganizationId, e.EmployeeCode }, "UQ_employees_code_per_org").IsUnique();

            entity.HasIndex(e => e.OfficialEmail, "UQ_employees_email").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.AltPhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("alt_phone_number");
            entity.Property(e => e.AssignedOfficeId).HasColumnName("assigned_office_id");
            entity.Property(e => e.DateOfJoining).HasColumnName("date_of_joining");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.DesignationId).HasColumnName("designation_id");
            entity.Property(e => e.EmployeeCode)
                .HasMaxLength(20)
                .HasColumnName("employee_code");
            entity.Property(e => e.EmploymentStatus)
                .HasMaxLength(50)
                .HasColumnName("employment_status");
            entity.Property(e => e.ExitReason).HasColumnName("exit_reason");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.LastWorkingDay).HasColumnName("last_working_day");
            entity.Property(e => e.NoticePeriodDays).HasColumnName("notice_period_days");
            entity.Property(e => e.OfficialEmail)
                .HasMaxLength(100)
                .HasColumnName("official_email");
            entity.Property(e => e.OfficialImageUrl).HasColumnName("official_image_url");
            entity.Property(e => e.OrganizationId)
                .HasDefaultValue(1)
                .HasColumnName("organization_id");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.ReportingManagerId).HasColumnName("reporting_manager_id");
            entity.Property(e => e.ResignationDate).HasColumnName("resignation_date");
            entity.Property(e => e.SecondaryManagerId).HasColumnName("secondary_manager_id");
            entity.Property(e => e.HolidayGroupId).HasColumnName("holiday_group_id"); 
               entity.Property(e => e.SalaryStructureId).HasColumnName("salary_structure_id");

            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.WorkMode)
                .HasMaxLength(20)
                .HasColumnName("work_mode");

            entity.HasOne(d => d.AssignedOffice).WithMany(p => p.Employees)
                .HasForeignKey(d => d.AssignedOfficeId)
                .HasConstraintName("FK_employees_office");

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_employees_department");

            entity.HasOne(d => d.Organization).WithMany(p => p.Employees)   
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_employees_org");

                entity.HasOne(d => d.SalaryStructure)
            .WithMany()       
            .HasForeignKey(d => d.SalaryStructureId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_employees_salary_structure");

            entity.HasOne(d => d.Policy).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PolicyId)
                .HasConstraintName("FK_employees_work_policy");

            entity.HasOne(d => d.ReportingManager).WithMany(p => p.InverseReportingManager)
                .HasForeignKey(d => d.ReportingManagerId)
                .HasConstraintName("FK_employees_primary_manager");

            entity.HasOne(d => d.SecondaryManager).WithMany(p => p.InverseSecondaryManager)
                .HasForeignKey(d => d.SecondaryManagerId)
                .HasConstraintName("FK_employees_secondary_manager");

            entity.HasOne(d => d.Shift).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK_employees_shift");

             
        });

        modelBuilder.Entity<EmployeeDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__employee__38E9A2245703BCEB");

            entity.ToTable("employee_details");

            entity.HasIndex(e => e.EmployeeId, "UQ__employee__C52E0BA92C4B3888").IsUnique();

            entity.Property(e => e.DetailId).HasColumnName("detail_id");
            entity.Property(e => e.AadharNumber)
                .HasMaxLength(20)
                .HasColumnName("aadhar_number");
            entity.Property(e => e.BloodGroup)
                .HasMaxLength(5)
                .HasColumnName("blood_group");
            entity.Property(e => e.CurrentAddressLine)
                .HasMaxLength(250)
                .HasColumnName("current_address_line");
            entity.Property(e => e.CurrentCity)
                .HasMaxLength(100)
                .HasColumnName("current_city");
            entity.Property(e => e.CurrentCountryId).HasColumnName("current_country_id");
            entity.Property(e => e.CurrentStateId).HasColumnName("current_state_id");
            entity.Property(e => e.CurrentZipCode)
                .HasMaxLength(10)
                .HasColumnName("current_zip_code");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FatherName)
                .HasMaxLength(100)
                .HasColumnName("father_name");
            entity.Property(e => e.GithubUrl)
                .HasMaxLength(200)
                .HasColumnName("github_url");
            entity.Property(e => e.LinkedinUrl)
                .HasMaxLength(200)
                .HasColumnName("linkedin_url");
            entity.Property(e => e.MaritalStatus)
                .HasMaxLength(20)
                .HasColumnName("marital_status");
            entity.Property(e => e.PanNumber)
                .HasMaxLength(20)
                .HasColumnName("pan_number");
            entity.Property(e => e.PassportNumber)
                .HasMaxLength(30)
                .HasColumnName("passport_number");

            entity.HasOne(d => d.CurrentCountry).WithMany(p => p.EmployeeDetails)
                .HasForeignKey(d => d.CurrentCountryId)
                .HasConstraintName("fk_emp_details_country");

            entity.HasOne(d => d.CurrentState).WithMany(p => p.EmployeeDetails)
                .HasForeignKey(d => d.CurrentStateId)
                .HasConstraintName("fk_emp_details_state");

            entity.HasOne(d => d.Employee).WithOne(p => p.EmployeeDetail)
                .HasForeignKey<EmployeeDetail>(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_emp_details_employee");
        });

        modelBuilder.Entity<EmployeeDocument>(entity =>
        {
            entity.HasKey(e => e.DocumentId).HasName("PK__employee__9666E8AC9D3C4FBE");

            entity.ToTable("employee_documents");

            entity.Property(e => e.DocumentId).HasColumnName("document_id");
            entity.Property(e => e.DocumentMasterId).HasColumnName("document_master_id");
            entity.Property(e => e.DocumentPath).HasColumnName("document_path");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .HasColumnName("document_type");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.HrRemarks).HasColumnName("hr_remarks");
            entity.Property(e => e.UploadDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("upload_date");
            entity.Property(e => e.VerificationStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .HasColumnName("verification_status");
            entity.Property(e => e.VerifiedById).HasColumnName("verified_by_id");
            entity.Property(e => e.VerifiedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("verified_date");

            entity.HasOne(d => d.DocumentMaster).WithMany(p => p.EmployeeDocuments)
                .HasForeignKey(d => d.DocumentMasterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_emp_doc_master");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeDocumentEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_emp_docs_owner");

            entity.HasOne(d => d.VerifiedBy).WithMany(p => p.EmployeeDocumentVerifiedBies)
                .HasForeignKey(d => d.VerifiedById)
                .HasConstraintName("fk_emp_docs_verifier");
        });

        modelBuilder.Entity<EmployeePolicyMapping>(entity =>
        {
            entity.HasKey(e => e.MappingId).HasName("pk_employee_policy_mapping");

            entity.ToTable("employee_policy_mapping");

            entity.HasIndex(e => new { e.EmployeeId, e.EffectiveFrom, e.EffectiveTo }, "ix_emp_policy_lookup").HasFilter("([is_active]=(1))");

            entity.Property(e => e.MappingId).HasColumnName("mapping_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
        });

        modelBuilder.Entity<EmployeeRemoteLocation>(entity =>
        {
            entity.HasKey(e => e.RemoteId).HasName("PK__employee__BFECF14E44D76C30");

            entity.ToTable("employee_remote_locations");

            entity.Property(e => e.RemoteId).HasColumnName("remote_id");
            entity.Property(e => e.ApprovedById).HasColumnName("approved_by_id");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.GeoRadiusMeters)
                .HasDefaultValue(200)
                .HasColumnName("geo_radius_meters");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("is_active");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.LocationAlias)
                .HasMaxLength(100)
                .HasColumnName("location_alias");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");

            entity.HasOne(d => d.ApprovedBy).WithMany(p => p.EmployeeRemoteLocationApprovedBies)
                .HasForeignKey(d => d.ApprovedById)
                .HasConstraintName("fk_remote_loc_approver");

            entity.HasOne(d => d.Employee).WithMany(p => p.EmployeeRemoteLocationEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_remote_loc_employee");
        });

        modelBuilder.Entity<Hash>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Field }).HasName("PK_HangFire_Hash");

            entity.ToTable("Hash", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Hash_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Field).HasMaxLength(100);
        });

      

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangFire_Job");

            entity.ToTable("Job", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Job_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => e.StateName, "IX_HangFire_Job_StateName").HasFilter("([StateName] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.StateName).HasMaxLength(20);
        });

        modelBuilder.Entity<JobParameter>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Name }).HasName("PK_HangFire_JobParameter");

            entity.ToTable("JobParameter", "HangFire");

            entity.Property(e => e.Name).HasMaxLength(40);

            entity.HasOne(d => d.Job).WithMany(p => p.JobParameters)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_JobParameter_Job");
        });

        modelBuilder.Entity<JobQueue>(entity =>
        {
            entity.HasKey(e => new { e.Queue, e.Id }).HasName("PK_HangFire_JobQueue");

            entity.ToTable("JobQueue", "HangFire");

            entity.Property(e => e.Queue).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FetchedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Layout>(entity =>
        {
            entity.HasKey(e => e.LayoutId).HasName("PK__layouts__255F1E9A41765EF4");

            entity.ToTable("layouts");

            entity.HasIndex(e => e.LayoutKey, "UQ__layouts__88F341574417C2F1").IsUnique();

            entity.Property(e => e.LayoutId).HasColumnName("layout_id");
            entity.Property(e => e.LayoutKey)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("layout_key");
            entity.Property(e => e.LayoutName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("layout_name");
        });

        modelBuilder.Entity<LeaveAllocationRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__leave_al__E92A9296C7B5568D");

            entity.ToTable("leave_allocation_rule");

            entity.HasIndex(e => new { e.OrganizationId, e.LeaveTypeId }, "IX_leave_allocation_rule_org_leave");

            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.CarryForward).HasColumnName("carry_forward");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.DaysPerMonth).HasColumnName("days_per_month");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.LeaveTypeId).HasColumnName("leave_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.RuleName)
                .HasMaxLength(100)
                .HasColumnName("rule_name");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveAllocationRules)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_leave_allocation_rule_leave_type");
        });



             modelBuilder.Entity<LeaveBalance>(entity =>
                {
                    entity.ToTable("leave_balance");

                    entity.HasKey(e => e.BalanceId);

                    entity.Property(e => e.BalanceId)
                        .HasColumnName("balance_id");

                    entity.Property(e => e.EmployeeId)
                        .HasColumnName("employee_id")
                        .IsRequired();

                    entity.Property(e => e.LeaveTypeId)
                        .HasColumnName("leave_type_id")
                        .IsRequired();

                    entity.Property(e => e.Month)
                        .HasColumnName("month")
                        .IsRequired();

                    entity.Property(e => e.Year)
                        .HasColumnName("year")
                        .IsRequired();

                    entity.Property(e => e.Balance)
                        .HasColumnName("balance")
                        .HasColumnType("decimal(5,2)")
                        .IsRequired();

                    entity.Property(e => e.Used)
                        .HasColumnName("used")
                        .HasColumnType("decimal(5,2)")
                        .HasDefaultValue(0);

                    entity.Property(e => e.PreDeducted)
                        .HasColumnName("pre_deducted")
                        .HasColumnType("decimal(5,2)")
                        .HasDefaultValue(0);

                    entity.Property(e => e.RuleSnapshotId)
                        .HasColumnName("rule_snapshot_id");

                    entity.Property(e => e.CreatedAt)
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("GETDATE()");

                    // Foreign Keys

                    entity.HasOne(d => d.Employee)
                        .WithMany()
                        .HasForeignKey(d => d.EmployeeId)
                        .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(d => d.LeaveType)
                        .WithMany()
                        .HasForeignKey(d => d.LeaveTypeId)
                        .OnDelete(DeleteBehavior.Cascade);
                });

        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(e => e.LeaveId).HasName("pk_leave_request");

            entity.ToTable("leave_request");

            entity.HasIndex(e => new { e.EmployeeId, e.FromDate, e.ToDate }, "ix_leave_emp_date");

            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.ActionAt).HasColumnName("action_at");
            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("applied_at");
            entity.Property(e => e.ApprovedById).HasColumnName("approved_by_id");
            entity.Property(e => e.CalendarDays).HasColumnName("calendar_days");
            entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
            entity.Property(e => e.FromDate).HasColumnName("from_date");
            entity.Property(e => e.IsSpecialRequest).HasColumnName("is_special_request");
            entity.Property(e => e.LeaveTypeId).HasColumnName("leave_type_id");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .HasColumnName("reason");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.ToDate).HasColumnName("to_date");
            entity.Property(e => e.TotalDays)
                .HasColumnType("decimal(4, 1)")
                .HasColumnName("total_days");

            entity.HasOne(d => d.LeaveType).WithMany(p => p.LeaveRequests)
                .HasForeignKey(d => d.LeaveTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_leave_request_type");
        });

        modelBuilder.Entity<LeaveRuleDesignation>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__leave_ru__3213E83F53567EBD");

                entity.ToTable("leave_rule_designation");

                // Updated Index: Replace RuleId with RuleName
                entity.HasIndex(e => new { e.RuleName, e.DesignationId, e.OrganizationId }, "UQ_rule_designation")
                    .IsUnique();

                entity.HasIndex(e => new { e.DesignationId, e.OrganizationId }, "IX_leave_rule_designation_designation");

                entity.Property(e => e.Id).HasColumnName("id");
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_at");

                entity.Property(e => e.DesignationId).HasColumnName("designation_id");
                entity.Property(e => e.OrganizationId).HasColumnName("organization_id");

               
                entity.Property(e => e.RuleName)
                    .HasColumnName("rule_name")
                    .HasMaxLength(100)
                    .IsRequired();
                    entity.Property(e => e.RuleId).HasColumnName("rule_id");

                
            });

        modelBuilder.Entity<LeaveType>(entity =>
        {
            entity.HasKey(e => e.LeaveTypeId).HasName("pk_leave_type");

            entity.ToTable("leave_type");

            entity.Property(e => e.LeaveTypeId).HasColumnName("leave_type_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsBalanceBased)
                .HasDefaultValue(true)
                .HasColumnName("is_balance_based");
            entity.Property(e => e.IsSpecialPolicy).HasColumnName("is_special_policy");
            entity.Property(e => e.LeaveTypeName)
                .HasMaxLength(50)
                .HasColumnName("leave_type_name");
            entity.Property(e => e.MaxDaysPerApplication).HasColumnName("max_days_per_application");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
        });

        modelBuilder.Entity<List>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_List");

            entity.ToTable("List", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_List_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<MigrationConfig>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Migratio__3214EC07849426A0");

            entity.ToTable("migration_configs");

            entity.Property(e => e.Category)
                .HasMaxLength(50)
                .HasDefaultValue("Core")
                .HasColumnName("category");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entity_type");
            entity.Property(e => e.FieldKey)
                .HasMaxLength(100)
                .HasColumnName("field_key");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsRequired)
                .HasDefaultValue(false)
                .HasColumnName("is_required");
            entity.Property(e => e.Keywords).HasColumnName("keywords");
            entity.Property(e => e.Label)
                .HasMaxLength(100)
                .HasColumnName("label");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.ValidationType)
                .HasMaxLength(50)
                .HasColumnName("validation_type");
        });

        modelBuilder.Entity<MigrationJob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__migratio__3213E83FCD14EF5B");

            entity.ToTable("migration_jobs");

            entity.HasIndex(e => new { e.OrgId, e.Status, e.CreatedAt }, "IX_MigrationJobs_OrgId_Status").IsDescending(false, false, true);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentStep)
                .HasMaxLength(100)
                .HasColumnName("current_step");
            entity.Property(e => e.EntityType)
                .HasMaxLength(50)
                .HasColumnName("entity_type");
            entity.Property(e => e.ErrorLog).HasColumnName("error_log");
            entity.Property(e => e.FailedCount)
                .HasDefaultValue(0)
                .HasColumnName("failed_count");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.ImportedCount)
                .HasDefaultValue(0)
                .HasColumnName("imported_count");
            entity.Property(e => e.LastHeartbeat).HasColumnName("last_heartbeat");
            entity.Property(e => e.MappingJson).HasColumnName("mapping_json");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.RequestedByUserId).HasColumnName("requested_by_user_id");
            entity.Property(e => e.SharedPassword)
                .HasMaxLength(100)
                .HasColumnName("shared_password");
            entity.Property(e => e.StartedAt)
                .HasColumnType("datetime")
                .HasColumnName("started_at");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Queued")
                .HasColumnName("status");
            entity.Property(e => e.TotalRecords)
                .HasDefaultValue(0)
                .HasColumnName("total_records");
            entity.Property(e => e.ValidatedCount)
                .HasDefaultValue(0)
                .HasColumnName("validated_count");
            entity.Property(e => e.ValidationSummary).HasColumnName("validation_summary");
        });

        modelBuilder.Entity<MigrationJobRow>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__migratio__3213E83F2A14D680");

            entity.ToTable("migration_job_rows");

            entity.HasIndex(e => new { e.JobId, e.RowHash }, "UQ_MigrationJobRows_JobHash").IsUnique();

            entity.HasIndex(e => e.RowHash, "idx_migration_job_rows_hash");

            entity.HasIndex(e => e.JobId, "idx_migration_job_rows_job_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.JobId).HasColumnName("job_id");
            entity.Property(e => e.RowHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("row_hash");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PENDING")
                .HasColumnName("status");

            entity.HasOne(d => d.Job).WithMany(p => p.MigrationJobRows)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_MigrationJobRows_Job");
        });

        modelBuilder.Entity<OfficeLocation>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__office_l__771831EA8921FC3F");

            entity.ToTable("office_locations");

            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.AllowedIpAddress)
                .HasMaxLength(50)
                .HasColumnName("allowed_ip_address");
            entity.Property(e => e.GeoRadiusMeters)
                .HasDefaultValue(100)
                .HasColumnName("geo_radius_meters");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.LocationName)
                .HasMaxLength(100)
                .HasColumnName("location_name");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
                entity.HasOne<OfficeLocation>()
                .WithMany()
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");

            entity.HasOne(d => d.Organization).WithMany(p => p.OfficeLocations)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("fk_office_organization");
        });

        
        modelBuilder.Entity<OrgRole>(entity =>
        {
            entity.HasKey(e => e.OrgRoleId).HasName("PK__org_role__AE20327865E11F01");

            entity.ToTable("org_roles");

            entity.HasIndex(e => new { e.OrgId, e.Name }, "UX_OrgRole_Org_Name").IsUnique();

            entity.Property(e => e.OrgRoleId).HasColumnName("org_role_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.OrgId).HasColumnName("org_id");

            entity.HasOne(d => d.Org).WithMany(p => p.OrgRoles)
                .HasForeignKey(d => d.OrgId)
                .HasConstraintName("FK__org_roles__org_i__69C6B1F5");
        });

        modelBuilder.Entity<OrgWorkSchedule>(entity =>
        {
            entity.HasKey(e => e.ScheduleId).HasName("PK__org_work__C46A8A6F6F202BCF");

            entity.ToTable("org_work_schedule");

            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.WorkingDaysMask).HasColumnName("working_days_mask");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.OrgId).HasName("PK__organiza__F6AD8012C894CC2C");

            entity.ToTable("organizations");

            entity.HasIndex(e => e.CreatedByIp, "ix_organizations_created_by_ip");

            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.AddressLine1)
                .HasMaxLength(200)
                .HasColumnName("address_line_1");
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(200)
                .HasColumnName("address_line_2");
            entity.Property(e => e.City)
                .HasMaxLength(50)
                .HasColumnName("city");
            entity.Property(e => e.ContactPhoneNo)
                .HasMaxLength(20)
                .HasColumnName("contact_phone_no");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CreatedByIp)
                .HasMaxLength(50)
                .HasColumnName("created_by_ip");
            entity.Property(e => e.FaviconUrl).HasColumnName("favicon_url");
            entity.Property(e => e.IdGhost).HasColumnName("id_ghost");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsSetupCompleted).HasColumnName("is_setup_completed");
            entity.Property(e => e.LogoUrl).HasColumnName("logo_url");
            entity.Property(e => e.OfficialEmail)
                .HasMaxLength(100)
                .HasColumnName("official_email");
            entity.Property(e => e.OrganizationName)
                .HasMaxLength(100)
                .HasColumnName("organization_name");
            entity.Property(e => e.RegistrationNumber)
                .HasMaxLength(50)
                .HasColumnName("registration_number");
            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.TagLine)
                .HasMaxLength(200)
                .HasColumnName("tag_line");
            entity.Property(e => e.TaxId)
                .HasMaxLength(50)
                .HasColumnName("tax_id");
            entity.Property(e => e.WebsiteUrl)
                .HasMaxLength(200)
                .HasColumnName("website_url");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(20)
                .HasColumnName("zip_code");

            entity.HasOne(d => d.Country).WithMany(p => p.Organizations)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("fk_org_countries");

            entity.HasOne(d => d.State).WithMany(p => p.Organizations)
                .HasForeignKey(d => d.StateId)
                .HasConstraintName("fk_org_states");
        });

        modelBuilder.Entity<OrganizationDocumentRequirement>(entity =>
        {
            entity.HasKey(e => e.RequirementId).HasName("PK__organiza__2A73C1AD549B0294");

            entity.ToTable("organization_document_requirements");

            entity.Property(e => e.RequirementId).HasColumnName("requirement_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DocumentMasterId).HasColumnName("document_master_id");
            entity.Property(e => e.DocumentType)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("document_type");
            entity.Property(e => e.IsMandatory)
                .HasDefaultValue(true)
                .HasColumnName("is_mandatory");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");

            entity.HasOne(d => d.DocumentMaster).WithMany(p => p.OrganizationDocumentRequirements)
                .HasForeignKey(d => d.DocumentMasterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_req_doc_master");

            entity.HasOne(d => d.Organization).WithMany(p => p.OrganizationDocumentRequirements)
                .HasForeignKey(d => d.OrganizationId)
                .HasConstraintName("fk_organization");
        });

        modelBuilder.Entity<OtpVerification>(entity =>
        {
            entity.HasKey(e => e.Target).HasName("PK__otp_veri__D428373BB5711A1D");

            entity.ToTable("otp_verifications");

            entity.Property(e => e.Target)
                .HasMaxLength(100)
                .HasColumnName("target");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsVerified)
                .HasDefaultValue(false)
                .HasColumnName("is_verified");
            entity.Property(e => e.OtpCode)
                .HasMaxLength(6)
                .HasColumnName("otp_code");
            entity.Property(e => e.OtpExpiry)
                .HasColumnType("datetime")
                .HasColumnName("otp_expiry");
            entity.Property(e => e.VerificationType)
                .HasMaxLength(20)
                .HasColumnName("verification_type");
        });

        modelBuilder.Entity<PermissionMaster>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__permissi__E5331AFA46C32186");

            entity.ToTable("permission_master");

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.IsSystemDefault)
                .HasDefaultValue(false)
                .HasColumnName("is_system_default");
            entity.Property(e => e.ModuleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("module_name");
            entity.Property(e => e.PermissionKey)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("permission_key");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CC4930ECAB");

            entity.ToTable("roles");

            entity.HasIndex(e => e.RoleName, "UQ__roles__783254B12C390036").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<RoleLayoutAccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role_lay__3213E83FE2D5D727");

            entity.ToTable("role_layout_access");

            entity.HasIndex(e => new { e.RoleId, e.LayoutId }, "UQ_Role_Layout").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsAllowed)
                .HasDefaultValue(true)
                .HasColumnName("is_allowed");
            entity.Property(e => e.LayoutId).HasColumnName("layout_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.SystemRoleId).HasColumnName("system_role_id");

            entity.HasOne(d => d.Layout).WithMany(p => p.RoleLayoutAccesses)
                .HasForeignKey(d => d.LayoutId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RLA_Layout");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleLayoutAccesses)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RLA_Role");

            entity.HasOne(d => d.SystemRole).WithMany(p => p.RoleLayoutAccesses)
                .HasForeignKey(d => d.SystemRoleId)
                .HasConstraintName("FK_rla_system_role");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId).HasName("PK__role_per__B1E85A101ACA9D16");

            entity.ToTable("role_permissions");

            entity.Property(e => e.RolePermissionId).HasColumnName("role_permission_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.OrgRoleId).HasColumnName("org_role_id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");

            entity.HasOne(d => d.OrgRole).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.OrgRoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_role_permissions_OrgRole");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_role_permissions_Master");
        });

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("PK_HangFire_Schema");

            entity.ToTable("Schema", "HangFire");

            entity.Property(e => e.Version).ValueGeneratedNever();
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangFire_Server");

            entity.ToTable("Server", "HangFire");

            entity.HasIndex(e => e.LastHeartbeat, "IX_HangFire_Server_LastHeartbeat");

            entity.Property(e => e.Id).HasMaxLength(200);
            entity.Property(e => e.LastHeartbeat).HasColumnType("datetime");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Value }).HasName("PK_HangFire_Set");

            entity.ToTable("Set", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Set_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => new { e.Key, e.Score }, "IX_HangFire_Set_Score");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(256);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.SettingId).HasName("PK__Settings__256E1E3237D819FC");

            entity.HasIndex(e => e.Name, "UQ__Settings__72E12F1BA5DD51A5").IsUnique();

            entity.Property(e => e.SettingId).HasColumnName("setting_id");
            entity.Property(e => e.Active)
                .HasDefaultValue(true)
                .HasColumnName("active");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Value)
                .HasMaxLength(500)
                .HasColumnName("value");
        });

        modelBuilder.Entity<ShiftMaster>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__shift_ma__7B26722003C3CB91");

            entity.ToTable("shift_master");

            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.GracePeriodMinutes)
                .HasDefaultValue(0)
                .HasColumnName("grace_period_minutes");
            entity.Property(e => e.ShiftName)
                .HasMaxLength(50)
                .HasColumnName("shift_name");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
        });

        modelBuilder.Entity<SpecialLeaveRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__special___18D3B90F1DFBA4AA");

            entity.ToTable("special_leave_request");

            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.HrComment)
                .HasMaxLength(500)
                .HasColumnName("hr_comment");
            entity.Property(e => e.LeaveId).HasColumnName("leave_id");
            entity.Property(e => e.RequestedLeaveName)
                .HasMaxLength(150)
                .HasColumnName("requested_leave_name");
            entity.Property(e => e.SupportingDocument)
                .HasMaxLength(500)
                .HasColumnName("supporting_document");

            entity.HasOne(d => d.Leave).WithMany(p => p.SpecialLeaveRequests)
                .HasForeignKey(d => d.LeaveId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_special_leave_request_leave");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => e.StateId).HasName("PK__states__81A474173732D6AC");

            entity.ToTable("states");

            entity.Property(e => e.StateId).HasColumnName("state_id");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.StateName)
                .HasMaxLength(100)
                .HasColumnName("state_name");

            entity.HasOne(d => d.Country).WithMany(p => p.States)
                .HasForeignKey(d => d.CountryId)
                .HasConstraintName("fk_states_countries");
        });

        modelBuilder.Entity<State1>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Id }).HasName("PK_HangFire_State");

            entity.ToTable("State", "HangFire");

            entity.HasIndex(e => e.CreatedAt, "IX_HangFire_State_CreatedAt");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(100);

            entity.HasOne(d => d.Job).WithMany(p => p.State1s)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_State_Job");
        });

        modelBuilder.Entity<SystemRole>(entity =>
        {
            entity.HasKey(e => e.SystemRoleId).HasName("PK__system_r__FC70476129808870");

            entity.ToTable("system_roles");

            entity.Property(e => e.SystemRoleId).HasColumnName("system_role_id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<WorkPolicy>(entity =>
        {
            entity.HasKey(e => e.PolicyId).HasName("pk_work_policy");

            entity.ToTable("work_policy");

            entity.HasIndex(e => e.OrganizationId, "ix_work_policy_organization_id");

            entity.Property(e => e.PolicyId).HasColumnName("policy_id");
            entity.Property(e => e.AdditionalBreakMinutes).HasColumnName("additional_break_minutes");
            entity.Property(e => e.BreakEndTime).HasColumnName("break_end_time");
            entity.Property(e => e.BreakStartTime).HasColumnName("break_start_time");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.HalfDayThresholdMinutes)
                .HasDefaultValue(240)
                .HasColumnName("half_day_threshold_minutes");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsBreakPaid).HasColumnName("is_break_paid");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.IsFlexibleShift).HasColumnName("is_flexible_shift");
            entity.Property(e => e.IsWfhAllowed).HasColumnName("is_wfh_allowed");
            entity.Property(e => e.IsWfoRequired)
                .HasDefaultValue(true)
                .HasColumnName("is_wfo_required");
            entity.Property(e => e.LateAfterMinutes)
                .HasDefaultValue(15)
                .HasColumnName("late_after_minutes");
            entity.Property(e => e.MaxBreakCount).HasColumnName("max_break_count");
            entity.Property(e => e.MaxBreakMinutes).HasColumnName("max_break_minutes");
            entity.Property(e => e.OrganizationId).HasColumnName("organization_id");
            entity.Property(e => e.PolicyName)
                .HasMaxLength(100)
                .HasColumnName("policy_name");
            entity.Property(e => e.RequiredWorkingMinutes)
                .HasDefaultValue(480)
                .HasColumnName("required_working_minutes");
            entity.Property(e => e.ShiftEndTime).HasColumnName("shift_end_time");
            entity.Property(e => e.ShiftStartTime).HasColumnName("shift_start_time");
        });


        modelBuilder.Entity<WorkPolicyWeekOff>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__work_pol__3214101F"); // Primary Key

                entity.ToTable("work_policy_week_off");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.PolicyId).HasColumnName("policy_id");

                entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");

                entity.Property(e => e.IsHalfDay)
                    .HasDefaultValue(false)
                    .HasColumnName("is_half_day");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnName("created_at");

                // Foreign Key Relationship
                entity.HasOne(d => d.WorkPolicy)
                    .WithMany(p => p.WeekOffs)
                    .HasForeignKey(d => d.PolicyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("fk_work_policy_week_off_work_policy");
            });




        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.OrgId).HasColumnName("org_id");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            entity.Property(x => x.PostType).HasColumnName("post_type");
            entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(x => x.IsPublished).HasColumnName("is_published");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.ImagePath).HasColumnName("image_path");
            entity.Property(x => x.VisibleFrom).HasColumnName("visible_from");
            entity.Property(x => x.VisibleUntil).HasColumnName("visible_until");

            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PostType).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.CreatedByEmployee)
                .WithMany(e => e.CreatedPosts)
                 .HasForeignKey(x => x.CreatedByUserId)
                 .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(x => x.Targets)
                  .WithOne(x => x.Post)
                  .HasForeignKey(x => x.PostId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ================= POST TARGET =================
        modelBuilder.Entity<PostTarget>(entity =>
        {
            entity.ToTable("post_targets");

            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrgId).HasColumnName("org_id");
           entity.Property(x => x.PostId).HasColumnName("post_id");
            entity.Property(x => x.TargetId).HasColumnName("target_id");
                entity.Property(x => x.TargetType).HasColumnName("target_type");

            entity.HasOne(x => x.Post)
                .WithMany(x => x.Targets)
                .HasForeignKey(x => x.PostId);
        });

        // ================= TASK =================
      modelBuilder.Entity<TaskEntity>(e =>
            {
                e.ToTable("tasks");
                e.HasKey(t => t.Id);

                
                e.Property(t => t.Id).HasColumnName("id");
                e.Property(t => t.OrgId).HasColumnName("org_id");
                e.Property(t => t.PostId).HasColumnName("post_id");
                e.Property(t => t.AssignedByUserId).HasColumnName("assigned_by_user_id");
                e.Property(t => t.DueDate).HasColumnName("due_date");
                e.Property(t => t.CompletedAt).HasColumnName("completed_at");
                e.Property(t => t.IsDeleted).HasColumnName("is_deleted");
                e.Property(t => t.CreatedAt).HasColumnName("created_at");
                e.Property(t => t.UpdatedAt).HasColumnName("updated_at");

               
                e.Property(t => t.ReviewerType).HasColumnName("reviewer_type").HasMaxLength(50).HasDefaultValue("Self");
                e.Property(t => t.ReviewerEmployeeId).HasColumnName("reviewer_employee_id");
                e.Property(t => t.ReviewRemarks).HasColumnName("review_remarks");
                e.Property(t => t.ReviewedAt).HasColumnName("reviewed_at");

                
                e.Property(t => t.Title).HasMaxLength(200).IsRequired();
                e.Property(t => t.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Pending");
                e.Property(t => t.Priority).IsRequired().HasDefaultValue((byte)2);

                e.HasOne(t => t.Post)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(t => t.PostId)
                    .OnDelete(DeleteBehavior.SetNull);

               
                e.HasOne(t => t.AssignedByEmployee)
                    .WithMany() 
                    .HasForeignKey(t => t.AssignedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                
                e.HasOne(t => t.ReviewerEmployee)
                    .WithMany() 
                    .HasForeignKey(t => t.ReviewerEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);

               
                e.HasMany(t => t.Assignments).WithOne().HasForeignKey(a => a.TaskId);
            });
        // ================= TASK ASSIGNMENT =================
        modelBuilder.Entity<TaskAssignment>(e =>
        {
            e.ToTable("task_assignments");
            e.HasKey(a => a.Id);
        
            e.Property(a => a.OrgId).HasColumnName("org_id");
            e.Property(a => a.TaskId).HasColumnName("task_id");
             e.Property(a => a.TargetId).HasColumnName("target_id");
             e.Property(a => a.TargetType).HasColumnName("target_type");
        
            e.HasOne(a => a.Task)
            .WithMany(t => t.Assignments)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        // ================= TASK PROGRESS =================
        modelBuilder.Entity<TaskEmployeeProgress>(e =>
            {
                e.ToTable("task_employee_progress");
                e.HasKey(p => p.Id);
            
                e.Property(p => p.OrgId).HasColumnName("org_id");
                e.Property(p => p.TaskId).HasColumnName("task_id");
                e.Property(p => p.EmployeeId).HasColumnName("employee_id");
                e.Property(p => p.Status).HasMaxLength(20).IsRequired().HasDefaultValue("Pending").HasColumnName("status");
                e.Property(p => p.CompletedAt).HasColumnName("completed_at");
                e.Property(p => p.Remarks).HasColumnName("remarks");
                e.Property(p => p.ReviewRemarks).HasColumnName("review_remarks");
                e.Property(p => p.ReviewedAt).HasColumnName("reviewed_at");
                e.Property(p => p.ReviewedByEmployeeId).HasColumnName("reviewed_by_employee_id");
                e.Property(p => p.SourceType).HasMaxLength(30).IsRequired().HasColumnName("source_type");
                e.Property(p => p.SourceId).HasColumnName("source_id");
                e.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");
                e.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("GETUTCDATE()");

            
                // Unique: ek task pe ek employee ka sirf ek progress row
                e.HasIndex(p => new { p.TaskId, p.EmployeeId }).IsUnique();
            
                e.HasOne(p => p.Task)
                .WithMany(t => t.Progress)
                .HasForeignKey(p => p.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
            
                e.HasOne(p => p.Employee)
                .WithMany()
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(p => p.ReviewedByEmployee)
                .WithMany()
                .HasForeignKey(p => p.ReviewedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
            });

        // ================= FOLLOW UPS =================
       modelBuilder.Entity<TaskFollowUp>(e =>
        {
            e.ToTable("task_follow_ups");
            e.HasKey(f => f.Id);
        
            e.Property(f => f.OrgId).HasColumnName("org_id");
            e.Property(f => f.TaskId).HasColumnName("task_id");
            e.Property(f => f.TargetType).HasColumnName("target_type");
            e.Property(f => f.Message).HasColumnName("message");
            e.Property(f => f.CreatedAt).HasColumnName("created_at");
            e.Property(f => f.CreatedByUserId).HasColumnName("created_by_user_id");
            e.Property(f => f.IsScheduled).HasColumnName("is_scheduled");
            e.Property(f => f.IsSent).HasColumnName("is_sent");
            e.Property(f => f.ScheduledAt).HasColumnName("scheduled_at");
            e.Property(f => f.SentAt).HasColumnName("sent_at");

            e.HasOne(f => f.Task)
                .WithMany(t => t.FollowUps)
                .HasForeignKey(f => f.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(f => f.CreatedByEmployee)
                .WithMany()
                .HasForeignKey(f => f.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ================= RECEIPTS =================
        modelBuilder.Entity<TaskFollowUpReceipt>(e =>
        {
            e.ToTable("task_follow_up_receipts");
            e.HasKey(r => r.Id);
        
            e.Property(r => r.IsRead).HasDefaultValue(false);
        
            e.Property(r => r.OrgId).HasColumnName("org_id");
            e.Property(r => r.FollowUpId).HasColumnName("follow_up_id");
            e.Property(r => r.EmployeeId).HasColumnName("employee_id");

            e.Property(r => r.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            e.Property(r => r.ReadAt).HasColumnName("read_at");             
            e.HasIndex(r => new { r.FollowUpId, r.EmployeeId }).IsUnique();
        
            e.HasOne(r => r.FollowUp)                                                                                                            
            .WithMany(f => f.Receipts)
            .HasForeignKey(r => r.FollowUpId)
            .OnDelete(DeleteBehavior.Cascade);
        });



        modelBuilder.Entity<TaskNote>(e =>
            {
                e.ToTable("task_notes");
                e.HasKey(t => t.Id);

                
                e.Property(t => t.Id).HasColumnName("id");
                e.Property(t => t.OrgId).HasColumnName("org_id");
                e.Property(t => t.TaskId).HasColumnName("task_id");
                e.Property(t => t.Content).HasColumnName("content").IsRequired();
                e.Property(t => t.CreatedByUserId).HasColumnName("created_by_user_id");
                e.Property(t => t.MentionedEmployeeId).HasColumnName("mentioned_employee_id");
                e.Property(t => t.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("getutcdate()");

               
                e.HasOne(t => t.Task)
                    .WithMany(p => p.Notes) 
                    .HasForeignKey(t => t.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Creator Relationship
                e.HasOne(t => t.CreatedByEmployee)
                    .WithMany()
                    .HasForeignKey(t => t.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Mentioned Employee Relationship
                e.HasOne(t => t.MentionedEmployee)
                    .WithMany()
                    .HasForeignKey(t => t.MentionedEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });



            modelBuilder.Entity<SystemMailSetting>(e =>
        {
            // Table Name
            e.ToTable("system_mail_settings");

            // Primary Key
            e.HasKey(t => t.Id);

            // Column Mappings
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.Host).HasColumnName("host").IsRequired().HasMaxLength(255);
            e.Property(t => t.Port).HasColumnName("port").HasDefaultValue(587);
            e.Property(t => t.Mail).HasColumnName("mail").IsRequired().HasMaxLength(255);
            e.Property(t => t.DisplayName).HasColumnName("display_name").HasMaxLength(255);
            e.Property(t => t.UserName).HasColumnName("user_name").HasMaxLength(255);
            e.Property(t => t.Password).HasColumnName("password").IsRequired().HasMaxLength(500);
            
            e.Property(t => t.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("getutcdate()")
                .ValueGeneratedOnAddOrUpdate();

            /* If you have a relationship to an Organization, 
            uncomment the block below:
            */
            /*
            e.HasOne(t => t.Organization)
                .WithMany() 
                .HasForeignKey(t => t.OrgId)
                .OnDelete(DeleteBehavior.Cascade);
            */
        });



        modelBuilder.Entity<SystemHoliday>(entity =>
    {
        entity.ToTable("system_holiday");

        entity.HasKey(x => x.Id).HasName("pk_system_holiday");

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.HolidayDate).HasColumnName("holiday_date");
        entity.Property(x => x.HolidayName).HasColumnName("holiday_name").HasMaxLength(200);
        entity.Property(x => x.Year).HasColumnName("year");
        entity.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(10).HasDefaultValue("IN");
        entity.Property(x => x.IsRecurring).HasColumnName("is_recurring");
        entity.Property(x => x.IsCustom).HasColumnName("is_custom");
        entity.Property(x => x.CreatedByOrgId).HasColumnName("created_by_org_id");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");

        entity.HasIndex(x => new { x.HolidayDate, x.HolidayName, x.CreatedByOrgId })
              .IsUnique()
              .HasDatabaseName("ux_system_holiday_unique");

        entity.HasOne(x => x.CreatedByOrg)
              .WithMany()
              .HasForeignKey(x => x.CreatedByOrgId)
              .OnDelete(DeleteBehavior.Restrict);
    });

    // 🔹 HolidayGroup
    modelBuilder.Entity<HolidayGroup>(entity =>
    {
        entity.ToTable("holiday_group");

        entity.HasKey(x => x.Id).HasName("pk_holiday_group");

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
        entity.Property(x => x.GroupName).HasColumnName("group_name").HasMaxLength(200);
        entity.Property(x => x.Year).HasColumnName("year");
        entity.Property(x => x.Description).HasColumnName("description");
        entity.Property(x => x.IsActive).HasColumnName("is_active");
        entity.Property(x => x.IsDefault).HasColumnName("is_default");
        entity.Property(x => x.CreatedAt).HasColumnName("created_at");

        entity.HasOne(x => x.Organization)
              .WithMany()
              .HasForeignKey(x => x.OrganizationId)
              .OnDelete(DeleteBehavior.Cascade);
    });

    // 🔹 HolidayGroupMapping
    modelBuilder.Entity<HolidayGroupMapping>(entity =>
    {
        entity.ToTable("holiday_group_mapping");

        entity.HasKey(x => x.Id).HasName("pk_holiday_group_mapping");

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.HolidayGroupId).HasColumnName("holiday_group_id");
        entity.Property(x => x.SystemHolidayId).HasColumnName("system_holiday_id");
        entity.Property(x => x.IsOptional).HasColumnName("is_optional");

        entity.HasIndex(x => new { x.HolidayGroupId, x.SystemHolidayId })
              .IsUnique()
              .HasDatabaseName("ux_hgm_group_holiday");

        entity.HasOne(x => x.HolidayGroup)
              .WithMany(x => x.GroupHolidays)
              .HasForeignKey(x => x.HolidayGroupId);

        entity.HasOne(x => x.SystemHoliday)
              .WithMany(x => x.HolidayMappings)
              .HasForeignKey(x => x.SystemHolidayId);
    });

    // 🔹 HolidayGroupEligibility
    modelBuilder.Entity<HolidayGroupEligibility>(entity =>
    {
        entity.ToTable("holiday_group_eligibility");

        entity.HasKey(x => x.Id).HasName("pk_holiday_group_eligibility");

        entity.Property(x => x.Id).HasColumnName("id");
        entity.Property(x => x.HolidayGroupId).HasColumnName("holiday_group_id");
        entity.Property(x => x.OfficeLocationId).HasColumnName("office_location_id");
        entity.Property(x => x.DepartmentId).HasColumnName("department_id");

        entity.HasOne(x => x.HolidayGroup)
              .WithMany(x => x.EligibilityRules)
              .HasForeignKey(x => x.HolidayGroupId);

        entity.HasOne(x => x.OfficeLocation)
              .WithMany()
              .HasForeignKey(x => x.OfficeLocationId)
              .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Department)
              .WithMany()
              .HasForeignKey(x => x.DepartmentId)
              .OnDelete(DeleteBehavior.Restrict);
    });


    modelBuilder.Entity<EmployeeBankAccount>(entity =>
        {
            entity.ToTable("employee_bank_account");

            entity.HasKey(x => x.BankAccountId).HasName("pk_employee_bank_account");

            entity.Property(x => x.BankAccountId).HasColumnName("bank_account_id");
            entity.Property(x => x.EmployeeId).HasColumnName("employee_id");
            entity.Property(x => x.OrganizationId).HasColumnName("organization_id");
            
            entity.Property(x => x.AccountHolderName).HasColumnName("account_holder_name").IsRequired().HasMaxLength(150);
            entity.Property(x => x.AccountNumber).HasColumnName("account_number").IsRequired().HasMaxLength(50);
            entity.Property(x => x.IFSCCode).HasColumnName("ifsc_code").IsRequired().HasMaxLength(20);
            entity.Property(x => x.BankName).HasColumnName("bank_name").IsRequired().HasMaxLength(150);
            entity.Property(x => x.BranchName).HasColumnName("branch_name").HasMaxLength(150);

            entity.Property(x => x.IsPrimary).HasColumnName("is_primary").HasDefaultValue(true);
            entity.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.OrganizationId).HasDatabaseName("ix_employee_bank_account_organization_id");
            entity.HasIndex(x => x.EmployeeId).HasDatabaseName("ix_employee_bank_account_employee_id");

            entity.HasOne(x => x.Employee)
                .WithMany() // Agar Employee model me List<EmployeeBankAccount> hai toh yaha paas kar sakte hain, e.g. .WithMany(x => x.BankAccounts)
                .HasForeignKey(x => x.EmployeeId)
                .HasConstraintName("fk_employee_bank_account_employee")
                .OnDelete(DeleteBehavior.Restrict);
        });



       modelBuilder.Entity<SalaryStructure>(entity =>
        {
            entity.ToTable("salary_structure");

            entity.HasKey(x => x.SalaryStructureId)
                .HasName("pk_salary_structure");

            entity.Property(x => x.SalaryStructureId)
                .HasColumnName("salary_structure_id");

            entity.Property(x => x.OrganizationId)
                .HasColumnName("organization_id");

            entity.Property(x => x.StructureName)
                .HasColumnName("structure_name")
                .IsRequired()
                .HasMaxLength(150);

            
            entity.Property(x => x.PayType)
                .HasColumnName("pay_type")
                .HasMaxLength(50)
                .HasDefaultValue("Monthly");

            entity.Property(x => x.IsDefault)
                .HasColumnName("is_default")
                .HasDefaultValue(false);

            entity.Property(x => x.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.OrganizationId)
                .HasDatabaseName("ix_salary_structure_organization_id");

            // ✅ RELATION
            entity.HasMany(x => x.Components)
                .WithOne(x => x.SalaryStructure)
                .HasForeignKey(x => x.SalaryStructureId)
                .HasConstraintName("fk_salary_component_salary_structure")
                .OnDelete(DeleteBehavior.Cascade);
        });



        modelBuilder.Entity<SalaryComponent>(entity =>
            {
                entity.ToTable("salary_component");

                entity.HasKey(x => x.SalaryComponentId)
                    .HasName("pk_salary_component");

                entity.Property(x => x.SalaryComponentId)
                    .HasColumnName("salary_component_id");

                entity.Property(x => x.SalaryStructureId)
                    .HasColumnName("salary_structure_id");

                // ✅ NEW (VERY IMPORTANT)
                entity.Property(x => x.ComponentMasterId)
                    .HasColumnName("component_master_id");

                entity.Property(x => x.ComponentName)
                    .HasColumnName("component_name")
                    .IsRequired()
                    .HasMaxLength(150);

                    entity.Property(x => x.OrganizationId)
           .HasColumnName("organization_id");

       entity.HasIndex(x => x.OrganizationId)
         .HasDatabaseName("ix_salary_component_organization_id");

                entity.Property(x => x.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(18, 2);

                entity.Property(x => x.IsEarning)
                    .HasColumnName("is_earning");

                entity.HasIndex(x => x.SalaryStructureId)
                    .HasDatabaseName("ix_salary_component_salary_structure_id");

                // ✅ RELATION → SalaryStructure
                entity.HasOne(x => x.SalaryStructure)
                    .WithMany(x => x.Components)
                    .HasForeignKey(x => x.SalaryStructureId)
                    .HasConstraintName("fk_salary_component_salary_structure")
                    .OnDelete(DeleteBehavior.Cascade);

                // ✅ RELATION → SalaryComponentMaster
                entity.HasOne(x => x.SalaryComponentMaster)
                    .WithMany()
                    .HasForeignKey(x => x.ComponentMasterId)
                    .HasConstraintName("fk_salary_component_master")
                    .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne<Organization>() 
                    .WithMany()
                    .HasForeignKey(x => x.OrganizationId)
                    .HasConstraintName("fk_salary_component_organization")
                    .OnDelete(DeleteBehavior.Restrict);
            });



                modelBuilder.Entity<SalaryComponentMaster>(entity =>
            {
                entity.ToTable("salary_component_master");

                entity.HasKey(x => x.Id)
                    .HasName("pk_salary_component_master");

                entity.Property(x => x.Id)
                    .HasColumnName("id");

                

                entity.Property(x => x.ComponentName)
                    .HasColumnName("component_name")
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(x => x.IsEarning)
                    .HasColumnName("is_earning");

                entity.Property(x => x.IsStatic)
                    .HasColumnName("is_static");

                entity.Property(x => x.IsActive)
                    .HasColumnName("is_active")
                    .HasDefaultValue(true);

                
            });


            modelBuilder.Entity<EmployeeSalaryMapping>(entity =>
            {
                entity.ToTable("employee_salary_mapping");

                entity.HasKey(x => x.Id).HasName("pk_employee_salary_mapping");

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.EmployeeId).HasColumnName("employee_id");
                entity.Property(x => x.SalaryStructureId).HasColumnName("salary_structure_id");
                entity.Property(x => x.OrganizationId).HasColumnName("organization_id");

                entity.Property(x => x.AssignedAt)
                    .HasColumnName("assigned_at")
                    .HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(x => x.OrganizationId).HasDatabaseName("ix_employee_salary_mapping_organization_id");
                entity.HasIndex(x => x.EmployeeId).HasDatabaseName("ix_employee_salary_mapping_employee_id");
                entity.HasIndex(x => x.SalaryStructureId).HasDatabaseName("ix_employee_salary_mapping_salary_structure_id");

               
                entity.HasOne(x => x.Employee)
                    .WithMany() 
                    .HasForeignKey(x => x.EmployeeId)
                    .HasConstraintName("fk_employee_salary_mapping_employee")
                    .OnDelete(DeleteBehavior.Restrict); 

             
                entity.HasOne(x => x.SalaryStructure)
                    .WithMany() 
                    .HasForeignKey(x => x.SalaryStructureId)
                    .HasConstraintName("fk_employee_salary_mapping_salary_structure")
                    .OnDelete(DeleteBehavior.Restrict);
            });



            modelBuilder.Entity<DesignationSalaryMapping>(entity =>
            {
                entity.ToTable("designation_salary_mapping");

                entity.HasKey(x => x.Id).HasName("pk_designation_salary_mapping");

                entity.Property(x => x.Id).HasColumnName("id");
                entity.Property(x => x.DesignationId).HasColumnName("designation_id");
                entity.Property(x => x.SalaryStructureId).HasColumnName("salary_structure_id");
                entity.Property(x => x.OrganizationId).HasColumnName("organization_id");

                // Indexes
                entity.HasIndex(x => x.OrganizationId).HasDatabaseName("ix_designation_salary_mapping_organization_id");
                entity.HasIndex(x => x.DesignationId).HasDatabaseName("ix_designation_salary_mapping_designation_id");
                entity.HasIndex(x => x.SalaryStructureId).HasDatabaseName("ix_designation_salary_mapping_salary_structure_id");

                // Relationship mapping for Designation
                entity.HasOne(x => x.Designation)
                    .WithMany() 
                    .HasForeignKey(x => x.DesignationId)
                    .HasConstraintName("fk_designation_salary_mapping_designation")
                    .OnDelete(DeleteBehavior.Restrict); 

            
                entity.HasOne(x => x.SalaryStructure)
                    .WithMany() 
                    .HasForeignKey(x => x.SalaryStructureId)
                    .HasConstraintName("fk_designation_salary_mapping_salary_structure")
                    .OnDelete(DeleteBehavior.Restrict);
            });




                    
                

    OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
