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
