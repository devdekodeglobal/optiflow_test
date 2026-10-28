using DEKODE.OptiFlow.DAL.Repository.Mapping;
using DEKODE.OptiFlow.DAL.Repository.Models;
using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;
using DEKODE.OptiFlow.Domain.Interfaces.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DEKODE.OptiFlow.DAL.Repository;

public class PlanogramRepository : IPlanogramRepository
{
    private readonly OptiFlowDbContext _context;

    public PlanogramRepository(OptiFlowDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Planogram?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.Planograms.FindAsync(new object[] { id }, cancellationToken);
        return model?.ToDomain();
    }

    public async Task<Planogram?> GetWithLinesByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _context.Planograms
            .Include(p => p.Lines)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        return model?.ToDomain();
    }

    public async Task<Planogram?> GetActivePlanogramAsync(CancellationToken cancellationToken = default)
    {
        var model = await _context.Planograms
            .Include(p => p.Lines)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        return model?.ToDomain();
    }

    public async Task<IReadOnlyList<Planogram>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var models = await _context.Planograms
            .Include(p => p.Lines)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return models.Select(m => m.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Planogram>> FindAsync(Expression<Func<Planogram, bool>> predicate, CancellationToken cancellationToken = default)
    {
        // Execute and filter in-memory for custom predicates
        var all = await GetAllAsync(cancellationToken);
        return all.AsQueryable().Where(predicate).ToList();
    }

    public async Task<PagedResult<Planogram>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Planogram, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _context.Planograms.Include(p => p.Lines).AsNoTracking();
        int totalCount = await query.CountAsync(cancellationToken);

        var models = await query
            .OrderByDescending(p => p.EffectiveDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var domainItems = models.Select(m => m.ToDomain()).ToList();
        return new PagedResult<Planogram>(domainItems, totalCount, pageNumber, pageSize);
    }

    public async Task<IReadOnlyList<PlanogramLine>> GetLinesByStoreAsync(string storeName, CancellationToken cancellationToken = default)
    {
        var models = await _context.PlanogramLines
            .Where(l => l.StoreName == storeName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return models.Select(m => m.ToDomain()).ToList();
    }

    public async Task AddAsync(Planogram entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var model = entity.ToDbModel();
        await _context.Planograms.AddAsync(model, cancellationToken);
        // Sync generated ID back after save
        entity.Id = model.Id;
    }

    public async Task AddRangeAsync(IEnumerable<Planogram> entities, CancellationToken cancellationToken = default)
    {
        var models = entities.Select(e => e.ToDbModel()).ToList();
        await _context.Planograms.AddRangeAsync(models, cancellationToken);
    }

    public void Update(Planogram entity)
    {
        var model = entity.ToDbModel();
        _context.Planograms.Update(model);
    }

    public void Remove(Planogram entity)
    {
        var model = _context.Planograms.Find(entity.Id);
        if (model != null)
        {
            _context.Planograms.Remove(model);
        }
    }
}
