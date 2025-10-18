using Maui.Core.Features;

namespace Maui.Features;

public partial class TodayPage : ContentPage
{
	public TodayPage(TodayPageModel todayPageModel)
	{
		InitializeComponent();
		BindingContext = todayPageModel;
	}
}