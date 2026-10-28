namespace DEKODE.OptiFlow.Domain.Entities.Exceptions;

public class ConcurrencyConflictException : OptiFlowDomainException
{
    public ConcurrencyConflictException(string message, Exception? innerException = null)
        : base(message, innerException!) { }
}
