using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities
{
    public partial class LeaveMaster
    {
        public int LeaveMasterId { get; set; }
        public string LeaveTypeName { get; set; } = null!;
        public int? MaxDaysPerApplication { get; set; }
        public bool IsBalanceBased { get; set; }
        public bool IsSpecialPolicy { get; set; }
        public bool IsSystemDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}