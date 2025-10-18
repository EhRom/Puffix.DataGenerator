namespace DataGenerator.Domain.Calendar.Models;

public class Holiday
{
    public int Year => Date.Year;

    public DateOnly Date { get; set; }

    public string Name { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Date} ({Name})-{base.ToString()}";
    }
}
