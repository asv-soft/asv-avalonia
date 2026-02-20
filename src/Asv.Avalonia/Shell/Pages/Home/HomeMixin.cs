using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class HomeMixin
{
    extension(ShellMixin.PageBuilder builder)
    {
        public ShellMixin.PageBuilder UseDefaultHomePage(Action<Builder>? configure = null)
        {
            configure ??= b => b.UseDefault();
            configure(new Builder(builder));
            builder.Register<HomePageViewModel, HomePageView>(HomePageViewModel.PageId);
            return builder;
        }

        public Builder Home => new Builder(builder);
    }

    public class Builder(ShellMixin.PageBuilder builder)
    {
        public Builder UseDefault()
        {
            builder.Parent.Parent.Extensions.Register<IHomePage, HomePageDefaultToolsExtension>();
            return this;
        }

        public Builder RegisterExtension<TExtension>()
            where TExtension : class, IExtensionFor<IHomePage>
        {
            builder.Parent.Parent.Extensions.Register<IHomePage, TExtension>();
            return this;
        }

        public Builder RegisterTool<TActionViewModel>()
            where TActionViewModel : class, IActionViewModel
        {
            builder.Parent.Parent.Services.AddKeyedTransient<IActionViewModel, TActionViewModel>(
                HomePageDefaultToolsExtension.Contract
            );
            return this;
        }
    }
}
