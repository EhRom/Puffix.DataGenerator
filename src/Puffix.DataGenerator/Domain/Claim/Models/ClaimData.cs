using DataGenerator.Domain.Generator.Models;
using System.Runtime.InteropServices.Marshalling;

namespace DataGenerator.Domain.Claim.Models;

public class ClaimData : Data<ClaimData>, IData
{
    private const string DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss";
    public string CustomerId { get; init; } = string.Empty;

    public string FileId { get; init; } = string.Empty;

    public ClaimType ClaimType { get; init; }

    public ClaimPriority ClaimPriority { get; init; }

    public DateTime ClaimCreateDate { get; init; }

    public DateTime ClaimCloseDate { get; init; }

    public ClaimData(DateOnly date, bool isHoliday, string holidayName, string customerId, string fileId, ClaimType claimType, ClaimPriority claimPriority, DateTime claimCreateDate, DateTime claimCloseDate)
        : base(date, isHoliday, holidayName)
    {
        CustomerId = customerId;
        FileId = fileId;
        ClaimType = claimType;
        ClaimPriority = claimPriority;
        ClaimCreateDate = claimCreateDate;
        ClaimCloseDate = claimCloseDate;
    }

    public static ClaimData CreateNew(DateOnly date, bool isHoliday, string holidayName, string customerId, string fileId, ClaimType claimType, ClaimPriority claimPriority, DateTime claimCreateDate, DateTime claimCloseDate)
    {
        return new ClaimData(date, isHoliday, holidayName, customerId, fileId, claimType, claimPriority, claimCreateDate, claimCloseDate);
    }

    public static IEnumerable<string> GetHeader()
    {
        return [
            nameof(CustomerId),
            nameof(FileId),
            nameof(ClaimType),
            nameof(ClaimPriority),
            nameof(ClaimCreateDate),
            nameof(ClaimCloseDate)
        ];
    }

    public override IEnumerable<string> GetContent()
    {
        return [
            CustomerId,
            FileId,
            ClaimType.ToString(),
            ClaimPriority.ToString(),
            ClaimCreateDate.ToString(DATE_FORMAT),
            ClaimCloseDate.ToString(DATE_FORMAT)
        ];
    }
}