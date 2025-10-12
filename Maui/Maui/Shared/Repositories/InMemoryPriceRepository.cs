using System.Collections.Concurrent;
using Maui.Shared.Models;

namespace Maui.Shared.Repositories;

/// <summary>
/// Thread-safe in-memory implementation of the price repository.
/// Stores prices indexed by price area and timestamp for fast retrieval.
/// </summary>
public class InMemoryPriceRepository : IPriceRepository
{
    private readonly ConcurrentDictionary<string, SortedList<DateTime, PriceRecord>> _pricesByArea = new();

    public event EventHandler? PricesUpdated;

    public void StorePrices(IEnumerable<PriceRecord> records)
    {
        if (records == null) throw new ArgumentNullException(nameof(records));

        var recordsList = records.ToList();
        if (recordsList.Count == 0) return;

        foreach (var record in recordsList)
        {
            var areaStore = _pricesByArea.GetOrAdd(
                record.PriceArea,
                _ => new SortedList<DateTime, PriceRecord>());

            lock (areaStore)
            {
                areaStore[record.TimeUtc] = record;
            }
        }

        PricesUpdated?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyList<PriceRecord> GetPrices(string priceArea, DateTime startUtc, DateTime endUtc)
    {
        if (string.IsNullOrWhiteSpace(priceArea))
            throw new ArgumentException("Price area cannot be null or empty", nameof(priceArea));

        if (!_pricesByArea.TryGetValue(priceArea, out var areaStore))
            return Array.Empty<PriceRecord>();

        lock (areaStore)
        {
            return areaStore.Values
                .Where(r => r.TimeUtc >= startUtc && r.TimeUtc < endUtc)
                .ToList();
        }
    }
}
