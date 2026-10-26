using DEKODE.OptiFlow.Domain.Entities;
using System.Linq.Expressions;

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

public interface IPlanogramImporter
{
    Task<Planogram> ImportFromCsvStreamAsync(Stream csvStream, string planogramName, CancellationToken cancellationToken = default);
    Task<Planogram> ImportFromCsvFileAsync(string filePath, string planogramName, CancellationToken cancellationToken = default);
}

public interface IAllocationService
{
    bool RunAllocation();
    Allocation CalculateAllocation(AllocationExecutionContext context);
    Task<Allocation> RunAndPersistAllocationAsync(AllocationExecutionContext context, CancellationToken cancellationToken = default);
}

// -------------------------------------------------------------
// PERSISTENCE REPOSITORY & UNIT OF WORK INTERFACES
// -------------------------------------------------------------

public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

public interface IPlanogramRepository : IRepository<Planogram>
{
    Task<Planogram?> GetWithLinesByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Planogram?> GetActivePlanogramAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanogramLine>> GetLinesByStoreAsync(string storeName, CancellationToken cancellationToken = default);
}

public interface IAllocationRepository : IRepository<Allocation>
{
    Task<Allocation?> GetWithLinesByIdAsync(int id, CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IPlanogramRepository Planograms { get; }
    IAllocationRepository Allocations { get; }
    IRepository<PlanogramLine> PlanogramLines { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
}
