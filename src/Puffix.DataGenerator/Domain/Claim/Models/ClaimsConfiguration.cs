namespace DataGenerator.Domain.Claim.Models;

public class ClaimsConfiguration
{
    public long AverageClaimPerDay { get; set; }

    public long DefaultAverageClaimPerDayVariation { get; set; }

    public double PercentageOfNewCustomers { get; set; }

    public double PercentageOfNewFiles { get; set; }

    public int AverageResolutionDelay { get; set; }

    public int DefaultAverageResolutionDelayVariation { get; set; }
}
