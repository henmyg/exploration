using Maui.Core.Shared.Services;

namespace Maui
{
    internal class ShellNavigationService : INavigationService
    {
        public Task GoToAsync(string route, IDictionary<string, object>? parameters = null)
        {
            if (parameters == null)
            {
                return Shell.Current.GoToAsync(route);
            }
            return Shell.Current.GoToAsync(route, parameters);
        }
    }
}
