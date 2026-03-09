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
            builder.AddPostConfigureCallbacks(builder => loader.InitPlugins(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder, PluginBootloaderOptions pluginOptions)
    {
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
            builder.Shell.Pages.Home.UseExtension<HomePagePluginsMarketExtension>();

            builder.Commands.Register<OpenPluginsMarketCommand>();
            builder.Shell.Pages.Settings.AddSubPage<
                SettingsPluginsSourcesViewModel,
                SettingsPluginsSourcesView,
                SettingsPluginsTreePageMenu
            >(SettingsPluginsSourcesViewModel.PageId);
            builder.ViewLocator.RegisterViewFor<SourceDialogViewModel, SourceDialogView>();
            builder.Shell.Pages.Register<PluginsMarketPageViewModel, PluginsMarketPageView>(
                PluginsMarketPageViewModel.PageId
            );
            return this;
        }

        public IHostApplicationBuilder Parent => builder;

        public Builder UseOptionalInstalled()
        {
            builder.Shell.Pages.Register<InstalledPluginsPageViewModel, InstalledPluginsPageView>(
                InstalledPluginsPageViewModel.PageId
            );
            builder.Shell.Pages.Home.UseExtension<HomePageInstalledPluginsExtension>();

            builder.Commands.Register<OpenInstalledPluginsCommand>();
            return this;
        }
    }
}
