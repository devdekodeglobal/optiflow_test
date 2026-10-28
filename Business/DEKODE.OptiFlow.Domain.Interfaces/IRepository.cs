using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces.Pagination;
using System.Linq.Expressions;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : EntityBase
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<PagedResult<TEntity>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
