using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IPlanogramRepository : IRepository<Planogram>
{
    Task<Planogram?> GetWithLinesByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Planogram?> GetActivePlanogramAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanogramLine>> GetLinesByStoreAsync(string storeName, CancellationToken cancellationToken = default);
}
