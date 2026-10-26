using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using DEKODE.OptiFlow.Domain.Interfaces;
using MatchType = DEKODE.OptiFlow.Domain.Entities.Enums.MatchType;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Engine;

/// <summary>
/// Default production OptiFlow Allocation Engine implementing the full waterfall logic,
/// tiered matching, and merchandising guardrails.
/// </summary>
public class OptiFlowAllocationEngine : AllocationEngineBase
{
    private const int MaxUnitsPerSku = 2;
    private const int MaxColorsPerModel = 3;
    private const double MinBreadthThreshold = 0.85;
    private const double PriceBandTolerance = 0.20;

    public OptiFlowAllocationEngine(IItemAttributeParser parser, ISimilarityScorer scorer)
        : base(parser, scorer)
    {
    }

    protected override List<AllocationLine> ProcessWaterfallAllocation(
        List<PlanogramLine> planogramLines,
        Dictionary<string, WarehouseCatalogItem> catalog,
        Dictionary<string, List<WarehouseBatchNode>> fifoQueues,
        Dictionary<string, int> remainingWhStock,
        Dictionary<(string Facility, string Brand, CommodityType Commodity), List<(string ItemCode, int Qty)>> liveStoreStock,
        Dictionary<(string Facility, string Brand, CommodityType Commodity), List<string>> histSalesRank,
        Dictionary<(string Facility, string Brand, CommodityType Commodity), List<string>> recentSalesRank,
        IReadOnlyDictionary<string, string> storeNameMapping,
        IReadOnlyList<string> activeCategories)
    {
        var allocations = new List<AllocationLine>();

        // Tracking state across the waterfall run
        var storeBrandAllocatedSkus = new Dictionary<(string Store, string Brand), HashSet<string>>();
        var storeBrandAllocatedQtyTotal = new Dictionary<(string Store, string Brand), int>();
        var storeSkuCounts = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);
        var storeModelColors = new Dictionary<string, Dictionary<string, HashSet<string>>>(StringComparer.OrdinalIgnoreCase);

        int gapIndex = 0;

