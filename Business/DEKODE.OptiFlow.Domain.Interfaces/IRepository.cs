using DEKODE.OptiFlow.Domain.Entities;
using System.Linq.Expressions;

namespace DEKODE.OptiFlow.Domain.Interfaces;

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
