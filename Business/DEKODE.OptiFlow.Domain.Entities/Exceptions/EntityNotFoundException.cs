namespace DEKODE.OptiFlow.Domain.Entities.Exceptions;

public class EntityNotFoundException : OptiFlowDomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.") { }
}
