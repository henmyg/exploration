using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using Maui.Core.Shared.Repositories;
using System.Collections.ObjectModel;

namespace Maui.Core.Features.PriceGraph
{
    public partial class PriceGraphViewModel : ObservableObject
    {
        private readonly IPriceRepository _priceRepository;
        private readonly string _priceArea;

        [ObservableProperty]
        private string _text = "A graph will appear here soon";

        [ObservableProperty]
        private ObservableCollection<PricePoint> _prices = [];

        [ObservableProperty]
        private DateTimePoint[] _chartData = [];

        [ObservableProperty]
        private string _timeRange = string.Empty;

        public Func<DateTime, string> XAxisFormatter { get; } =
            date => {
                return date.TimeOfDay == TimeSpan.Zero
                    ? date.ToString("dd/MM HH:mm")
                    : date.ToString("HH:mm");
            };

        public PriceGraphViewModel(IPriceRepository priceRepository)
        {
            _priceRepository = priceRepository;
            _priceArea = "DK1"; // TODO: Make configurable
            _priceRepository.PricesUpdated += OnPricesUpdated;
            LoadPrices();
        }

        private void OnPricesUpdated(object? sender, EventArgs e)
        {
            LoadPrices();
        }

        private void LoadPrices()
        {
            var (startUtc, endUtc) = PriceGraphOperations.GetTodayAndTomorrowRange();
            var priceRecords = _priceRepository.GetPrices(_priceArea, startUtc, endUtc);

            Prices = priceRecords.ToPricePoints();
            ChartData = priceRecords.ToChartData();
            TimeRange = PriceGraphOperations.FormatTimeRange(startUtc, endUtc);
        }
    }

    /// <summary>
    /// Represents a single point on the price graph
    /// </summary>
    public record PricePoint(DateTime Time, decimal PriceDkk);

    /// <summary>
    /// Pure functions for price graph operations
    /// </summary>
    internal static class PriceGraphOperations
    {
        /// <summary>
        /// Gets the UTC date range for today and tomorrow (midnight to midnight)
        /// </summary>
        public static (DateTime startUtc, DateTime endUtc) GetTodayAndTomorrowRange()
        {
            var nowLocal = DateTime.Now;
            var startOfTodayLocal = nowLocal.Date;
            var endOfTomorrowLocal = startOfTodayLocal.AddDays(2);

            var startUtc = startOfTodayLocal.ToUniversalTime();
            var endUtc = endOfTomorrowLocal.ToUniversalTime();

            return (startUtc, endUtc);
        }

        /// <summary>
        /// Converts price records to observable collection of price points for display
        /// </summary>
        public static ObservableCollection<PricePoint> ToPricePoints(this IEnumerable<Maui.Core.Shared.Models.PriceRecord> priceRecords)
        {
            return new ObservableCollection<PricePoint>(
                priceRecords
                    .Where(p => p.PriceDkk.HasValue)
                    .Select(p => new PricePoint(
                        p.TimeUtc.ToLocalTime(),
                        p.PriceDkk!.Value))
                    .OrderBy(p => p.Time)
            );
        }

        /// <summary>
        /// Converts price records to DateTimePoint array for chart display with time-based positioning
        /// </summary>
        public static DateTimePoint[] ToChartData(this IEnumerable<Maui.Core.Shared.Models.PriceRecord> priceRecords)
        {
            return priceRecords
                .Where(p => p.PriceDkk.HasValue)
                .OrderBy(p => p.TimeUtc)
                .Select(p => new DateTimePoint
                {
                    DateTime = p.TimeUtc.ToLocalTime(),
                    Value = (double)p.PriceDkk!.Value
                })
                .ToArray();
        }

        /// <summary>
        /// Formats a time range for display
        /// </summary>
        public static string FormatTimeRange(DateTime startUtc, DateTime endUtc)
        {
            var startLocal = startUtc.ToLocalTime();
            var endLocal = endUtc.ToLocalTime();
            return $"{startLocal:dd/MM HH:mm} - {endLocal:dd/MM HH:mm}";
        }
    }
}
