namespace DEKODE.OptiFlow.DTO.Allocation;

public class StoreStockInputDto
{
    public string Facility { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public int BatchStock { get; set; }
}
