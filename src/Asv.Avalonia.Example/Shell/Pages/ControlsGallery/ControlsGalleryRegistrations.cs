using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Example;

public static class ControlsGalleryRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public Builder ControlsGallery => new(builder);

        public PagesRegistrations.Builder RegisterControlsGallery(Action<Builder>? configure = null)
        {
            builder.AppBuilder.Extensions.Register<
                IHomePage,
                HomePageControlsGalleryPageExtension
            >();
            builder.AppBuilder.Extensions.Register<IShell, ShellLeftMenuExtenderExample>();
            builder.AppBuilder.Extensions.Register<
                IControlsGalleryPage,
                ControlsGalleryPageExtension
            >();
            builder.AppBuilder.Pages.Register<
                ControlsGalleryPageViewModel,
                ControlsGalleryPageView
            >(ControlsGalleryPageViewModel.PageId);

            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(PagesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            RegisterDialogSubPage()
                .RegisterHistoricalSubPage()
                .RegisterInfoBoxSubPage()
                .RegisterMapControlsSubPage()
                .RegisterMarkdownSubPage()
                .RegisterPropertyEditorSubPage()
                .RegisterWorkspaceSubPage()
                .RegisterRttBoxesSubPage();
            return this;
        }

        public Builder RegisterDialogSubPage()
        {
            return RegisterSubPage<
                DialogControlsPageViewModel,
                DialogControlsPageView,
                DialogControlsTreeMenu
            >(DialogControlsPageViewModel.PageId);
        }

        public Builder RegisterHistoricalSubPage()
        {
            return RegisterSubPage<
                HistoricalControlsPageViewModel,
                HistoricalControlsPageView,
                HistoricalControlsTreeMenu
            >(HistoricalControlsPageViewModel.PageId);
        }

        public Builder RegisterInfoBoxSubPage()
        {
            return RegisterSubPage<
                InfoBoxControlsPageViewModel,
                InfoBoxControlsPageView,
                InfoBoxControlsTreeMenu
            >(InfoBoxControlsPageViewModel.PageId);
        }

        public Builder RegisterMapControlsSubPage()
        {
            return RegisterSubPage<
                MapControlsPageViewModel,
                MapControlsPageView,
                MapControlsTreeMenu
            >(MapControlsPageViewModel.PageId);
        }

        public Builder RegisterMarkdownSubPage()
        {
            return RegisterSubPage<MarkdownPageViewModel, MarkdownPageView, MarkdownTreeMenu>(
                MarkdownPageViewModel.PageId
            );
        }

        public Builder RegisterPropertyEditorSubPage()
        {
            return RegisterSubPage<
                PropertyEditorPageViewModel,
                PropertyEditorPageView,
                PropertyEditorTreeMenu
            >(PropertyEditorPageViewModel.PageId);
        }

        public Builder RegisterWorkspaceSubPage()
        {
            return RegisterSubPage<WorkspacePageViewModel, WorkspacePageView, WorkspaceTreeMenu>(
                WorkspacePageViewModel.PageId
            );
        }

        public Builder RegisterRttBoxesSubPage()
        {
            return RegisterSubPage<RttBoxesPageViewModel, RttBoxesPageView, RttBoxesTreeMenu>(
                RttBoxesPageViewModel.PageId
            );
        }

        public Builder RegisterSubPage<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView
        >(string subPageId)
            where TViewModel : class, IControlsGallerySubPage
            where TView : Control
        {
            builder.AppBuilder.TreePage.Register<
                IControlsGalleryPage,
                IControlsGallerySubPage,
                TViewModel,
                TView
            >(subPageId);

            return this;
        }

        public Builder RegisterSubPage<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TTreeMenu
        >(string subPageId)
            where TTreeMenu : class, ITreePageMenuItem
            where TViewModel : class, IControlsGallerySubPage
            where TView : Control
        {
            builder.AppBuilder.Services.AddKeyedTransient<ITreePageMenuItem, TTreeMenu>(
                ControlsGalleryPageExtension.Contract
            );
            return RegisterSubPage<TViewModel, TView>(subPageId);
        }
    }
}
