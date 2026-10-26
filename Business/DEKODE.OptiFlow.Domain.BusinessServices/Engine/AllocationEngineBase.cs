using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using DEKODE.OptiFlow.Domain.Interfaces;
using MatchType = DEKODE.OptiFlow.Domain.Entities.Enums.MatchType;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Engine;

/// <summary>
/// Internal state models and lookups for the allocation waterfall run.
/// </summary>
public class WarehouseCatalogItem
{
    public required string ItemCode { get; init; }
    public required string ItemName { get; init; }
    public required decimal Mrp { get; init; }
    public required int TotalStock { get; set; }
    public required string Brand { get; init; }
    public required CommodityType Commodity { get; init; }
    public required ItemAttributes Attributes { get; init; }
}

public class WarehouseBatchNode
{
    public required string Barcode { get; init; }
    public required int Quantity { get; set; }
    public required int StockAgeInDays { get; init; }
}

public record CandidateEvaluation(
    WarehouseCatalogItem Candidate,
    double Score,
    DEKODE.OptiFlow.Domain.Entities.Enums.MatchType MatchType,
    string Reason
);

/// <summary>
/// Template Method Pattern base class defining the invariant skeleton of the allocation waterfall algorithm.
/// Primitive steps are partitioned into cohesive, overridable or modular sub-methods (KISS, SOLID, DRY).
/// </summary>
public abstract class AllocationEngineBase : IAllocationEngine
{
    protected readonly IItemAttributeParser Parser;
    protected readonly ISimilarityScorer Scorer;

    protected AllocationEngineBase(IItemAttributeParser parser, ISimilarityScorer scorer)
    {
        Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        Scorer = scorer ?? throw new ArgumentNullException(nameof(scorer));
    }

    /// <summary>
    /// The Template Method: orchestrates the exact multi-tier waterfall flow.
    /// </summary>
    public (IReadOnlyList<AllocationLine> Lines, AllocationSummary Summary) Execute(AllocationExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Step 1: Pre-process inputs and build indexed catalog & FIFO queues
        var catalog = BuildWarehouseCatalog(context.WarehouseBatches);
        var fifoQueues = BuildFifoBatchQueues(context.WarehouseBatches);
        var remainingWhStock = catalog.Values.ToDictionary(k => k.ItemCode, v => v.TotalStock);

        // Step 2: Index store on-hand (SOH) and sales rankings
        var liveStoreStock = IndexLiveStoreStock(context.StoreStock, catalog, context.StoreNameMapping);
        var (histSalesRank, recentSalesRank) = IndexSalesRankings(context.SalesRecords, catalog, context.SalesLookbackDays);

        // Step 3: Determine ordered execution sequence across active tiers
        var orderedPlanogramLines = DetermineMasterOrder(context.PlanogramLines, context.StrategyStoreLists, context.ActiveCategories);

        // Step 4: Execute waterfall allocations across stores and planogram lines
        var allocations = ProcessWaterfallAllocation(
            orderedPlanogramLines,
            catalog,
            fifoQueues,
            remainingWhStock,
            liveStoreStock,
            histSalesRank,
            recentSalesRank,
            context.StoreNameMapping,
            context.ActiveCategories);

        // Step 5: Calculate and rollup KPIs
        var summary = BuildSummaryMetrics(allocations, orderedPlanogramLines, context.ActiveCategories);

        return (allocations, summary);
    }

    // =========================================================================
    // TEMPLATE STEPS / EXTENSION POINTS (Template Method Pattern)
    // =========================================================================

