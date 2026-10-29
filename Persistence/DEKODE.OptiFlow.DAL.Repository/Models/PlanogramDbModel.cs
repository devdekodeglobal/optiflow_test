namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class PlanogramDbModel : DbModelBase
{
    public Guid? CreatedByUserId { get; set; }
    public Guid FacilityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SeasonOrMonth { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Draft, Archived
    public bool IsActive { get; set; } = true;

    public virtual UserDbModel? CreatedByUser { get; set; }
    public virtual FacilityDbModel? Facility { get; set; }
    public virtual ICollection<PlanogramLineDbModel> Lines { get; set; } = new List<PlanogramLineDbModel>();
}
