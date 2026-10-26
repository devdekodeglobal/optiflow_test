namespace DEKODE.OptiFlow.Domain.BusinessServices.Engine;

public class WarehouseBatchNode
{
    public required string Barcode { get; init; }
    public required int Quantity { get; set; }
    public required int StockAgeInDays { get; init; }
}
