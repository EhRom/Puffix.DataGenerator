using Puffix.Rest;

namespace DataGenerator.Infra;

public interface IHolidayApiHttpRepository : IRestHttpRepository<IHolidayApiQueryInformation, IHolidayApiToken>
{
}
