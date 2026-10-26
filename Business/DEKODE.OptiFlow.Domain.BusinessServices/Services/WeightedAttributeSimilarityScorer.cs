using DEKODE.OptiFlow.Domain.Entities;
using DEKODE.OptiFlow.Domain.Interfaces;

namespace DEKODE.OptiFlow.Domain.BusinessServices.Services;

/// <summary>
/// Merchandising attribute similarity distance calculator.
/// Weights: Shape(30) + Material(25) + FrameType(20) + Color(15) + Gender(10) = 100 max.
/// </summary>
public class WeightedAttributeSimilarityScorer : ISimilarityScorer
{
    public double CalculateSimilarity(ItemAttributes target, ItemAttributes candidate)
    {
        if (target == null || candidate == null)
        {
            return 0.0;
        }

        double score = 0.0;

        if (!string.IsNullOrEmpty(target.Shape) &&
            !string.IsNullOrEmpty(candidate.Shape) &&
            string.Equals(target.Shape, candidate.Shape, StringComparison.OrdinalIgnoreCase))
        {
            score += 30.0;
        }

        if (!string.IsNullOrEmpty(target.Material) &&
            !string.IsNullOrEmpty(candidate.Material) &&
            string.Equals(target.Material, candidate.Material, StringComparison.OrdinalIgnoreCase))
        {
            score += 25.0;
        }

        if (!string.IsNullOrEmpty(target.FrameType) &&
            !string.IsNullOrEmpty(candidate.FrameType) &&
            string.Equals(target.FrameType, candidate.FrameType, StringComparison.OrdinalIgnoreCase))
        {
            score += 20.0;
        }

        if (!string.IsNullOrEmpty(target.Color) &&
            !string.IsNullOrEmpty(candidate.Color) &&
            string.Equals(target.Color, candidate.Color, StringComparison.OrdinalIgnoreCase))
        {
            score += 15.0;
        }

        if (!string.IsNullOrEmpty(target.Gender) &&
            !string.IsNullOrEmpty(candidate.Gender) &&
            string.Equals(target.Gender, candidate.Gender, StringComparison.OrdinalIgnoreCase))
        {
            score += 10.0;
        }

        return score;
    }
}
