using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IAllocationEngine
{
    (IReadOnlyList<AllocationLine> Lines, AllocationSummary Summary) Execute(AllocationExecutionContext context);
}
