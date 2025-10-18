using DataGenerator.Domain.Generator.Models;

namespace DataGenerator.Domain.Generator;

public interface IDataService
{
    IEnumerable<string> GetDataHeaders();

    void GenerateData(IDataContainer dataContainer, DateOnly currentDate);
}
