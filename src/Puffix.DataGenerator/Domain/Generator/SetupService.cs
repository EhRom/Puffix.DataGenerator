using DataGenerator.Domain.Generator.Models;
using Microsoft.Extensions.Configuration;
using Puffix.ConsoleLogMagnifier;

namespace DataGenerator.Domain.Generator;

public class SetupService(IConfiguration configuration) : ISetupService
{
    private const string START_TEXT = "stat";
    private const string END_TEXT = "end";

    private readonly Lazy<int> maxTryCountLazy = new(() =>
    {
        return configuration.GetValue<int>(nameof(maxTryCount))!;
    });

    private int maxTryCount => maxTryCountLazy.Value;

    public IPeriod SetAutomaticPeriod()
    {
        const int monthCount = 12;
        DateTime referenceDate = DateTime.Now.AddMonths(-monthCount);

        DateOnly startDate = new DateOnly(referenceDate.Year, referenceDate.Month, 1);
        DateOnly endDate = startDate.AddMonths(monthCount).AddDays(-1);

        return Period.CreateNew(startDate, endDate, false);
    }

    public IPeriod SetStartAndEndPeriod(bool isWholeYear)
    {
        DateOnly startPeriod;
        DateOnly endPeriod;

        bool validPeriod;
        int currentTryCount;

        (bool, DateOnly) enterPeriod(int maxTryCount, int currentTryCount, string periodName)
                => EnterPeriod(maxTryCount, currentTryCount, periodName, isWholeYear);

        validPeriod = false;
        currentTryCount = 0;
        do
        {
            (validPeriod, startPeriod) = enterPeriod(maxTryCount, ++currentTryCount, START_TEXT);
        } while (!validPeriod);


        validPeriod = false;
        currentTryCount = 0;
        do
        {
            (validPeriod, endPeriod) = enterPeriod(maxTryCount, ++currentTryCount, END_TEXT);
        } while (!validPeriod);

        return Period.CreateNew(startPeriod, endPeriod, isWholeYear);
    }

    private static (bool, DateOnly) EnterPeriod(int maxTryCount, int currentTryCount, string periodName, bool isWholeYear)
    {
        const string YEAR_TEXT = "year";
        const string DATE_TEXT = "date";
        string periodText = isWholeYear ? YEAR_TEXT : DATE_TEXT;

        const string YEAR_FORMAT = "integer between 2000 and 2100";
        const string DATE_FORMAT = "dd/MM/yyyy or yyyy-MM-dd, years between 2000 and 2100";
        string periodFormat = isWholeYear ? YEAR_FORMAT : DATE_FORMAT;

        ConsoleHelper.WriteInfo($"Enter the {periodName} {periodText} ({periodFormat}):");
        if (currentTryCount > 1)
            ConsoleHelper.WriteVerbose($"Try {currentTryCount} / {maxTryCount}");

        string? enteredPeriod = Console.ReadLine();

        ConsoleHelper.ClearLastLines();

        DateOnly date = default;
        int year = default;

        bool valid = !string.IsNullOrEmpty(enteredPeriod) &&
                                isWholeYear ?
                                    ValidateYear(enteredPeriod!, out year) :
                                    ValidateDate(enteredPeriod!, out date);

        date = valid && isWholeYear ? new DateOnly(year, 1, 1) : date;

        if (!valid)
        {
            ConsoleHelper.WriteWarning($"The entered {periodText} ('{enteredPeriod}') is not valid.");
            if (currentTryCount >= maxTryCount)
                throw new ArgumentException($"The entered {periodText} is not in the right format. Format accepted: {periodFormat}.");
        }
        else
            ConsoleHelper.WriteSuccess($"The {periodName} {periodText} is '{enteredPeriod}'.");

        return (valid, date);
    }

    private static bool ValidateDate(string dateValue, out DateOnly date)
    {
        const string isoDateFormat = "yyyy-MM-dd";
        const string frenchDateFormat = "dd/MM/yyyy";

        bool isValid = DateOnly.TryParseExact(dateValue, isoDateFormat, out date) ||
                       DateOnly.TryParseExact(dateValue, frenchDateFormat, out date);

        isValid = isValid && date.Year > 2000 && date.Year < 2100;

        return isValid;
    }

    private static bool ValidateYear(string yearValue, out int year)
    {
        year = 0;
        bool isValid = int.TryParse(yearValue, out year);

        isValid = isValid && year >= 2000 && year <= 2100;

        return isValid;
    }
}
