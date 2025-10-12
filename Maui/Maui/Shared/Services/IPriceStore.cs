using Maui.Shared.Models;

namespace Maui.Shared.Services;

/// <summary>
/// Interface for the in-memory price store.
/// Simple data repository - no business logic.
/// </summary>
public interface IPriceStore
{
    /// <summary>
    /// Stores or updates price records.
    /// Replaces existing records for the same timestamp and price area.
    /// </summary>
    void StorePrices(IEnumerable<PriceRecord> records);

    /// <summary>
    /// Gets all price records for a specific price area within a time range.
    /// </summary>
    IReadOnlyList<PriceRecord> GetPrices(string priceArea, DateTime startUtc, DateTime endUtc);

    /// <summary>
    /// Event raised when prices are updated in the store.
    /// </summary>
    event EventHandler? PricesUpdated;
}
