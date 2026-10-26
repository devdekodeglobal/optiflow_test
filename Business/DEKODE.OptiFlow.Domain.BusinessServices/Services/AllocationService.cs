using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Services;

/// <summary>
/// Domain Business Service orchestrating the allocation lifecycle.
/// Follows DDD principles, keeping business services thin and delegating mathematical computation to the engine.
/// </summary>
public class AllocationService : IAllocationService
{
    private readonly IAllocationEngine _allocationEngine;

    public AllocationService(IAllocationEngine allocationEngine)
    {
        _allocationEngine = allocationEngine ?? throw new ArgumentNullException(nameof(allocationEngine));
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
}
