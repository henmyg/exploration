using Maui.Core.Features.CurrentPrice;
using Maui.Core.Features.PriceGraph;
using Maui.Core.Features.SyncStatus;

namespace Maui.Core.Features
{
    public class TodayPageModel(
        SyncStatusViewModel syncStatus,
        CurrentPriceViewModel currentPrice,
        PriceGraphViewModel priceGraph)
    {
        public SyncStatusViewModel SyncStatus => syncStatus;
        public CurrentPriceViewModel CurrentPrice => currentPrice;
        public PriceGraphViewModel PriceGraph => priceGraph;
    }
}
