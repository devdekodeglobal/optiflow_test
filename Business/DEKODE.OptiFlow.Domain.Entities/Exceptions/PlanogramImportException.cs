namespace DEKODE.OptiFlow.Domain.Entities.Exceptions;

public class PlanogramImportException : OptiFlowDomainException
{
    public int RowIndex { get; }

    public PlanogramImportException(string message, int rowIndex, Exception? innerException = null)
        : base($"Row {rowIndex}: {message}", innerException!)
    {
        RowIndex = rowIndex;
    }
}