        foreach (var plLine in planogramLines)
        {
            gapIndex++;
            var storeName = plLine.StoreName;
            var mappedFacility = MapStoreName(storeName, storeNameMapping);
            var brandName = plLine.BrandName;
            var commodity = plLine.Commodity;

            // Compute current live SOH for (store, brand, commodity)
            var storeStockKey = (mappedFacility.ToLowerInvariant(), brandName.ToLowerInvariant(), commodity);
            var stockItems = liveStoreStock.TryGetValue(storeStockKey, out var items)
                ? items
                : new List<(string ItemCode, int Qty)>();

            double liveSoh = stockItems.Sum(x => x.Qty);
            double targetQty = plLine.TargetCapacity;
            double deficit = targetQty - liveSoh;

            // Audit line when target is satisfied by existing SOH
            if (deficit <= 0)
            {
                allocations.Add(new AllocationLine
                {
                    GapId = gapIndex,
                    StoreName = storeName,
                    StoreCategory = plLine.StoreCategory,
                    StoreType = plLine.StoreType,
                    BrandCode = plLine.BrandCode,
                    BrandName = brandName,
                    Commodity = commodity,
                    Facing = plLine.Facing,
                    BackStock = plLine.BackStock,
                    CurrentSoh = liveSoh,
                    Deficit = 0,
                    RequestedItemCode = "N/A",
                    AllocatedItemCode = "N/A",
                    AllocatedItemName = "Target met by SOH",
                    AllocatedQty = 0,
                    MatchType = MatchType.Exact,
                    Mrp = 0m,
                    MatchReason = "Target met by existing SOH",
                    InitialGap = 0,
                    RemainingGap = 0,
                    InitialWhStock = 0,
                    RemainingWhStock = 0
                });
                continue;
            }

            int deficitQty = (int)Math.Ceiling(deficit);

            // Filter warehouse candidates: strictly same brand and same commodity
            var candidates = catalog.Values
                .Where(c => string.Equals(c.Brand, brandName, StringComparison.OrdinalIgnoreCase) && c.Commodity == commodity)
                .ToList();

            var topSalesItems = recentSalesRank.TryGetValue(storeStockKey, out var ts) ? ts : new List<string>();
            var histSalesItems = histSalesRank.TryGetValue(storeStockKey, out var hs) ? hs : new List<string>();
            var skusOnShelf = stockItems.Select(s => s.ItemCode).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Determine Target / Reference Attributes & MRP for similarity calculation
            var (targetAttrs, targetMrp, requestedItemCode) = DetermineReferenceAttributes(
                topSalesItems, histSalesItems, stockItems, catalog);

            int allocatedForLine = 0;

            while (allocatedForLine < deficitQty)
            {
                // Pass 1: Prefer unique SKUs (not on shelf, not already in shipment)
                var bestCandidateEval = FindBestCandidate(
                    candidates,
                    allowDuplicates: false,
                    storeName,
                    brandName,
                    targetAttrs,
                    targetMrp,
                    remainingWhStock,
                    storeSkuCounts,
                    storeModelColors,
                    storeBrandAllocatedSkus,
                    skusOnShelf,
                    topSalesItems,
                    histSalesItems);

                // Pass 2: Fall back to allowing duplicates if needed
                if (bestCandidateEval == null)
                {
                    bestCandidateEval = FindBestCandidate(
                        candidates,
                        allowDuplicates: true,
                        storeName,
                        brandName,
                        targetAttrs,
                        targetMrp,
                        remainingWhStock,
                        storeSkuCounts,
                        storeModelColors,
                        storeBrandAllocatedSkus,
                        skusOnShelf,
                        topSalesItems,
                        histSalesItems);
                }

                // If warehouse cannot fulfill any further units, emit UNRESOLVED gap
                if (bestCandidateEval == null)
                {
                    int remainingGap = deficitQty - allocatedForLine;
                    allocations.Add(new AllocationLine
                    {
                        GapId = gapIndex,
                        StoreName = storeName,
                        StoreCategory = plLine.StoreCategory,
                        StoreType = plLine.StoreType,
                        BrandCode = plLine.BrandCode,
                        BrandName = brandName,
                        Commodity = commodity,
                        Facing = plLine.Facing,
                        BackStock = plLine.BackStock,
                        CurrentSoh = liveSoh,
                        Deficit = deficit,
                        RequestedItemCode = requestedItemCode,
                        AllocatedQty = 0,
                        MatchType = MatchType.Unresolved,
                        MatchReason = "No stock available in Corporate Office",
                        InitialGap = remainingGap,
                        RemainingGap = remainingGap,
                        InitialWhStock = 0,
                        RemainingWhStock = 0
                    });
                    break;
                }

                var cand = bestCandidateEval.Candidate;
                var candCode = cand.ItemCode;

                int initialWh = remainingWhStock.TryGetValue(candCode, out var whQty) ? whQty : 0;
                int currentStoreSkuCount = GetStoreSkuCount(storeSkuCounts, storeName, candCode);
                int maxCanAllocBySku = MaxUnitsPerSku - currentStoreSkuCount;

                int qtyToAlloc = Math.Min(initialWh, Math.Min(deficitQty - allocatedForLine, maxCanAllocBySku));

                // 85% Assortment Breadth Guardrail
                var storeBrandKey = (storeName, brandName);
                var allocatedSkusSet = storeBrandAllocatedSkus.TryGetValue(storeBrandKey, out var set)
                    ? set
                    : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                bool isNewSku = !allocatedSkusSet.Contains(candCode);
                int currentUnique = allocatedSkusSet.Count;
                int currentTotal = storeBrandAllocatedQtyTotal.TryGetValue(storeBrandKey, out var tot) ? tot : 0;

                while (qtyToAlloc > 0)
                {
                    int projUnique = currentUnique + (isNewSku ? 1 : 0);
                    int projTotal = currentTotal + qtyToAlloc;
                    if (projTotal == 0 || ((double)projUnique / projTotal) >= MinBreadthThreshold)
                    {
                        break;
                    }
                    qtyToAlloc--; // Shrink batch to protect breadth
                }

                if (qtyToAlloc <= 0)
                {
                    break; // Cannot allocate without violating 85% breadth cap
                }

                // Drain FIFO queues: oldest batch first
                int unitsNeeded = qtyToAlloc;
                var allocatedBatches = new List<(string Barcode, int Qty)>();
                if (fifoQueues.TryGetValue(candCode, out var queue))
                {
                    while (unitsNeeded > 0 && queue.Count > 0)
                    {
                        var head = queue[0];
                        int take = Math.Min(unitsNeeded, head.Quantity);
                        head.Quantity -= take;
                        unitsNeeded -= take;
                        allocatedBatches.Add((head.Barcode, take));

                        if (head.Quantity == 0)
                        {
                            queue.RemoveAt(0);
                        }
                    }
                }

                int actualAllocQty = qtyToAlloc - unitsNeeded;
                if (actualAllocQty <= 0)
                {
                    break;
                }

                // Update state tracking
                if (!storeBrandAllocatedSkus.TryGetValue(storeBrandKey, out var sbSet))
                {
                    sbSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    storeBrandAllocatedSkus[storeBrandKey] = sbSet;
                }
                sbSet.Add(candCode);
                storeBrandAllocatedQtyTotal[storeBrandKey] = currentTotal + actualAllocQty;

                SetStoreSkuCount(storeSkuCounts, storeName, candCode, currentStoreSkuCount + actualAllocQty);

                remainingWhStock[candCode] -= actualAllocQty;
                allocatedForLine += actualAllocQty;

                int remainingWh = remainingWhStock[candCode];
                int lineRemainingGap = deficitQty - allocatedForLine;

                // Color limit tracking and soft warning
                var model = cand.Attributes.Model;
                var color = cand.Attributes.Color;
                bool colorWarning = false;

                if (!string.IsNullOrEmpty(model) && !string.IsNullOrEmpty(color))
                {
                    if (!storeModelColors.TryGetValue(storeName, out var modelMap))
                    {
                        modelMap = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
                        storeModelColors[storeName] = modelMap;
                    }

                    if (!modelMap.TryGetValue(model, out var colorSet))
                    {
                        colorSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        modelMap[model] = colorSet;
                    }

                    colorSet.Add(color);
                    if (colorSet.Count > 2)
                    {
                        colorWarning = true;
                    }
                }

                var actualRequested = bestCandidateEval.MatchType == MatchType.Exact
                    ? candCode
                    : requestedItemCode;

                int candStoreSoh = stockItems.Where(s => string.Equals(s.ItemCode, candCode, StringComparison.OrdinalIgnoreCase)).Sum(s => s.Qty);

                foreach (var (barcode, bQty) in allocatedBatches)
                {
                    allocations.Add(new AllocationLine
                    {
                        GapId = gapIndex,
                        StoreName = storeName,
                        StoreCategory = plLine.StoreCategory,
                        StoreType = plLine.StoreType,
                        BrandCode = plLine.BrandCode,
                        BrandName = brandName,
                        Commodity = commodity,
                        Facing = plLine.Facing,
                        BackStock = plLine.BackStock,
                        CurrentSoh = liveSoh,
                        Deficit = deficit,
                        RequestedItemCode = actualRequested,
                        AllocatedItemCode = cand.ItemCode,
                        AllocatedItemName = cand.ItemName,
                        AllocatedBarcode = barcode,
                        AllocatedQty = bQty,
                        MatchType = bestCandidateEval.MatchType,
                        Mrp = cand.Mrp,
                        RequestedAttributes = targetAttrs,
                        AllocatedAttributes = cand.Attributes,
                        MatchReason = bestCandidateEval.Reason,
                        ColorLimitWarning = colorWarning,
                        SimilarityScore = bestCandidateEval.Score,
                        InitialWhStock = initialWh,
                        RemainingWhStock = remainingWh,
                        InitialGap = deficitQty,
                        RemainingGap = lineRemainingGap,
                        StoreSkuSohAfter = candStoreSoh + actualAllocQty
                    });
                }
            }
        }

