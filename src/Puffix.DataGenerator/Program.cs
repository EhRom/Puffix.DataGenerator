using DataGenerator.Domain.Calendar;
using DataGenerator.Domain.Claim;
using DataGenerator.Domain.Generator;
using DataGenerator.Domain.Generator.Models;
using DataGenerator.Domain.Products;
using DataGenerator.Infra;
using Microsoft.Extensions.Configuration;
using Puffix.ConsoleLogMagnifier;
using Puffix.IoC;
using Puffix.IoC.Configuration;
using System.Diagnostics;

ConsoleHelper.Write("Welcome to the data generator console App.");
ConsoleHelper.Write("This help will generate sample data for your sample dashboards.");

IIoCContainerWithConfiguration container;
try
{
    IConfiguration configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
           .Build();

    container = IoCContainer.BuildContainer(configuration);
}
catch (Exception error)
{
    ConsoleHelper.Write("Error while initializing the console App");
    ConsoleHelper.WriteError(error);

    ConsoleHelper.WriteVerbose("Press any key to quit.");
    ConsoleHelper.ReadKey();
    return;
}

ConsoleKey key;
do
{
    ConsoleHelper.WriteInfo("Press:");
    ConsoleHelper.WriteInfo("- Y to specify start and end year for data generation, and generate data");
    ConsoleHelper.WriteInfo("- D to specify start and end date for data generation, and generate data");
    ConsoleHelper.WriteInfo("- A automatic choice of dates (previous month) for data generation, and generate data");
    ConsoleHelper.WriteInfo("- Q to quit.");
    key = ConsoleHelper.ReadKey();

    try
    {
        ConsoleHelper.WriteVerbose("Load configuration.");


        if (key == ConsoleKey.Q)
        {
            ConsoleHelper.WriteInfo("Thank you for using the data generator console App. See you soon!");
        }
        else if (key == ConsoleKey.Y || key == ConsoleKey.D || key == ConsoleKey.A)
        {
            ISetupService setupService = container.Resolve<ISetupService>();

            IHolidayService holidayService = container.Resolve<IHolidayService>();

            string generationType = container.Configuration["generationType"]!;
            IDataService dataService = string.Equals(generationType, "claims") ?
                                container.Resolve<IClaimService>() :
                                container.Resolve<IProductService>();

            IGeneratorService generatorService = container.Resolve<IGeneratorService>
            (
                IoCNamedParameter.CreateNew("configuration", container.Configuration),
                IoCNamedParameter.CreateNew(nameof(holidayService), holidayService),
                IoCNamedParameter.CreateNew(nameof(dataService), dataService)
            );

            IPeriod period = key == ConsoleKey.A ?
                                            setupService.SetAutomaticPeriod() :
                                            setupService.SetStartAndEndPeriod(key == ConsoleKey.Y);

            ConsoleHelper.WriteInfo($"Generate data for the period between {period.StartPeriodText} and {period.EndPeriodText}");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            string generatedFilePath = await generatorService.GenerateAndPersistData(period);

            stopwatch.Stop();
            ConsoleHelper.WriteSuccess($"The file is generated. File path: {generatedFilePath} (Duration: {stopwatch.Elapsed})");
        }
        else
        {
            ConsoleHelper.WriteWarning($"The key {key} is not a known command (for the moment :-) )");
        }
    }
    catch (Exception error)
    {
        ConsoleHelper.WriteError("Error while generating data");
        ConsoleHelper.WriteError(error);
        ConsoleHelper.WriteNewLine(2);
    }
} while (key != ConsoleKey.Q);