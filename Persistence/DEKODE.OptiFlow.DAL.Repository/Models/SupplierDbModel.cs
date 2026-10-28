namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class SupplierDbModel : DbModelBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<ProductSupplierDbModel> SuppliedProducts { get; set; } = new List<ProductSupplierDbModel>();
    public virtual ICollection<InventoryStockDbModel> InventoryStocks { get; set; } = new List<InventoryStockDbModel>();
}

