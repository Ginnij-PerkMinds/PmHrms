using PmHrmsAPI.PmHrmsDAL.Models;
using PmHrmsAPI.PmHrmsDAL.DbEntities;
using static PmHrmsAPI.PmHrmsDAL.Utility.PmHrmsConstants;

namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
public class WorkPolicyResult
{
    public WorkPolicy Policy { get; set; }
    public WorkPolicySource Source { get; set; }
}
}