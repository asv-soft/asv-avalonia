using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class TreePageMixin
{
    extension(ViewLocatorMixin.Builder builder)
    {
        public ViewLocatorMixin.Builder RegisterTreePage()
        {
            builder.RegisterViewFor<GroupTreePageItemViewModel, GroupTreePageItemView>();
            return builder;
        }
    }
    
    extension(ShellMixin.Builder builder)
    {
        public Builder TreeSubPages => new(builder);
    }
    
    extension(IServiceProvider services)
    {
        public TTreeSubpage CreateTreeSubPage<TContext, TTreeSubpage>(string subPageId, ITreeSubPageContext<TContext> context)
            where TContext : class, ITreePageViewModel
            where TTreeSubpage : ITreeSubpage
        {
            return services.CreateViewModel<TTreeSubpage, ITreeSubPageContext<TContext>>(subPageId, context);
        }
    }
    
    public class Builder(ShellMixin.Builder builder)
    {
        public Builder Register<TContext, TSubPageViewModel, TSubPageView>(string pageId)
            where TSubPageViewModel : class, ITreeSubpage
            where TSubPageView : Control
            where TContext : class, ITreePageViewModel
        {
            builder.Parent.ViewModel.RegisterKeyedWithArgs<ITreeSubpage, TSubPageViewModel, ITreeSubPageContext<TContext>>(pageId);
            builder.Parent.ViewLocator.RegisterViewFor<TSubPageViewModel, TSubPageView>();
            return this;
        }

        public ShellMixin.Builder Parent => builder;
    }
}