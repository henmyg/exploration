namespace Maui.Shared.Models;

/// <summary>
/// Application-specific price record stored in the in-memory store.
/// Simplified model containing only the data we need.
/// </summary>
public record PriceRecord
{
    /// <summary>
    /// Timestamp in UTC
    /// </summary>
    public required DateTime TimeUtc { get; init; }

    /// <summary>
    /// Timestamp in Danish time (Europe/Copenhagen)
    /// </summary>
    public required DateTime TimeDk { get; init; }

    /// <summary>
    /// Price area (DK1 or DK2)
    /// </summary>
    public required string PriceArea { get; init; }

    /// <summary>
    /// Price in Danish Kroner (DKK) per MWh
    /// </summary>
    public decimal? PriceDkk { get; init; }

    /// <summary>
    /// Price in Euro (EUR) per MWh
    /// </summary>
    public decimal? PriceEur { get; init; }
}
