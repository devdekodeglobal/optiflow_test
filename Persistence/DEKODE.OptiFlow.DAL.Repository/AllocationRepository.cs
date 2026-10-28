using DEKODE.OptiFlow.DAL.Repository.Mapping;
using DEKODE.OptiFlow.DAL.Repository.Models;
using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;
using DEKODE.OptiFlow.Domain.Interfaces.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DEKODE.OptiFlow.DAL.Repository;

public class AllocationRepository : IAllocationRepository
{
    private readonly OptiFlowDbContext _context;

    public AllocationRepository(OptiFlowDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Allocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.Allocations.FindAsync(new object[] { id }, cancellationToken);
        return model?.ToDomain();
    }

    public async Task<Allocation?> GetWithLinesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.Allocations
            .Include(a => a.Lines)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return model?.ToDomain();
    }

    public async Task<IReadOnlyList<Allocation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var models = await _context.Allocations
            .Include(a => a.Lines)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return models.Select(m => m.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Allocation>> FindAsync(Expression<Func<Allocation, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var all = await GetAllAsync(cancellationToken);
        return all.AsQueryable().Where(predicate).ToList();
    }

    public async Task<PagedResult<Allocation>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Allocation, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Allocations.Include(a => a.Lines).AsNoTracking();
        int totalCount = await query.CountAsync(cancellationToken);

        var models = await query
            .OrderByDescending(a => a.RunDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var domainItems = models.Select(m => m.ToDomain()).ToList();
        return new PagedResult<Allocation>(domainItems, totalCount, pageNumber, pageSize);
    }

    public async Task AddAsync(Allocation entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var model = entity.ToDbModel();
        await _context.Allocations.AddAsync(model, cancellationToken);
        // Sync generated ID back to domain entity
        entity.Id = model.Id;
    }

    public async Task AddRangeAsync(IEnumerable<Allocation> entities, CancellationToken cancellationToken = default)
    {
        var models = entities.Select(e => e.ToDbModel()).ToList();
        await _context.Allocations.AddRangeAsync(models, cancellationToken);
    }

    public void Update(Allocation entity)
    {
        var model = entity.ToDbModel();
        _context.Allocations.Update(model);
    }

    public void Remove(Allocation entity)
    {
        var model = _context.Allocations.Find(entity.Id);
        if (model != null)
        {
            _context.Allocations.Remove(model);
        }
    }
}
