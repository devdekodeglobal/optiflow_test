using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Entities.Enums;

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
