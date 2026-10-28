using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

/// <summary>
/// Business Service interface orchestrating planogram ingestion, validation, and lifecycle queries.
/// </summary>
public interface IPlanogramService
{
    Task<Planogram> ImportFromCsvStreamAsync(Stream csvStream, string planogramName, CancellationToken cancellationToken = default);
    Task<Planogram> ImportFromCsvFileAsync(string filePath, string planogramName, CancellationToken cancellationToken = default);
    Task<Planogram?> GetPlanogramByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Planogram?> GetActivePlanogramAsync(CancellationToken cancellationToken = default);
    Task<DEKODE.OptiFlow.Domain.Interfaces.Pagination.PagedResult<Planogram>> GetPagedPlanogramsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
