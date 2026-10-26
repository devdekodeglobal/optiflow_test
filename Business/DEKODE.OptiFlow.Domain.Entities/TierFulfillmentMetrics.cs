namespace DEKODE.OptiFlow.Domain.Entities;

public class TierFulfillmentMetrics
{
    public int Target { get; set; }
    public int Filled { get; set; }
    public double Pct => Target > 0 ? Math.Round((double)Filled / Target * 100, 2) : 0.0;
}
