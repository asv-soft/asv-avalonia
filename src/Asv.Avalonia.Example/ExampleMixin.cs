using System;
using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia.IO;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Example;

public static class ExampleMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseExampleApp(Action<Builder>? configure = null)
        {
            configure ??= b => b.UseDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseDefault()
        {
#if DEBUG
            builder.Shell.Pages.MapTest.UseDefault();
#endif
            builder.Shell.Pages.TextFile.UseDefault();
            builder.Shell.Pages.ControlGallery.UseDefault();
            builder.Extensions.Register<IHomePageItem, DeviceActionExample>();
            builder.FileAssociation.Register<TextFileHandler>();
            builder.ModuleIo.RegisterDevice<ExampleDeviceManagerExtension>();
            return builder;
        }
    }

    extension(PageMixin.Builder builder)
    {
        public ControlGalleryBuilder ControlGallery => new ControlGalleryBuilder(builder);
        public MapTestBuilder MapTest => new MapTestBuilder(builder);
        public TextFileBuilder TextFile => new TextFileBuilder(builder);
    }

    public class ControlGalleryBuilder(PageMixin.Builder builder)
    {
        public ControlGalleryBuilder UseDefault()
        {
            builder.Parent.Parent.Extensions.Register<
                IHomePage,
                HomePageControlsGalleryPageExtension
            >();
            builder.Parent.Parent.ViewLocator.RegisterViewFor<
                DialogItemImageViewModel,
                DialogItemImageView
            >();
            builder.Parent.Parent.Extensions.Register<IShell, ShellLeftMenuExtenderExample>();
            builder.Parent.Parent.Extensions.Register<
                IControlsGalleryPage,
                ControlsGalleryPageExtension
            >();
            builder.Parent.Parent.Shell.Pages.Register<
                ControlsGalleryPageViewModel,
                ControlsGalleryPageView
            >(ControlsGalleryPageViewModel.PageId);
            UseDialogSubPage()
                .UseHistoricalSubPage()
                .UseInfoBoxSubPage()
                .UseMapControlsSubPage()
                .UseMarkdownSubPage()
                .UsePropertyEditorSubPage()
                .UseWorkspaceSubPage()
                .UseRttBoxesSubPage();
            return this;
        }

        public ControlGalleryBuilder UseDialogSubPage()
        {
            return UseSubPage<
                DialogControlsPageViewModel,
                DialogControlsPageView,
                DialogControlsTreeMenu
            >(DialogControlsPageViewModel.PageId);
        }

        public ControlGalleryBuilder UseHistoricalSubPage()
        {
            return UseSubPage<
                HistoricalControlsPageViewModel,
                HistoricalControlsPageView,
                HistoricalControlsTreeMenu
            >(HistoricalControlsPageViewModel.PageId);
        }

        public ControlGalleryBuilder UseInfoBoxSubPage()
        {
            return UseSubPage<
                InfoBoxControlsPageViewModel,
                InfoBoxControlsPageView,
                InfoBoxControlsTreeMenu
            >(InfoBoxControlsPageViewModel.PageId);
        }

        public ControlGalleryBuilder UseMapControlsSubPage()
        {
            return UseSubPage<MapControlsPageViewModel, MapControlsPageView, MapControlsTreeMenu>(
                MapControlsPageViewModel.PageId
            );
        }

        public ControlGalleryBuilder UseMarkdownSubPage()
        {
            return UseSubPage<MarkdownPageViewModel, MarkdownPageView, MarkdownTreeMenu>(
                MarkdownPageViewModel.PageId
            );
        }

        public ControlGalleryBuilder UsePropertyEditorSubPage()
        {
            return UseSubPage<
                PropertyEditorPageViewModel,
                PropertyEditorPageView,
                PropertyEditorTreeMenu
            >(PropertyEditorPageViewModel.PageId);
        }

        public ControlGalleryBuilder UseWorkspaceSubPage()
        {
            return UseSubPage<WorkspacePageViewModel, WorkspacePageView, WorkspaceTreeMenu>(
                WorkspacePageViewModel.PageId
            );
        }

        public ControlGalleryBuilder UseRttBoxesSubPage()
        {
            return UseSubPage<RttBoxesPageViewModel, RttBoxesPageView, RttBoxesTreeMenu>(
                RttBoxesPageViewModel.PageId
            );
        }

        public ControlGalleryBuilder UseSubPage<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView
        >(string subPageId)
            where TViewModel : class, IControlsGallerySubPage
            where TView : Control
        {
            builder.Parent.Parent.Shell.TreeSubPages.Register<
                IControlsGalleryPage,
                IControlsGallerySubPage,
                TViewModel,
                TView
            >(subPageId);

            return this;
        }

        public ControlGalleryBuilder UseSubPage<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TTreeMenu
        >(string subPageId)
            where TTreeMenu : class, ITreePage
            where TViewModel : class, IControlsGallerySubPage
            where TView : Control
        {
            builder.Parent.Parent.Services.AddKeyedTransient<ITreePage, TTreeMenu>(
                ControlsGalleryPageExtension.Contract
            );
            return UseSubPage<TViewModel, TView>(subPageId);
        }
    }

    public class MapTestBuilder(PageMixin.Builder builder)
    {
        public MapTestBuilder UseDefault()
        {
            builder.Parent.Parent.Extensions.Register<IHomePage, HomePageMapTestPageExtension>();
            builder.Parent.Parent.Shell.Pages.Register<MapTestPageViewModel, MapTestPageView>(
                MapTestPageViewModel.PageId
            );
            return this;
        }
    }

    public class TextFileBuilder(PageMixin.Builder builder)
    {
        public TextFileBuilder UseDefault()
        {
            builder.Parent.Parent.Shell.Pages.Register<TextFilePageViewModel, TextFilePageView>(
                TextFilePageViewModel.PageId
            );
            return this;
        }
    }
}
