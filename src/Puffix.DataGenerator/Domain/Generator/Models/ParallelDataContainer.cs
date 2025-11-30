using DataGenerator.Domain.Calendar.Models;
using System.Collections.Concurrent;

namespace DataGenerator.Domain.Generator.Models;

public class ParallelDataContainer(IEnumerable<Holiday> holidays, IPeriod period, string dataFilePath) : BaseDataContainer(holidays, period, dataFilePath), IDataContainer
{
    private ConcurrentDictionary<DateOnly, ICollection<IData>> generatedData = [];

    private int rowCount = 0;

    public override long RowCount => rowCount;

    public static IDataContainer CreateNew(IEnumerable<Holiday> holidays, IPeriod period, string generatedFilePath)
    {
        return new ParallelDataContainer(holidays, period, generatedFilePath);
    }

    public override void AddData(IData data)
    {
        if (!generatedData.ContainsKey(data.Date))
            generatedData[data.Date] = [];

        generatedData[data.Date].Add(data);

        if (++rowCount % SAVE_TO_FILE_TRESHOLD == 0)
        {
            ICollection<IData> dataCollection = generatedData.Values.SelectMany(v => v).ToList();
            LaunchSaveData(dataCollection);

            generatedData = [];
        }
    }

    public override string GetCsvContent(char csvSeparatorCharacter)
    {
        ICollection<IData> dataCollection = generatedData.Values.SelectMany(v => v).OrderBy(v => v.Date).ToList();

        return IDataContainer.GetCsvContent(csvSeparatorCharacter, dataCollection);
    }
}