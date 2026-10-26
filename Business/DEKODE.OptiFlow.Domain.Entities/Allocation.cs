namespace DEKODE.OptiFlow.Domain.Entities;

public class Allocation : EntityBase
{
    public DateTime RunDate { get; set; } = DateTime.UtcNow;
    public string RunBy { get; set; } = "system";
    public List<AllocationLine> Lines { get; set; } = new();
    public AllocationSummary? Summary { get; set; }
}
