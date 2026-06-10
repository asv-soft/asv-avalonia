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
    public const string PageId = "workspace-example";
    public const MaterialIconKind PageIcon = MaterialIconKind.Table;

    public WorkspacePageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance,
            DesignTime.LoggerFactory,
            DesignTime.UnitService,
            NullMapService.Instance,
            DesignTime.DialogService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        SetParent(DesignTime.Shell);
        Events.Catch(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public WorkspacePageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        IMapService mapService,
        IDialogService dialogService
    )
        : base(PageId, context)
    {
        Init(context.Context);
        _loggerFactory = loggerFactory;
        MapViewModel = new MapViewModel("map-view", mapService).DisposeItWith(Disposable);
        GenerateExceptionCommand = new ReactiveCommand(GenerateException).DisposeItWith(Disposable);
        var hideAll = new MenuItem("hide-all", "Hide all")
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

        var showAll = new MenuItem("show-all", "Show all")
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

        var showError = new MenuItem("show-error", "Show error")
        {
            Command = new ReactiveCommand(
                async (_, cancel) =>
                {
                    await this.RiseShellInfoMessage(
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
            new PropertyEditorWidgetViewModel(
                "left-property-editor",
                "Property editor",
                loggerFactory
            )
            {
                Position = WorkspaceDock.Left,
                ItemsSource =
                {
                    new PropertyGeoPointReactive(
                        "location1",
                        new ReactiveProperty<GeoPoint>(),
                        unitService,
                        dialogService
                    )
                    {
                        Header = "Location 1",
                    },
                    new PropertyGeoPointReactive(
                        "location2",
                        new ReactiveProperty<GeoPoint>(),
                        unitService,
                        dialogService
                    )
                    {
                        Header = "Location 2",
                    },
                },
            },
            new WrapPanelWidgetViewModel("primary-rtt-panel", loggerFactory)
            {
                Position = WorkspaceDock.Right,
                ItemsSource =
                {
                    new SingleRttBoxViewModel("single-1")
                    {
                        Header = "Single RTT 1",
                        ShortHeader = "RTT1",
                        ValueString = "15.25",
                    },
                    new SingleRttBoxViewModel("single-2")
                    {
                        Header = "Single RTT 2",
                        ShortHeader = "RTT2",
                        ValueString = "15.25",
                    },
                },
            },
            new WrapPanelWidgetViewModel("secondary-rtt-panel", loggerFactory)
            {
                Position = WorkspaceDock.Right,
                ItemsSource =
                {
                    new SingleRttBoxViewModel("single-1")
                    {
                        Header = "Single RTT 1",
                        ShortHeader = "RTT1",
                        ValueString = "15.25",
                    },
                    new SingleRttBoxViewModel("single-2")
                    {
                        Header = "Single RTT 2",
                        ShortHeader = "RTT2",
                        ValueString = "15.25",
                    },
                },
            },
            new MapWidget("map-widget", loggerFactory, mapService)
            {
                Position = WorkspaceDock.Bottom,
                Icon = MaterialIconKind.Map,
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
                Header = "Map Widget",
                IsExpanded = true,
                CanExpand = true,
            },
            new MapWidget("map-with-anchor", loggerFactory, mapService)
            {
                Position = WorkspaceDock.Bottom,
                Icon = MaterialIconKind.Map,
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
                Header = "Map Widget2",
                IsExpanded = true,
                CanExpand = true,
                Anchors =
                {
                    new MapAnchor("drone-1")
                    {
                        Header = "Drone 1",
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
        var changeStatus = new MenuItem("change-status", "Change status")
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

    public ReactiveCommand GenerateExceptionCommand { get; }

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

    private ValueTask InternalCatchEvent(
        IViewModel src,
        AsyncRoutedEvent<IViewModel> e,
        CancellationToken cancel
    )
    {
        if (e is PageCloseAttemptEvent close)
        {
            close.AddRestriction(new Restriction(this, "Test restriction for close"));
        }

        return ValueTask.CompletedTask;
    }

    private async ValueTask GenerateException(Unit unit, CancellationToken cancel)
    {
        try
        {
            ThrowDemoException();
        }
        catch (Exception exception)
        {
            await this.RiseShellErrorMessage(
                "Workspace exception",
                "Generated exception for ShellView popup demo.",
                exception,
                cancel
            );
        }
    }

    private static void ThrowDemoException()
    {
        throw new InvalidOperationException("Generated exception from ControlGallery Workspace.");
    }
}
