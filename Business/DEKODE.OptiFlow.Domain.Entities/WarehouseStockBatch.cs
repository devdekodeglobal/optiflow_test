namespace DEKODE.OptiFlow.Domain.Entities;

public class WarehouseStockBatch : EntityBase
{
    public string Barcode { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemCategory { get; set; } = string.Empty;
    public string Facility { get; set; } = "Corporate Office";
    public int BatchStock { get; set; }
    public decimal Mrp { get; set; }
    public int StockAgeInDays { get; set; }
}
