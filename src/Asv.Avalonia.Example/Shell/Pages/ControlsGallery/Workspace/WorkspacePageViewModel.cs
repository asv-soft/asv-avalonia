using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.InfoMessage;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public class WorkspacePageViewModel : ControlsGallerySubPage
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<IWorkspaceWidget> _itemsSource;
    public const string PageId = "wrokspace_example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Table;

    public WorkspacePageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance, 
            DesignTime.LoggerFactory, 
            DesignTime.UnitService, 
            NullMapService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        Parent = DesignTime.Shell;
        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public WorkspacePageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        IMapService mapService
    )
        : base(PageId, context)
    {
        Init(context.Context);
        _loggerFactory = loggerFactory;
        MapViewModel = new MapViewModel($"{PageId}.map", mapService).DisposeItWith(Disposable);
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
            Command = new ReactiveCommand(
                async (_, cancel) =>
                {
                    await this.RaiseShellInfoMessage(
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
                        ),
                        cancel
                    );
                }
            ),
        };
        Menu.Add(showAll);
        Menu.Add(hideAll);
        Menu.Add(showError);
        _itemsSource =
        [
            new PropertyEditorWidgetViewModel("prop_left", "Property editor", loggerFactory)
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
                    new SingleRttBoxViewModel("rtt-single-1")
                    {
                        Header = "Single RTT 1",
                        ShortHeader = "RTT1",
                        ValueString = "15.25",
                    },
                    new SingleRttBoxViewModel("rtt-single-2")
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
                    new SingleRttBoxViewModel("rtt-single-1")
                    {
                        Header = "Single RTT 1",
                        ShortHeader = "RTT1",
                        ValueString = "15.25",
                    },
                    new SingleRttBoxViewModel("rtt-single-2")
                    {
                        Header = "Single RTT 2",
                        ShortHeader = "RTT2",
                        ValueString = "15.25",
                    },
                },
            },
            new MapWidget("map", loggerFactory, mapService)
            {
                Position = WorkspaceDock.Bottom,
                Icon = MaterialIconKind.Map,
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
                Header = "Map Widget",
                IsExpanded = true,
                CanExpand = true,
            },
            new MapWidget("ma2p", loggerFactory, mapService)
            {
                Position = WorkspaceDock.Bottom,
                Icon = MaterialIconKind.Map,
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
                Header = "Map Widget2",
                IsExpanded = true,
                CanExpand = true,
                Anchors =
                {
                    new MapAnchor<IMapAnchor>("drone1")
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

    public void Init(IControlsGalleryPage context)
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
    }

    public NotifyCollectionChangedSynchronizedViewList<IWorkspaceWidget> Items { get; }

    public MapViewModel MapViewModel { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return MapViewModel;

        foreach (var item in _itemsSource)
        {
            yield return item;
        }

        foreach (var item in base.GetChildren())
        {
            yield return item;
        }
    }

    private ValueTask InternalCatchEvent(IViewModel src, AsyncRoutedEvent<IViewModel> e, CancellationToken cancel)
    {
        if (e is PageCloseAttemptEvent close)
        {
            close.AddRestriction(new Restriction(this, "Test restriction for close"));
        }

        return ValueTask.CompletedTask;
    }
}
