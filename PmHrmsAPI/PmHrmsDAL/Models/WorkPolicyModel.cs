using System.ComponentModel.DataAnnotations;

namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class WorkPolicyModel
    {
        [Required]
        [MaxLength(100)]
        public string PolicyName { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int RequiredWorkingMinutes { get; set; }

        [Range(0, 1440)]
        public int LateAfterMinutes { get; set; }

        [Range(0, 1440)]
        public int HalfDayThresholdMinutes { get; set; }

        public bool IsWfhAllowed { get; set; }
        public bool IsWfoRequired { get; set; }
        public bool IsActive { get; set; } = true;

        public TimeOnly? ShiftStartTime { get; set; }
        public TimeOnly? ShiftEndTime { get; set; }
        public TimeOnly? BreakStartTime { get; set; }
        public TimeOnly? BreakEndTime { get; set; }

        public bool IsFlexibleShift { get; set; }

        [Range(0, 300)]
        public int AdditionalBreakMinutes { get; set; }

        [Range(0, 300)]
        public int MaxBreakMinutes { get; set; }

        [Range(0, 20)]
        public int MaxBreakCount { get; set; }

        public bool IsDefault { get; set; }

        public bool IsBreakPaid { get; set; }

        public List<WorkPolicyWeekOffModel> WeekOffs { get; set; } = new();
    }
}
