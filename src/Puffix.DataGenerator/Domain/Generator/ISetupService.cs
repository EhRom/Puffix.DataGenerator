using DataGenerator.Domain.Generator.Models;

namespace DataGenerator.Domain.Generator;

public interface ISetupService
{
    IPeriod SetAutomaticPeriod();

    IPeriod SetStartAndEndPeriod(bool isWholeYear);
}
