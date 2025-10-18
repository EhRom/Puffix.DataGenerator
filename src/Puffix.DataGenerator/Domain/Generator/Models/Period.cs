namespace DataGenerator.Domain.Generator.Models;

public class Period(DateOnly startPeriod, DateOnly endPeriod, bool isWholeYear) : IPeriod
{
    public DateOnly Start { get; init; } = startPeriod;

    public DateOnly End { get; init; } = endPeriod;

    public bool IsWholeYear { get; init; } = isWholeYear;

    public string StartPeriodText => IsWholeYear ? Start.Year.ToString() : Start.ToString();

    public string EndPeriodText => IsWholeYear ? End.Year.ToString() : End.ToString();

    public static Period CreateNew(DateOnly startPeriod, DateOnly endPeriod, bool isWholeYear)
    {
        if (startPeriod > endPeriod)
            (startPeriod, endPeriod) = (endPeriod, startPeriod);

        endPeriod = isWholeYear ? new DateOnly(endPeriod.Year, 12, 31) : endPeriod;

        return new Period(startPeriod, endPeriod, isWholeYear);
    }
}
