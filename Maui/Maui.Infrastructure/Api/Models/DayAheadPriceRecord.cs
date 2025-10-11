using System.Text.Json.Serialization;

namespace Maui.Infrastructure.Api.Models
{
    public class DayAheadPriceRecord
    {
        [JsonPropertyName("TimeUTC")]
        public DateTime TimeUTC { get; set; }

        [JsonPropertyName("TimeDK")]
        public DateTime TimeDK { get; set; }

        [JsonPropertyName("PriceArea")]
        public string PriceArea { get; set; } = string.Empty;

        [JsonPropertyName("DayAheadPriceEUR")]
        public decimal? DayAheadPriceEUR { get; set; }

        [JsonPropertyName("DayAheadPriceDKK")]
        public decimal? DayAheadPriceDKK { get; set; }
    }
}
