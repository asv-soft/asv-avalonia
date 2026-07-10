using Asv.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Plugins;

public static class SettingsRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public Builder Settings => new(builder);

        public PagesRegistrations.Builder RegisterSettings(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(PagesRegistrations.Builder builder) : IDependencyBuilder
    {
        private bool _isPluginSettingsGroupRegistered;

        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            return RegisterMarket().RegisterInstalled();
        }

        public Builder RegisterMarket()
        {
            EnsurePluginSettingsGroup();
            builder.AppBuilder.Settings.AddSubPage<
                PluginsMarketPageViewModel,
                PluginsMarketPageView,
                PluginsMarketTreePageMenu
            >(PluginsMarketPageViewModel.PageId);
            builder.AppBuilder.Settings.AddSubPage<
                SettingsPluginsSourcesViewModel,
                SettingsPluginsSourcesView,
                SettingsPluginsTreePageMenu
            >(SettingsPluginsSourcesViewModel.PageId);
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                SourceDialogViewModel,
                SourceDialogView
            >();
            return this;
        }

        public Builder RegisterInstalled()
        {
            EnsurePluginSettingsGroup();
            builder.AppBuilder.Settings.AddSubPage<
                InstalledPluginsPageViewModel,
                InstalledPluginsPageView,
                InstalledPluginsTreePageMenu
            >(InstalledPluginsPageViewModel.PageId);
            return this;
        }

        private void EnsurePluginSettingsGroup()
        {
            if (_isPluginSettingsGroupRegistered)
            {
                return;
            }

            builder.AppBuilder.Services.AddKeyedTransient<
                ITreePageMenuItem,
                PluginSettingsTreePageMenu
            >(SettingsPageViewModel.PageId);
            _isPluginSettingsGroupRegistered = true;
        }
    }
}
