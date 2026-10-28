using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Entities;

/// <summary>
/// Result Pattern model for pure calculation runs.
/// Returns calculated allocation, warnings, and domain validation errors
/// without throwing unhandled exceptions.
/// </summary>
public class AllocationCalculationResult
{
    public bool IsSuccess => Errors.Count == 0 && Allocation != null;
    public Allocation? Allocation { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();

    public static AllocationCalculationResult Success(Allocation allocation, IEnumerable<string>? warnings = null) =>
        new()
        {
            Allocation = allocation,
            Warnings = warnings?.ToList() ?? new List<string>()
        };

    public static AllocationCalculationResult Failure(IEnumerable<string> errors, IEnumerable<string>? warnings = null) =>
        new()
        {
            Errors = errors.ToList(),
            Warnings = warnings?.ToList() ?? new List<string>()
        };

    public static AllocationCalculationResult Failure(string error) =>
        Failure(new[] { error });

    public static AllocationCalculationResult Failure(string error, IEnumerable<string>? warnings) =>
        Failure(new[] { error }, warnings);
}
