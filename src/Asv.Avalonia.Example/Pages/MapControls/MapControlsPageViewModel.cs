using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class MapControlsPageViewModel
    : TreeSubpage<ControlsGalleryPageViewModel>,
        IControlsGallerySubPage
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
        Anchors = new MapViewModel("Anchor", loggerFactory);
        Anchors.Anchors.Add(
            new MapAnchor<IMapAnchor>("1", loggerFactory)
            {
                Icon = MaterialIconKind.Navigation,
                Location = new GeoPoint(53, 53, 0),
            }
        );
    }

    public override ValueTask Init(ControlsGalleryPageViewModel context) => ValueTask.CompletedTask;

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override IExportInfo Source => SystemModule.Instance;
    public MapViewModel Anchors { get; }
}
