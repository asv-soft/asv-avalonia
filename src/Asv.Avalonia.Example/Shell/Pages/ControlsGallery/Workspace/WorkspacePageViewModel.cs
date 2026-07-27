using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Charts;
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
            DesignTime.DialogService,
            DesignTime.ThemeService
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
        IDialogService dialogService,
        IThemeService themeService
    )
        : base(PageId, context)
    {
        Init(context.Context);
        _loggerFactory = loggerFactory;
        MapViewModel = new MapViewModel("map-view", mapService).DisposeItWith(Disposable);
        var hideAll = new MenuItem("hide-all", RS.WorkspacePageViewModel_HideAll_Header)
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

        var showAll = new MenuItem("show-all", RS.WorkspacePageViewModel_ShowAll_Header)
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

        var showError = new MenuItem("show-error", RS.WorkspacePageViewModel_ShowError_Header)
        {
            Command = new ReactiveCommand(
                async (_, cancel) =>
                {
                    await this.RiseShellInfoMessage(
                        new ShellMessage(
                            RS.WorkspacePageViewModel_ErrorMessage_Title,
                            RS.WorkspacePageViewModel_ErrorMessage_Message,
                            ShellErrorState.Error,
                            RS.WorkspacePageViewModel_ErrorMessage_Description,
                            MaterialIconKind.Cross,
                            showAll.Command,
                            null,
                            commandTitle: RS.WorkspacePageViewModel_ErrorMessage_CommandTitle,
                            TimeSpan.FromSeconds(5)
                        ),
                        cancel
                    );
                }
            ),
        };
        var generateException = new MenuItem(
            "generate-exception",
            RS.WorkspacePageViewModel_GenerateException_Header
        )
        {
            Icon = MaterialIconKind.AlertCircle,
            IconColor = AsvColorKind.Error,
            Order = 100,
            Command = new ReactiveCommand(GenerateException).DisposeItWith(Disposable),
        };
        Menu.Add(showAll);
        Menu.Add(hideAll);
        Menu.Add(showError);
        Menu.Add(generateException);
        var signalPlot = CreateSignalPlotWidget(themeService);
        var dashboard = CreateDashboardWidget();
        StartSignalPlotDemo(signalPlot);

        _itemsSource =
        [
            new PropertyEditorWidgetViewModel(
                "left-property-editor",
                RS.WorkspacePageViewModel_PropertyEditor_Header,
                loggerFactory
            )
            {
                Position = WorkspaceDock.Left,
                Icon = MaterialIconKind.Tune,
                IconColor = AsvColorKind.Info5,
                IsExpanded = true,
                CanExpand = true,
                IsVisible = true,
                ItemsSource =
                {
                    new PropertyGeoPointReactive(
                        "location1",
                        new ReactiveProperty<GeoPoint>(),
                        unitService,
                        dialogService
                    )
                    {
                        Header = RS.WorkspacePageViewModel_Location1_Header,
                    },
                    new PropertyGeoPointReactive(
                        "location2",
                        new ReactiveProperty<GeoPoint>(),
                        unitService,
                        dialogService
                    )
                    {
                        Header = RS.WorkspacePageViewModel_Location2_Header,
                    },
                },
            },
            signalPlot,
            dashboard,
            new MapWidget("map-widget", loggerFactory, mapService)
            {
                Position = WorkspaceDock.Bottom,
                Icon = MaterialIconKind.Map,
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
                Header = RS.WorkspacePageViewModel_MapWidget_Header,
                IsExpanded = true,
                CanExpand = true,
            },
            new MapWidget("map-with-anchor", loggerFactory, mapService)
            {
                Position = WorkspaceDock.Bottom,
                Icon = MaterialIconKind.Map,
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
                Header = RS.WorkspacePageViewModel_MapWidget2_Header,
                IsExpanded = true,
                CanExpand = true,
                Anchors =
                {
                    new MapAnchor("drone-1")
                    {
                        Header = RS.WorkspacePageViewModel_Drone1_Header,
                        Location = new GeoPoint(53.0, 53.0, 100),
                        Icon = MaterialIconKind.Airplane,
                    },
                },
            },
        ];
        _itemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        _itemsSource.SetParent(this).DisposeItWith(Disposable);
        Items = _itemsSource.ToNotifyCollectionChangedSlim();
    }

    public void Init(IControlsGalleryPage context)
    {
        var changeStatus = new MenuItem(
            "change-status",
            RS.WorkspacePageViewModel_ChangeStatus_Header
        )
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

    private ValueTask InternalCatchEvent(
        IViewModel src,
        AsyncRoutedEvent<IViewModel> e,
        CancellationToken cancel
    )
    {
        if (e is PageCloseAttemptEvent close)
        {
            close.AddRestriction(
                new Restriction(this, RS.WorkspacePageViewModel_CloseRestriction_Message)
            );
        }

        return ValueTask.CompletedTask;
    }

    private SignalPlotWidget CreateSignalPlotWidget(IThemeService themeService)
    {
        return new SignalPlotWidget("signal-plot-widget", themeService)
        {
            Position = WorkspaceDock.Right,
            Header = RS.WorkspacePageViewModel_SignalPlot_Header,
            Icon = MaterialIconKind.Signal,
            IconColor = AsvColorKind.Info5,
            IsExpanded = true,
            CanExpand = false,
            IsVisible = true,
            HistorySize = 5,
            Order = 10,
        };
    }

    private static DashboardWidget CreateDashboardWidget()
    {
        var dashboard = new DashboardWidget("telemetry-dashboard")
        {
            Position = WorkspaceDock.Right,
            Header = RS.WorkspacePageViewModel_TelemetryDashboard_Header,
            Icon = MaterialIconKind.ViewGallery,
            IconColor = AsvColorKind.Info4,
            IsExpanded = true,
            CanExpand = true,
            IsVisible = true,
            Order = 20,
        };

        dashboard.Tiles.Add(
            new TextTileViewModel("workspace-dashboard-battery")
            {
                Density = TileDensity.Regular,
                Header = RS.WorkspacePageViewModel_Battery_Header,
                ShortHeader = RS.WorkspacePageViewModel_Battery_ShortHeader,
                Icon = MaterialIconKind.Battery80,
                IconColor = AsvColorKind.Warning,
                StatusIcon = MaterialIconKind.CheckCircle,
                StatusIconColor = AsvColorKind.Success,
                Text = "76",
                TextColor = AsvColorKind.Warning,
                Units = "%",
                StatusText = "15.6 V / 4.8 A",
                StatusTextColor = AsvColorKind.Unknown,
                Progress = 76,
                ProgressColor = AsvColorKind.Warning,
            }
        );

        dashboard.Tiles.Add(
            new TextTileViewModel("workspace-dashboard-fix")
            {
                Density = TileDensity.Inline,
                Header = RS.WorkspacePageViewModel_GnssFix_Header,
                ShortHeader = RS.WorkspacePageViewModel_GnssFix_ShortHeader,
                Icon = MaterialIconKind.CrosshairsGps,
                IconColor = AsvColorKind.Info5,
                StatusIcon = MaterialIconKind.CheckCircle,
                StatusIconColor = AsvColorKind.Success,
                Text = RS.WorkspacePageViewModel_GnssFix_Text,
                TextColor = AsvColorKind.Success,
            }
        );

        return dashboard;
    }

    private void StartSignalPlotDemo(SignalPlotWidget signalPlot)
    {
        var tick = 0;
        UpdateSignalPlot(signalPlot, tick);
        Observable
            .Timer(TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500))
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => UpdateSignalPlot(signalPlot, ++tick))
            .DisposeItWith(Disposable);
    }

    private static void UpdateSignalPlot(SignalPlotWidget signalPlot, int tick)
    {
        const int SampleCount = 128;
        var samples = new double[SampleCount];
        var phase = tick * 0.18;

        for (var i = 0; i < samples.Length; i++)
        {
            var x = i / 8.0;
            samples[i] = Math.Sin(x + phase) + (0.35 * Math.Sin((x * 2.7) - phase));
        }

        signalPlot.Refresh(samples);
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
                RS.WorkspacePageViewModel_Exception_Title,
                RS.WorkspacePageViewModel_Exception_Message,
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
