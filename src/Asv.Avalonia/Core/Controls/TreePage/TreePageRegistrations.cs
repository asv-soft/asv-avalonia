using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class TreePageRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder TreePage => builder.Core.Controls.TreePage;
    }

    extension(ControlsRegistrations.Builder builder)
    {
        public Builder TreePage => new(builder);

        public ControlsRegistrations.Builder RegisterTreePage(Action<Builder>? configure = null)
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                GroupTreePageItemViewModel,
                GroupTreePageItemView
            >();
            return builder;
        }
    }

    public class Builder(ControlsRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder Register<
            TContext,
            TTreeSubpage,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TSubPageViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TSubPageView
        >(string pageId)
            where TTreeSubpage : class, ITreeSubpage
            where TSubPageViewModel : class, TTreeSubpage
            where TSubPageView : Control
            where TContext : class, ITreePageViewModel
        {
            builder.AppBuilder.ViewModel.RegisterKeyedWithArgs<
                TTreeSubpage,
                TSubPageViewModel,
                ITreeSubPageContext<TContext>
            >(pageId);
            builder.AppBuilder.ViewLocator.RegisterViewFor<TSubPageViewModel, TSubPageView>();
            return this;
        }
    }
}
