using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DEKODE.OptiFlow.DAL.Repository;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : EntityBase
{
    protected readonly OptiFlowDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(OptiFlowDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.CreatedAtUtc = DateTime.UtcNow;
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.CreatedAtUtc = now;
        }
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        DbSet.Update(entity);
    }

    public virtual void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        DbSet.Remove(entity);
    }
}

public class PlanogramRepository : Repository<Planogram>, IPlanogramRepository
{
    public PlanogramRepository(OptiFlowDbContext context) : base(context) { }

    public async Task<Planogram?> GetWithLinesByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Planogram?> GetActivePlanogramAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Lines)
            .OrderByDescending(p => p.EffectiveDate)
            .FirstOrDefaultAsync(p => p.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<PlanogramLine>> GetLinesByStoreAsync(string storeName, CancellationToken cancellationToken = default)
    {
        return await Context.PlanogramLines
            .AsNoTracking()
            .Where(l => l.StoreName.ToLower() == storeName.ToLower())
            .ToListAsync(cancellationToken);
    }
}

public class AllocationRepository : Repository<Allocation>, IAllocationRepository
{
    public AllocationRepository(OptiFlowDbContext context) : base(context) { }

    public async Task<Allocation?> GetWithLinesByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
