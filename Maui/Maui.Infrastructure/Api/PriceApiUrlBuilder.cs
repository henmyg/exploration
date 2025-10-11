namespace Maui.Infrastructure.Api;

public static class PriceApiUrlBuilder
{
    private const string BaseUrl = "https://api.energidataservice.dk/dataset/";
    private const string Dataset = "Elspotprices";

    public static string BuildDayAheadPricesUrl(
        string priceArea,
        DateTime startDate,
        DateTime endDate)
    {
        var startUtc = startDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm");
        var endUtc = endDate.ToUniversalTime().ToString("yyyy-MM-dd'T'HH':'mm");

        return $"{BaseUrl}{Dataset}?start={startUtc}&end={endUtc}&filter={{\"PriceArea\":[\"{priceArea}\"]}}";
    }
}
