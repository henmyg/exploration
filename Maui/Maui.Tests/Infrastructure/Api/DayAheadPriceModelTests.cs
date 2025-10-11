using System.Text.Json;
using Maui.Infrastructure.Api.Models;
using Xunit;

namespace Maui.Tests.Infrastructure.Api
{
    public class DayAheadPriceModelTests
    {
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = false
        };

        public record SerializeInput(
            string Area,
            string Eur,
            string Dkk
        );

        public record SerializeExpectation(
            string Area,
            decimal Eur,
            decimal Dkk
        );

        public static IEnumerable<object[]> SerializationTestData =>
            new List<object[]>
            {
                new object[]
                {
                    new SerializeInput("DK1", "50.10", "373.75"),
                    new SerializeExpectation("DK1", 50.10m, 373.75m)
                },
                new object[]
                {
                    new SerializeInput("DK2", "52.30", "390.20"),
                    new SerializeExpectation("DK2", 52.30m, 390.20m)
                },
                new object[]
                {
                    new SerializeInput("SE3", "45.00", "335.50"),
                    new SerializeExpectation("SE3", 45.00m, 335.50m)
                }
             };

        [Theory]
        [MemberData(nameof(SerializationTestData))]
        public void CanDeserialize_SingleRecord(SerializeInput input, SerializeExpectation expected)
        {
            // Arrange
            var json = $$"""
            {
                "TimeUTC": "2025-10-11T12:00:00",
                "TimeDK": "2025-10-11T14:00:00",
                "PriceArea": "{{input.Area}}",
                "DayAheadPriceEUR": {{input.Eur}},
                "DayAheadPriceDKK": {{input.Dkk}}
            }
            """;

            // Act
            var record = JsonSerializer.Deserialize<DayAheadPriceRecord>(json, _jsonOptions);

            // Assert
            Assert.NotNull(record);
            Assert.Equal(new DateTime(2025, 10, 11, 12, 0, 0), record.TimeUTC);
            Assert.Equal(new DateTime(2025, 10, 11, 14, 0, 0), record.TimeDK);
            Assert.Equal(expected.Area, record.PriceArea);
            Assert.Equal(expected.Eur, record.DayAheadPriceEUR);
            Assert.Equal(expected.Dkk, record.DayAheadPriceDKK);
        }

        [Fact]
        public void CanDeserialize_RecordWithNullPrices()
        {
            // Arrange
            var json = """
            {
                "TimeUTC": "2025-10-11T12:00:00",
                "TimeDK": "2025-10-11T14:00:00",
                "PriceArea": "DK2",
                "DayAheadPriceEUR": null,
                "DayAheadPriceDKK": null
            }
            """;

            // Act
            var record = JsonSerializer.Deserialize<DayAheadPriceRecord>(json, _jsonOptions);

            // Assert
            Assert.NotNull(record);
            Assert.Equal("DK2", record.PriceArea);
            Assert.Null(record.DayAheadPriceEUR);
            Assert.Null(record.DayAheadPriceDKK);
        }

        [Fact]
        public void CanDeserialize_ResponseWithMultipleRecords()
        {
            // Arrange
            var json = """
            {
                "records": [
                    {
                        "TimeUTC": "2025-10-11T12:00:00",
                        "TimeDK": "2025-10-11T14:00:00",
                        "PriceArea": "DK1",
                        "DayAheadPriceEUR": 50.10,
                        "DayAheadPriceDKK": 373.75
                    },
                    {
                        "TimeUTC": "2025-10-11T12:15:00",
                        "TimeDK": "2025-10-11T14:15:00",
                        "PriceArea": "DK1",
                        "DayAheadPriceEUR": 52.30,
                        "DayAheadPriceDKK": 390.20
                    }
                ]
            }
            """;

            // Act
            var response = JsonSerializer.Deserialize<DayAheadPricesResponse>(json, _jsonOptions);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(2, response.Records.Count);
            Assert.Equal("DK1", response.Records[0].PriceArea);
            Assert.Equal(50.10m, response.Records[0].DayAheadPriceEUR);
            Assert.Equal(52.30m, response.Records[1].DayAheadPriceEUR);
        }

        [Fact]
        public void CanSerialize_Record()
        {
            // Arrange
            var record = new DayAheadPriceRecord
            {
                TimeUTC = new DateTime(2025, 10, 11, 12, 0, 0),
                TimeDK = new DateTime(2025, 10, 11, 14, 0, 0),
                PriceArea = "DK1",
                DayAheadPriceEUR = 50.10m,
                DayAheadPriceDKK = 373.75m
            };

            // Act
            var json = JsonSerializer.Serialize(record, _jsonOptions);

            // Assert
            Assert.Contains("\"TimeUTC\"", json);
            Assert.Contains("\"PriceArea\":\"DK1\"", json);
            Assert.Contains("50.10", json);
        }
    }
}
