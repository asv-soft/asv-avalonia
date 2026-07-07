using System.Diagnostics;
using System.Reflection;
using Asv.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Plugins;

public static class PluginsMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseModulePlugins(Action<Builder>? configure = null)
        {
            // we need to create bootloader before service configured
            // cause plugin may want to build or replace some Services
            var pluginOptions =
                builder
                    .Configuration.GetSection(PluginBootloaderOptions.SectionName)
                    .Get<PluginBootloaderOptions>()
                ?? new PluginBootloaderOptions();
            configure ??= b => b.UseDefault();
            configure(new Builder(builder, pluginOptions));
            var loader = new PluginBootloader(Options.Create(pluginOptions), builder.Environment);
            builder.Services.AddSingleton<IPluginBootloader>(loader);
            builder.AddPostConfigureCallbacks(loader.InitPlugins);
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder, PluginBootloaderOptions pluginOptions)
    {
        private bool _isPluginSettingsGroupRegistered;

        public Builder UseDefault()
        {
            return UseOptionalMarket().UseOptionalInstalled();
        }

        public Builder WithApiPackage(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            Options.ApiPackageName =
                assembly.GetName().Name ?? throw new InvalidOperationException();
            var attributes = assembly.GetCustomAttributes(
                typeof(AssemblyInformationalVersionAttribute),
                false
            );

            ArgumentNullException.ThrowIfNull(attributes);
            if (attributes.Length == 0)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var nameAttribute = (AssemblyInformationalVersionAttribute)attributes[0];
            ArgumentException.ThrowIfNullOrEmpty(nameAttribute.InformationalVersion);

            Options.ApiVersion = SemVersion.Parse(nameAttribute.InformationalVersion).ToString();

            return this;
        }

        public PluginBootloaderOptions Options => pluginOptions;

        public Builder UseOptionalMarket()
        {
            EnsurePluginSettingsGroup();
            builder.Shell.Pages.Settings.AddSubPage<
                PluginsMarketPageViewModel,
                PluginsMarketPageView,
                PluginsMarketTreePageMenu
            >(PluginsMarketPageViewModel.PageId);
            builder.Shell.Pages.Settings.AddSubPage<
                SettingsPluginsSourcesViewModel,
                SettingsPluginsSourcesView,
                SettingsPluginsTreePageMenu
            >(SettingsPluginsSourcesViewModel.PageId);
            builder.ViewLocator.RegisterViewFor<SourceDialogViewModel, SourceDialogView>();
            return this;
        }

        public IHostApplicationBuilder Parent => builder;

        public Builder UseOptionalInstalled()
        {
            EnsurePluginSettingsGroup();
            builder.Shell.Pages.Settings.AddSubPage<
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

            builder.Services.AddKeyedTransient<ITreePageMenuItem, PluginSettingsTreePageMenu>(
                SettingsPageViewModel.PageId
            );
            _isPluginSettingsGroupRegistered = true;
        }
    }
}
