namespace DataGenerator.Domain.Generator.Models;

public class DataCollectionEventArgs(string dataFilePath, ICollection<IData> dataCollection) : EventArgs
{
    public string DataFilePath { get; } = dataFilePath;

    public ICollection<IData> DataCollection { get; } = dataCollection;
}
