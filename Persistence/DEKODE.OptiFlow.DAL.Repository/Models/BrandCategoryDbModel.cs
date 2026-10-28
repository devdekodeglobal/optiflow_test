namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class BrandCategoryDbModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<BrandDbModel> Brands { get; set; } = new List<BrandDbModel>();
}

