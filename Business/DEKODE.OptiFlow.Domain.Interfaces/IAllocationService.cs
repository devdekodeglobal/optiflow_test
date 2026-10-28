using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IAllocationService
{
    bool RunAllocation();
    Allocation CalculateAllocation(AllocationExecutionContext context);
    AllocationCalculationResult CalculateAllocationWithResult(AllocationExecutionContext context);
    Task<Allocation> RunAndPersistAllocationAsync(AllocationExecutionContext context, CancellationToken cancellationToken = default);
    Task<Allocation?> GetAllocationByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