    protected virtual Dictionary<string, WarehouseCatalogItem> BuildWarehouseCatalog(
        IReadOnlyList<WarehouseStockBatch> batches)
    {
        var catalog = new Dictionary<string, WarehouseCatalogItem>(StringComparer.OrdinalIgnoreCase);

        var grouped = batches
            .GroupBy(b => b.ItemCode, StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            var first = group.First();
            var totalQty = group.Sum(b => b.BatchStock);
            var attrs = Parser.Parse(first.ItemName);

            var brand = !string.IsNullOrWhiteSpace(attrs.Brand)
                ? attrs.Brand
                : ExtractBrandFromDescription(first.ItemName);

            var commodity = ResolveCommodityType(first.ItemCategory, first.ItemName);

            catalog[group.Key] = new WarehouseCatalogItem
            {
                ItemCode = group.Key,
                ItemName = first.ItemName,
                Mrp = first.Mrp,
                TotalStock = totalQty,
                Brand = brand,
                Commodity = commodity,
                Attributes = attrs
            };
        }

        return catalog;
    }

    protected virtual Dictionary<string, List<WarehouseBatchNode>> BuildFifoBatchQueues(
        IReadOnlyList<WarehouseStockBatch> batches)
    {
        var queues = new Dictionary<string, List<WarehouseBatchNode>>(StringComparer.OrdinalIgnoreCase);

        var grouped = batches
            .Where(b => b.BatchStock > 0)
            .GroupBy(b => b.ItemCode, StringComparer.OrdinalIgnoreCase);

        foreach (var group in grouped)
        {
            // FIFO: Oldest stock age in days descending first
            var queue = group
                .OrderByDescending(b => b.StockAgeInDays)
                .Select(b => new WarehouseBatchNode
                {
                    Barcode = b.Barcode,
                    Quantity = b.BatchStock,
                    StockAgeInDays = b.StockAgeInDays
                })
                .ToList();

            queues[group.Key] = queue;
        }

        return queues;
    }

    protected virtual Dictionary<(string Facility, string Brand, CommodityType Commodity), List<(string ItemCode, int Qty)>> IndexLiveStoreStock(
        IReadOnlyList<StoreStock> storeStocks,
        Dictionary<string, WarehouseCatalogItem> catalog,
        IReadOnlyDictionary<string, string> storeNameMapping)
    {
        var liveStock = new Dictionary<(string Facility, string Brand, CommodityType Commodity), List<(string ItemCode, int Qty)>>();

        var grouped = storeStocks
            .Where(s => s.BatchStock > 0)
            .GroupBy(s => (Facility: MapStoreName(s.Facility, storeNameMapping), s.ItemCode));

        foreach (var group in grouped)
        {
            var sumQty = group.Sum(g => g.BatchStock);
            if (catalog.TryGetValue(group.Key.ItemCode, out var catItem))
            {
                var key = (group.Key.Facility.ToLowerInvariant(), catItem.Brand.ToLowerInvariant(), catItem.Commodity);
                if (!liveStock.TryGetValue(key, out var list))
                {
                    list = new List<(string ItemCode, int Qty)>();
                    liveStock[key] = list;
                }
                list.Add((group.Key.ItemCode, sumQty));
            }
        }

        return liveStock;
    }

