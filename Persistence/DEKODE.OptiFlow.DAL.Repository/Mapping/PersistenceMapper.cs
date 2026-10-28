using System.Text.Json;
using DEKODE.OptiFlow.DAL.Repository.Models;
using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;
using MatchType = DEKODE.OptiFlow.Domain.Entities.Enums.MatchType;

namespace DEKODE.OptiFlow.DAL.Repository.Mapping;

public static class PersistenceMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    // --- Planogram Mappings ---
    public static PlanogramDbModel ToDbModel(this Planogram entity)
    {
        return new PlanogramDbModel
        {
            Id = entity.Id,
            Name = entity.Name,
            SeasonOrMonth = entity.SeasonOrMonth,
            EffectiveDate = entity.EffectiveDate,
            IsActive = entity.IsActive,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            Lines = entity.Lines.Select(l => l.ToDbModel(entity.Id)).ToList()
        };
    }

    public static PlanogramLineDbModel ToDbModel(this PlanogramLine entity, Guid planogramId = default)
    {
        return new PlanogramLineDbModel
        {
            Id = entity.Id,
            PlanogramId = planogramId != Guid.Empty ? planogramId : entity.PlanogramId,
            StoreBrandProductTypeKey = entity.StoreBrandProductTypeKey,
            StoreName = entity.StoreName,
            BrandName = entity.BrandName,
            BrandCode = entity.BrandCode,
            Commodity = entity.Commodity.ToString(),
            CommodityType = entity.Commodity.ToString(),
            StoreCategory = entity.StoreCategory,
            CategoryTier = entity.CategoryTier.ToString(),
            StoreType = entity.StoreType.ToString(),
            DispatchPlan = entity.DispatchPlan.ToString(),
            Facing = entity.Facing,
            FacingCapacity = (int)Math.Round(entity.Facing),
            BackStock = entity.BackStock,
            BackStockCapacity = (int)Math.Round(entity.BackStock),
            TargetCapacity = entity.TargetCapacity,
            MinCapacity = (int)Math.Round(entity.Facing),
            Soh = entity.Soh,
            SkuCount = entity.SkuCount,
            AvgMonthlySale = entity.AvgMonthlySale,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    public static Planogram ToDomain(this PlanogramDbModel model)
    {
        var domain = new Planogram
        {
            Id = model.Id,
            Name = model.Name,
            SeasonOrMonth = model.SeasonOrMonth ?? string.Empty,
            EffectiveDate = model.EffectiveDate,
            IsActive = model.IsActive,
            CreatedAtUtc = model.CreatedAtUtc,
            UpdatedAtUtc = model.UpdatedAtUtc,
            Lines = model.Lines?.Select(l => l.ToDomain()).ToList() ?? new()
        };

        foreach (var line in domain.Lines)
        {
            line.Planogram = domain;
        }

        return domain;
    }

    public static PlanogramLine ToDomain(this PlanogramLineDbModel model)
    {
        return new PlanogramLine
        {
            Id = model.Id,
            PlanogramId = model.PlanogramId,
            StoreBrandProductTypeKey = model.StoreBrandProductTypeKey,
            StoreName = model.StoreName,
            BrandName = model.BrandName,
            BrandCode = model.BrandCode,
            Commodity = Enum.TryParse<CommodityType>(model.Commodity, true, out var c) ? c : CommodityType.Frame,
            StoreCategory = model.StoreCategory,
            CategoryTier = Enum.TryParse<StoreCategoryTier>(model.CategoryTier, true, out var ct) ? ct : StoreCategoryTier.B,
            StoreType = Enum.TryParse<StoreType>(model.StoreType, true, out var st) ? st : StoreType.SIS,
            DispatchPlan = Enum.TryParse<DispatchPlan>(model.DispatchPlan, true, out var dp) ? dp : DispatchPlan.Weekly,
            Facing = model.Facing,
            BackStock = model.BackStock,
            Soh = model.Soh,
            SkuCount = model.SkuCount,
            AvgMonthlySale = model.AvgMonthlySale,
            CreatedAtUtc = model.CreatedAtUtc,
            UpdatedAtUtc = model.UpdatedAtUtc
        };
    }

    // --- Allocation Mappings ---
    public static AllocationDbModel ToDbModel(this Allocation entity)
    {
        return new AllocationDbModel
        {
            Id = entity.Id,
            RunDate = entity.RunDate,
            RunBy = entity.RunBy,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            SummaryJson = entity.Summary != null ? JsonSerializer.Serialize(entity.Summary, JsonOptions) : null,
            Lines = entity.Lines.Select(l => l.ToDbModel(entity.Id)).ToList()
        };
    }

    public static AllocationLineDbModel ToDbModel(this AllocationLine entity, Guid allocationId = default)
    {
        return new AllocationLineDbModel
        {
            Id = entity.Id,
            AllocationId = allocationId != Guid.Empty ? allocationId : Guid.Empty,
            GapId = entity.GapId,
            StoreName = entity.StoreName,
            StoreCategory = entity.StoreCategory,
            StoreType = entity.StoreType,
            Region = entity.Region,
            Zone = entity.Zone,
            BrandCode = entity.BrandCode,
            BrandName = entity.BrandName,
            Commodity = entity.Commodity.ToString(),
            Facing = entity.Facing,
            BackStock = entity.BackStock,
            CurrentSoh = entity.CurrentSoh,
            CurrentSohUnits = (int)Math.Round(entity.CurrentSoh),
            Deficit = entity.Deficit,
            InitialGap = entity.InitialGap,
            RemainingGap = entity.RemainingGap,
            RequestedItemCode = entity.RequestedItemCode,
            AllocatedItemCode = entity.AllocatedItemCode,
            AllocatedItemName = entity.AllocatedItemName,
            AllocatedBarcode = entity.AllocatedBarcode,
            AllocatedQty = entity.AllocatedQty,
            AllocatedQuantity = entity.AllocatedQty,
            TargetCapacity = (int)Math.Round(entity.Facing + entity.BackStock),
            MatchType = entity.MatchType.ToString(),
            Mrp = entity.Mrp,
            MatchReason = entity.MatchReason,
            ReasonCode = entity.MatchReason,
            ColorLimitWarning = entity.ColorLimitWarning,
            SimilarityScore = entity.SimilarityScore,
            RequestedAttributesJson = entity.RequestedAttributes != null ? JsonSerializer.Serialize(entity.RequestedAttributes, JsonOptions) : null,
            AllocatedAttributesJson = entity.AllocatedAttributes != null ? JsonSerializer.Serialize(entity.AllocatedAttributes, JsonOptions) : null,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }

    public static Allocation ToDomain(this AllocationDbModel model)
    {
        return new Allocation
        {
            Id = model.Id,
            RunDate = model.RunDate,
            RunBy = model.RunBy,
            CreatedAtUtc = model.CreatedAtUtc,
            UpdatedAtUtc = model.UpdatedAtUtc,
            Summary = !string.IsNullOrWhiteSpace(model.SummaryJson)
                ? JsonSerializer.Deserialize<AllocationSummary>(model.SummaryJson, JsonOptions)
                : null,
            Lines = model.Lines?.Select(l => l.ToDomain()).ToList() ?? new()
        };
    }

    public static AllocationLine ToDomain(this AllocationLineDbModel model)
    {
        return new AllocationLine
        {
            Id = model.Id,
            GapId = model.GapId,
            StoreName = model.StoreName,
            StoreCategory = model.StoreCategory,
            StoreType = model.StoreType,
            Region = model.Region,
            Zone = model.Zone,
            BrandCode = model.BrandCode,
            BrandName = model.BrandName,
            Commodity = Enum.TryParse<CommodityType>(model.Commodity, true, out var c) ? c : CommodityType.Frame,
            Facing = model.Facing,
            BackStock = model.BackStock,
            CurrentSoh = model.CurrentSoh,
            Deficit = model.Deficit,
            InitialGap = model.InitialGap,
            RemainingGap = model.RemainingGap,
            RequestedItemCode = model.RequestedItemCode,
            AllocatedItemCode = model.AllocatedItemCode,
            AllocatedItemName = model.AllocatedItemName,
            AllocatedBarcode = model.AllocatedBarcode,
            AllocatedQty = model.AllocatedQty,
            MatchType = Enum.TryParse<MatchType>(model.MatchType, true, out var m) ? m : MatchType.Unresolved,
            Mrp = model.Mrp,
            MatchReason = model.MatchReason,
            ColorLimitWarning = model.ColorLimitWarning,
            SimilarityScore = model.SimilarityScore,
            RequestedAttributes = !string.IsNullOrWhiteSpace(model.RequestedAttributesJson)
                ? JsonSerializer.Deserialize<ItemAttributes>(model.RequestedAttributesJson, JsonOptions)
                : null,
            AllocatedAttributes = !string.IsNullOrWhiteSpace(model.AllocatedAttributesJson)
                ? JsonSerializer.Deserialize<ItemAttributes>(model.AllocatedAttributesJson, JsonOptions)
                : null,
            CreatedAtUtc = model.CreatedAtUtc,
            UpdatedAtUtc = model.UpdatedAtUtc
        };
    }
}
