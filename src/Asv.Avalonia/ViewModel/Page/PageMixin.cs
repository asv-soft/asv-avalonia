using System.Diagnostics;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class PageMixin
{
    extension(ShellMixin.Builder builder)
    {
        public Builder Pages => new(builder);
    }
    
    extension(IServiceProvider services)
    {
        public IPage CreatePage(string pageId, IPageContext context)
        {
            return services.CreateViewModel<IPage, IPageContext>(pageId, context);
        }
    }
    
    public class Builder(ShellMixin.Builder builder)
    {
        public Builder Register<TPageViewModel, TPageView>(string pageId)
            where TPageViewModel : class, IPage
            where TPageView : Control
        {
            builder.Parent.ViewModel.RegisterKeyedWithArgs<IPage, TPageViewModel, IPageContext>(pageId);
            builder.Parent.ViewLocator.RegisterViewFor<TPageViewModel, TPageView>();
            return this;
        }

        public ShellMixin.Builder Parent => builder;
    }
}