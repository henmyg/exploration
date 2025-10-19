using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore.Defaults;
using Maui.Core.Features.PriceGraph;
using Maui.Core.Features.SyncStatus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Maui.Core.Features.Debug
{
    public partial class PriceGraphStatesViewModel : ObservableObject, IPriceGraphViewModel, IPreviewView<PriceGraphStatesViewModel.State>
    {
        private readonly Func<DateTime> _getNowUtc;

        [ObservableProperty]
        private State _viewState = State.Today;

        [ObservableProperty]
        private ObservableCollection<PricePoint> _prices = [];

        [ObservableProperty]
        private DateTimePoint[] _chartData = [];

        [ObservableProperty]
        private long _now;

        [ObservableProperty]
        private string _timeRange = string.Empty;

        public Func<DateTime, string> XAxisFormatter { get; } = PriceGraphOperations.XAxisFormatter;

        public IReadOnlyList<ViewStateOption<State>> States => [
            new("Today and Tomorrow", State.TodayAndTomorrow),
            new("Today Only", State.Today),
            new("No Data", State.None)
        ];

        public PriceGraphStatesViewModel(Func<DateTime>? getNowUtc = null)
        {
            _getNowUtc = getNowUtc ?? (() => DateTime.UtcNow);
            Now = _getNowUtc().Ticks;
            UpdateDataForState();
        }

        partial void OnViewStateChanged(State value)
        {
            UpdateDataForState();
        }

        private void UpdateDataForState()
        {
            var nowUtc = _getNowUtc();
            var nowLocal = nowUtc.ToLocalTime();
            var startOfTodayLocal = nowLocal.Date;

            switch (ViewState)
            {
                case State.TodayAndTomorrow:
                    GenerateTodayAndTomorrowData(startOfTodayLocal);
                    break;
                case State.Today:
                    GenerateTodayData(startOfTodayLocal);
                    break;
                case State.None:
                    GenerateNoData();
                    break;
            }
        }

        private void GenerateTodayAndTomorrowData(DateTime startOfTodayLocal)
        {
            var chartPoints = new List<DateTimePoint>();
            var pricePoints = new List<PricePoint>();

            // Generate 48 hours of data (today + tomorrow)
            for (int hour = 0; hour < 48; hour++)
            {
                var time = startOfTodayLocal.AddHours(hour).ToUniversalTime();
                // Simulate price variation: low at night, high during day
                var basePrice = 1500m; // Base price in DKK
                var hourOfDay = hour % 24;
                var variation = (decimal)Math.Sin(hourOfDay * Math.PI / 12) * 500m; // Varies between -500 and +500
                var price = basePrice + variation;

                chartPoints.Add(new DateTimePoint
                {
                    DateTime = time,
                    Value = (double)price
                });

                pricePoints.Add(new PricePoint(time, price));
            }

            ChartData = [.. chartPoints];
            Prices = new ObservableCollection<PricePoint>(pricePoints);

            var startUtc = startOfTodayLocal.ToUniversalTime();
            var endUtc = startOfTodayLocal.AddDays(2).ToUniversalTime();
            TimeRange = PriceGraphOperations.FormatTimeRange(startUtc, endUtc);
        }

        private void GenerateTodayData(DateTime startOfTodayLocal)
        {
            var chartPoints = new List<DateTimePoint>();
            var pricePoints = new List<PricePoint>();

            // Generate 24 hours of data (today only)
            for (int hour = 0; hour < 24; hour++)
            {
                var time = startOfTodayLocal.AddHours(hour).ToUniversalTime();
                var basePrice = 1500m;
                var variation = (decimal)Math.Sin(hour * Math.PI / 12) * 500m;
                var price = basePrice + variation;

                chartPoints.Add(new DateTimePoint
                {
                    DateTime = time,
                    Value = (double)price
                });

                pricePoints.Add(new PricePoint(time, price));
            }

            ChartData = [.. chartPoints];
            Prices = new ObservableCollection<PricePoint>(pricePoints);

            var startUtc = startOfTodayLocal.ToUniversalTime();
            var endUtc = startOfTodayLocal.AddDays(1).ToUniversalTime();
            TimeRange = PriceGraphOperations.FormatTimeRange(startUtc, endUtc);
        }

        private void GenerateNoData()
        {
            ChartData = [];
            Prices = [];
            TimeRange = "No data available";
        }

        public void Dispose()
        {
        }

        public enum State
        {
            TodayAndTomorrow, Today, None
        }
    }
}
