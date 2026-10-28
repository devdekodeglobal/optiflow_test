namespace DEKODE.OptiFlow.DAL.Repository.Models;

public class SalesRecordDbModel : DbModelBase
{
    public Guid FacilityId { get; set; }
    public Guid ProductItemId { get; set; }
    public DateTime TransactionDate { get; set; }
    public int Quantity { get; set; }
    public decimal UnitRetailPrice { get; set; }
    public decimal NetAmount { get; set; }
    public string? FinancialYear { get; set; }
    public string? PeriodMonth { get; set; }
    public string Status { get; set; } = "Completed";

    public virtual FacilityDbModel Facility { get; set; } = null!;
    public virtual ProductItemDbModel ProductItem { get; set; } = null!;
}

