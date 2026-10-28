using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DEKODE.OptiFlow.DAL.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Normalized_OptiFlow_Schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "brand_categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brand_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "facility_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facility_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaxNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "brands",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BrandCategoryId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_brands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_brands_brand_categories_BrandCategoryId",
                        column: x => x.BrandCategoryId,
                        principalTable: "brand_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FacilityTypeId = table.Column<int>(type: "integer", nullable: false),
                    Tier = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_facilities_facility_types_FacilityTypeId",
                        column: x => x.FacilityTypeId,
                        principalTable: "facility_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommodityType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Shape = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Material = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HsnCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "allocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceFacilityId = table.Column<Guid>(type: "uuid", nullable: true),
                    RunDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RunBy = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Calculated"),
                    TotalQuantityAllocated = table.Column<int>(type: "integer", nullable: false),
                    SummaryJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_allocations_facilities_SourceFacilityId",
                        column: x => x.SourceFacilityId,
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SeasonOrMonth = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planograms_facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Barcode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SkuCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    BaseMrp = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    DefaultPurchaseCost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    GstRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_items_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "product_suppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationshipType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SupplierProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StandardCost = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_suppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_product_suppliers_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_product_suppliers_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "planogram_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanogramId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrandId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommodityType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FacingCapacity = table.Column<int>(type: "integer", nullable: false),
                    BackStockCapacity = table.Column<int>(type: "integer", nullable: false),
                    MinCapacity = table.Column<int>(type: "integer", nullable: false),
                    StoreBrandProductTypeKey = table.Column<string>(type: "text", nullable: false),
                    StoreName = table.Column<string>(type: "text", nullable: false),
                    BrandName = table.Column<string>(type: "text", nullable: false),
                    BrandCode = table.Column<string>(type: "text", nullable: false),
                    Commodity = table.Column<string>(type: "text", nullable: false),
                    StoreCategory = table.Column<string>(type: "text", nullable: false),
                    CategoryTier = table.Column<string>(type: "text", nullable: false),
                    StoreType = table.Column<string>(type: "text", nullable: false),
                    DispatchPlan = table.Column<string>(type: "text", nullable: false),
                    Facing = table.Column<double>(type: "double precision", nullable: false),
                    BackStock = table.Column<double>(type: "double precision", nullable: false),
                    TargetCapacity = table.Column<double>(type: "double precision", nullable: false),
                    Soh = table.Column<double>(type: "double precision", nullable: false),
                    SkuCount = table.Column<int>(type: "integer", nullable: true),
                    AvgMonthlySale = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planogram_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planogram_lines_brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planogram_lines_planograms_PlanogramId",
                        column: x => x.PlanogramId,
                        principalTable: "planograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "allocation_lines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AllocationId = table.Column<Guid>(type: "uuid", nullable: false),
                    DestinationFacilityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProductItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    AllocatedQuantity = table.Column<int>(type: "integer", nullable: false),
                    TargetCapacity = table.Column<int>(type: "integer", nullable: false),
                    CurrentSohUnits = table.Column<int>(type: "integer", nullable: false),
                    SalesVelocity = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    GapId = table.Column<int>(type: "integer", nullable: false),
                    StoreName = table.Column<string>(type: "text", nullable: false),
                    StoreCategory = table.Column<string>(type: "text", nullable: false),
                    StoreType = table.Column<string>(type: "text", nullable: false),
                    Region = table.Column<string>(type: "text", nullable: true),
                    Zone = table.Column<string>(type: "text", nullable: true),
                    BrandCode = table.Column<string>(type: "text", nullable: false),
                    BrandName = table.Column<string>(type: "text", nullable: false),
                    Commodity = table.Column<string>(type: "text", nullable: false),
                    Facing = table.Column<double>(type: "double precision", nullable: false),
                    BackStock = table.Column<double>(type: "double precision", nullable: false),
                    CurrentSoh = table.Column<double>(type: "double precision", nullable: false),
                    Deficit = table.Column<double>(type: "double precision", nullable: false),
                    InitialGap = table.Column<double>(type: "double precision", nullable: false),
                    RemainingGap = table.Column<double>(type: "double precision", nullable: false),
                    RequestedItemCode = table.Column<string>(type: "text", nullable: true),
                    AllocatedItemCode = table.Column<string>(type: "text", nullable: true),
                    AllocatedItemName = table.Column<string>(type: "text", nullable: true),
                    AllocatedBarcode = table.Column<string>(type: "text", nullable: true),
                    AllocatedQty = table.Column<int>(type: "integer", nullable: false),
                    MatchType = table.Column<string>(type: "text", nullable: false),
                    Mrp = table.Column<decimal>(type: "numeric", nullable: false),
                    MatchReason = table.Column<string>(type: "text", nullable: false),
                    ColorLimitWarning = table.Column<bool>(type: "boolean", nullable: false),
                    SimilarityScore = table.Column<double>(type: "double precision", nullable: true),
                    RequestedAttributesJson = table.Column<string>(type: "text", nullable: true),
                    AllocatedAttributesJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allocation_lines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_allocation_lines_allocations_AllocationId",
                        column: x => x.AllocationId,
                        principalTable: "allocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_allocation_lines_facilities_DestinationFacilityId",
                        column: x => x.DestinationFacilityId,
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_allocation_lines_product_items_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "product_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inventory_stocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    BatchNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QuantityOnHand = table.Column<int>(type: "integer", nullable: false),
                    PurchasePrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    SellingPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    GrnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StockAgeDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_stocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inventory_stocks_facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_stocks_product_items_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "product_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inventory_stocks_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sales_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FacilityId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UnitRetailPrice = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    FinancialYear = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PeriodMonth = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Completed"),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_records_facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalTable: "facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_sales_records_product_items_ProductItemId",
                        column: x => x.ProductItemId,
                        principalTable: "product_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "brand_categories",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "INTL_BRAND", "International Brand" },
                    { 2, "PRIVATE_LABEL", "Private Label" },
                    { 3, "LUXURY", "Luxury" },
                    { 4, "LOCAL_LABEL", "Local Label" }
                });

            migrationBuilder.InsertData(
                table: "facility_types",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "CORPORATE_OFFICE", "Corporate Office / Central Warehouse" },
                    { 2, "WAREHOUSE", "Regional Distribution Center" },
                    { 3, "RETAIL_STANDALONE", "Retail Standalone Store" },
                    { 4, "RETAIL_SIS", "Shop-in-Shop Retail" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_allocation_lines_AllocationId_DestinationFacilityId_Product~",
                table: "allocation_lines",
                columns: new[] { "AllocationId", "DestinationFacilityId", "ProductItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_allocation_lines_DestinationFacilityId",
                table: "allocation_lines",
                column: "DestinationFacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_allocation_lines_ProductItemId",
                table: "allocation_lines",
                column: "ProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_allocations_SourceFacilityId",
                table: "allocations",
                column: "SourceFacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_brand_categories_Code",
                table: "brand_categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_brands_BrandCategoryId",
                table: "brands",
                column: "BrandCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_brands_Code",
                table: "brands",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facilities_Code",
                table: "facilities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facilities_FacilityTypeId",
                table: "facilities",
                column: "FacilityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_facility_types_Code",
                table: "facility_types",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stocks_FacilityId_ProductItemId",
                table: "inventory_stocks",
                columns: new[] { "FacilityId", "ProductItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stocks_FacilityId_ProductItemId_BatchNumber_Suppl~",
                table: "inventory_stocks",
                columns: new[] { "FacilityId", "ProductItemId", "BatchNumber", "SupplierId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stocks_ProductItemId",
                table: "inventory_stocks",
                column: "ProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_stocks_SupplierId",
                table: "inventory_stocks",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_planogram_lines_BrandId",
                table: "planogram_lines",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_planogram_lines_PlanogramId_BrandId_CommodityType",
                table: "planogram_lines",
                columns: new[] { "PlanogramId", "BrandId", "CommodityType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planograms_FacilityId",
                table: "planograms",
                column: "FacilityId");

            migrationBuilder.CreateIndex(
                name: "IX_product_items_Barcode",
                table: "product_items",
                column: "Barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_items_ProductId",
                table: "product_items",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_suppliers_ProductId_SupplierId_RelationshipType",
                table: "product_suppliers",
                columns: new[] { "ProductId", "SupplierId", "RelationshipType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_suppliers_SupplierId",
                table: "product_suppliers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_products_BrandId",
                table: "products",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_products_ProductCode",
                table: "products",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sales_records_FacilityId_ProductItemId",
                table: "sales_records",
                columns: new[] { "FacilityId", "ProductItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_sales_records_ProductItemId",
                table: "sales_records",
                column: "ProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_records_TransactionDate",
                table: "sales_records",
                column: "TransactionDate");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_Code",
                table: "suppliers",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "allocation_lines");

            migrationBuilder.DropTable(
                name: "inventory_stocks");

            migrationBuilder.DropTable(
                name: "planogram_lines");

            migrationBuilder.DropTable(
                name: "product_suppliers");

            migrationBuilder.DropTable(
                name: "sales_records");

            migrationBuilder.DropTable(
                name: "allocations");

            migrationBuilder.DropTable(
                name: "planograms");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "product_items");

            migrationBuilder.DropTable(
                name: "facilities");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "facility_types");

            migrationBuilder.DropTable(
                name: "brands");

            migrationBuilder.DropTable(
                name: "brand_categories");
        }
    }
}
