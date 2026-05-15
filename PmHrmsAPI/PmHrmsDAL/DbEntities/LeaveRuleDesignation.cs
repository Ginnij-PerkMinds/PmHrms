using System;
using System.Collections.Generic;

namespace PmHrmsAPI.PmHrmsDAL.DbEntities;

public partial class LeaveRuleDesignation
{
    public int Id { get; set; }
     
    public int? RuleId { get; set; }
     public string RuleName       { get; set; } = null!;  
    
    public int DesignationId { get; set; }

    public int OrganizationId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual LeaveAllocationRule Rule { get; set; } = null!;
}
