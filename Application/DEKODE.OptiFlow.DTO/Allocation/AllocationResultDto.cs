namespace DEKODE.OptiFlow.DTO.Allocation;

public class AllocationResultDto
{
    public Guid? Id { get; set; }
    public DateTime RunDate { get; set; }
    public string RunBy { get; set; } = string.Empty;
    public bool IsPersisted { get; set; }
    public AllocationSummaryDto? Summary { get; set; }
    public List<AllocationLineResultDto> Lines { get; set; } = new();
}
