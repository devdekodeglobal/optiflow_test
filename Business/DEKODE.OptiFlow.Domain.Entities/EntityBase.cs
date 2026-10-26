namespace DEKODE.OptiFlow.Domain.Entities;

public abstract class EntityBase
{
    public int Id { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public byte[]? ConcurrencyToken { get; set; }
}
