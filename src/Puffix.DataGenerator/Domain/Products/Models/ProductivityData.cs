using DataGenerator.Domain.Generator.Models;

namespace DataGenerator.Domain.Products.Models;

public class ProductivityData : Data<ProductivityData>, IData
{
    public string ProductName { get; init; } = string.Empty;

    public double Volume { get; init; }

    public double PeopleTime { get; init; }

    public ProductivityData(DateOnly date, bool isHoliday, string holidayName, string productName, double volume, double peopleTime)
        : base(date, isHoliday, holidayName)
    {
        ProductName = productName;
        Volume = IsHoliday || IsWeekend ? 0d : volume;
        PeopleTime = IsHoliday || IsWeekend ? 0d : peopleTime;
    }

    public static IData CreateNew(DateOnly date, bool isHoliday, string holidayName, string productName, double volume, double peopleTime)
    {
        return new ProductivityData(date, isHoliday, holidayName, productName, volume, peopleTime);
    }

    public static IEnumerable<string> GetHeader()
    {
        return [
            nameof(Date),
            nameof(ProductName),
            nameof(Volume),
            nameof(PeopleTime)
        ];
    }

    public override IEnumerable<string> GetContent()
    {
        return [
            Date.ToString(),
            ProductName,
            Volume.ToString(),
            PeopleTime.ToString()

        ];
    }
}
