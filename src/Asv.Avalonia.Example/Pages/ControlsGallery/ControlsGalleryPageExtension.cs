using System.Composition;
using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IControlsGalleryPage>]
[method: ImportingConstructor]
public class ControlsGalleryPageExtension(ILoggerFactory loggerFactory)
    : IExtensionFor<IControlsGalleryPage>
{
    public void Extend(IControlsGalleryPage context, CompositeDisposable contextDispose)
    {
        context.Nodes.Add(
            new TreePage(
                DialogControlsPageViewModel.PageId,
                RS.DialogControlsPageViewModel_Title,
                DialogControlsPageViewModel.PageIcon,
                DialogControlsPageViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );

        context.Nodes.Add(
            new TreePage(
                HistoricalControlsPageViewModel.PageId,
                RS.HistoricalControlsPageViewModel_Title,
                HistoricalControlsPageViewModel.PageIcon,
                HistoricalControlsPageViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );

        context.Nodes.Add(
            new TreePage(
                InfoBoxControlsPageViewModel.PageId,
                RS.InfoBoxControlsPageViewModel_Title,
                InfoBoxControlsPageViewModel.PageIcon,
                InfoBoxControlsPageViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );

        context.Nodes.Add(
            new TreePage(
                MapControlsPageViewModel.PageId,
                RS.MapControlsPageViewModel_Title,
                MapControlsPageViewModel.PageIcon,
                MapControlsPageViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );

        context.Nodes.Add(
            new TreePage(
                PropertyEditorPageViewModel.PageId,
                "Property Editor",
                PropertyEditorPageViewModel.PageIcon,
                PropertyEditorPageViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );

        context.Nodes.Add(
            new TreePage(
                WorkspacePageViewModel.PageId,
                "Workspace",
                WorkspacePageViewModel.PageIcon,
                WorkspacePageViewModel.PageId,
                NavigationId.Empty,
                loggerFactory
            ).DisposeItWith(contextDispose)
        );
    }
}
