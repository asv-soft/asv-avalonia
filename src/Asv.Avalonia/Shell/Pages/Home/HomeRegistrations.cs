using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class HomeRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public Builder Home => new(builder);

        public PagesRegistrations.Builder RegisterHomePage(Action<Builder>? configure = null)
        {
            configure ??= b => b.UseDefault();
            configure(new Builder(builder));
            builder.Register<HomePageViewModel, HomePageView>(HomePageViewModel.PageId);
            return builder;
        }
    }

    public class Builder(PagesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder UseDefault()
        {
            builder.AppBuilder.Extensions.Register<IHomePage, HomePageDefaultToolsExtension>();

            return this;
        }

        public Builder UseExtension<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TExtension
        >()
            where TExtension : class, IExtensionFor<IHomePage>
        {
            builder.AppBuilder.Extensions.Register<IHomePage, TExtension>();
            return this;
        }

        public Builder UseTool<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TActionViewModel
        >()
            where TActionViewModel : class, IActionViewModel
        {
            builder.AppBuilder.Services.AddKeyedTransient<IActionViewModel, TActionViewModel>(
                HomePageDefaultToolsExtension.Contract
            );
            return this;
        }

        public Builder UseItemExtension<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TExtension
        >()
            where TExtension : class, IExtensionFor<IHomePageItem>
        {
            builder.AppBuilder.Extensions.Register<IHomePageItem, TExtension>();
            return this;
        }
    }
}
