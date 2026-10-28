namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class InventoryStockDbModel : DbModelBase
{
    public Guid FacilityId { get; set; }
    public Guid ProductItemId { get; set; }
    public Guid SupplierId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public DateTime? GrnDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public int StockAgeDays { get; set; }

    public virtual FacilityDbModel Facility { get; set; } = null!;
    public virtual ProductItemDbModel ProductItem { get; set; } = null!;
    public virtual SupplierDbModel Supplier { get; set; } = null!;
}

