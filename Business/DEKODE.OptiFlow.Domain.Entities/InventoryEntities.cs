using DEKODE.OptiFlow.Domain.Entities.Enums;
using DEKODE.OptiFlow.Domain.Entities.Exceptions;

namespace DEKODE.OptiFlow.Domain.Entities;

public class Planogram : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string SeasonOrMonth { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public List<PlanogramLine> Lines { get; set; } = new();

    public void AddLine(PlanogramLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        line.Validate();
        Lines.Add(line);
    }
}

public class PlanogramLine : EntityBase
{
    public int PlanogramId { get; set; }
    public Planogram? Planogram { get; set; }

    public string StoreBrandProductTypeKey { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string StoreCategory { get; set; } = "B";
    public StoreCategoryTier CategoryTier { get; set; } = StoreCategoryTier.B;
    public StoreType StoreType { get; set; } = StoreType.SIS;
    public DispatchPlan DispatchPlan { get; set; } = DispatchPlan.Weekly;

    public string BrandCode { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string BrandType { get; set; } = string.Empty;
    public string BrandCategory { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public CommodityType Commodity { get; set; } = CommodityType.Frame;

    public double Facing { get; set; }
    public double BackStock { get; set; }
    public double StoreDisplay { get; set; }
    public double Soh { get; set; }
    public double TargetCapacity => Facing + BackStock;

    public int? SkuCount { get; set; }
    public double? SaleQty6M { get; set; }
    public double? LastMonthSaleQty { get; set; }
    public double? UniquenessRatio { get; set; }
    public double? AvgMonthlySale { get; set; }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(StoreName))
            throw new PlanogramValidationException("StoreName cannot be empty.", StoreName, BrandName);

        if (string.IsNullOrWhiteSpace(BrandName))
            throw new PlanogramValidationException("BrandName cannot be empty.", StoreName, BrandName);

        if (Facing < 0)
            throw new PlanogramValidationException($"Facing cannot be negative ({Facing}).", StoreName, BrandName);

        if (BackStock < 0)
            throw new PlanogramValidationException($"BackStock cannot be negative ({BackStock}).", StoreName, BrandName);
    }
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
