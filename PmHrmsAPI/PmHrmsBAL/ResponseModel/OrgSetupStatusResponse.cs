using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.ResponseModel

{
    public class OrgSetupStatusResponse
    {
        public bool IsSetupCompleted { get; set; }
        public SetupStep NextStep { get; set; }
        public int ProgressPercentage { get; set; } 
    }
}