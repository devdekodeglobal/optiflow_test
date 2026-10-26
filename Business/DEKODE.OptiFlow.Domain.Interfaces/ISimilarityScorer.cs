using DEKODE.OptiFlow.Domain.Entities;

namespace DEKODE.OptiFlow.Domain.Interfaces;

public interface ISimilarityScorer
{
    double CalculateSimilarity(ItemAttributes target, ItemAttributes candidate);
}
