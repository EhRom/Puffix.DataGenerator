using DataGenerator.Domain.Calendar.Models;
using System.Security.Cryptography;

namespace DataGenerator.Domain.Generator.Models;

public abstract class BaseDataContainer(IEnumerable<Holiday> holidays, IPeriod period, string dataFilePath) : IDataContainer
{
    protected const int SAVE_TO_FILE_TRESHOLD = 20000;

    private bool disposed = false;

    private readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

    private readonly string dataFilePath = dataFilePath;

    public event EventHandler<DataCollectionEventArgs>? SaveData;

    public IPeriod Period { get; init; } = period;

    public IEnumerable<Holiday> Holidays { get; init; } = holidays;

    public abstract long RowCount { get; }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects).
                randomNumberGenerator.Dispose();
            }

            disposed = true;
        }
    }

    public abstract void AddData(IData data);

    protected void LaunchSaveData(ICollection<IData> dataCollection)
    {
        if (SaveData is not null)
            SaveData!.Invoke(this, new DataCollectionEventArgs(dataFilePath, dataCollection));
    }

    public abstract string GetCsvContent(char csvSeparatorCharacter);

    public bool IsHolidayOrWeekend(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ||
                Holidays.Where(h => h.Date == date).Any();
    }

    public long GetRandomLongValue(long valueVariation)
    {
        valueVariation = Math.Abs(valueVariation);

        long randomValue = NextRandomValue(0, valueVariation);

        return randomValue;
    }

    public long GetRandomLongValue(long defaultValue, long valueVariation)
    {
        valueVariation = Math.Abs(valueVariation);

        long randomValue = NextRandomValue(valueVariation * -1, valueVariation);

        return defaultValue + randomValue;
    }

    public double GetRandomDoubleValue(double defaultValue, long valueVariation, long valueVaraitionDivisor)
    {
        valueVariation = Math.Abs(valueVariation);

        double randomValue = (double)NextRandomValue(valueVariation * -1, valueVariation) / valueVaraitionDivisor;

        return defaultValue + randomValue;
    }

    public long NextRandomValue(long minValue, long maxExclusiveValue)
    {
        if (minValue >= maxExclusiveValue)
            (maxExclusiveValue, minValue) = (minValue, maxExclusiveValue);

        long diff = maxExclusiveValue - minValue;
        long upperBound = long.MaxValue / diff * diff;

        long randomNumber;
        do
        {
            randomNumber = GetRandomLong();
        } while (randomNumber >= upperBound);

        randomNumber = Math.Abs(randomNumber);

        return minValue + randomNumber % diff;
    }

    private long GetRandomLong()
    {
        byte[] randomBytes = GenerateRandomBytes(sizeof(long));
        return BitConverter.ToInt64(randomBytes, 0);
    }

    private byte[] GenerateRandomBytes(int bytesNumber)
    {
        var buffer = new byte[bytesNumber];
        randomNumberGenerator.GetBytes(buffer, 0, bytesNumber);
        return buffer;
    }

}
