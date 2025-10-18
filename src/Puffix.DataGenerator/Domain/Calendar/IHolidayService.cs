
using DataGenerator.Domain.Calendar.Models;

namespace DataGenerator.Domain.Calendar;

public interface IHolidayService
{
    Task<IEnumerable<Holiday>> GetHolidays(DateOnly startDate, DateOnly endDate);

    Task<IEnumerable<Holiday>> GetHolidays(int startYear, int endYear);

    Task<IEnumerable<Holiday>> GetHolidays(int year);
}
