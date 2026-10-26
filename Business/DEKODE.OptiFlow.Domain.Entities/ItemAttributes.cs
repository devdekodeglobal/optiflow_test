namespace DEKODE.OptiFlow.Domain.Entities;

public record ItemAttributes
{
    public string Model { get; init; } = string.Empty;
    public string Brand { get; init; } = string.Empty;
    public string Gender { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string Size { get; init; } = string.Empty;
    public string FrameType { get; init; } = string.Empty;
    public string Shape { get; init; } = string.Empty;
    public string Material { get; init; } = string.Empty;
}
