namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class AllocationDbModel : DbModelBase
{
    public Guid? UserId { get; set; }
    public Guid? SourceFacilityId { get; set; } // Central Warehouse / Corporate Office
    public DateTime RunDate { get; set; } = DateTime.UtcNow;
    public string RunBy { get; set; } = "system";
    public string Status { get; set; } = "Calculated"; // Calculated, Approved, Dispatched, Completed
    public int TotalQuantityAllocated { get; set; }
    public string? SummaryJson { get; set; }

    public virtual UserDbModel? User { get; set; }
    public virtual FacilityDbModel? SourceFacility { get; set; }
    public virtual ICollection<AllocationLineDbModel> Lines { get; set; } = new List<AllocationLineDbModel>();
}
