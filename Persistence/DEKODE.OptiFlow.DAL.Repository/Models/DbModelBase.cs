namespace DEKODE.OptiFlow.DAL.Repository.Models;

public abstract class DbModelBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public byte[]? ConcurrencyToken { get; set; }
}
