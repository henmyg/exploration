namespace Maui.Features.Debug.ViewStates;

public partial class ViewStatesPage : ContentPage
{
    public ViewStatesPage()
    {
        InitializeComponent();
    }

    private async void OnSyncStatusClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("debug/viewstates/syncstatus");
    }
}