        return allocations;
    }

    private CandidateEvaluation? FindBestCandidate(
        List<WarehouseCatalogItem> candidates,
        bool allowDuplicates,
        string storeName,
        string brandName,
        ItemAttributes targetAttrs,
        decimal targetMrp,
        Dictionary<string, int> remainingWhStock,
        Dictionary<string, Dictionary<string, int>> storeSkuCounts,
        Dictionary<string, Dictionary<string, HashSet<string>>> storeModelColors,
        Dictionary<(string Store, string Brand), HashSet<string>> storeBrandAllocatedSkus,
        HashSet<string> skusOnShelf,
        List<string> topSalesItems,
        List<string> histSalesItems)
    {
        WarehouseCatalogItem? bestCand = null;
        double bestScore = -1.0;
        MatchType matchType = MatchType.Unresolved;
        string reason = "No stock available";

        var storeBrandKey = (storeName, brandName);
        var inShipmentSkus = storeBrandAllocatedSkus.TryGetValue(storeBrandKey, out var set)
            ? set
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var cand in candidates)
        {
            var candCode = cand.ItemCode;
            if (!remainingWhStock.TryGetValue(candCode, out var remStock) || remStock <= 0)
            {
                continue;
            }

            // Guardrail 1: Max 2 units per SKU per store
            int countAllocated = GetStoreSkuCount(storeSkuCounts, storeName, candCode);
            if (countAllocated >= MaxUnitsPerSku)
            {
                continue;
            }

            // Guardrail 2: Max 3 colors per model per store (strict block on 4th new color)
            var candModel = cand.Attributes.Model;
            var candColor = cand.Attributes.Color;
            if (!string.IsNullOrEmpty(candModel) && !string.IsNullOrEmpty(candColor))
            {
                if (storeModelColors.TryGetValue(storeName, out var modelMap) &&
                    modelMap.TryGetValue(candModel, out var existingColors))
                {
                    if (!existingColors.Contains(candColor) && existingColors.Count >= MaxColorsPerModel)
                    {
                        continue;
                    }
                }
            }

            // Guardrail 3: Assortment Uniqueness Pass Filter
            bool isDuplicate = skusOnShelf.Contains(candCode) || inShipmentSkus.Contains(candCode);
            if (!allowDuplicates && isDuplicate)
            {
                continue;
            }

            // Similarity and Scoring
            double simScore = Scorer.CalculateSimilarity(targetAttrs, cand.Attributes);

            bool priceMatch = true;
            if (targetMrp > 0)
            {
                var priceDiff = (double)(Math.Abs(cand.Mrp - targetMrp) / targetMrp);
                if (priceDiff > PriceBandTolerance)
                {
                    priceMatch = false;
                    simScore -= 50.0; // Penalty for exceeding ±20% price band
                }
            }

            bool isExactTier1 = topSalesItems.Contains(candCode, StringComparer.OrdinalIgnoreCase);
            bool isExactTier2 = histSalesItems.Contains(candCode, StringComparer.OrdinalIgnoreCase);
            bool isOnShelf = skusOnShelf.Contains(candCode);

            if (isExactTier1)
            {
                simScore += 200.0;
            }
            else if (isExactTier2)
            {
                simScore += 150.0;
            }
            else if (isOnShelf)
            {
                simScore += 100.0;
            }

            if (simScore > bestScore)
            {
                bestScore = simScore;
                bestCand = cand;

                if (isExactTier1)
                {
                    matchType = MatchType.Exact;
                    reason = "Tier 1 Exact Match: Recent sales";
                }
                else if (isExactTier2)
                {
                    matchType = MatchType.Exact;
                    reason = "Tier 2 Exact Match: Historical sales";
                }
                else if (isOnShelf)
                {
                    matchType = MatchType.Exact;
                    reason = "Tier 2 Exact Match: Matching store shelf stock";
                }
                else if (priceMatch)
                {
                    matchType = MatchType.Similar;
                    reason = $"Tier 3 Substitute: Same Brand ({brandName}), shape ({cand.Attributes.Shape}), material ({cand.Attributes.Material})";
                }
                else
                {
                    matchType = MatchType.Substitute;
                    reason = "Substitute fallback (price band exceeded)";
                }
            }
        }

        return bestCand != null
            ? new CandidateEvaluation(bestCand, bestScore, matchType, reason)
            : null;
    }

    private (ItemAttributes Attrs, decimal Mrp, string ItemCode) DetermineReferenceAttributes(
        List<string> topSalesItems,
        List<string> histSalesItems,
        List<(string ItemCode, int Qty)> stockItems,
        Dictionary<string, WarehouseCatalogItem> catalog)
    {
        if (topSalesItems.Count > 0 && catalog.TryGetValue(topSalesItems[0], out var topCat))
        {
            return (topCat.Attributes, topCat.Mrp, topCat.ItemCode);
        }

        if (histSalesItems.Count > 0 && catalog.TryGetValue(histSalesItems[0], out var histCat))
        {
            return (histCat.Attributes, histCat.Mrp, histCat.ItemCode);
        }

        if (stockItems.Count > 0 && catalog.TryGetValue(stockItems[0].ItemCode, out var shelfCat))
        {
            return (shelfCat.Attributes, shelfCat.Mrp, shelfCat.ItemCode);
        }

        return (new ItemAttributes(), 0m, "N/A");
    }

    private static int GetStoreSkuCount(Dictionary<string, Dictionary<string, int>> storeSkuCounts, string store, string sku)
    {
        return storeSkuCounts.TryGetValue(store, out var map) && map.TryGetValue(sku, out var cnt) ? cnt : 0;
    }

    private static void SetStoreSkuCount(Dictionary<string, Dictionary<string, int>> storeSkuCounts, string store, string sku, int count)
    {
        if (!storeSkuCounts.TryGetValue(store, out var map))
        {
            map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            storeSkuCounts[store] = map;
        }
        map[sku] = count;
    }
}
