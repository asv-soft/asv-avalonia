using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Plugins;

public static class PluginsMixin
{
    extension(SettingsPageMixin.Builder builder)
    {
        public SettingsPageMixin.Builder UsePluginsSettings()
        {
            builder
                .AddSubPage<SettingsPluginsSourcesViewModel, SettingsPluginsSourcesView, SettingsPluginsTreePageMenu>(
                    SettingsPluginsSourcesViewModel.PageId);
            builder.Parent.Parent.Parent.ViewLocator.RegisterViewFor<SourceDialogViewModel, SourceDialogView>();
            
            return builder;
        }
    }
    
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UsePlugins(Action<Builder>? configure = null)
        {
            builder.Services.AddSingleton<IPluginManager, PluginManager>();
            configure ??= b => b.UseDefault();
            configure(new Builder(builder));
            return builder;
        }
    }
    
    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder UseDefault()
        {
            builder.Shell.Pages.Settings.UsePluginsSettings();
            return UseMarket()
                .UseInstalled();
        }

        private Builder UseInstalled()
        {
            builder.Shell.Pages.Home.RegisterExtension<HomePagePluginsMarketExtension>();
            builder.Shell.Pages
                .Register<InstalledPluginsPageViewModel, InstalledPluginsPageView>(InstalledPluginsPageViewModel.PageId);
            return this;
        }

        public IHostApplicationBuilder Parent => builder;

        public Builder UseMarket()
        {
            builder.Shell.Pages.Home.RegisterExtension<HomePageInstalledPluginsExtension>();
            builder.Shell.Pages
                .Register<PluginsMarketPageViewModel, PluginsMarketPageView>(PluginsMarketPageViewModel.PageId);
            return this;
        }
    }
}