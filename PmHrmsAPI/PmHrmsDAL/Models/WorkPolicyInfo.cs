using System;
using System.Collections.Generic;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;


namespace PmHrmsAPI.PmHrmsDAL.Models
{
    

public class WorkPolicyInfo
{
    public int PolicyId { get; set; }
    public string PolicyName { get; set; }
    public WorkPolicySource Source { get; set; } 
}
}