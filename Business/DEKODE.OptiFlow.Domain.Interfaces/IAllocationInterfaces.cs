using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IItemAttributeParser
{
    ItemAttributes Parse(string? itemName);
}

public interface ISimilarityScorer
{
    double CalculateSimilarity(ItemAttributes target, ItemAttributes candidate);
}

public interface IAllocationEngine
{
    (IReadOnlyList<AllocationLine> Lines, AllocationSummary Summary) Execute(AllocationExecutionContext context);
}

public interface IAllocationService
{
    bool RunAllocation();
    Allocation CalculateAllocation(AllocationExecutionContext context);
}
