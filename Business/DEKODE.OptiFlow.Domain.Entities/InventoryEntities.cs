using DEKODE.OptiFlow.Domain.Entities.Enums;

namespace DEKODE.OptiFlow.Domain.Entities;

public class PlanogramLine : EntityBase
{
    public string StoreName { get; set; } = string.Empty;
    public string StoreCategory { get; set; } = string.Empty; // e.g. "A++", "A+", "A", "B+", "B", "C"
    public string StoreType { get; set; } = string.Empty;
    public string BrandCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public CommodityType Commodity { get; set; } = CommodityType.Frame;
    public double Facing { get; set; }
    public double BackStock { get; set; }

    public double TargetCapacity => Facing + BackStock;
}

public class WarehouseStockBatch : EntityBase
{
    public string Barcode { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemCategory { get; set; } = string.Empty;
    public string Facility { get; set; } = "Corporate Office";
    public int BatchStock { get; set; }
    public decimal Mrp { get; set; }
    public int StockAgeInDays { get; set; }
}

public class StoreStock : EntityBase
{
    public string Facility { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int BatchStock { get; set; }
}

public class SalesRecord : EntityBase
{
    public string FacilityName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime? OrderDate { get; set; }
}
