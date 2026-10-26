using DEKODE.OptiFlow.Domain.Entities.Enums;

namespace DEKODE.OptiFlow.Domain.Entities;

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
