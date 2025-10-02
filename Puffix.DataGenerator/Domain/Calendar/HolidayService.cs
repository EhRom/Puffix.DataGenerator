using DataGenerator.Domain.Calendar.Models;
using DataGenerator.Infra;
using Microsoft.Extensions.Configuration;

namespace DataGenerator.Domain.Calendar;

public class HolidayService(IConfiguration configuration, IHolidayApiHttpRepository holidayApiHttpRepository) : IHolidayService
{
    private readonly IHolidayApiHttpRepository holidayApiHttpRepository = holidayApiHttpRepository;

    private readonly Lazy<string> holidayServiceUriLazy = new(() =>
    {
        return configuration[nameof(holidayServiceUri)] ?? string.Empty;
    });

    private string holidayServiceUri => holidayServiceUriLazy.Value;

    public async Task<IEnumerable<Holiday>> GetHolidays(DateOnly startDate, DateOnly endDate)
    {
        return await GetHolidays(startDate.Year, endDate.Year);
    }

    public async Task<IEnumerable<Holiday>> GetHolidays(int startYear, int endYear)
    {
        IEnumerable<Holiday> holidays = new List<Holiday>();

        if (startYear > endYear)
            (endYear, startYear) = (startYear, endYear);

        for (int currentYear = startYear; currentYear <= endYear; currentYear++)
        {
            holidays = holidays.Union(await GetHolidays(currentYear));
        }

        return holidays.ToList();
    }

    public async Task<IEnumerable<Holiday>> GetHolidays(int year)
    {
        IDictionary<string, string> queryParameters = new Dictionary<string, string>();
        IHolidayApiQueryInformation queryInformation = holidayApiHttpRepository.BuildUnauthenticatedQuery(HttpMethod.Get, holidayServiceUri, $"{year}.json", queryParameters, string.Empty);

        Dictionary<string, string> result = await holidayApiHttpRepository.HttpJsonAsync<Dictionary<string, string>>(queryInformation);
        return ConvertHolidays(result);
    }

    private IEnumerable<Holiday> ConvertHolidays(Dictionary<string, string> baseHolidays)
    {
        foreach (string baseHolidayKey in baseHolidays.Keys)
        {
            yield return new Holiday()
            {
                Date = DateOnly.ParseExact(baseHolidayKey, "yyyy-MM-dd"),
                Name = baseHolidays[baseHolidayKey]
            };
        }
    }
}
