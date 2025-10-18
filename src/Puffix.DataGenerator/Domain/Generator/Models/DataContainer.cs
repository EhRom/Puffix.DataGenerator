using DataGenerator.Domain.Calendar.Models;

namespace DataGenerator.Domain.Generator.Models;

public class DataContainer(IEnumerable<Holiday> holidays, IPeriod period, string dataFilePath) : BaseDataContainer(holidays, period, dataFilePath), IDataContainer
{
    private Dictionary<DateOnly, ICollection<IData>> generatedData = [];

    private int rowCount = 0;

    public override long RowCount => rowCount;

    public static IDataContainer CreateNew(IEnumerable<Holiday> holidays, IPeriod period, string dataFilePath)
    {
        return new DataContainer(holidays, period, dataFilePath);
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
        ICollection<IData> dataCollection = generatedData.Values.SelectMany(v => v).ToList();

        return IDataContainer.GetCsvContent(csvSeparatorCharacter, dataCollection);
    }
}