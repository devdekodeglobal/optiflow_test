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

    public AllocationCalculationResult CalculateAllocationWithResult(AllocationExecutionContext context)
    {
        if (context == null)
            return AllocationCalculationResult.Failure("Execution context cannot be null.");

        if (context.PlanogramLines == null || context.PlanogramLines.Count == 0)
            return AllocationCalculationResult.Failure("Execution context must contain at least one planogram line.");

        var warnings = new List<string>();

        if (context.WarehouseBatches == null || context.WarehouseBatches.Count == 0)
            warnings.Add("Warehouse batches list is empty; no items could be dispatched.");

        try
        {
            var allocation = CalculateAllocation(context);
            return AllocationCalculationResult.Success(allocation, warnings);
        }
        catch (Exception ex)
        {
            return AllocationCalculationResult.Failure(ex.Message, warnings);
        }
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

    public async Task<Allocation?> GetAllocationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_unitOfWork == null)
            throw new InvalidOperationException("UnitOfWork is not configured for allocation data access.");

        return await _unitOfWork.Allocations.GetWithLinesByIdAsync(id, cancellationToken);
    }
}
