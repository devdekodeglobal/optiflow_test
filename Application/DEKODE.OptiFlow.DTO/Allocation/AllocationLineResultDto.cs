namespace DEKODE.OptiFlow.DTO.Allocation;

public class AllocationLineResultDto
{
    public int GapId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreCategory { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Commodity { get; set; } = string.Empty;
    public double Facing { get; set; }
    public double BackStock { get; set; }
    public double CurrentSoh { get; set; }
    public double Deficit { get; set; }

    public string? AllocatedItemCode { get; set; }
    public string? AllocatedItemName { get; set; }
    public string? AllocatedBarcode { get; set; }
    public int AllocatedQty { get; set; }

    public string MatchType { get; set; } = "Unresolved";
    public decimal Mrp { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public bool ColorLimitWarning { get; set; }
    public double? SimilarityScore { get; set; }
    public int StoreSkuSohAfter { get; set; }
}
