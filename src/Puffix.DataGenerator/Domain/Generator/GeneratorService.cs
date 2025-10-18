using DataGenerator.Domain.Calendar;
using DataGenerator.Domain.Calendar.Models;
using DataGenerator.Domain.Generator.Models;
using Microsoft.Extensions.Configuration;
using Puffix.ConsoleLogMagnifier;
using System.Diagnostics;

namespace DataGenerator.Domain.Generator;

public class GeneratorService(IConfiguration configuration, IHolidayService holidayService, IDataService dataService) : IGeneratorService
{
    private readonly IHolidayService holidayService = holidayService;
    private readonly IDataService dataService = dataService;

    private readonly Lazy<string> outputDirectoryPathLazy = new(() =>
    {
        return configuration[nameof(outputDirectoryPath)]!;
    });
    private readonly Lazy<string> fileNamePrefixLazy = new(() =>
    {
        return configuration[nameof(fileNamePrefix)]!;
    });
    private readonly Lazy<char> csvSeparatorCharacterLazy = new(() =>
    {
        return configuration[nameof(csvSeparatorCharacter)]?[0] ?? ';';
    });

    private string outputDirectoryPath => outputDirectoryPathLazy.Value;
    private string fileNamePrefix => fileNamePrefixLazy.Value;
    private char csvSeparatorCharacter => csvSeparatorCharacterLazy.Value;

    public async Task<string> GenerateAndPersistData(IPeriod period)
    {
        Stopwatch processStopwatch = new Stopwatch();
        processStopwatch.Start();

        ConsoleHelper.WriteVerbose("Create file.");

        string generatedFilePath = await CreateDataToFile(period);

        processStopwatch.Stop();
        ConsoleHelper.WriteVerbose($"File creation duration: {processStopwatch.Elapsed}");

        processStopwatch.Restart();

        ConsoleHelper.WriteVerbose("Get holidays.");
        IEnumerable<Holiday> holidays = await holidayService.GetHolidays(period.Start, period.End);

        processStopwatch.Stop();
        ConsoleHelper.WriteVerbose($"Holidays retrieval duration: {processStopwatch.Elapsed}");

        processStopwatch.Restart();

        ConsoleHelper.WriteVerbose("Generate data.");
        using IDataContainer dataContainer = GenerateData(holidays, period, generatedFilePath);

        processStopwatch.Stop();
        ConsoleHelper.WriteVerbose($"Data generation duration: {processStopwatch.Elapsed}. {dataContainer.RowCount} rows generated");

        processStopwatch.Restart();

        ConsoleHelper.WriteVerbose("Append data to file.");
        await AppendToDataFile(dataContainer, generatedFilePath);

        processStopwatch.Stop();

        ConsoleHelper.WriteVerbose($"Data append duration: {processStopwatch.Elapsed}");
        return generatedFilePath;
    }

    private IDataContainer GenerateData(IEnumerable<Holiday> holidays, IPeriod period, string generatedFilePath)
    {
        ConsoleHelper.WriteVerbose("Initialize data container.");

        //IDataContainer dataContainer = DataContainer.CreateNew(holidays, period, generatedFilePath);
        IDataContainer dataContainer = ParallelDataContainer.CreateNew(holidays, period, generatedFilePath);

        dataContainer.SaveData += DataContainer_SaveData;

        ConsoleHelper.WriteVerbose($"Generate productivity data from {period.Start} to {period.End}.");

        int totalDays = period.End.ToDateTime(default).Subtract(period.Start.ToDateTime(default)).Days + 1;

        DateOnly[] dates = Enumerable.Range(0, totalDays)
                                    .Select(period.Start.AddDays)
                                    .ToArray();

        for (int dateIndex = 0; dateIndex < totalDays; dateIndex++)
        {
            dataService.GenerateData(dataContainer, dates[dateIndex]);
        }

        return dataContainer;
    }

    private async void DataContainer_SaveData(object? sender, DataCollectionEventArgs dataCollectionEventArgs)
    {
        string fileContent = IDataContainer.GetCsvContent(csvSeparatorCharacter, dataCollectionEventArgs.DataCollection);
        await File.AppendAllTextAsync(dataCollectionEventArgs.DataFilePath, fileContent);
    }

    private async Task<string> CreateDataToFile(IPeriod period)
    {
        string generatedFileName = BuildFilePath(outputDirectoryPath, fileNamePrefix, period.Start, period.End);

        string headerContent = IDataContainer.GetCsvHeaders(csvSeparatorCharacter, dataService.GetDataHeaders());
        await File.WriteAllTextAsync(generatedFileName, headerContent);

        return generatedFileName;
    }

    private async Task AppendToDataFile(IDataContainer dataContainer, string generatedFileName)
    {
        string fileContent = dataContainer.GetCsvContent(csvSeparatorCharacter);
        await File.AppendAllTextAsync(generatedFileName, fileContent);
    }

    private static string BuildFilePath(string outputDirectory, string fileNamePrefix, DateOnly startDate, DateOnly endDate)
    {
        if (!Path.IsPathFullyQualified(outputDirectory))
            outputDirectory = Path.GetFullPath(outputDirectory);

        if (!Directory.Exists(outputDirectory))
            Directory.CreateDirectory(outputDirectory);

        return Path.Combine(outputDirectory, BuildFileName(fileNamePrefix, startDate, endDate));
    }

    private static string BuildFileName(string fileNamePrefix, DateOnly startDate, DateOnly endDate)
    {
        const string DATA_FORMAT = "yyyyMMdd";
        string guid = Guid.NewGuid().ToString("N");

        return $"{fileNamePrefix}-{startDate.ToString(DATA_FORMAT)}-{endDate.ToString(DATA_FORMAT)}-{guid}.csv";
    }
}
