using Maui.Core.Features.Settings.Configuration;

namespace Maui.Core.Features.Settings
{
    public class SettingsPageModel(
        SettingsViewModel settings)
    {
        public SettingsViewModel Settings => settings;
    }
}
