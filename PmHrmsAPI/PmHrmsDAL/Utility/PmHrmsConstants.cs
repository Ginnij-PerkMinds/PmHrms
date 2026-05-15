namespace PmHrmsAPI.PmHrmsDAL.Utility
{

    public static class PmHrmsConstants
    {

        public static class FolderNames
        {
            public const string ProfilePics = "ProfilePics";
            public const string Documents = "EmployeeDocs";
            public const string CompanyLogos = "CompanyLogos";
            public const string Expenses = "ExpenseBills";

            public const string PostImages = "PostImagesPath";
        }

        public enum DocumentStatus
        {
            Pending,
            Approved,
            Rejected
        }

        public enum LeaveStatus
        {
            Pending = 1,
            Approved = 2,
            Rejected = 3,
            Cancelled = 4
        }


        public enum AttendanceStatus
        {
            Present = 1,
            Late = 2,
            HalfDay = 3,
            Absent = 4,
            MissedCheckOut = 5,
            Working = 6,
            Holiday = 7,
            WeekOff = 8
        }


        public enum PayrollRunStatus { Pending, Running, Completed, Failed }
        public enum TriggerType { Scheduled, Manual }
        public enum EmployeePayrollStatus { Calculated, Failed }
        public enum PaymentStatus { Pending, Paid, OnHold }





        public enum SetupStep
        {
            EMAIL = 1,
            FIRST_NAME = 2,
            LAST_NAME = 3,
            PASSWORD = 4,
            WORK_POLICY = 5,
            OFFICE_LOCATION = 6,
            COMPLETED = 7
        }


        public enum WorkPolicySource
        {
            EMPLOYEE_LEVEL = 1,
            DESIGNATION_LEVEL = 2,
            DEFAULT = 3
        }

        public enum OfficeLocationSource
        {
            EMPLOYEE_LEVEL = 1,

            DEFAULT = 2
        }

        public enum TargetType { Employee, Department, Designation, All }
        public enum TaskPriority { Low = 1, Medium = 2, High = 3, Critical = 4 }
        public enum TaskStatus
        {
            Pending = 0,
            InProgress = 1,
            Completed = 2,
            ReviewRequested = 3,
            UnderReview = 4
        }

        public enum SalarySource
        {
            EMPLOYEE_LEVEL,
            DESIGNATION_LEVEL,
            DEFAULT
        }

        public enum HolidayResolutionSource
        {
            DirectAssignment,
            EligibilityRule,
            DefaultGroup,
            None
        }


        public static class TaskStatuses
        {
            public const string Pending = nameof(TaskStatus.Pending);
            public const string InProgress = nameof(TaskStatus.InProgress);
            public const string Completed = nameof(TaskStatus.Completed);
            public const string ReviewRequested = nameof(TaskStatus.ReviewRequested);
            public const string UnderReview = nameof(TaskStatus.UnderReview);

            public static readonly string[] EmployeeWorkflow =
            {
                Pending,
                InProgress,
                ReviewRequested,
                Completed
            };

            public static readonly string[] ReviewerWorkflow =
            {
                UnderReview,
                Completed
            };
        }


        public enum PayrollRunType
        {
            Regular,
            Supplementary,
            Revised
        }


        public enum FollowUpTarget { All, Pending, Specific }


        public static class EmailTypes
        {
            public const string Signup = "SIGNUP_OTP";
            public const string Login = "LOGIN_OTP";
            public const string Resend = "RESEND_OTP";
            public const string Onboarding = "ONBOARDING_WELCOME";
        }


        public static class AppSettingsKeys
        {
            public const string AppName = "PmHrms";
            public const string BaseUrl = "https://localhost:7000";
        }

        public static class AttendanceMessages
        {
            // Exceptions
            public const string OnLeaveToday = "You are on leave today";
            public const string AlreadyCheckedInToday = "Already checked in today.";
            public const string EmployeeNotFound = "Employee not found.";
            public const string PolicyNotFound = "Employee or Work Policy not found.";
            public const string AttendanceNotPaused = "Attendance is not paused.";
            public const string NoCheckInFound = "No check-in found.";
            public const string AlreadyCheckedOut = "Already checked out.";
            public const string InvalidLocation = "You are not at an allowed location.";

            // Success / Return messages
            public const string CheckInSuccess = "Check-in successful.";
            public const string CheckInLateSuccess = "Check-in successful (Marked Late).";
            public const string PauseStarted = "Pause started.";
            public const string ResumeWork = "Work resumed.";
            public const string CheckOutSuccess = "Check-out successful.";
            public const string NotCheckedInToday = "Not checked in today.";
            public const string WorkPolicyNotConfigured = "Work policy not configured.";
        }

        public static class DepartmentMessages
        {
            // Exceptions
            public const string DuplicateDepartment = "Department already exists.";

        }

        public static class DesignationMessages
        {
            // Validation
            public const string DepartmentRequired = "Department is required";
            public const string HierarchyLevelInvalid = "Hierarchy level must be greater than 0";
            public const string DepartmentNotBelongOrg = "Department does not belong to this organization";
            public const string DuplicateDesignation = "Designation already exists in this department";
            public const string DuplicateHierarchy = "Hierarchy level already exists in this department";
        }

        public static class EmployeeMessages
        {
            // Validation
            public const string EmployeeCodeRequired = "Employee code is required";
            public const string FirstNameRequired = "First name is required";
            public const string OfficialEmailRequired = "Official email is required";
            public const string OrgIdNotFound = "Organization ID not found in token";
            public const string InvalidPhoneFormat = "Invalid phone number format";

            // Duplicates
            public const string EmployeeCodeExists = "Employee code already exists";
            public const string OfficialEmailExists = "Official email already exists";
            public const string PhoneNumberExists = "Phone number already exists";

            // Role assignment
            public const string InvalidOrgRole = "Selected organization role is invalid for this employee";
            public const string EmployeeLoginNotFound = "Employee login account not found for role assignment";
        }

        public static class EmployeeBankAccountMessages
        {
            //Exceptions
            public const string InvalidUpdate = "EmployeeId and OrganizationId cannot be changed during bank account update.";
        }

        public static class EmployeeDocumentMessages
        {
            //Exceptions
            public const string InvalidStatus = "Invalid status";
            public const string CannotVerifyPending = "Cannot verify as Pending";
        }

        public static class ExpenseMessages
        {
            //Exceptions
            public const string InvalidExpenseType = "Invalid or inactive expense type.";
            public const string CategoryDisabled = "This category is disabled by your organization.";
            public const string ClaimExceedsLimit = "Claim amount exceeds the allowed limit.";

            //success 
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
        }

        public static class GlobalSearchScopes
        {
            //Validation
            public const string EmployeeSingular = "employee";
            public const string EmployeePlural = "employees";
            public const string DepartmentSingular = "department";
            public const string DepartmentPlural = "departments";
            public const string DesignationSingular = "designation";
            public const string DesignationPlural = "designations";
            public const string DocumentSingular = "document";
            public const string DocumentPlural = "documents";
        }

        public static class HolidayMessages
        {
            //Exceptions
            public const string EmployeeNotFound = "Employee not found";
            public const string YearRequired = "Year is required.";
            public const string GroupNameRequired = "Group name is required.";
            public const string SelectHolidayRequired = "Select at least one holiday.";
            public const string InvalidHolidaySelection = "One or more selected holidays are invalid for this organization and year.";
            public const string HolidayGroupNotFound = "Holiday group not found.";
            public const string HolidayNameRequired = "Holiday name is required.";
            public const string HolidayDateRequired = "Holiday date is required.";
            public const string HolidayDateYearMismatch = "Holiday date must fall within the selected year.";
            public const string HolidayAlreadyExists = "A holiday with this name already exists on that date.";
            public const string HolidayNotFound = "Holiday not found.";
            public const string DeleteCustomHoliday = "You can only delete custom holidays created by your organization.";
            public const string SavedSuccessfully = "Saved successfully. Employee assignment queued.";


            //HolidayTypes
            public const string WeekOff = "WEEKOFF";
            public const string Holiday = "HOLIDAY";


            //CountryCodes
            public const string India = "IN";
        }

        public static class LeaveMessages
        {
            //Exceptions
            public const string InvalidLeaveType = "Invalid Leave Type.";
            public const string LeaveTypeNotFound = "Leave type not found.";
            public const string ToDateBeforeFromDate = "To date cannot be before from date.";
            public const string MaxDaysAllowed = "Max {0} days allowed."; // use string.Format
            public const string InsufficientBalance = "Insufficient balance. Available: {0}, Requested: {1}.";
            public const string NotFound = "Not found.";
            public const string AddAtLeastOneLeaveType = "Add at least one leave type.";
        }

        public static class OrganizationMessages
        {
            //Exceptions
            public const string LogoFileRequired = "Logo file is required";


            //OrganizationDefaults
            public const string CustomHolidayCountryCode = "CSTM";
            public const string TempEmailDomain = "@perkminds.temp";
            public const string GuestFirstName = "Guest";
            public const string AdminLastName = "Admin";
            public const string GhostLockedPassword = "GHOST_LOCKED";
        }

        public static class OrgRoleMessages
        {
            //Exceptions
            public const string RoleCreated = "Role created";
            public const string RoleNotFound = "Role not found";
            public const string RoleUpdated = "Role updated";
        }

        public static class PostMessages 
        {
            //Validation
            public const string All = "All";
            public const string AllEmployees = "All Employees";
        }

        public static class RoleLayoutMessages
        {
            //Values
            public const string EmployeeLayoutKey = "EMPLOYEE";
        }

        public static class RolePermissionMessages
        {  
           //Validation
           public const int InvalidId = 0; // Used for ID validation checks
        }

        public static class SalaryStructureMessages
        {
            //Exceptions
            public const string StructureNameRequired = "Structure name is required.";
            public const string ComponentRequired = "At least one component is required.";
        

            //SalaryStructure   
            public const string DefaultPayType = "Monthly";
           
        }

        public static class TaskMessages
        {
            //Validation
            public const string Employee = "Employee";
            public const string DepartmentHead = "DepartmentHead";
            public const string Self = "Self";
        

            //TaskTargetTypes                  
            public const string Department = "Department";
            public const string Designation = "Designation";
            public const string All = "All";
            public const string Direct = "Direct";
        

            //TaskPriorityLabels 
            public const string Low = "Low";
            public const string Medium = "Medium";
            public const string High = "High";
            public const string Critical = "Critical";
            public const string Unknown = "Unknown";
        

            //TaskDefaults       
            public const string SystemAssigned = "System";
        }

        public static class WorkPolicyMessages
        {
            // Exceptions
            public const string InvalidDesignation = "Invalid Designation";
            public const string RequiredMinutes = "Required working minutes must be greater than zero";
            public const string LateMinutesExceed = "Late minutes cannot exceed working minutes";
            public const string HalfDayThresholdExceed = "Half-day threshold cannot exceed required working minutes";
            public const string BreakStartEndRequired = "Both break start and break end time are required.";
            public const string BreakStartBeforeEnd = "Break start time must be before break end time.";
            public const string ShiftTimingsRequired = "Shift timings are required for fixed shifts.";
            public const string ShiftStartBeforeEnd = "Shift start time must be before end time.";
            public const string BreakWithinShift = "Break timings must fall within the shift timings.";
            public const string BreakCountRequired = "Break count must be at least 1 when break time is configured.";
            public const string MaxWeekOffsExceeded = "A work policy can only have up to 7 week offs.";
            public const string InvalidWeekOffs = "One or more selected week offs are invalid.";
            public const string DuplicateWeekOff = "{0} is selected more than once as a week off.";
        }

    }

}