using DEKODE.OptiFlow.Domain.BusinessServices.Engine;
using DEKODE.OptiFlow.Domain.BusinessServices.Services;
using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using FluentAssertions;
using Xunit;
using MatchType = DEKODE.OptiFlow.Domain.Entities.Enums.MatchType;

namespace DEKODE.OptiFlow.UnitTests;

public class AllocationEngineWaterfallTests
{
    private readonly OptiFlowAllocationEngine _engine;

    public AllocationEngineWaterfallTests()
    {
        var parser = new ItemAttributeParser();
        var scorer = new WeightedAttributeSimilarityScorer();
        _engine = new OptiFlowAllocationEngine(parser, scorer);
    }

    private static string MakeItemName(string model, string color = "Red", string shape = "Rectangle", string mat = "Metal") =>
        $"{model} - Rayban - Male - {color} - 53 - Full Frame - {shape} - {mat}";

    [Fact]
    public void Waterfall_TargetMetBySOH_EmitsAuditLineWithZeroQty()
    {
        // Store Alpha has Facing=1, BackStock=1 (Target=2). Live SOH is already 2 units.
        var context = new AllocationExecutionContext
        {
            PlanogramLines = new List<PlanogramLine>
            {
                new() { StoreName = "Store Alpha", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 1, BackStock = 1, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatch>
            {
                new() { ItemCode = "SKU_1", ItemName = MakeItemName("M1"), BatchStock = 10, Mrp = 1000m, Barcode = "BC_1", StockAgeInDays = 10 }
            },
            StoreStock = new List<StoreStock>
            {
                new() { Facility = "Store Alpha", ItemCode = "SKU_1", BatchStock = 2 }
            },
            StrategyStoreLists = new Dictionary<string, IReadOnlyList<string>>
            {
                ["A"] = new List<string> { "Store Alpha" }
            },
            ActiveCategories = new List<string> { "A" }
        };

        var (lines, summary) = _engine.Execute(context);

        lines.Should().HaveCount(1);
        var auditLine = lines[0];
        auditLine.AllocatedQty.Should().Be(0);
        auditLine.AllocatedItemName.Should().Be("Target met by SOH");
        auditLine.MatchReason.Should().Be("Target met by existing SOH");
        summary.TotalItemsAllocated.Should().Be(0);
        summary.TotalTargetDeficit.Should().Be(0);
    }

    [Fact]
    public void Waterfall_Tier1_ExactRecentSalesMatch_AllocatesTopSeller()
    {
        // Store A demands 2 units. Top sales has SKU_TOP sold recently.
        var context = new AllocationExecutionContext
        {
            PlanogramLines = new List<PlanogramLine>
            {
                new() { StoreName = "Store A", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 1, BackStock = 1, StoreCategory = "A" }
            },
            WarehouseBatches = new List<WarehouseStockBatch>
            {
                new() { ItemCode = "SKU_TOP", ItemName = MakeItemName("M_TOP"), BatchStock = 10, Mrp = 1000m, Barcode = "BC_TOP", StockAgeInDays = 5 },
                new() { ItemCode = "SKU_OTHER", ItemName = MakeItemName("M_OTHER"), BatchStock = 10, Mrp = 1000m, Barcode = "BC_OTHER", StockAgeInDays = 1 }
            },
            SalesRecords = new List<SalesRecord>
            {
                new() { FacilityName = "Store A", ItemCode = "SKU_TOP", Quantity = 10, OrderDate = DateTime.UtcNow.AddDays(-5) },
                new() { FacilityName = "Store A", ItemCode = "SKU_OTHER", Quantity = 2, OrderDate = DateTime.UtcNow.AddDays(-5) }
            },
            SalesLookbackDays = 30,
            StrategyStoreLists = new Dictionary<string, IReadOnlyList<string>>
            {
                ["A"] = new List<string> { "Store A" }
            },
            ActiveCategories = new List<string> { "A" }
        };

        var (lines, summary) = _engine.Execute(context);

        lines.Should().NotBeEmpty();
        lines.Should().Contain(l => l.AllocatedItemCode == "SKU_TOP" && l.MatchType == MatchType.Exact);
        lines.Sum(l => l.AllocatedQty).Should().Be(2);
        summary.TotalItemsAllocated.Should().Be(2);
        summary.ExactMatches.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Waterfall_PriorityOrdering_HigherTierConsumesScarceStockFirst()
    {
        // Warehouse has only 2 units of SKU_EXCLUSIVE.
        // Store Premium (A++) and Store Standard (B) both need 2 units. Store Premium must win.
        var context = new AllocationExecutionContext
        {
            PlanogramLines = new List<PlanogramLine>
            {
                new() { StoreName = "Store Premium", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 1, BackStock = 1, StoreCategory = "A++" },
                new() { StoreName = "Store Standard", BrandName = "Rayban", Commodity = CommodityType.Frame, Facing = 1, BackStock = 1, StoreCategory = "B" }
            },
            WarehouseBatches = new List<WarehouseStockBatch>
            {
                new() { ItemCode = "SKU_EXCLUSIVE", ItemName = MakeItemName("M_EXCLUSIVE"), BatchStock = 1, Mrp = 1500m, Barcode = "BC_EXC", StockAgeInDays = 10 }
            },
            StrategyStoreLists = new Dictionary<string, IReadOnlyList<string>>
            {
                ["A++"] = new List<string> { "Store Premium" },
                ["B"] = new List<string> { "Store Standard" }
            },
            ActiveCategories = new List<string> { "A++", "B" }
        };

        var (lines, summary) = _engine.Execute(context);

        // Store Premium has priority and consumes the scarce warehouse unit first
        var premiumLines = lines.Where(l => l.StoreName == "Store Premium" && l.AllocatedQty > 0).ToList();
        premiumLines.Sum(l => l.AllocatedQty).Should().Be(1);

        // Store Standard should receive 0 units because stock was consumed by higher priority tier
        var standardAllocated = lines.Where(l => l.StoreName == "Store Standard" && l.AllocatedQty > 0).Sum(l => l.AllocatedQty);
        standardAllocated.Should().Be(0);

        // Unresolved line emitted for Store Standard
        lines.Should().Contain(l => l.StoreName == "Store Standard" && l.MatchType == MatchType.Unresolved);
    }
}
