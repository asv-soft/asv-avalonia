using System.Composition;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<ControlsGalleryPageViewModel>]
[method: ImportingConstructor]
public class ControlsGalleryPageExtension(ILoggerFactory loggerFactory)
    : AsyncDisposableOnce,
        IExtensionFor<ControlsGalleryPageViewModel>
{
    public void Extend(ControlsGalleryPageViewModel context, CompositeDisposable contextDispose)
    {
        context.Nodes.Add(
            new TreePage(
                DialogControlsPageViewModel.PageId,
                RS.DialogPageViewModel_Title,
                MaterialIconKind.Abacus,
                DialogControlsPageViewModel.PageId,
                NavigationId.Empty,
                DesignTime.LoggerFactory
            ).DisposeItWith(contextDispose)
        );
        context.Nodes.Add(
            new TreePage(
                HistoricalControlsPageViewModel.PageId,
                RS.HistoricalPageViewModel_Title,
                MaterialIconKind.Abacus,
                HistoricalControlsPageViewModel.PageId,
                NavigationId.Empty,
                DesignTime.LoggerFactory
            ).DisposeItWith(contextDispose)
        );
        context.Nodes.Add(
            new TreePage(
                InfoBoxControlsPageViewModel.PageId,
                RS.InfoBoxPageViewModel_Title,
                MaterialIconKind.Abacus,
                InfoBoxControlsPageViewModel.PageId,
                NavigationId.Empty,
                DesignTime.LoggerFactory
            ).DisposeItWith(contextDispose)
        );
        context.Nodes.Add(
            new TreePage(
                MapControlsPageViewModel.PageId,
                RS.MapExamplePageViewModel_Title,
                MaterialIconKind.Abacus,
                MapControlsPageViewModel.PageId,
                NavigationId.Empty,
                DesignTime.LoggerFactory
            ).DisposeItWith(contextDispose)
        );
    }
}
