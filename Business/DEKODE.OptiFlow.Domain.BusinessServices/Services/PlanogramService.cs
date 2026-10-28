using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Services;

/// <summary>
/// Domain Business Service orchestrating planogram operations (import and lifecycle queries)
/// with full transactional persistence.
/// </summary>
public class PlanogramService : IPlanogramService
{
    private readonly IPlanogramImporter _importer;
    private readonly IUnitOfWork? _unitOfWork;

    public PlanogramService(IPlanogramImporter importer, IUnitOfWork? unitOfWork = null)
    {
        _importer = importer ?? throw new ArgumentNullException(nameof(importer));
        _unitOfWork = unitOfWork;
    }

    public async Task<Planogram> ImportFromCsvFileAsync(string filePath, string planogramName, CancellationToken cancellationToken = default)
    {
        var planogram = await _importer.ImportFromCsvFileAsync(filePath, planogramName, cancellationToken);
        if (_unitOfWork != null)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.Planograms.AddAsync(planogram, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }, cancellationToken);
        }
        return planogram;
    }

    public async Task<Planogram> ImportFromCsvStreamAsync(Stream csvStream, string planogramName, CancellationToken cancellationToken = default)
    {
        var planogram = await _importer.ImportFromCsvStreamAsync(csvStream, planogramName, cancellationToken);
        if (_unitOfWork != null)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                await _unitOfWork.Planograms.AddAsync(planogram, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }, cancellationToken);
        }
        return planogram;
    }

    public async Task<Planogram?> GetPlanogramByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_unitOfWork == null)
            throw new InvalidOperationException("UnitOfWork is not configured for planogram data access.");

        return await _unitOfWork.Planograms.GetWithLinesByIdAsync(id, cancellationToken);
    }

    public async Task<Planogram?> GetActivePlanogramAsync(CancellationToken cancellationToken = default)
    {
        if (_unitOfWork == null)
            throw new InvalidOperationException("UnitOfWork is not configured for planogram data access.");

        return await _unitOfWork.Planograms.GetActivePlanogramAsync(cancellationToken);
    }

    public async Task<DEKODE.OptiFlow.Domain.Interfaces.Pagination.PagedResult<Planogram>> GetPagedPlanogramsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (_unitOfWork == null)
            throw new InvalidOperationException("UnitOfWork is not configured for planogram data access.");

        return await _unitOfWork.Planograms.GetPagedAsync(pageNumber, pageSize, null, cancellationToken);
    }
}
