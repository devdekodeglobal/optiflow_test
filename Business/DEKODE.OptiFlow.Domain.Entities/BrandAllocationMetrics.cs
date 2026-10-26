namespace DEKODE.OptiFlow.Domain.Entities;

public class BrandAllocationMetrics
{
    public string BrandName { get; set; } = string.Empty;
    public int FilledUnits { get; set; }
    public decimal RetailValue { get; set; }
    public int ExactCount { get; set; }
    public int SimilarCount { get; set; }
    public int SubstituteCount { get; set; }
    public int UnresolvedCount { get; set; }
}
