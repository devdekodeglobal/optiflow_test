namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class ProductSupplierDbModel : DbModelBase
{
    public Guid ProductId { get; set; }
    public Guid SupplierId { get; set; }
    public string RelationshipType { get; set; } = string.Empty; // Manufacturer, MarketedBy, AuthorizedDistributor, DirectVendor
    public string? SupplierProductCode { get; set; }
    public decimal StandardCost { get; set; }
    public int LeadTimeDays { get; set; } = 7;
    public bool IsPrimary { get; set; }

    public virtual ProductDbModel Product { get; set; } = null!;
    public virtual SupplierDbModel Supplier { get; set; } = null!;
}

