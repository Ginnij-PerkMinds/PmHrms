using System;
using System.Collections.Generic;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;


namespace PmHrmsAPI.PmHrmsDAL.Models
{
    
public class OfficeLocationInfo

{
    public int LocationId { get; set; }
    public string LocationName { get; set; }
    public OfficeLocationSource Source { get; set; } 
}
}