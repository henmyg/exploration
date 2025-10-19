using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using Maui.Core.Shared.Repositories;
using Maui.Infrastructure.Services;
using System.Collections.ObjectModel;

namespace Maui.Core.Features.PriceGraph
{
    public partial class PriceGraphViewModel : ObservableObject, IPriceGraphViewModel
    {
        private readonly IPriceRepository _priceRepository;
        private readonly ITaskDelayer _taskDelayer;
        private readonly Func<DateTime> _getNowUtc;
        private readonly string _priceArea;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Task? _nowUpdateTask;

        [ObservableProperty]
        private ObservableCollection<PricePoint> _prices = [];

        [ObservableProperty]
        private DateTimePoint[] _chartData = [];

        [ObservableProperty]
        private long _now;

        [ObservableProperty]
        private string _timeRange = string.Empty;

        public Func<DateTime, string> XAxisFormatter { get; } =
            dateUtc => {
                var date = dateUtc.ToLocalTime();
                return date.TimeOfDay == TimeSpan.Zero
                    ? date.ToString("dd/MM HH:mm")
                    : date.ToString("HH:mm");
            };

        public PriceGraphViewModel(
            IPriceRepository priceRepository,
            ITaskDelayer taskDelayer,
            Func<DateTime>? getNowUtc = null)
        {
            _priceRepository = priceRepository ?? throw new ArgumentNullException(nameof(priceRepository));
            _taskDelayer = taskDelayer ?? throw new ArgumentNullException(nameof(taskDelayer));
            _getNowUtc = getNowUtc ?? (() => DateTime.UtcNow);
            _priceArea = "DK1"; // TODO: Make configurable
            _priceRepository.PricesUpdated += OnPricesUpdated;

            // Initialize Now
            _now = _getNowUtc().Ticks;

            LoadPrices();

            // Start the "Now" update loop (every minute)
            _nowUpdateTask = RunNowUpdateLoopAsync(_cancellationTokenSource.Token);
        }

        private void OnPricesUpdated(object? sender, EventArgs e)
        {
            LoadPrices();
        }

        private void LoadPrices()
        {
            var (startUtc, endUtc) = PriceGraphOperations.GetTodayAndTomorrowRange(_getNowUtc());
            var priceRecords = _priceRepository.GetPrices(_priceArea, startUtc, endUtc);

            Prices = priceRecords.ToPricePoints();
            ChartData = priceRecords.ToChartData();
            TimeRange = PriceGraphOperations.FormatTimeRange(startUtc, endUtc);
        }

        private async Task RunNowUpdateLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _taskDelayer.DelayAsync(TimeSpan.FromMinutes(1), cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Now = _getNowUtc().Ticks;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when disposed
                    break;
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            _priceRepository.PricesUpdated -= OnPricesUpdated;
            GC.SuppressFinalize(this);
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
        /// Gets the UTC date range for today and tomorrow (midnight to midnight in local time, converted to UTC)
        /// </summary>
        /// <param name="nowUtc">Current UTC time</param>
        /// <returns>Start and end times in UTC representing midnight-to-midnight in local timezone</returns>
        public static (DateTime startUtc, DateTime endUtc) GetTodayAndTomorrowRange(DateTime nowUtc)
        {
            // Convert to local to determine "today" in user's timezone
            var nowLocal = nowUtc.ToLocalTime();
            var startOfTodayLocal = nowLocal.Date;
            var endOfTomorrowLocal = startOfTodayLocal.AddDays(2);

            // Convert back to UTC for database queries
            var startUtc = startOfTodayLocal.ToUniversalTime();
            var endUtc = endOfTomorrowLocal.ToUniversalTime();

            return (startUtc, endUtc);
        }

        /// <summary>
        /// Converts price records to observable collection of price points for display
        /// </summary>
        public static ObservableCollection<PricePoint> ToPricePoints(this IEnumerable<Shared.Models.PriceRecord> priceRecords)
        {
            return new ObservableCollection<PricePoint>(
                priceRecords
                    .Where(p => p.PriceDkk.HasValue)
                    .Select(p => new PricePoint(
                        p.TimeUtc,
                        p.PriceDkk!.Value))
                    .OrderBy(p => p.Time)
            );
        }

        /// <summary>
        /// Converts price records to DateTimePoint array for chart display with time-based positioning
        /// </summary>
        public static DateTimePoint[] ToChartData(this IEnumerable<Maui.Core.Shared.Models.PriceRecord> priceRecords)
        {
            return [.. priceRecords
                .Where(p => p.PriceDkk.HasValue)
                .OrderBy(p => p.TimeUtc)
                .Select(p => new DateTimePoint
                {
                    DateTime = p.TimeUtc,
                    Value = (double)p.PriceDkk!.Value
                })];
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
