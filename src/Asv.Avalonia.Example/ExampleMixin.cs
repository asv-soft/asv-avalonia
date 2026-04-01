using System;
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
            builder.Shell.Pages.ControlGallery.UseDefault();
            builder.Extensions.Register<IHomePageItem, DeviceActionExample>();
            builder.FileAssociation.Register<ExampleFileHandler>();
            builder.ModuleIo.RegisterDevice<ExampleDeviceManagerExtension>();

            return builder;
        }
    }

    extension(ShellMixin.PageBuilder builder)
    {
        public ControlGalleryBuilder ControlGallery => new ControlGalleryBuilder(builder);
        public MapTestBuilder MapTest => new MapTestBuilder(builder);
    }

    public class ControlGalleryBuilder(ShellMixin.PageBuilder builder)
    {
        public ControlGalleryBuilder UseDefault()
        {
            builder.Parent.Parent.Services.AddSingleton<
                IAsyncCommand,
                OpenControlsGalleryPageCommand
            >();
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

        public ControlGalleryBuilder UseSubPage<TViewModel, TView>(string subPageId)
            where TViewModel : class, IControlsGallerySubPage
            where TView : Control
        {
            builder.Parent.Parent.Services.AddKeyedTransient<IControlsGallerySubPage, TViewModel>(
                subPageId
            );
            builder.Parent.Parent.ViewLocator.RegisterViewFor<TViewModel, TView>();
            return this;
        }

        public ControlGalleryBuilder UseSubPage<TViewModel, TView, TTreeMenu>(string subPageId)
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

    public class MapTestBuilder(ShellMixin.PageBuilder builder)
    {
        public MapTestBuilder UseDefault()
        {
            builder.Parent.Parent.Services.AddSingleton<IAsyncCommand, OpenMapTestPageCommand>();
            builder.Parent.Parent.Extensions.Register<IHomePage, HomePageMapTestPageExtension>();
            builder.Parent.Parent.Shell.Pages.Register<MapTestPageViewModel, MapTestPageView>(
                MapTestPageViewModel.PageId
            );
            return this;
        }
    }
}
