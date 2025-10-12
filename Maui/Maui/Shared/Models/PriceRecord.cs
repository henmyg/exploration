namespace Maui.Shared.Models;

/// <summary>
/// Application-specific price record stored in the in-memory store.
/// Simplified model containing only the data we need.
/// Time is stored in UTC; local time conversions should be done in ViewModels.
/// </summary>
public record PriceRecord
{
    /// <summary>
    /// Timestamp in UTC
    /// </summary>
    public required DateTime TimeUtc { get; init; }

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
