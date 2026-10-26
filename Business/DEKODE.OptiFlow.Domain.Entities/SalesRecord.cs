namespace DEKODE.OptiFlow.Domain.Entities;

public class SalesRecord : EntityBase
{
    public string FacilityName { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime? OrderDate { get; set; }
}
