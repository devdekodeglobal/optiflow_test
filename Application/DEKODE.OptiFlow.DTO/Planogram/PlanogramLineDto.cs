namespace DEKODE.OptiFlow.DTO.Planogram;

public class PlanogramLineDto
{
    public Guid Id { get; set; }
    public string StoreBrandProductTypeKey { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string BrandCode { get; set; } = string.Empty;
    public string Commodity { get; set; } = "Frame";
    public string StoreCategory { get; set; } = "B";
    public string StoreType { get; set; } = "SIS";
    public string DispatchPlan { get; set; } = "Weekly";
    public double Facing { get; set; }
    public double BackStock { get; set; }
    public double TargetCapacity { get; set; }
    public double Soh { get; set; }
    public int? SkuCount { get; set; }
    public double? AvgMonthlySale { get; set; }
}
