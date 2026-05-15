namespace PmHrmsAPI.PmHrmsBAL.ResponseModel
{
    public sealed class ComponentResult
    {
        public string Name { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string Type { get; init; } = string.Empty;
    }
}
