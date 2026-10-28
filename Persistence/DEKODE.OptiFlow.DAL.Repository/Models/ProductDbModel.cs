namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class ProductDbModel : DbModelBase
{
    public string ProductCode { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public string CommodityType { get; set; } = string.Empty;
    public string? Gender { get; set; }
    public string? Shape { get; set; }
    public string? Material { get; set; }
    public string? HsnCode { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual BrandDbModel Brand { get; set; } = null!;
    public virtual ICollection<ProductItemDbModel> Items { get; set; } = new List<ProductItemDbModel>();
    public virtual ICollection<ProductSupplierDbModel> ProductSuppliers { get; set; } = new List<ProductSupplierDbModel>();
}

