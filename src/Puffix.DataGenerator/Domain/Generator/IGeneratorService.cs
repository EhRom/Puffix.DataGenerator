using DataGenerator.Domain.Generator.Models;

namespace DataGenerator.Domain.Generator;

public interface IGeneratorService
{
    Task<string> GenerateAndPersistData(IPeriod period);
}