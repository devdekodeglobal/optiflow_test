namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class BrandDbModel : DbModelBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BrandCategoryId { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual BrandCategoryDbModel BrandCategory { get; set; } = null!;
    public virtual ICollection<ProductDbModel> Products { get; set; } = new List<ProductDbModel>();
    public virtual ICollection<PlanogramLineDbModel> PlanogramLines { get; set; } = new List<PlanogramLineDbModel>();
}

