using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DEKODE.OptiFlow.DAL.Repository;

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
