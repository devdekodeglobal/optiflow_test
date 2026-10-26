namespace DEKODE.OptiFlow.Domain.Entities;

public record AllocationExecutionContext
{
    public IReadOnlyList<PlanogramLine> PlanogramLines { get; init; } = Array.Empty<PlanogramLine>();
    public IReadOnlyList<WarehouseStockBatch> WarehouseBatches { get; init; } = Array.Empty<WarehouseStockBatch>();
    public IReadOnlyList<StoreStock> StoreStock { get; init; } = Array.Empty<StoreStock>();
    public IReadOnlyList<SalesRecord> SalesRecords { get; init; } = Array.Empty<SalesRecord>();
    public IReadOnlyDictionary<string, IReadOnlyList<string>> StrategyStoreLists { get; init; } = new Dictionary<string, IReadOnlyList<string>>();
    public IReadOnlyList<string> ActiveCategories { get; init; } = new List<string> { "A++", "A+", "A", "B+", "B", "C" };
    public int? SalesLookbackDays { get; init; }
    public IReadOnlyDictionary<string, string> StoreNameMapping { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
