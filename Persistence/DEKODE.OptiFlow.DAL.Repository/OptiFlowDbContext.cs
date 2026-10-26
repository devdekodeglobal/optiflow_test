using DEKODE.OptiFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DEKODE.OptiFlow.DAL.Repository;

public class OptiFlowDbContext : DbContext
{
    public OptiFlowDbContext(DbContextOptions<OptiFlowDbContext> options)
        : base(options)
    {
    }

    public DbSet<Planogram> Planograms => Set<Planogram>();
    public DbSet<PlanogramLine> PlanogramLines => Set<PlanogramLine>();
    public DbSet<Allocation> Allocations => Set<Allocation>();
    public DbSet<AllocationLine> AllocationLines => Set<AllocationLine>();
    public DbSet<WarehouseStockBatch> WarehouseStockBatches => Set<WarehouseStockBatch>();
    public DbSet<StoreStock> StoreStocks => Set<StoreStock>();
    public DbSet<SalesRecord> SalesRecords => Set<SalesRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Planogram Configuration
        modelBuilder.Entity<Planogram>(entity =>
        {
            entity.ToTable("planograms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SeasonOrMonth).HasMaxLength(50);
            entity.Property(e => e.EffectiveDate).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ConcurrencyToken).IsRowVersion();

            entity.HasMany(e => e.Lines)
                .WithOne(l => l.Planogram)
                .HasForeignKey(l => l.PlanogramId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // PlanogramLine Configuration
        modelBuilder.Entity<PlanogramLine>(entity =>
        {
            entity.ToTable("planogram_lines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StoreName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.BrandName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.BrandCode).HasMaxLength(20);
            entity.Property(e => e.Commodity).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.StoreCategory).HasMaxLength(20);
            entity.Property(e => e.CategoryTier).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.StoreType).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.DispatchPlan).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.ConcurrencyToken).IsRowVersion();

            entity.HasIndex(e => new { e.PlanogramId, e.StoreName, e.BrandName, e.Commodity });
            entity.HasIndex(e => e.StoreName);
        });

        // Allocation Configuration
        modelBuilder.Entity<Allocation>(entity =>
        {
            entity.ToTable("allocations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RunDate).IsRequired();
            entity.Property(e => e.RunBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConcurrencyToken).IsRowVersion();

            entity.Property(e => e.Summary)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<AllocationSummary>(v, (System.Text.Json.JsonSerializerOptions?)null)
                );

            entity.HasMany(e => e.Lines)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AllocationLine Configuration
        modelBuilder.Entity<AllocationLine>(entity =>
        {
            entity.ToTable("allocation_lines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StoreName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.BrandName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Commodity).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.MatchType).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.AllocatedBarcode).HasMaxLength(100);
            entity.Property(e => e.AllocatedItemCode).HasMaxLength(100);
            entity.Property(e => e.Mrp).HasPrecision(18, 2);

            entity.OwnsOne(e => e.RequestedAttributes, attrs =>
            {
                attrs.Property(p => p.Model).HasColumnName("requested_model").HasMaxLength(100);
                attrs.Property(p => p.Brand).HasColumnName("requested_brand").HasMaxLength(100);
                attrs.Property(p => p.Gender).HasColumnName("requested_gender").HasMaxLength(50);
                attrs.Property(p => p.Color).HasColumnName("requested_color").HasMaxLength(50);
                attrs.Property(p => p.Size).HasColumnName("requested_size").HasMaxLength(50);
                attrs.Property(p => p.FrameType).HasColumnName("requested_frametype").HasMaxLength(50);
                attrs.Property(p => p.Shape).HasColumnName("requested_shape").HasMaxLength(50);
                attrs.Property(p => p.Material).HasColumnName("requested_material").HasMaxLength(50);
            });

            entity.OwnsOne(e => e.AllocatedAttributes, attrs =>
            {
                attrs.Property(p => p.Model).HasColumnName("allocated_model").HasMaxLength(100);
                attrs.Property(p => p.Brand).HasColumnName("allocated_brand").HasMaxLength(100);
                attrs.Property(p => p.Gender).HasColumnName("allocated_gender").HasMaxLength(50);
                attrs.Property(p => p.Color).HasColumnName("allocated_color").HasMaxLength(50);
                attrs.Property(p => p.Size).HasColumnName("allocated_size").HasMaxLength(50);
                attrs.Property(p => p.FrameType).HasColumnName("allocated_frametype").HasMaxLength(50);
                attrs.Property(p => p.Shape).HasColumnName("allocated_shape").HasMaxLength(50);
                attrs.Property(p => p.Material).HasColumnName("allocated_material").HasMaxLength(50);
            });

            entity.HasIndex(e => new { e.StoreName, e.BrandName, e.Commodity });
            entity.HasIndex(e => e.AllocatedBarcode);
        });

        // Stock and Sales Configuration
        modelBuilder.Entity<WarehouseStockBatch>(entity =>
        {
            entity.ToTable("warehouse_stock_batches");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Barcode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ItemCode).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Mrp).HasPrecision(18, 2);
            entity.HasIndex(e => e.Barcode);
            entity.HasIndex(e => e.ItemCode);
        });

        modelBuilder.Entity<StoreStock>(entity =>
        {
            entity.ToTable("store_stocks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Facility).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ItemCode).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.Facility, e.ItemCode });
        });

        modelBuilder.Entity<SalesRecord>(entity =>
        {
            entity.ToTable("sales_records");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FacilityName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.ItemCode).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => new { e.FacilityName, e.ItemCode });
        });
    }
}
