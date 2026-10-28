namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class FacilityTypeDbModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<FacilityDbModel> Facilities { get; set; } = new List<FacilityDbModel>();
}