    protected virtual (
        Dictionary<(string Facility, string Brand, CommodityType Commodity), List<string>> Historical,
        Dictionary<(string Facility, string Brand, CommodityType Commodity), List<string>> Recent
    ) IndexSalesRankings(
        IReadOnlyList<SalesRecord> salesRecords,
        Dictionary<string, WarehouseCatalogItem> catalog,
        int? lookbackDays)
    {
        var hist = new Dictionary<(string Facility, string Brand, CommodityType Commodity), List<string>>();
        var recent = new Dictionary<(string Facility, string Brand, CommodityType Commodity), List<string>>();

        if (salesRecords == null || salesRecords.Count == 0)
        {
            return (hist, recent);
        }

        var validRecords = salesRecords
            .Where(r => r.Quantity > 0 && catalog.ContainsKey(r.ItemCode))
            .ToList();

        if (validRecords.Count == 0)
        {
            return (hist, recent);
        }

        // 1. All-time Historical Ranking
        var histGrouped = validRecords
            .GroupBy(r => (Facility: r.FacilityName.ToLowerInvariant(), r.ItemCode))
            .Select(g => new { g.Key.Facility, g.Key.ItemCode, Qty = g.Sum(x => x.Quantity) })
            .OrderBy(x => x.Facility)
            .ThenByDescending(x => x.Qty);

        foreach (var row in histGrouped)
        {
            var catItem = catalog[row.ItemCode];
            var key = (row.Facility, catItem.Brand.ToLowerInvariant(), catItem.Commodity);
            if (!hist.TryGetValue(key, out var list))
            {
                list = new List<string>();
                hist[key] = list;
            }
            list.Add(row.ItemCode);
        }

        // 2. Recent Lookback Ranking
        var recordsWithDates = validRecords.Where(r => r.OrderDate.HasValue).ToList();
        if (lookbackDays.HasValue && lookbackDays.Value > 0 && recordsWithDates.Count > 0)
        {
            var maxDate = recordsWithDates.Max(r => r.OrderDate!.Value);
            var cutoffDate = maxDate.AddDays(-lookbackDays.Value);

            var recentGrouped = recordsWithDates
                .Where(r => r.OrderDate!.Value >= cutoffDate)
                .GroupBy(r => (Facility: r.FacilityName.ToLowerInvariant(), r.ItemCode))
                .Select(g => new { g.Key.Facility, g.Key.ItemCode, Qty = g.Sum(x => x.Quantity) })
                .OrderBy(x => x.Facility)
                .ThenByDescending(x => x.Qty);

            foreach (var row in recentGrouped)
            {
                var catItem = catalog[row.ItemCode];
                var key = (row.Facility, catItem.Brand.ToLowerInvariant(), catItem.Commodity);
                if (!recent.TryGetValue(key, out var list))
                {
                    list = new List<string>();
                    recent[key] = list;
                }
                list.Add(row.ItemCode);
            }
        }
        else
        {
            // Fallback: If no lookback window specified or no dates, recent sales mirrors historical sales
            recent = hist;
        }

        return (hist, recent);
    }

    protected virtual List<PlanogramLine> DetermineMasterOrder(
        IReadOnlyList<PlanogramLine> lines,
        IReadOnlyDictionary<string, IReadOnlyList<string>> strategyStoreLists,
        IReadOnlyList<string> activeCategories)
    {
        var masterStoreOrder = new List<string>();
        foreach (var category in activeCategories)
        {
            if (strategyStoreLists.TryGetValue(category, out var storeList))
            {
                masterStoreOrder.AddRange(storeList);
            }
        }

        var storeRank = masterStoreOrder
            .Select((store, index) => new { store, index })
            .ToDictionary(x => x.store, x => x.index, StringComparer.OrdinalIgnoreCase);

        return lines
            .Where(l => storeRank.ContainsKey(l.StoreName))
            .OrderBy(l => storeRank[l.StoreName])
            .ToList();
    }

    protected abstract List<AllocationLine> ProcessWaterfallAllocation(
        List<PlanogramLine> planogramLines,
        Dictionary<string, WarehouseCatalogItem> catalog,
        Dictionary<string, List<WarehouseBatchNode>> fifoQueues,
        Dictionary<string, int> remainingWhStock,
        Dictionary<(string Facility, string Brand, CommodityType Commodity), List<(string ItemCode, int Qty)>> liveStoreStock,
        Dictionary<(string Facility, string Brand, CommodityType Commodity), List<string>> histSalesRank,
        Dictionary<(string Facility, string Brand, CommodityType Commodity), List<string>> recentSalesRank,
        IReadOnlyDictionary<string, string> storeNameMapping,
        IReadOnlyList<string> activeCategories);

