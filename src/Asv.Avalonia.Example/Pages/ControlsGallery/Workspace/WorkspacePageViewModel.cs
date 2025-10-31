using System.Composition;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class WorkspacePageViewModel : ControlsGallerySubPage
{
    private readonly ObservableList<IWorkspaceWidget> _itemsSource;
    public const string PageId = "wrokspace-example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Table;

    public WorkspacePageViewModel()
        : this(DesignTime.LoggerFactory, DesignTime.UnitService)
    {
        DesignTime.ThrowIfNotDesignMode();
        Parent = DesignTime.Shell;
    }

    [ImportingConstructor]
    public WorkspacePageViewModel(ILoggerFactory loggerFactory, IUnitService unitService)
        : base(PageId, loggerFactory)
    {
        _itemsSource =
        [
            new PropertyEditorWidgetViewModel("Poprerty editor", "prop-left", loggerFactory)
            {
                Position = WorkspaceDock.Left,
                ItemsSource =
                {
                    new GeoPointPropertyViewModel(
                        "location1",
                        new ReactiveProperty<GeoPoint>(),
                        loggerFactory,
                        unitService
                    ),
                    new GeoPointPropertyViewModel(
                        "location1",
                        new ReactiveProperty<GeoPoint>(),
                        loggerFactory,
                        unitService
                    ),
                },
                Menu = { new MenuItem("action1", "Action 1", loggerFactory) },
            },
        ];
        _itemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        _itemsSource.SetRoutableParent(this).DisposeItWith(Disposable);
        Items = _itemsSource.ToNotifyCollectionChangedSlim();
    }

    public NotifyCollectionChangedSynchronizedViewList<IWorkspaceWidget> Items { get; }

    public override IExportInfo Source => SystemModule.Instance;
}
