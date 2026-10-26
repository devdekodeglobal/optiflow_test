namespace DEKODE.OptiFlow.Domain.Entities;

public class StoreStock : EntityBase
{
    public string Facility { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int BatchStock { get; set; }
}
