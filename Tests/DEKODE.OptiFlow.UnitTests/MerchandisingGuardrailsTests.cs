using DEKODE.OptiFlow.Domain.BusinessServices.Engine;
using DEKODE.OptiFlow.Domain.BusinessServices.Services;
using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using FluentAssertions;
using Xunit;
using MatchType = DEKODE.OptiFlow.Domain.Entities.Enums.MatchType;

namespace DEKODE.OptiFlow.UnitTests;

public class MerchandisingGuardrailsTests
{
    private readonly OptiFlowAllocationEngine _engine;

    public MerchandisingGuardrailsTests()
    {
        var parser = new ItemAttributeParser();
        var scorer = new WeightedAttributeSimilarityScorer();
        _engine = new OptiFlowAllocationEngine(parser, scorer);
    }

    private static string MakeItemName(string model, string color = "Red", string shape = "Rectangle", string mat = "Metal") =>
        $"{model} - Rayban - Male - {color} - 53 - Full Frame - {shape} - {mat}";

    [Fact]
    public void Guardrail_DeficitCap_PreventsOverAllocation()
    {
        // Deficit is 3, warehouse has 50 units. Allocation must not exceed 3.
        var context = new AllocationExecutionContext
        {
            PlanogramLines = new List<PlanogramLine>
            {
                new() { StoreName = "Store Alpha", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 1, BackStock = 2, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatch>
            {
                new() { ItemCode = "SKU_PLENTY", ItemName = MakeItemName("M1"), BatchStock = 50, Mrp = 1000m, Barcode = "BC_1", StockAgeInDays = 5 }
            },
            StrategyStoreLists = new Dictionary<string, IReadOnlyList<string>>
            {
                ["A"] = new List<string> { "Store Alpha" }
            },
            ActiveCategories = new List<string> { "A" }
        };

        var (lines, _) = _engine.Execute(context);

        var totalAllocated = lines.Where(l => l.StoreName == "Store Alpha").Sum(l => l.AllocatedQty);
        totalAllocated.Should().BeLessThanOrEqualTo(3);
    }

    [Fact]
    public void Guardrail_MaxTwoUnitsPerSku_EnforcedPerStore()
    {
        // Store demands 6 units. Warehouse only has 2 SKUs with abundant stock.
        // Each SKU must be capped at 2 units maximum per store.
        var context = new AllocationExecutionContext
        {
            PlanogramLines = new List<PlanogramLine>
            {
                new() { StoreName = "Store Beta", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 3, BackStock = 3, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatch>
            {
                new() { ItemCode = "SKU_A", ItemName = MakeItemName("MA"), BatchStock = 20, Mrp = 1000m, Barcode = "BCA", StockAgeInDays = 10 },
                new() { ItemCode = "SKU_B", ItemName = MakeItemName("MB"), BatchStock = 20, Mrp = 1000m, Barcode = "BCB", StockAgeInDays = 5 }
            },
            StrategyStoreLists = new Dictionary<string, IReadOnlyList<string>>
            {
                ["A"] = new List<string> { "Store Beta" }
            },
            ActiveCategories = new List<string> { "A" }
        };

        var (lines, _) = _engine.Execute(context);

        var skuAllocCounts = lines
            .Where(l => l.AllocatedQty > 0 && !string.IsNullOrEmpty(l.AllocatedItemCode))
            .GroupBy(l => l.AllocatedItemCode!)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.AllocatedQty));

        foreach (var (sku, count) in skuAllocCounts)
        {
            count.Should().BeLessThanOrEqualTo(2, $"SKU {sku} exceeded max limit of 2 units");
        }
    }

    [Fact]
    public void Guardrail_MaxThreeColorsPerModel_BlocksFourthNewColor()
    {
        // Model "RX_MULTI" has 4 colors: Red, Blue, Green, Yellow.
        // After 3 colors are allocated, the 4th color must be skipped during candidate selection.
        var context = new AllocationExecutionContext
        {
            PlanogramLines = new List<PlanogramLine>
            {
                new() { StoreName = "Store Color", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 2, BackStock = 2, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatch>
            {
                new() { ItemCode = "SKU_RED", ItemName = MakeItemName("RX_MULTI", color: "Red"), BatchStock = 1, Mrp = 1000m, Barcode = "BC_R", StockAgeInDays = 10 },
                new() { ItemCode = "SKU_BLUE", ItemName = MakeItemName("RX_MULTI", color: "Blue"), BatchStock = 1, Mrp = 1000m, Barcode = "BC_B", StockAgeInDays = 9 },
                new() { ItemCode = "SKU_GREEN", ItemName = MakeItemName("RX_MULTI", color: "Green"), BatchStock = 1, Mrp = 1000m, Barcode = "BC_G", StockAgeInDays = 8 },
                new() { ItemCode = "SKU_YELLOW", ItemName = MakeItemName("RX_MULTI", color: "Yellow"), BatchStock = 1, Mrp = 1000m, Barcode = "BC_Y", StockAgeInDays = 7 }
            },
            StrategyStoreLists = new Dictionary<string, IReadOnlyList<string>>
            {
                ["A"] = new List<string> { "Store Color" }
            },
            ActiveCategories = new List<string> { "A" }
        };

        var (lines, _) = _engine.Execute(context);

        var allocatedColors = lines
            .Where(l => l.AllocatedQty > 0 && l.AllocatedAttributes?.Model == "RX_MULTI")
            .Select(l => l.AllocatedAttributes?.Color)
            .Distinct()
            .ToList();

        allocatedColors.Count.Should().BeLessThanOrEqualTo(3, "Store must not exceed 3 distinct colors for a single model");
    }

    [Fact]
    public void Guardrail_PriceBandTolerance_PenalizesAndClassifiesAsSubstitute()
    {
        // Reference item in store sales has MRP = 1000.
        // Candidate has MRP = 1800 (+80% above price band of +/- 20%).
        // It must be classified as SUBSTITUTE, not SIMILAR.
        var context = new AllocationExecutionContext
        {
            PlanogramLines = new List<PlanogramLine>
            {
                new() { StoreName = "Store Price", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 1, BackStock = 0, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatch>
            {
                new() { ItemCode = "SKU_BENCH", ItemName = MakeItemName("M_BENCH"), BatchStock = 0, Mrp = 1000m, Barcode = "BC_BENCH", StockAgeInDays = 10 },
                new() { ItemCode = "SKU_HIGH_PRICE", ItemName = MakeItemName("M_HIGH"), BatchStock = 2, Mrp = 1800m, Barcode = "BC_HIGH", StockAgeInDays = 5 }
            },
            SalesRecords = new List<SalesRecord>
            {
                new() { FacilityName = "Store Price", ItemCode = "SKU_BENCH", Quantity = 5 }
            },
            StrategyStoreLists = new Dictionary<string, IReadOnlyList<string>>
            {
                ["A"] = new List<string> { "Store Price" }
            },
            ActiveCategories = new List<string> { "A" }
        };

        var (lines, _) = _engine.Execute(context);

        var allocatedLines = lines.Where(l => l.AllocatedQty > 0).ToList();
        allocatedLines.Should().NotBeEmpty();
        allocatedLines.Should().OnlyContain(l => l.MatchType == MatchType.Substitute || l.MatchType == MatchType.Exact);
        allocatedLines.Should().NotContain(l => l.MatchType == MatchType.Similar);
    }

    [Fact]
    public void Guardrail_FifoStockAgeDrain_PrioritizesOldestBatchesFirst()
    {
        // Two batches of same SKU: BAR_OLD is 100 days old, BAR_NEW is 5 days old.
        // Deficit = 1. Allocation must consume BAR_OLD first.
        var context = new AllocationExecutionContext
        {
            PlanogramLines = new List<PlanogramLine>
            {
                new() { StoreName = "Store Fifo", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 1, BackStock = 0, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatch>
            {
                new() { ItemCode = "SKU_FIFO", ItemName = MakeItemName("M_FIFO"), BatchStock = 5, Mrp = 1000m, Barcode = "BAR_NEW", StockAgeInDays = 5 },
                new() { ItemCode = "SKU_FIFO", ItemName = MakeItemName("M_FIFO"), BatchStock = 5, Mrp = 1000m, Barcode = "BAR_OLD", StockAgeInDays = 100 }
            },
            StrategyStoreLists = new Dictionary<string, IReadOnlyList<string>>
            {
                ["A"] = new List<string> { "Store Fifo" }
            },
            ActiveCategories = new List<string> { "A" }
        };

        var (lines, _) = _engine.Execute(context);

        var allocatedLine = lines.FirstOrDefault(l => l.AllocatedQty > 0);
        allocatedLine.Should().NotBeNull();
        allocatedLine!.AllocatedBarcode.Should().Be("BAR_OLD");
    }
}
