using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DEKODE.OptiFlow.DAL.Repository;

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
