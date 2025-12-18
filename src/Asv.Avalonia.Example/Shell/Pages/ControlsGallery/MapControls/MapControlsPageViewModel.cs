using System.Collections.Generic;
using System.Composition;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class MapControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "map_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.Map;

    public MapControlsPageViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public MapControlsPageViewModel(ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
    {
        MapViewModel = new MapViewModel("Map", loggerFactory)
            .DisposeItWith(Disposable)
            .SetRoutableParent(this);

        MapViewModel.Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        MapViewModel.Anchors.SetRoutableParent(this).DisposeItWith(Disposable);

        MapViewModel.Anchors.Add(
            new MapAnchor<IMapAnchor>("1", loggerFactory) { Icon = MaterialIconKind.Navigation }
        );
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return MapViewModel;

        foreach (var child in base.GetChildren())
        {
            yield return child;
        }
    }

    public MapViewModel MapViewModel { get; }

    public override IExportInfo Source => SystemModule.Instance;
}
