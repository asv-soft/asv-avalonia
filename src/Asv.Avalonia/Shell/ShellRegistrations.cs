using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ShellRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder Shell => new(builder);

        public IHostApplicationBuilder RegisterMobileShell(Action<Builder>? configure = null)
        {
            if (builder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeShell(configure);
            }

            builder.RegisterShell(configure);
            builder.ViewModel.Register<IShell, MobileShellViewModel>();
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }

        public IHostApplicationBuilder RegisterDesktopShell(Action<Builder>? configure = null)
        {
            if (builder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeShell(configure);
            }

            builder.RegisterShell(configure);
            builder.ViewModel.Register<IShell, DesktopShellViewModel>();
            builder
                .Services.AddTransient<ShellWindow>()
                .AddTransient<IDebugWindow, DebugWindowViewModel>();

            builder.ViewLocator.RegisterViewFor<DebugWindowViewModel, DebugWindow>();
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }

        private IHostApplicationBuilder RegisterDesignTimeShell(Action<Builder>? configure = null)
        {
            builder.RegisterShell(configure);
            builder.ViewModel.Register<IShell, DesignTimeShellViewModel>();
            configure?.Invoke(new Builder(builder));
            return builder;
        }

        private IHostApplicationBuilder RegisterShell(Action<Builder>? configure = null)
        {
            builder.Extensions.Register<IShell, MainMenuDefaultMenuExtender>();
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder;

        public Builder RegisterDefault()
        {
            this.RegisterStatus();
            this.RegisterMainMenu();
            this.RegisterPages();
            return this;
        }
    }
}
