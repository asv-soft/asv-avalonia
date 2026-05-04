using System.Diagnostics;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ShellMixin
{
    extension(IHostApplicationBuilder builder)
    {
        private IHostApplicationBuilder UseShell(Action<Builder>? configure = null)
        {
            builder.Extensions.Register<IShell, MainMenuDefaultMenuExtender>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeShell(Action<Builder>? configure = null)
        {
            builder.UseShell().Services.AddTransient<IShell, DesignTimeShellViewModel>();
            configure?.Invoke(new Builder(builder));
            return builder;
        }

        public IHostApplicationBuilder UseMobileShell(Action<Builder>? configure = null)
        {
            builder.UseShell().Services.AddTransient<IShell, MobileShellViewModel>();
            configure?.Invoke(new Builder(builder));
            return builder;
        }

        public IHostApplicationBuilder UseDesktopShell(Action<Builder>? configure = null)
        {
            builder
                .UseShell()
                .Services.AddTransient<IShell, DesktopShellViewModel>()
                .AddTransient<ShellWindow>()
                .AddTransient<IDebugWindow, DebugWindowViewModel>();

            builder.ViewLocator.RegisterViewFor<DebugWindowViewModel, DebugWindow>();
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }

        public Builder Shell => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder Parent => builder;

        public Builder RegisterDefault()
        {
            this.MainMenu.UseDefault();
            this.Status.UseNavigationStatus();
            this.Pages.UseDefaultHomePage().UseSettingsPage();
            return this;
        }
    }
}
