namespace DEKODE.OptiFlow.Domain.Entities.Exceptions;

public class OptiFlowDomainException : Exception
{
    public OptiFlowDomainException(string message) : base(message) { }
    public OptiFlowDomainException(string message, Exception innerException) : base(message, innerException) { }
}
