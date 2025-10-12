using Maui.Shared.Models;

namespace Maui.Shared;

public static class PriceHelper
{
    /// <summary>
    /// Gets the current price from price records.
    /// </summary>
    public static PriceRecord? GetCurrentPrice(
        this IEnumerable<PriceRecord> records,
        DateTime? now = null
    ) => records
        .Where(r => r.TimeUtc <= (now ?? DateTime.UtcNow))
        .OrderByDescending(r => r.TimeUtc)
        .FirstOrDefault();
}
