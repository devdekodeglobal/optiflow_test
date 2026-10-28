using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using DEKODE.OptiFlow.DTO.Allocation;
using DEKODE.OptiFlow.DTO.Planogram;

namespace DEKODE.OptiFlow.WebAPI.Mapping;

public static class MappingExtensions
{
    public static AllocationExecutionContext ToExecutionContext(this AllocationExecutionRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var lines = request.PlanogramLines.Select(p => new PlanogramLine
        {
            StoreName = p.StoreName,
            BrandName = p.BrandName,
            BrandCode = p.BrandCode,
            StoreCategory = p.StoreCategory,
            CategoryTier = StoreCategoryExtensions.ParseTier(p.StoreCategory),
            StoreType = ParseStoreType(p.StoreType),
            Commodity = ParseCommodity(p.Commodity),
            Facing = p.Facing,
            BackStock = p.BackStock
        }).ToList();

        var whBatches = request.WarehouseBatches.Select(w => new WarehouseStockBatch
        {
            Barcode = w.Barcode,
            ItemCode = w.ItemCode,
            ItemName = w.ItemName,
            ItemCategory = w.ItemCategory,
            Facility = w.Facility,
            BatchStock = w.BatchStock,
            Mrp = w.Mrp,
            StockAgeInDays = w.StockAgeInDays
        }).ToList();

        var storeStocks = request.StoreStock.Select(s => new StoreStock
        {
            Facility = s.Facility,
            ItemCode = s.ItemCode,
            BatchStock = s.BatchStock
        }).ToList();

        var sales = request.SalesRecords.Select(r => new SalesRecord
        {
            FacilityName = r.FacilityName,
            ItemCode = r.ItemCode,
            Quantity = r.Quantity,
            OrderDate = r.OrderDate
        }).ToList();

        var storeLists = request.StrategyStoreLists
            .ToDictionary(k => k.Key, v => (IReadOnlyList<string>)v.Value);

        return new AllocationExecutionContext
        {
            PlanogramLines = lines,
            WarehouseBatches = whBatches,
            StoreStock = storeStocks,
            SalesRecords = sales,
            StrategyStoreLists = storeLists,
            ActiveCategories = request.ActiveCategories,
            SalesLookbackDays = request.SalesLookbackDays,
            StoreNameMapping = request.StoreNameMapping
        };
    }

    public static AllocationResultDto ToDto(this Allocation allocation, bool isPersisted = false)
    {
        ArgumentNullException.ThrowIfNull(allocation);

        return new AllocationResultDto
        {
            Id = allocation.Id != Guid.Empty ? allocation.Id : null,
            RunDate = allocation.RunDate,
            RunBy = allocation.RunBy,
            IsPersisted = isPersisted || allocation.Id != Guid.Empty,
            Summary = allocation.Summary?.ToDto(),
            Lines = allocation.Lines.Select(l => l.ToDto()).ToList()
        };
    }

    public static AllocationSummaryDto ToDto(this AllocationSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        var tierDict = summary.TierFulfillment.ToDictionary(
            k => k.Key,
            v => new TierFulfillmentDto
            {
                Target = v.Value.Target,
                Filled = v.Value.Filled,
                Pct = v.Value.Pct
            });

        var brandStats = summary.BrandStats.Select(b => new BrandMetricsDto
        {
            BrandName = b.BrandName,
            FilledUnits = b.FilledUnits,
            RetailValue = b.RetailValue,
            ExactCount = b.ExactCount,
            SimilarCount = b.SimilarCount,
            SubstituteCount = b.SubstituteCount,
            UnresolvedCount = b.UnresolvedCount
        }).ToList();

        return new AllocationSummaryDto
        {
            TotalStores = summary.TotalStores,
            TotalDeficitLines = summary.TotalDeficitLines,
            ExactMatches = summary.ExactMatches,
            SimilarMatches = summary.SimilarMatches,
            BrandFallbacks = summary.BrandFallbacks,
            Unresolved = summary.Unresolved,
            TotalItemsAllocated = summary.TotalItemsAllocated,
            StoresWithDeficits = summary.StoresWithDeficits,
            BrandsProcessed = summary.BrandsProcessed,
            TotalTargetDeficit = summary.TotalTargetDeficit,
            TotalSoh = summary.TotalSoh,
            TotalRetailValue = summary.TotalRetailValue,
            TierFulfillment = tierDict,
            BrandStats = brandStats
        };
    }

    public static AllocationLineResultDto ToDto(this AllocationLine line)
    {
        return new AllocationLineResultDto
        {
            GapId = line.GapId,
            StoreName = line.StoreName,
            StoreCategory = line.StoreCategory,
            BrandName = line.BrandName,
            Commodity = line.Commodity.ToString(),
            Facing = line.Facing,
            BackStock = line.BackStock,
            CurrentSoh = line.CurrentSoh,
            Deficit = line.Deficit,
            AllocatedItemCode = line.AllocatedItemCode,
            AllocatedItemName = line.AllocatedItemName,
            AllocatedBarcode = line.AllocatedBarcode,
            AllocatedQty = line.AllocatedQty,
            MatchType = line.MatchType.ToString(),
            Mrp = line.Mrp,
            MatchReason = line.MatchReason,
            ColorLimitWarning = line.ColorLimitWarning,
            SimilarityScore = line.SimilarityScore,
            StoreSkuSohAfter = line.StoreSkuSohAfter
        };
    }

    public static PlanogramDto ToDto(this DEKODE.OptiFlow.Domain.Entities.Planogram planogram)
    {
        return new PlanogramDto
        {
            Id = planogram.Id,
            Name = planogram.Name,
            SeasonOrMonth = planogram.SeasonOrMonth,
            EffectiveDate = planogram.EffectiveDate,
            IsActive = planogram.IsActive,
            TotalLines = planogram.Lines.Count,
            Lines = planogram.Lines.Select(l => new PlanogramLineDto
            {
                Id = l.Id,
                StoreBrandProductTypeKey = l.StoreBrandProductTypeKey,
                StoreName = l.StoreName,
                BrandName = l.BrandName,
                BrandCode = l.BrandCode,
                Commodity = l.Commodity.ToString(),
                StoreCategory = l.StoreCategory,
                StoreType = l.StoreType.ToString(),
                DispatchPlan = l.DispatchPlan.ToString(),
                Facing = l.Facing,
                BackStock = l.BackStock,
                TargetCapacity = l.TargetCapacity,
                Soh = l.Soh,
                SkuCount = l.SkuCount,
                AvgMonthlySale = l.AvgMonthlySale
            }).ToList()
        };
    }

    private static CommodityType ParseCommodity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return CommodityType.Frame;
        return value.Contains("sunglass", StringComparison.OrdinalIgnoreCase)
            ? CommodityType.Sunglass
            : CommodityType.Frame;
    }

    private static StoreType ParseStoreType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return StoreType.SIS;
        if (value.Contains("SIS", StringComparison.OrdinalIgnoreCase)) return StoreType.SIS;
        if (value.Contains("EBO", StringComparison.OrdinalIgnoreCase)) return StoreType.EBO;
        if (value.Contains("OPTICAL", StringComparison.OrdinalIgnoreCase)) return StoreType.Optical;
        return StoreType.Other;
    }
}
