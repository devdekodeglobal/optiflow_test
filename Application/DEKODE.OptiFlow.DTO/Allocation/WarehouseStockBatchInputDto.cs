namespace DEKODE.OptiFlow.DTO.Allocation;

public class WarehouseStockBatchInputDto
{
    public string Barcode { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemCategory { get; set; } = "Frame";
    public string Facility { get; set; } = "Corporate Office";
    public int BatchStock { get; set; }
    public decimal Mrp { get; set; }
    public int StockAgeInDays { get; set; }
}
