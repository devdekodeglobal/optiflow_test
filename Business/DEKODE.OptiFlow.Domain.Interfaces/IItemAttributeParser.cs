using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface IItemAttributeParser
{
    ItemAttributes Parse(string? itemName);
}
