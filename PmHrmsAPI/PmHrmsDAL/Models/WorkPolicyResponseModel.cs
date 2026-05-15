namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public class WorkPolicyResponseModel
    {
        public int PolicyId { get; set; }

        public string PolicyName { get; set; } = string.Empty;

        public int RequiredWorkingMinutes { get; set; }

        public int LateAfterMinutes { get; set; }

        public int HalfDayThresholdMinutes { get; set; }

        public bool IsWfhAllowed { get; set; }

        public bool IsWfoRequired { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

       
        public string RequiredWorkingHoursFormatted =>
            $"{RequiredWorkingMinutes / 60}h {RequiredWorkingMinutes % 60}m";
        
        public TimeOnly? ShiftStartTime { get; set; }            
        public TimeOnly? ShiftEndTime { get; set; }
        public TimeOnly? BreakStartTime { get; set; }
        public TimeOnly? BreakEndTime { get; set; }
        public bool IsFlexibleShift { get; set; }
        public int AdditionalBreakMinutes { get; set; }
        public int MaxBreakMinutes { get; set; }
        public int MaxBreakCount { get; set; }
        public bool IsBreakPaid { get; set; }
        public bool IsDefault { get; set; }
        public List<WorkPolicyWeekOffResponseModel> WeekOffs { get; set; } = new();
    }
}
