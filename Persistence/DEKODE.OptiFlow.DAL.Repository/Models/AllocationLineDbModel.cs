namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class AllocationLineDbModel : DbModelBase
{
    public Guid AllocationId { get; set; }
    public Guid? DestinationFacilityId { get; set; } // Retail Store
    public Guid? ProductItemId { get; set; } // SKU / Barcode item
    public int AllocatedQuantity { get; set; }
    public int TargetCapacity { get; set; }
    public int CurrentSohUnits { get; set; }
    public decimal SalesVelocity { get; set; }
    public string ReasonCode { get; set; } = string.Empty;

    // Legacy/denormalized compatibility fields for allocation engine compatibility
    public int GapId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreCategory { get; set; } = string.Empty;
    public string StoreType { get; set; } = string.Empty;
    public string? Region { get; set; }
    public string? Zone { get; set; }
    public string BrandCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Commodity { get; set; } = string.Empty;
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
    public string MatchType { get; set; } = string.Empty;
    public decimal Mrp { get; set; }
    public string MatchReason { get; set; } = string.Empty;
    public bool ColorLimitWarning { get; set; }
    public double? SimilarityScore { get; set; }
    public string? RequestedAttributesJson { get; set; }
    public string? AllocatedAttributesJson { get; set; }

    public virtual AllocationDbModel? Allocation { get; set; }
    public virtual FacilityDbModel? DestinationFacility { get; set; }
    public virtual ProductItemDbModel? ProductItem { get; set; }
}
