using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces.Parsing;

/// <summary>
/// Domain parsing utility responsible purely for parsing, coercing, and validating
/// raw merchandising CSV streams/files into a pure in-memory Planogram domain aggregate.
/// (Interface Segregation: Separates text stream parsing from database/business workflows).
/// </summary>
public interface IPlanogramCsvParser
{
    Task<Planogram> ParseFromCsvStreamAsync(Stream csvStream, string planogramName, CancellationToken cancellationToken = default);
    Task<Planogram> ParseFromCsvFileAsync(string filePath, string planogramName, CancellationToken cancellationToken = default);
}
