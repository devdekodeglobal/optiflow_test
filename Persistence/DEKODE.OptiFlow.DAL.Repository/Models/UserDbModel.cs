namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class UserDbModel : DbModelBase
{
    public string Username { get; set; } = string.Empty;
    public string NormalizedUsername { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;

    public Guid RoleId { get; set; }
    public virtual RoleDbModel Role { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }

    public virtual ICollection<AllocationDbModel> Allocations { get; set; } = new List<AllocationDbModel>();
    public virtual ICollection<PlanogramDbModel> CreatedPlanograms { get; set; } = new List<PlanogramDbModel>();
}

