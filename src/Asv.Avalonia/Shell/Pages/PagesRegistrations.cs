using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class PagesRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder Pages => builder.Shell.Pages;
    }

    extension(ShellRegistrations.Builder builder)
    {
        public Builder Pages => new(builder);

        public ShellRegistrations.Builder RegisterPages(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ShellRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterHomePage();
            this.RegisterSettingsPage();
            this.RegisterLogViewerPage();
            return this;
        }

        public Builder Register<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TPageViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TPageView
        >(string pageId)
            where TPageViewModel : class, IPage
            where TPageView : Control
        {
            AppBuilder.ViewModel.RegisterKeyedWithArgs<IPage, TPageViewModel, IPageContext>(pageId);
            AppBuilder.ViewLocator.RegisterViewFor<TPageViewModel, TPageView>();
            return this;
        }
    }
}
