using System.Collections.Generic;
using System.Composition;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class MapControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "map_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.Map;

    public MapControlsPageViewModel()
        : this(NullLayoutService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public MapControlsPageViewModel(ILayoutService layoutService, ILoggerFactory loggerFactory)
        : base(PageId, layoutService, loggerFactory)
    {
        MapViewModel = new MapViewModel("Map", layoutService, loggerFactory)
            .DisposeItWith(Disposable)
            .SetRoutableParent(this);

        MapViewModel.Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        MapViewModel.Anchors.SetRoutableParent(this).DisposeItWith(Disposable);

        MapViewModel.Anchors.Add(
            new MapAnchor<IMapAnchor>("1", layoutService, loggerFactory)
            {
                Icon = MaterialIconKind.Navigation,
            }
        );
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return MapViewModel;

        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }
    }

    public MapViewModel MapViewModel { get; }

    public override IExportInfo Source => SystemModule.Instance;
}
