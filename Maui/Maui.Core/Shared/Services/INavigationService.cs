namespace Maui.Core.Shared.Services
{
    public interface INavigationService
    {
        Task GoToAsync(string route, IDictionary<string, object>? parameters = null);
    }
}
