namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class FacilityDbModel : DbModelBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int FacilityTypeId { get; set; }
    public string? Tier { get; set; }
    public string? Region { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual FacilityTypeDbModel FacilityType { get; set; } = null!;
    public virtual ICollection<InventoryStockDbModel> InventoryStocks { get; set; } = new List<InventoryStockDbModel>();
    public virtual ICollection<SalesRecordDbModel> SalesRecords { get; set; } = new List<SalesRecordDbModel>();
    public virtual ICollection<PlanogramDbModel> Planograms { get; set; } = new List<PlanogramDbModel>();
    public virtual ICollection<AllocationDbModel> SourcedAllocations { get; set; } = new List<AllocationDbModel>();
    public virtual ICollection<AllocationLineDbModel> ReceivedAllocationLines { get; set; } = new List<AllocationLineDbModel>();
}

