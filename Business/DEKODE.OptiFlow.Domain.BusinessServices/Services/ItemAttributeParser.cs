using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Services;

/// <summary>
/// Parses raw product description strings into structured domain ItemAttributes.
/// Format: Model - Brand - Gender - Color - Size - FrameType - Shape - Material
/// </summary>
public class ItemAttributeParser : IItemAttributeParser
{
    public ItemAttributes Parse(string? itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return new ItemAttributes();
        }

        var parts = itemName.Split(" - ", StringSplitOptions.TrimEntries);
        if (parts.Length == 0 || string.IsNullOrEmpty(parts[0]))
        {
            return new ItemAttributes();
        }

        return new ItemAttributes
        {
            Model = parts.Length > 0 ? parts[0] : string.Empty,
            Brand = parts.Length > 1 ? parts[1] : string.Empty,
            Gender = parts.Length > 2 ? parts[2] : string.Empty,
            Color = parts.Length > 3 ? parts[3] : string.Empty,
            Size = parts.Length > 4 ? parts[4] : string.Empty,
            FrameType = parts.Length > 5 ? parts[5] : string.Empty,
            Shape = parts.Length > 6 ? parts[6] : string.Empty,
            Material = parts.Length > 7 ? parts[7] : string.Empty
        };
    }
}
