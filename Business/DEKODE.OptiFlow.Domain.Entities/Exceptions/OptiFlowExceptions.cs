namespace DEKODE.OptiFlow.Domain.Entities.Exceptions;

public class OptiFlowDomainException : Exception
{
    public OptiFlowDomainException(string message) : base(message) { }
    public OptiFlowDomainException(string message, Exception innerException) : base(message, innerException) { }
}

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

public class PlanogramImportException : OptiFlowDomainException
{
    public int RowIndex { get; }

    public PlanogramImportException(string message, int rowIndex, Exception? innerException = null)
        : base($"Row {rowIndex}: {message}", innerException!)
    {
        RowIndex = rowIndex;
    }
}

public class ConcurrencyConflictException : OptiFlowDomainException
{
    public ConcurrencyConflictException(string message, Exception? innerException = null)
        : base(message, innerException!) { }
}

public class EntityNotFoundException : OptiFlowDomainException
{
    public EntityNotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.") { }
}
