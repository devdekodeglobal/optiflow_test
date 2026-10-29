namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class RoleDbModel : DbModelBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<UserDbModel> Users { get; set; } = new List<UserDbModel>();
}

