using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IPlanogramImporter
{
    Task<Planogram> ImportFromCsvStreamAsync(Stream csvStream, string planogramName, CancellationToken cancellationToken = default);
    Task<Planogram> ImportFromCsvFileAsync(string filePath, string planogramName, CancellationToken cancellationToken = default);
}
