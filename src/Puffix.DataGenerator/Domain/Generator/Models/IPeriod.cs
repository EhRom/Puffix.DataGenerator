namespace DataGenerator.Domain.Generator.Models;

public interface IPeriod
{
    DateOnly Start { get; }

    DateOnly End { get; }

    bool IsWholeYear { get; }

    string StartPeriodText { get; }

    string EndPeriodText { get; }
}
