using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IAllocationRepository : IRepository<Allocation>
{
    Task<Allocation?> GetWithLinesByIdAsync(int id, CancellationToken cancellationToken = default);
}