    protected virtual AllocationSummary BuildSummaryMetrics(
        List<AllocationLine> allocations,
        List<PlanogramLine> planogramLines,
        IReadOnlyList<string> activeCategories)
    {
        var summary = new AllocationSummary
        {
            TotalStores = planogramLines.Select(p => p.StoreName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            TotalDeficitLines = allocations.Where(a => a.Deficit > 0).Select(a => a.GapId).Distinct().Count(),
            ExactMatches = allocations.Count(a => a.MatchType == MatchType.Exact && a.AllocatedQty > 0),
            SimilarMatches = allocations.Count(a => a.MatchType == MatchType.Similar && a.AllocatedQty > 0),
            BrandFallbacks = allocations.Count(a => a.MatchType == MatchType.Substitute && a.AllocatedQty > 0),
            Unresolved = allocations.Count(a => a.MatchType == MatchType.Unresolved),
            TotalItemsAllocated = allocations.Sum(a => a.AllocatedQty),
            TotalRetailValue = allocations.Sum(a => a.AllocatedQty * a.Mrp),
            TotalTargetDeficit = (int)Math.Ceiling(allocations.GroupBy(a => a.GapId).Sum(g => Math.Max(0, g.First().Deficit))),
            TotalSoh = (int)allocations.GroupBy(a => a.GapId).Sum(g => Math.Max(0, g.First().CurrentSoh)),
            StoresWithDeficits = allocations.Where(a => a.Deficit > 0).Select(a => a.StoreName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            BrandsProcessed = allocations.Select(a => a.BrandName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
            LastRunAt = DateTime.UtcNow
        };

        // Seed tier fulfillments
        foreach (var category in activeCategories)
        {
            var targetForTier = (int)planogramLines
                .Where(p => string.Equals(p.StoreCategory, category, StringComparison.OrdinalIgnoreCase))
                .Sum(p => Math.Max(0, Math.Ceiling(p.TargetCapacity)));

            var filledForTier = allocations
                .Where(a => string.Equals(a.StoreCategory, category, StringComparison.OrdinalIgnoreCase))
                .Sum(a => a.AllocatedQty);

            summary.TierFulfillment[category] = new TierFulfillmentMetrics
            {
                Target = targetForTier,
                Filled = filledForTier
            };
        }

        // Per-brand stats
        var brandGroups = allocations.GroupBy(a => a.BrandName, StringComparer.OrdinalIgnoreCase);
        foreach (var bg in brandGroups)
        {
            summary.BrandStats.Add(new BrandAllocationMetrics
            {
                BrandName = bg.Key,
                FilledUnits = bg.Sum(x => x.AllocatedQty),
                RetailValue = bg.Sum(x => x.AllocatedQty * x.Mrp),
                ExactCount = bg.Count(x => x.MatchType == MatchType.Exact && x.AllocatedQty > 0),
                SimilarCount = bg.Count(x => x.MatchType == MatchType.Similar && x.AllocatedQty > 0),
                SubstituteCount = bg.Count(x => x.MatchType == MatchType.Substitute && x.AllocatedQty > 0),
                UnresolvedCount = bg.Count(x => x.MatchType == MatchType.Unresolved)
            });
        }

        return summary;
    }

    // --- Helper utility methods ---
    protected static string MapStoreName(string storeName, IReadOnlyDictionary<string, string> mapping)
    {
        return mapping.TryGetValue(storeName, out var mapped) ? mapped : storeName;
    }

    protected static string ExtractBrandFromDescription(string name)
    {
        var parts = name.Split(" - ", StringSplitOptions.TrimEntries);
        return parts.Length > 1 ? parts[1] : string.Empty;
    }

    public static CommodityType ResolveCommodityType(string? category, string? name)
    {
        var cat = (category ?? string.Empty).ToLowerInvariant();
        var n = (name ?? string.Empty).ToLowerInvariant();
        if (cat.Contains("goggles") || cat.Contains("sunglass") || n.Contains("sunglass"))
        {
            return CommodityType.Sunglass;
        }
        return CommodityType.Frame;
    }
}
