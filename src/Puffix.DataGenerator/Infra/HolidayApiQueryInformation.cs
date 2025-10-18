using Puffix.Rest;

namespace DataGenerator.Infra;

public class HolidayApiQueryInformation(HttpMethod httpMethod, IHolidayApiToken? token, IDictionary<string, IEnumerable<string>> headers, string baseUri, string queryPath, IDictionary<string, string> queryParameters, string queryContent) :
    QueryInformation<IHolidayApiToken>(httpMethod, token, headers, baseUri, queryPath, queryParameters, queryContent),
    IHolidayApiQueryInformation
{
    public static IHolidayApiQueryInformation CreateNewUnauthenticatedQuery(HttpMethod httpMethod, IDictionary<string, IEnumerable<string>> headers, string apiUri, string queryPath, IDictionary<string, string> queryParameters, string queryContent)
    {
        return new HolidayApiQueryInformation(httpMethod, default, headers, apiUri, queryPath, queryParameters, queryContent);
    }

    public static IHolidayApiQueryInformation CreateNewAuthenticatedQuery(IHolidayApiToken token, HttpMethod httpMethod, IDictionary<string, IEnumerable<string>> headers, string apiUri, string queryPath, IDictionary<string, string> queryParameters, string queryContent)
    {
        return new HolidayApiQueryInformation(httpMethod, token, headers, apiUri, queryPath, queryParameters, queryContent);
    }
}
