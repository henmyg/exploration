using Maui.Core.Features.CurrentPrice;
using Maui.Core.Features.SyncStatus;

namespace Maui.Core.Features
{
    public class MainPageModel(
        SyncStatusViewModel syncStatus,
        CurrentPriceViewModel currentPrice)
    {
        public SyncStatusViewModel SyncStatus => syncStatus;
        public CurrentPriceViewModel CurrentPrice => currentPrice;
    }
}
