namespace DEKODE.OptiFlow.DTO.Allocation;

public class PlanogramLineInputDto
{
    public string StoreName { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string Commodity { get; set; } = "Frame"; // Frame, Sunglass
    public string StoreCategory { get; set; } = "B";  // A++, A+, A, B+, B, C
    public string StoreType { get; set; } = "SIS";
    public string BrandCode { get; set; } = string.Empty;
    public double Facing { get; set; }
    public double BackStock { get; set; }
}
