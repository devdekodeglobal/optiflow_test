namespace DEKODE.OptiFlow.DTO.Planogram;

public class PlanogramImportResponseDto
{
    public Guid PlanogramId { get; set; }
    public string PlanogramName { get; set; } = string.Empty;
    public string SeasonOrMonth { get; set; } = string.Empty;
    public int ImportedLinesCount { get; set; }
    public int DistinctStoresCount { get; set; }
    public int DistinctBrandsCount { get; set; }
    public double TotalTargetCapacity { get; set; }
    public DateTime ImportedAtUtc { get; set; }
}
