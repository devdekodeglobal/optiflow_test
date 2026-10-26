namespace DEKODE.OptiFlow.Domain.BusinessServices.Engine;

public record CandidateEvaluation(
    WarehouseCatalogItem Candidate,
    double Score,
    DEKODE.OptiFlow.Domain.Entities.Enums.MatchType MatchType,
    string Reason
);
