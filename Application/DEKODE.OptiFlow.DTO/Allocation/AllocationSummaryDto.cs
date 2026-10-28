namespace DEKODE.OptiFlow.DTO.Allocation;

public class AllocationSummaryDto
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
    public Dictionary<string, TierFulfillmentDto> TierFulfillment { get; set; } = new();
    public List<BrandMetricsDto> BrandStats { get; set; } = new();
}
