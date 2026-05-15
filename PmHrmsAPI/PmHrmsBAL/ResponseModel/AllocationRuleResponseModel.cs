namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
public class AllocationRuleResponseModel
    {
       public string RuleName              { get; set; } = null!;
        public bool IsDefault               { get; set; }
        public DateOnly EffectiveFrom       { get; set; }
        public DateOnly? EffectiveTo        { get; set; }
        public List<RuleItemResponseModel> Items             { get; set; } = new();
        public List<int> AssignedDesignationIds              { get; set; } = new();
    }
}