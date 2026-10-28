namespace DEKODE.OptiFlow.Domain.Entities.Exceptions;

public class PlanogramValidationException : OptiFlowDomainException
{
    public string? StoreName { get; }
    public string? BrandName { get; }

    public PlanogramValidationException(string message, string? storeName = null, string? brandName = null)
        : base(message)
    {
        StoreName = storeName;
        BrandName = brandName;
    }
}
