using DEKODE.OptiFlow.Domain.Entities.Enums;

namespace DEKODE.OptiFlow.Domain.Entities;

public class Allocation : EntityBase
{
    public DateTime RunDate { get; set; } = DateTime.UtcNow;
    public string RunBy { get; set; } = "system";
    public List<AllocationLine> Lines { get; set; } = new();
    public AllocationSummary? Summary { get; set; }
}

public class AllocationLine : EntityBase
{
    public int GapId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreCategory { get; set; } = string.Empty;
    public string StoreType { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? Zone { get; set; }

    public string BrandCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public CommodityType Commodity { get; set; } = CommodityType.Frame;

    public double Facing { get; set; }
    public double BackStock { get; set; }
    public double CurrentSoh { get; set; }
    public double Deficit { get; set; }

    public double InitialGap { get; set; }
    public double RemainingGap { get; set; }

    public string? RequestedItemCode { get; set; }
    public string? AllocatedItemCode { get; set; }
    public string? AllocatedItemName { get; set; }
    public string? AllocatedBarcode { get; set; }
    public int AllocatedQty { get; set; }

    public DEKODE.OptiFlow.Domain.Entities.Enums.MatchType MatchType { get; set; } = DEKODE.OptiFlow.Domain.Entities.Enums.MatchType.Unresolved;
    public decimal Mrp { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public bool ColorLimitWarning { get; set; }
    public double? SimilarityScore { get; set; }

    public ItemAttributes? RequestedAttributes { get; set; }
    public ItemAttributes? AllocatedAttributes { get; set; }

    public int InitialWhStock { get; set; }
    public int RemainingWhStock { get; set; }
    public int StoreSkuSohAfter { get; set; }
}

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

public class TierFulfillmentMetrics
{
    public int Target { get; set; }
    public int Filled { get; set; }
    public double Pct => Target > 0 ? Math.Round((double)Filled / Target * 100, 2) : 0.0;
}

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
