using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.InfoMessage;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class WorkspacePageViewModel : ControlsGallerySubPage
{
    private readonly ILoggerFactory _loggerFactory;
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
        _loggerFactory = loggerFactory;
        var hideAll = new MenuItem("action1", "Hide all", loggerFactory)
        {
            Command = new ReactiveCommand(x =>
            {
                if (_itemsSource != null)
                {
                    foreach (var workspaceWidget in _itemsSource)
                    {
                        workspaceWidget.IsVisible = false;
                    }
                }
            }),
        };

        var showAll = new MenuItem("action2", "Show all", loggerFactory)
        {
            Command = new ReactiveCommand(x =>
            {
                if (_itemsSource != null)
                {
                    foreach (var workspaceWidget in _itemsSource)
                    {
                        workspaceWidget.IsVisible = true;
                    }
                }
            }),
        };
        var showError = new MenuItem("action3", "Show error", loggerFactory)
        {
            Command = new ReactiveCommand(x =>
            {
                this.RaiseShellInfoMessage(
                    new ShellMessage(
                        "Error",
                        "This is test error message",
                        ShellErrorState.Error,
                        "This is description for test error message",
                        MaterialIconKind.Cross,
                        showAll.Command,
                        null,
                        commandTitle: "ShowAll",
                        TimeSpan.FromSeconds(5)
                    )
                );
            }),
        };
        Menu.Add(showAll);
        Menu.Add(hideAll);
        Menu.Add(showError);
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
                    )
                    {
                        Header = "Location 1",
                    },
                    new GeoPointPropertyViewModel(
                        "location1",
                        new ReactiveProperty<GeoPoint>(),
                        loggerFactory,
                        unitService
                    )
                    {
                        Header = "Location 2",
                    },
                },
            },
            new WrapPanelWidgetViewModel("rtt-box", loggerFactory)
            {
                Position = WorkspaceDock.Right,
                ItemsSource =
                {
                    new SingleRttBoxViewModel("rtt-single-1", loggerFactory)
                    {
                        Header = "Single RTT 1",
                        ShortHeader = "RTT1",
                        ValueString = "15.25",
                    },
                    new SingleRttBoxViewModel("rtt-single-2", loggerFactory)
                    {
                        Header = "Single RTT 2",
                        ShortHeader = "RTT2",
                        ValueString = "15.25",
                    },
                },
            },
            new WrapPanelWidgetViewModel("rtt-box2", loggerFactory)
            {
                Position = WorkspaceDock.Right,
                ItemsSource =
                {
                    new SingleRttBoxViewModel("rtt-single-1", loggerFactory)
                    {
                        Header = "Single RTT 1",
                        ShortHeader = "RTT1",
                        ValueString = "15.25",
                    },
                    new SingleRttBoxViewModel("rtt-single-2", loggerFactory)
                    {
                        Header = "Single RTT 2",
                        ShortHeader = "RTT2",
                        ValueString = "15.25",
                    },
                },
            },
            new MapWidget("map", loggerFactory)
            {
                Position = WorkspaceDock.Bottom,
                Icon = MaterialIconKind.Map,
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
                Header = "Map Widget",
                IsExpanded = true,
                CanExpand = true,
            },
            new MapWidget("ma2p", loggerFactory)
            {
                Position = WorkspaceDock.Bottom,
                Icon = MaterialIconKind.Map,
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
                Header = "Map Widget2",
                IsExpanded = true,
                CanExpand = true,
                Anchors =
                {
                    new MapAnchor<IMapAnchor>("drone1", loggerFactory)
                    {
                        Title = "Drone 1",
                        Location = new GeoPoint(53.0, 53.0, 100),
                        Icon = MaterialIconKind.Airplane,
                    },
                },
            },
        ];
        _itemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        _itemsSource.SetRoutableParent(this).DisposeItWith(Disposable);
        Items = _itemsSource.ToNotifyCollectionChangedSlim();
    }

    public override ValueTask Init(IControlsGalleryPage context)
    {
        var changeStatus = new MenuItem("action5", "Change status", _loggerFactory)
        {
            Command = new ReactiveCommand(x =>
            {
                if (context.Status == null)
                {
                    context.ChangeStatus(MaterialIconKind.Pencil, AsvColorKind.Error);
                }
                else
                {
                    context.ChangeStatus(null, AsvColorKind.None);
                }
            }),
        };
        Menu.Add(changeStatus);
        return base.Init(context);
    }

    public NotifyCollectionChangedSynchronizedViewList<IWorkspaceWidget> Items { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var item in _itemsSource)
        {
            yield return item;
        }

        foreach (var item in base.GetRoutableChildren())
        {
            yield return item;
        }
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is PageCloseAttemptEvent close)
        {
            close.AddRestriction(new Restriction(this, "Test restriction for close"));
        }
        return base.InternalCatchEvent(e);
    }

    public override IExportInfo Source => SystemModule.Instance;
}
