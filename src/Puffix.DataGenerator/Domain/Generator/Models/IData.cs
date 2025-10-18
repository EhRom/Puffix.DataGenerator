namespace DataGenerator.Domain.Generator.Models;

public interface IData
{
    DateOnly Date { get; init; }

    bool IsWeekend { get; }

    string HolidayName { get; init; }

    bool IsHoliday { get; init; }

    IEnumerable<string> GetContent();
}