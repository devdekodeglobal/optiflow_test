namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class PlanogramLineDbModel : DbModelBase
{
    public Guid PlanogramId { get; set; }
    public Guid BrandId { get; set; }
    public string CommodityType { get; set; } = string.Empty; // Frame, Sunglass
    public int FacingCapacity { get; set; }
    public int BackStockCapacity { get; set; }
    public int MinCapacity { get; set; }

    // Legacy/denormalized compatibility fields for existing domain services
    public string StoreBrandProductTypeKey { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string BrandCode { get; set; } = string.Empty;
    public string Commodity { get; set; } = string.Empty;
    public string StoreCategory { get; set; } = "B";
    public string CategoryTier { get; set; } = "B";
    public string StoreType { get; set; } = "SIS";
    public string DispatchPlan { get; set; } = "Weekly";
    public double Facing { get; set; }
    public double BackStock { get; set; }
    public double TargetCapacity { get; set; }
    public double Soh { get; set; }
    public int? SkuCount { get; set; }
    public double? AvgMonthlySale { get; set; }

    public virtual PlanogramDbModel? Planogram { get; set; }
    public virtual BrandDbModel? Brand { get; set; }
}
