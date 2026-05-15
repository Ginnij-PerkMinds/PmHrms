namespace PmHrmsAPI.PmHrmsDAL.Utility
{
    public static class PermissionKeys
    {

       
           // Organization
            public const string ORG_VIEW = "ORG_VIEW";
            public const string ORG_EDIT = "ORG_EDIT";
            public const string ORG_DELETE = "ORG_DELETE";
            public const string FILE_EXPORT = "FILE_EXPORT";
            public const string ORG_STATS_VIEW = "ORG_STATS_VIEW";

            // Employee
            public const string EMP_VIEW = "EMP_VIEW";
            public const string EMP_CREATE = "EMP_CREATE";
            public const string EMP_EDIT_PERSONAL = "EMP_EDIT_PERSONAL";
            public const string EMP_EDIT_OFFICIAL = "EMP_EDIT_OFFICIAL";
            public const string EMP_DELETE = "EMP_DELETE";
            public const string EMP_PROFILE_VIEW = "EMP_PROFILE_VIEW";
            public const string EMP_DOC_UPLOAD = "EMP_DOC_UPLOAD";
            public const string EMP_TIMELINE_VIEW = "EMP_TIMELINE_VIEW";
            public const string EMP_DOC_VERIFY = "EMP_DOC_VERIFY";

            // Department 
            public const string DEPT_VIEW = "DEPT_VIEW";
            public const string DEPT_CREATE = "DEPT_CREATE";
            public const string DEPT_EDIT = "DEPT_EDIT";
            public const string DEPT_DELETE = "DEPT_DELETE";

            // Designation
            public const string DESIG_VIEW = "DESIG_VIEW";
            public const string DESIG_CREATE = "DESIG_CREATE";
            public const string DESIG_EDIT = "DESIG_EDIT";
            public const string DESIG_DELETE = "DESIG_DELETE";

            // Documents
            public const string DOC_CONFIG_MANAGE = "DOC_CONFIG_MANAGE";

            // Attendance
            public const string ATT_VIEW_SELF = "ATT_VIEW_SELF";
            public const string ATT_VIEW_ALL = "ATT_VIEW_ALL";
            public const string ATT_EDIT_LOGS = "ATT_EDIT_LOGS";
            public const string ATT_REPORT_GEN = "ATT_REPORT_GEN";
            public const string ATT_STATS_VIEW = "ATT_STATS_VIEW";

            // Leave
            public const string LV_APPLY = "LV_APPLY";
            public const string LV_VIEW_ALL = "LV_VIEW_ALL";
            public const string LV_APPROVE_DENY = "LV_APPROVE_DENY";
            public const string LV_BALANCE_EDIT = "LV_BALANCE_EDIT";

            // Settings
            public const string ROLE_MANAGE = "ROLE_MANAGE";
            public const string PERM_ASSIGN = "PERM_ASSIGN";

            // Dashboard
            public const string DASHBOARD_ADMIN_VIEW = "DASHBOARD_ADMIN_VIEW";
            public const string DASHBOARD_QUICK_ACTIONS = "DASHBOARD_QUICK_ACTIONS";

            // Setup
            public const string ORG_SETUP_WIZARD = "ORG_SETUP_WIZARD";

            // Migration Permissions (Added to sync with DB)
            public const string MIGRATION_VIEW = "MIGRATION_VIEW";
            public const string DATA_MIGRATE = "DATA_MIGRATE";
            public const string MIGRATION_TEMPLATE_DOWNLOAD = "MIGRATION_TEMPLATE_DOWNLOAD";
            public const string MIGRATION_TEMPLATE_UPLOAD = "MIGRATION_TEMPLATE_UPLOAD";
            public const string MIGRATION_LOG_VIEW = "MIGRATION_LOG_VIEW";
            public const string MIGRATION_HISTORY_DELETE = "MIGRATION_HISTORY_DELETE";

            //EXTRA Permission (dlt after fixes)
            public const string DESIG_MANAGE = "DESIG_MANAGE";
            public const string DEPT_MANAGE = "DEPT_MANAGE";

       

                // New Work Policy Permissions
                public const string WORK_POLICY_VIEW = "WORK_POLICY_VIEW";
                public const string WORK_POLICY_CREATE = "WORK_POLICY_CREATE";
                public const string WORK_POLICY_EDIT = "WORK_POLICY_EDIT";
                public const string WORK_POLICY_DELETE = "WORK_POLICY_DELETE";



                //Task and post 
                public const string POST_VIEW   = "POST_VIEW";
                public const string POST_CREATE = "POST_CREATE";
                public const string POST_EDIT   = "POST_EDIT";
                public const string POST_DELETE = "POST_DELETE";
                public const string TASK_VIEW   = "TASK_VIEW";
                public const string TASK_CREATE = "TASK_CREATE";
                public const string TASK_EDIT   = "TASK_EDIT";
                public const string TASK_DELETE = "TASK_DELETE";

        //Expense
        public const string EXP_APPLY = "EXP_APPLY";
        public const string EXP_APPROVE_DENY = "EXP_APPROVE_DENY";
        public const string EXP_CONFIG_MANAGE = "EXP_CONFIG_MANAGE";

        //Holiday
        public const string HOLIDAY_VIEW = "HOLIDAY_VIEW";
        public const string HOLIDAY_CREATE = "HOLIDAY_CREATE";
        public const string HOLIDAY_EDIT = "HOLIDAY_EDIT";
        public const string HOLIDAY_DELETE = "HOLIDAY_DELETE";
    }

}