using DEKODE.OptiFlow.Domain.Entities.Enums;

namespace DEKODE.OptiFlow.Domain.Entities;

public class BrandCategory : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class Brand : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid BrandCategoryId { get; set; }
    public BrandCategory? BrandCategory { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Supplier : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Facility : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int FacilityTypeId { get; set; }
    public string? Tier { get; set; }
    public string? Region { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Product : EntityBase
{
    public string ProductCode { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public Brand? Brand { get; set; }
    public CommodityType CommodityType { get; set; } = CommodityType.Frame;
    public string? Gender { get; set; }
    public string? Shape { get; set; }
    public string? Material { get; set; }
    public string? HsnCode { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ProductItem> Items { get; set; } = new();
    public List<ProductSupplier> ProductSuppliers { get; set; } = new();
}

public class ProductSupplier : EntityBase
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string RelationshipType { get; set; } = string.Empty;
    public string? SupplierProductCode { get; set; }
    public decimal StandardCost { get; set; }
    public int LeadTimeDays { get; set; } = 7;
    public bool IsPrimary { get; set; }
}

public class ProductItem : EntityBase
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string? SkuCode { get; set; }
    public string? Description { get; set; }
    public decimal BaseMrp { get; set; }
    public decimal DefaultPurchaseCost { get; set; }
    public decimal GstRate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class InventoryStock : EntityBase
{
    public Guid FacilityId { get; set; }
    public Facility? Facility { get; set; }
    public Guid ProductItemId { get; set; }
    public ProductItem? ProductItem { get; set; }
    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int QuantityOnHand { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public DateTime? GrnDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public int StockAgeDays { get; set; }
}

