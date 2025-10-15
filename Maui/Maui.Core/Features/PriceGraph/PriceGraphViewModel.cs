using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.Core.Features.PriceGraph
{
    public partial class PriceGraphViewModel: ObservableObject
    {
        [ObservableProperty]
        private string _text = "A graph will appear here soon";
    }
}
