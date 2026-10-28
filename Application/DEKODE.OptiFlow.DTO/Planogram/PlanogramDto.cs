namespace DEKODE.OptiFlow.DTO.Planogram;

public class PlanogramDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SeasonOrMonth { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public bool IsActive { get; set; }
    public int TotalLines { get; set; }
    public List<PlanogramLineDto> Lines { get; set; } = new();
}
