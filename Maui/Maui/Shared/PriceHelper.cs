using Maui.Infrastructure.Api.Models;

namespace Maui.Shared;

public static class PriceHelper
{
    public static DayAheadPriceRecord? GetCurrentPrice(
        this IEnumerable<DayAheadPriceRecord> records,
        DateTime? now = null
    ) => records
        .Where(r => r.TimeUTC <= (now ?? DateTime.UtcNow))
        .OrderByDescending(r => r.TimeUTC)
        .FirstOrDefault();
}
