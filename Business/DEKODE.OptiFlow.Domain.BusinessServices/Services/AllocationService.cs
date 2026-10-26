using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Services;

/// <summary>
/// Domain Business Service orchestrating the allocation lifecycle and persistence with transactions.
/// </summary>
public class AllocationService : IAllocationService
{
    private readonly IAllocationEngine _allocationEngine;
    private readonly IUnitOfWork? _unitOfWork;

    public AllocationService(IAllocationEngine allocationEngine, IUnitOfWork? unitOfWork = null)
    {
        _allocationEngine = allocationEngine ?? throw new ArgumentNullException(nameof(allocationEngine));
        _unitOfWork = unitOfWork;
    }

    public bool RunAllocation()
    {
        return true;
    }

    public Allocation CalculateAllocation(AllocationExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var (lines, summary) = _allocationEngine.Execute(context);

        return new Allocation
        {
            RunDate = DateTime.UtcNow,
            RunBy = "OptiFlow_Engine_net10",
            Lines = lines.ToList(),
            Summary = summary
        };
    }

    public async Task<Allocation> RunAndPersistAllocationAsync(AllocationExecutionContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (_unitOfWork == null)
            throw new InvalidOperationException("UnitOfWork is not configured for persistent allocation runs.");

        var allocation = CalculateAllocation(context);

        // Execute persistence inside an explicit database transaction with Unit of Work
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await _unitOfWork.Allocations.AddAsync(allocation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }, cancellationToken);

        return allocation;
    }
}
