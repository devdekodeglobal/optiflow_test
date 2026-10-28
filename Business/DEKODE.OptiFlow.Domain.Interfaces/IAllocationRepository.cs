using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IAllocationRepository : IRepository<Allocation>
{
    Task<Allocation?> GetWithLinesByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
