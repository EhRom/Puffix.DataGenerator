using DataGenerator.Domain.Claim.Models;
using DataGenerator.Domain.Generator.Models;
using Microsoft.Extensions.Configuration;

namespace DataGenerator.Domain.Claim;

public class ClaimService(IConfiguration configuration) : IClaimService
{
    private static readonly TimeOnly startOfDay = new TimeOnly(8, 0);
    private static readonly TimeOnly endOfDay = new TimeOnly(17, 30);

    private static readonly long workedDayDurationInMinutes = Convert.ToInt64(TimeSpan.FromTicks(endOfDay.Ticks - startOfDay.Ticks).TotalMinutes);

    private const long minNumberOfDaysBeforeCustomerIdReuse = 20;
    private const long minNumberOfDaysBeforeFileIdReuse = 60;

    private ICollection<string> customersId = [];
    private ICollection<string> filesId = [];

    private readonly Lazy<ClaimsConfiguration> claimsConfigurationLazy = new(() =>
    {
        return configuration.GetSection(nameof(claimsConfiguration)).Get<ClaimsConfiguration>()!;
    });

    private ClaimsConfiguration claimsConfiguration => claimsConfigurationLazy.Value;

    public IEnumerable<string> GetDataHeaders()
    {
        return ClaimData.GetHeader();
    }

    public void GenerateData(IDataContainer dataContainer, DateOnly currentDate)
    {
        if (!dataContainer.IsHolidayOrWeekend(currentDate))
        {
            long claimPerDay = dataContainer.GetRandomLongValue(claimsConfiguration.AverageClaimPerDay, claimsConfiguration.DefaultAverageClaimPerDayVariation);
            for (long claimId = 0; claimId < claimPerDay; claimId++)
            {
                long dayDelayInMinutes = Math.Abs(dataContainer.GetRandomLongValue(workedDayDurationInMinutes));

                DateTime claimCreationDate = currentDate.ToDateTime(startOfDay.AddMinutes(dayDelayInMinutes));
                DateTime claimResolutionDate = GetRandomResolutionDate(dataContainer, claimsConfiguration, claimCreationDate);
                string customerId = GetRandomCustomerId(dataContainer, claimsConfiguration, claimCreationDate);
                string fileId = GetRandomFileId(dataContainer, claimsConfiguration, claimCreationDate);
                ClaimType claimType = GetRandomClaimType(dataContainer);
                ClaimPriority claimPriority = GetRandomClaimPriority(dataContainer);

                IData generatedData = ClaimData.CreateNew(currentDate, false, string.Empty, customerId, fileId, claimType, claimPriority, claimCreationDate, claimResolutionDate);
                dataContainer.AddData(generatedData);
            }
        }

        customersId = AutoCleanCollection(customersId, 10000, 20);
        filesId = AutoCleanCollection(filesId, 10000, 30);
    }

    public string GetRandomCustomerId(IDataContainer dataContainer, ClaimsConfiguration claimsConfiguration, DateTime claimCreationDate)
    {
        long triggerValue = claimsConfiguration.AverageClaimPerDay * minNumberOfDaysBeforeCustomerIdReuse;

        string customerId;
        double percentage = dataContainer.GetRandomDoubleValue(0.5, 50, 100);
        if (customersId.Count > triggerValue && percentage >= claimsConfiguration.PercentageOfNewCustomers)
        {
            int nextCustomerId = Convert.ToInt32(dataContainer.GetRandomLongValue(customersId.Count));
            customerId = customersId.Skip(nextCustomerId).Take(1).First();
        }
        else
        {
            customerId = Guid.NewGuid().ToString("N");
            customersId.Add(customerId);
        }

        return customerId;
    }

    public string GetRandomFileId(IDataContainer dataContainer, ClaimsConfiguration claimsConfiguration, DateTime claimCreationDate)
    {
        long triggerValue = claimsConfiguration.AverageClaimPerDay * minNumberOfDaysBeforeFileIdReuse;

        string fileId;
        double percentage = dataContainer.GetRandomDoubleValue(0.5, 50, 100);
        if (filesId.Count > triggerValue && percentage >= claimsConfiguration.PercentageOfNewFiles)
        {
            int nextFileId = Convert.ToInt32(dataContainer.GetRandomLongValue(filesId.Count));
            fileId = filesId.Skip(nextFileId).Take(1).First();
        }
        else
        {
            fileId = Guid.NewGuid().ToString("N");
            filesId.Add(fileId);
        }

        return fileId;
    }

    private ClaimType GetRandomClaimType(IDataContainer dataContainer)
    {
        // 70% Issue / 30% InformationRequest
        long percentage = Math.Abs(dataContainer.GetRandomLongValue(100));

        return percentage >= 70 ? ClaimType.InformationRequest : ClaimType.Issue;
    }

    private ClaimPriority GetRandomClaimPriority(IDataContainer dataContainer)
    {
        // 20% Low / 50% Medium / 30% High
        long percentage = dataContainer.GetRandomLongValue(100);

        return percentage >= 70 ? ClaimPriority.High :
                percentage >= 20 ? ClaimPriority.Medium : ClaimPriority.Low;
    }

    private DateTime GetRandomResolutionDate(IDataContainer dataContainer, ClaimsConfiguration claimsConfiguration, DateTime claimCreationDate)
    {
        long resolutionInDay = dataContainer.GetRandomLongValue(claimsConfiguration.AverageResolutionDelay, claimsConfiguration.DefaultAverageResolutionDelayVariation);

        DateTime claimResolutionDate;
        if (resolutionInDay == 0)
        {
            long delayBeforEndOfDayInMinutes = Convert.ToInt64(TimeSpan.FromTicks(endOfDay.Ticks - TimeOnly.FromDateTime(claimCreationDate).Ticks).TotalMinutes);

            long delayInMinutes = Math.Abs(dataContainer.GetRandomLongValue(delayBeforEndOfDayInMinutes));

            claimResolutionDate = claimCreationDate.AddMinutes(delayInMinutes);
        }
        else
        {
            long dayDelayInMinutes = dataContainer.GetRandomLongValue(workedDayDurationInMinutes);
            claimResolutionDate = DateOnly.FromDateTime(claimCreationDate.AddDays(resolutionInDay)).ToDateTime(startOfDay.AddMinutes(dayDelayInMinutes));
        }

        while (dataContainer.IsHolidayOrWeekend(DateOnly.FromDateTime(claimResolutionDate)))
        {
            claimResolutionDate = claimResolutionDate.AddDays(1);
        }

        return claimResolutionDate;
    }

    private static ICollection<string> AutoCleanCollection(ICollection<string> collection, int cleanTreshold, int percentageToClean)
    {
        // At most 50%
        percentageToClean = percentageToClean % 50;

        int collectionSize = collection.Count;

        if (collectionSize > cleanTreshold)
        {
            double countToKeep = Convert.ToDouble(100 - percentageToClean) / 100.0 * Convert.ToDouble(collectionSize);
            int countToKeepInt = Convert.ToInt32(countToKeep);

            collection = collection.Skip(collectionSize - countToKeepInt).Take(countToKeepInt).ToList();
        }

        return collection;
    }
}
