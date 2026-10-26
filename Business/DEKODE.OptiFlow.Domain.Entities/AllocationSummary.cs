namespace DEKODE.OptiFlow.Domain.Entities;

public class AllocationSummary
{
    public int TotalStores { get; set; }
    public int TotalDeficitLines { get; set; }
    public int ExactMatches { get; set; }
    public int SimilarMatches { get; set; }
    public int BrandFallbacks { get; set; }
    public int Unresolved { get; set; }
    public int TotalItemsAllocated { get; set; }
    public int StoresWithDeficits { get; set; }
    public int BrandsProcessed { get; set; }
    public int TotalTargetDeficit { get; set; }
    public int TotalSoh { get; set; }
    public decimal TotalRetailValue { get; set; }
    public int UniquenessBeforePct { get; set; }
    public int UniquenessAfterPct { get; set; }
    public Dictionary<string, TierFulfillmentMetrics> TierFulfillment { get; set; } = new();
    public List<BrandAllocationMetrics> BrandStats { get; set; } = new();
    public DateTime LastRunAt { get; set; } = DateTime.UtcNow;
}
