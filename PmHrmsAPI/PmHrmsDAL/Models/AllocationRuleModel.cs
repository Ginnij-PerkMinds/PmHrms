
namespace PmHrmsAPI.PmHrmsDAL.Models
{
    public class AllocationRuleModel
    {
         public string RuleName        { get; set; } = null!;
        public bool IsDefault         { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo  { get; set; }
        public List<RuleItemModel> Items          { get; set; } = new();
        public List<int> DesignationIds           { get; set; } = new();
    }
 }