using System.Text.Json.Serialization;

namespace Maui.Infrastructure.Api.Models
{
    public class DayAheadPricesResponse
    {
        [JsonPropertyName("records")]
        public List<DayAheadPriceRecord> Records { get; set; } = new();
    }
}
