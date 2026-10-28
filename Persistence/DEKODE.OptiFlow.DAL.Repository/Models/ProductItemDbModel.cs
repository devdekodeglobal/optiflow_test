namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class ProductItemDbModel : DbModelBase
{
    public Guid ProductId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string? SkuCode { get; set; }
    public string? Description { get; set; }
    public decimal BaseMrp { get; set; }
    public decimal DefaultPurchaseCost { get; set; }
    public decimal GstRate { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ProductDbModel Product { get; set; } = null!;
    public virtual ICollection<InventoryStockDbModel> InventoryStocks { get; set; } = new List<InventoryStockDbModel>();
    public virtual ICollection<SalesRecordDbModel> SalesRecords { get; set; } = new List<SalesRecordDbModel>();
    public virtual ICollection<AllocationLineDbModel> AllocationLines { get; set; } = new List<AllocationLineDbModel>();
}

