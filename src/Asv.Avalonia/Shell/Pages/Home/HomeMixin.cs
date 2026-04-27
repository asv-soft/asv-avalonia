using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class HomeMixin
{
    extension(PageMixin.Builder builder)
    {
        public PageMixin.Builder UseDefaultHomePage(Action<Builder>? configure = null)
        {
            configure ??= b => b.UseDefault();
            configure(new Builder(builder));
            builder.Register<HomePageViewModel, HomePageView>(HomePageViewModel.PageId);
            return builder;
        }

        public Builder Home => new(builder);
    }

    public class Builder(PageMixin.Builder builder)
    {
        public Builder UseDefault()
        {
            builder.Parent.Parent.Extensions.Register<IHomePage, HomePageDefaultToolsExtension>();
            return this;
        }

        public Builder UseExtension<TExtension>()
            where TExtension : class, IExtensionFor<IHomePage>
        {
            builder.Parent.Parent.Extensions.Register<IHomePage, TExtension>();
            return this;
        }

        public Builder UseTool<TActionViewModel>()
            where TActionViewModel : class, IActionViewModel
        {
            builder.Parent.Parent.Services.AddKeyedTransient<IActionViewModel, TActionViewModel>(
                HomePageDefaultToolsExtension.Contract
            );
            return this;
        }

        public Builder UseItemExtension<TExtension>()
            where TExtension : class, IExtensionFor<IHomePageItem>
        {
            builder.Parent.Parent.Extensions.Register<IHomePageItem, TExtension>();
            return this;
        }
    }
}
