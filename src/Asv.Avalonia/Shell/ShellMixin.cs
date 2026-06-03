using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ShellMixin
{
    extension(IViewModel sender)
    {
        public ValueTask GoTo(NavPath path)
        {
            var rootId = new NavId(ShellViewModel.TypeId);

            NavPath fullPath;
            if (path.Count > 0 && path[0] == rootId)
            {
                fullPath = path;
            }
            else
            {
                var items = new List<NavId> { rootId };
                items.AddRange(path);
                fullPath = new NavPath(items);
            }

            if (sender is IShell shell)
            {
                return GoToShell(shell, fullPath);
            }

            return sender.Events.Rise(new NavigateEvent<IViewModel>(sender, fullPath));
        }
    }

    private static async ValueTask GoToShell(IShell shell, NavPath path)
    {
        await shell.Navigation.GoTo(path);
    }

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
            this.Status.UseDefault().UseNavigationStatus();
            this.Pages.UseDefaultHomePage().UseSettingsPage();
            return this;
        }
    }
}
