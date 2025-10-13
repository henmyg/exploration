using Maui.Core.Features;

namespace Maui.Features;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
	}
}