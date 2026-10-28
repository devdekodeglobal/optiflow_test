namespace DEKODE.OptiFlow.DTO.Allocation;

/// <summary>
/// Explicit payload for pure execution / calculation (nothing fetched directly from DB).
/// </summary>
public class AllocationExecutionRequestDto
{
    public string RunBy { get; set; } = "API_User";
    public int? SalesLookbackDays { get; set; } = 30;
    public List<string> ActiveCategories { get; set; } = new() { "A++", "A+", "A", "B+", "B", "C" };
    public Dictionary<string, List<string>> StrategyStoreLists { get; set; } = new();
    public Dictionary<string, string> StoreNameMapping { get; set; } = new();

    public List<PlanogramLineInputDto> PlanogramLines { get; set; } = new();
    public List<WarehouseStockBatchInputDto> WarehouseBatches { get; set; } = new();
    public List<StoreStockInputDto> StoreStock { get; set; } = new();
    public List<SalesRecordInputDto> SalesRecords { get; set; } = new();
}
