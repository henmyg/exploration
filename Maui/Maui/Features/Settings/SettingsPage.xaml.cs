using Maui.Core.Features.Settings;

namespace Maui.Features.Settings;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsPageModel pageModel)
	{
		InitializeComponent();
		BindingContext = pageModel;
	}
}
