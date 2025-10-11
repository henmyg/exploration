using System.Data;
using System.Web;

namespace Maui.Infrastructure.Api;

public static class PriceApiUrlBuilder
{
    private const string BaseUrl = "https://api.energidataservice.dk";
    private const string Dataset = "DayAheadPrices";

    public static string BuildDayAheadPricesUrl(
        string priceArea,
        DateTime startDate,
        DateTime endDate)
    {
        var builder = new UriBuilder(BaseUrl);
        builder.Path = $"dataset/{Dataset}";
        builder.Port = -1;

        // Convert to Danish local time (Europe/Copenhagen timezone)
        var danishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        var startDanish = TimeZoneInfo.ConvertTime(startDate, danishTimeZone);
        var endDanish = TimeZoneInfo.ConvertTime(endDate, danishTimeZone);

        var query = HttpUtility.ParseQueryString(String.Empty);
        query["offset"] = "0";
        query["start"] = startDanish.ToString("yyyy-MM-dd'T'HH':'mm");
        query["end"] = endDanish.ToString("yyyy-MM-dd'T'HH':'mm");
        query["filter"] = $"{{\"PriceArea\":[\"{priceArea}\"]}}";
        query["sort"] = "TimeDK ASC";
        builder.Query = query.ToString();

        return builder.ToString();
    }
}
