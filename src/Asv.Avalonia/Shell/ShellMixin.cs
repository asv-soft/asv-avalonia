using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ShellMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public ShellBuilder Shell => new(builder);

        private IHostApplicationBuilder UseShell(Action<ShellBuilder>? configure = null)
        {
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeShell(Action<ShellBuilder>? configure = null)
        {
            builder.UseShell().Services.AddTransient<IShell, DesignTimeShellViewModel>();
            configure?.Invoke(new ShellBuilder(builder));
            return builder;
        }

        public IHostApplicationBuilder UseMobileShell(Action<ShellBuilder>? configure = null)
        {
            builder.UseShell().Services.AddTransient<IShell, MobileShellViewModel>();
            configure?.Invoke(new ShellBuilder(builder));
            return builder;
        }

        public IHostApplicationBuilder UseDesktopShell(Action<ShellBuilder>? configure = null)
        {
            builder
                .UseShell()
                .Services.AddTransient<IShell, DesktopShellViewModel>()
                .AddTransient<ShellWindow>()
                .AddTransient<IDebugWindow, DebugWindowViewModel>();

            builder.ViewLocator.RegisterViewFor<DebugWindowViewModel, DebugWindow>();
            configure?.Invoke(new ShellBuilder(builder));
            return builder;
        }
    }

    public class StatusBuilder(ShellBuilder builder)
    {
        public StatusBuilder Register<TStatusViewModel, TView>()
            where TStatusViewModel : class, IStatusItem
            where TView : Control
        {
            // register status item
            builder.Parent.Services.AddTransient<IStatusItem, TStatusViewModel>();
            builder.Parent.ViewLocator.RegisterViewFor<TStatusViewModel, TView>();
            return this;
        }

        public StatusBuilder UseNavigationStatus()
        {
            Register<NavigationStatusItemViewModel, NavigationStatusItemView>();
            return this;
        }
    }

    public class PageBuilder(ShellBuilder builder)
    {
        public PageBuilder Register<TPageViewModel, TPageView>(string pageId)
            where TPageViewModel : class, IPage
            where TPageView : Control
        {
            builder.Parent.Services.AddKeyedTransient<IPage, TPageViewModel>(pageId);
            builder.Parent.ViewLocator.RegisterViewFor<TPageViewModel, TPageView>();
            return this;
        }

        public ShellBuilder Parent => builder;
    }

    public class ShellBuilder
    {
        private readonly IHostApplicationBuilder _builder;

        public ShellBuilder(IHostApplicationBuilder builder)
        {
            _builder = builder;
            Status = new StatusBuilder(this);
            Pages = new PageBuilder(this);
        }

        public StatusBuilder Status { get; }
        public PageBuilder Pages { get; }
        public IHostApplicationBuilder Parent => _builder;
    }
}
