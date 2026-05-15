using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

public class OfficeLocationResult
{
    public OfficeLocation Location { get; set; }
    public OfficeLocationSource Source { get; set; } 
}