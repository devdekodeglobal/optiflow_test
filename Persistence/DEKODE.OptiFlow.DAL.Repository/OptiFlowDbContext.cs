using DEKODE.OptiFlow.DAL.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace DEKODE.OptiFlow.DAL.Repository;

public class OptiFlowDbContext : DbContext
{
    public OptiFlowDbContext(DbContextOptions<OptiFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<FacilityTypeDbModel> FacilityTypes => Set<FacilityTypeDbModel>();
    public DbSet<FacilityDbModel> Facilities => Set<FacilityDbModel>();
    public DbSet<BrandCategoryDbModel> BrandCategories => Set<BrandCategoryDbModel>();
    public DbSet<BrandDbModel> Brands => Set<BrandDbModel>();
    public DbSet<SupplierDbModel> Suppliers => Set<SupplierDbModel>();
    public DbSet<ProductDbModel> Products => Set<ProductDbModel>();
    public DbSet<ProductSupplierDbModel> ProductSuppliers => Set<ProductSupplierDbModel>();
    public DbSet<ProductItemDbModel> ProductItems => Set<ProductItemDbModel>();
    public DbSet<InventoryStockDbModel> InventoryStocks => Set<InventoryStockDbModel>();
    public DbSet<SalesRecordDbModel> SalesRecords => Set<SalesRecordDbModel>();
    public DbSet<PlanogramDbModel> Planograms => Set<PlanogramDbModel>();
    public DbSet<PlanogramLineDbModel> PlanogramLines => Set<PlanogramLineDbModel>();
    public DbSet<AllocationDbModel> Allocations => Set<AllocationDbModel>();
    public DbSet<AllocationLineDbModel> AllocationLines => Set<AllocationLineDbModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // FacilityType Configuration
        modelBuilder.Entity<FacilityTypeDbModel>(entity =>
        {
            entity.ToTable("facility_types");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasData(
                new FacilityTypeDbModel { Id = 1, Code = "CORPORATE_OFFICE", Name = "Corporate Office / Central Warehouse" },
                new FacilityTypeDbModel { Id = 2, Code = "WAREHOUSE", Name = "Regional Distribution Center" },
                new FacilityTypeDbModel { Id = 3, Code = "RETAIL_STANDALONE", Name = "Retail Standalone Store" },
                new FacilityTypeDbModel { Id = 4, Code = "RETAIL_SIS", Name = "Shop-in-Shop Retail" }
            );
        });

        // Facility Configuration
        modelBuilder.Entity<FacilityDbModel>(entity =>
        {
            entity.ToTable("facilities");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Tier).HasMaxLength(10);
            entity.Property(e => e.Region).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.FacilityType)
                .WithMany(ft => ft.Facilities)
                .HasForeignKey(e => e.FacilityTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BrandCategory Configuration
        modelBuilder.Entity<BrandCategoryDbModel>(entity =>
        {
            entity.ToTable("brand_categories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);

            entity.HasData(
                new BrandCategoryDbModel { Id = 1, Code = "INTL_BRAND", Name = "International Brand" },
                new BrandCategoryDbModel { Id = 2, Code = "PRIVATE_LABEL", Name = "Private Label" },
                new BrandCategoryDbModel { Id = 3, Code = "LUXURY", Name = "Luxury" },
                new BrandCategoryDbModel { Id = 4, Code = "LOCAL_LABEL", Name = "Local Label" }
            );
        });

        // Brand Configuration
        modelBuilder.Entity<BrandDbModel>(entity =>
        {
            entity.ToTable("brands");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.BrandCategory)
                .WithMany(bc => bc.Brands)
                .HasForeignKey(e => e.BrandCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Supplier Configuration
        modelBuilder.Entity<SupplierDbModel>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TaxNumber).HasMaxLength(50);
            entity.Property(e => e.ContactEmail).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Product Configuration
        modelBuilder.Entity<ProductDbModel>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProductCode).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.ProductCode).IsUnique();
            entity.Property(e => e.CommodityType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.Shape).HasMaxLength(50);
            entity.Property(e => e.Material).HasMaxLength(50);
            entity.Property(e => e.HsnCode).HasMaxLength(20);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(e => e.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductSupplier (Many-to-Many junction) Configuration
        modelBuilder.Entity<ProductSupplierDbModel>(entity =>
        {
            entity.ToTable("product_suppliers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RelationshipType).IsRequired().HasMaxLength(30);
            entity.Property(e => e.SupplierProductCode).HasMaxLength(100);
            entity.Property(e => e.StandardCost).HasPrecision(12, 2);

            entity.HasIndex(e => new { e.ProductId, e.SupplierId, e.RelationshipType }).IsUnique();

            entity.HasOne(e => e.Product)
                .WithMany(p => p.ProductSuppliers)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.SuppliedProducts)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ProductItem Configuration
        modelBuilder.Entity<ProductItemDbModel>(entity =>
        {
            entity.ToTable("product_items");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Barcode).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Barcode).IsUnique();
            entity.Property(e => e.SkuCode).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.BaseMrp).HasPrecision(12, 2);
            entity.Property(e => e.DefaultPurchaseCost).HasPrecision(12, 2);
            entity.Property(e => e.GstRate).HasPrecision(5, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Items)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // InventoryStock Configuration
        modelBuilder.Entity<InventoryStockDbModel>(entity =>
        {
            entity.ToTable("inventory_stocks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BatchNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PurchasePrice).HasPrecision(12, 2);
            entity.Property(e => e.SellingPrice).HasPrecision(12, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(12, 2);

            entity.HasIndex(e => new { e.FacilityId, e.ProductItemId, e.BatchNumber, e.SupplierId }).IsUnique();
            entity.HasIndex(e => new { e.FacilityId, e.ProductItemId });

            entity.HasOne(e => e.Facility)
                .WithMany(f => f.InventoryStocks)
                .HasForeignKey(e => e.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProductItem)
                .WithMany(pi => pi.InventoryStocks)
                .HasForeignKey(e => e.ProductItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.InventoryStocks)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SalesRecord Configuration
        modelBuilder.Entity<SalesRecordDbModel>(entity =>
        {
            entity.ToTable("sales_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitRetailPrice).HasPrecision(12, 2);
            entity.Property(e => e.NetAmount).HasPrecision(12, 2);
            entity.Property(e => e.FinancialYear).HasMaxLength(10);
            entity.Property(e => e.PeriodMonth).HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(30).HasDefaultValue("Completed");

            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => new { e.FacilityId, e.ProductItemId });

            entity.HasOne(e => e.Facility)
                .WithMany(f => f.SalesRecords)
                .HasForeignKey(e => e.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProductItem)
                .WithMany(pi => pi.SalesRecords)
                .HasForeignKey(e => e.ProductItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Planogram Configuration
        modelBuilder.Entity<PlanogramDbModel>(entity =>
        {
            entity.ToTable("planograms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SeasonOrMonth).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.EffectiveDate).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ConcurrencyToken).IsRowVersion();

            entity.HasOne(e => e.Facility)
                .WithMany(f => f.Planograms)
                .HasForeignKey(e => e.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Lines)
                .WithOne(l => l.Planogram)
                .HasForeignKey(l => l.PlanogramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlanogramLine Configuration
        modelBuilder.Entity<PlanogramLineDbModel>(entity =>
        {
            entity.ToTable("planogram_lines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CommodityType).IsRequired().HasMaxLength(30);

            entity.HasIndex(e => new { e.PlanogramId, e.BrandId, e.CommodityType }).IsUnique();

            entity.HasOne(e => e.Brand)
                .WithMany(b => b.PlanogramLines)
                .HasForeignKey(e => e.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Allocation Configuration
        modelBuilder.Entity<AllocationDbModel>(entity =>
        {
            entity.ToTable("allocations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Calculated");

            entity.HasOne(e => e.SourceFacility)
                .WithMany(f => f.SourcedAllocations)
                .HasForeignKey(e => e.SourceFacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Lines)
                .WithOne(l => l.Allocation)
                .HasForeignKey(l => l.AllocationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AllocationLine Configuration
        modelBuilder.Entity<AllocationLineDbModel>(entity =>
        {
            entity.ToTable("allocation_lines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SalesVelocity).HasPrecision(8, 2);
            entity.Property(e => e.ReasonCode).HasMaxLength(50);

            entity.HasIndex(e => new { e.AllocationId, e.DestinationFacilityId, e.ProductItemId });

            entity.HasOne(e => e.DestinationFacility)
                .WithMany(f => f.ReceivedAllocationLines)
                .HasForeignKey(e => e.DestinationFacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProductItem)
                .WithMany(pi => pi.AllocationLines)
                .HasForeignKey(e => e.ProductItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
