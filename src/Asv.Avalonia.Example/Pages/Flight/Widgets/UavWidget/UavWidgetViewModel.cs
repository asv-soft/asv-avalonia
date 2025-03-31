using System;
using System.Collections.Generic;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using R3;
using Observable = R3.Observable;

namespace Asv.Avalonia.Example;

public class UavWidgetViewModel : ExtendableHeadlinedViewModel<IUavFlightWidget>, IUavFlightWidget
{
    private WorkspaceDock _position;

    private readonly GnssClientEx? _gnssClient;
    private const string WidgetId = "widget-uav";
    private const int CriticalAltitude = 40;
    private const int DangerHighSpeed = 10;
    private const int DangerSateliteCount = 10;
    private static readonly Range WarningSateliteAmount = 15..20;
    
    private static readonly Color GreenColor = Color.Parse("#21c088");
    private static readonly Color OrangeColor = Color.Parse("#e48f4d");
    private static readonly Color RedColor = Color.Parse("#cc5058");
    private static readonly Color YellowColor = Color.Parse("#dfc34a");

    public UavWidgetViewModel()
        : base(SystemModule.Name)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs("1");
        AltitudeStatus.Value.Color = GreenColor;
        BatteryStatusBrush.Value.Color = GreenColor;
        GnssStatusBrush.Value.Color = GreenColor;
        HomeAzimuth.Value = -115;
    }

    public UavWidgetViewModel(
        IDeviceManager service,
        IClientDevice device,
        INavigationService navigation,
        IUnitService unitService,
        IFlightMode flightContext,
        ILoggerFactory loggerFactory
    )
        : base(WidgetId)
    {
        ArgumentNullException.ThrowIfNull(device);
        Device = device;
        Position = WorkspaceDock.Left;
        Icon = DeviceIconMixin.GetIcon(device.Id);
        IconBrush = DeviceIconMixin.GetIconBrush(device.Id);
        AltitudeUnit.Value = unitService
            .Units[AltitudeBase.Id];
        VelocityUnit.Value = unitService
            .Units[VelocityBase.Id];
        AngleUnit.Value = unitService
            .Units[AngleBase.Id];
        BearingUnit.Value = unitService
            .Units[BearingBase.Id];
        CapacityUnit.Value = unitService
            .Units[CapacityBase.Id];

        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(device.Id.AsString());
        MissionProgress.Value = new MissionProgressViewModel(
            "missionProgress",
            device,
            unitService,
            flightContext,
            loggerFactory
        );
        var positionClientEx = device.GetMicroservice<PositionClientEx>() ??
                               throw new ArgumentException(
                                   $"Unable to load {nameof(PositionClientEx)} from {device.Id}");
        _gnssClient = device.GetMicroservice<GnssClientEx>() ??
                      throw new ArgumentException(
                          $"Unable to load {nameof(PositionClientEx)} from {device.Id}");
        var controlClient = device.GetMicroservice<ControlClient>() ??
                            throw new ArgumentException(
                                $"Unable to load {nameof(PositionClientEx)} from {device.Id}");
        var missionClient = device.GetMicroservice<MissionClientEx>() ??
                            throw new ArgumentException(
                                $"Unable to load {nameof(PositionClientEx)} from {device.Id}");
        var telemetryClient = device.GetMicroservice<TelemetryClientEx>() ??
                              throw new ArgumentException(
                                  $"Unable to load {nameof(PositionClientEx)} from {device.Id}");
        var heartbeatClient = device.GetMicroservice<HeartbeatClient>() ??
                              throw new ArgumentException(
                                  $"Unable to load {nameof(PositionClientEx)} from {device.Id}");
        ModeClient? modeClient = device switch
        {
            ArduCopterClientDevice => device.GetMicroservice<ArduCopterModeClient>(),
            ArduPlaneClientDevice => device.GetMicroservice<ArduPlaneModeClient>(),
            _ => null,
        };
        TakeOff = new ReactiveCommand(
            async (_, _) =>
            {
                var dialog = new SetAltitudeDialogViewModel(navigation);
                var altitude = await dialog.ApplyDialog();
                if (altitude != 0)
                {
                    await this.ExecuteCommand(
                        TakeOffCommand.Id,
                        new DoubleCommandArg(altitude)
                    );
                }
            }
        ).DisposeItWith(Disposable);
        Rtl = new BindableAsyncCommand(RTLCommand.Id, this);
        Land = new BindableAsyncCommand(LandCommand.Id, this);
        Guided = new BindableAsyncCommand(GuidedModeCommand.Id, this);
        AutoMode = new BindableAsyncCommand(AutoModeCommand.Id, this);
        StartMission = new BindableAsyncCommand(StartMissionCommand.Id, this);
        LinkQuality = heartbeatClient.LinkQuality.Select(x => $"{(int)x * 100} %").ToBindableReactiveProperty<string>();
        heartbeatClient.Link.State.Subscribe(state =>
            Dispatcher.UIThread.InvokeAsync(() => LinkState.Value = LinkQualityStatus(state)));
        service
            .Router.OnRxMessage.ThrottleFirst(TimeSpan.FromMicroseconds(100))
            .Subscribe(_ =>
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (modeClient is not null)
                    {
                        CurrentFlightMode.Value = modeClient.CurrentMode.CurrentValue.Name;
                    }

                    Azimuth.Value = AngleUnit.Value.Current.Value.Print(Heading.Value, "N2");
                    AltitudeMsl.Value = positionClientEx.Base.GlobalPosition.CurrentValue is null
                        ? RS.Not_Available
                        : AltitudeUnit.Value.Current.Value.Print(
                            positionClientEx.Base.GlobalPosition.CurrentValue.Alt / 1000
                        );
                    AltitudeAgl.Value = positionClientEx.Base.GlobalPosition.CurrentValue is null
                        ? RS.Not_Available
                        : AltitudeUnit.Value.Current.Value.Print(
                            positionClientEx.Base.GlobalPosition.CurrentValue.RelativeAlt / 1000
                        );
                    Roll.Value = positionClientEx.Roll.CurrentValue;
                    Pitch.Value = positionClientEx.Pitch.CurrentValue;
                    Heading.Value = positionClientEx.Yaw.CurrentValue;
                    Velocity.Value = VelocityUnit.Value.Current.Value.PrintFromSi(
                        _gnssClient.Main.GroundVelocity.CurrentValue
                    );
                    SateliteCount.Value = _gnssClient.Main.Info.CurrentValue.SatellitesVisible;
                    VdopCount.Value = _gnssClient.Main.Info.CurrentValue.Vdop is null
                        ? RS.Not_Available
                        : $"{_gnssClient.Main.Info.CurrentValue.Vdop!.Value} VDOP";
                    HdopCount.Value = _gnssClient.Main.Info.CurrentValue.Hdop is null
                        ? RS.Not_Available
                        : $"{_gnssClient.Main.Info.CurrentValue.Hdop!.Value} HDOP";
                    RtkMode.Value =
                        $"{GpsFixTypeToString(_gnssClient.Main.Info.CurrentValue.FixType)}";
                    GnssStatus();
                    if (positionClientEx.Base.GlobalPosition.CurrentValue != null)
                    {
                        SpeedAltitudeCheck(
                            positionClientEx.Base.GlobalPosition.CurrentValue.RelativeAlt / 1000,
                            Math.Round(_gnssClient.Main.GroundVelocity.CurrentValue)
                        );
                    }

                    if (IsArmed.Value != positionClientEx.IsArmed.CurrentValue)
                    {
                        UpdateStatusText(
                            positionClientEx.IsArmed.CurrentValue
                                ? RS.UavWidgetViewModel_StatusText_Armed
                                : RS.UavWidgetViewModel_StatusText_DisArmed
                        );
                    }

                    IsArmed.Value = positionClientEx.IsArmed.CurrentValue;
                    BatteryConsumed.Value =
                        telemetryClient.BatteryCurrent.CurrentValue == 0
                            ? RS.Not_Available
                            : $"{CapacityUnit.CurrentValue.Current.Value.Print(telemetryClient.BatteryCurrent.CurrentValue * positionClientEx.ArmedTime.CurrentValue.TotalHours, "N2")}{CapacityUnit.CurrentValue.Current.Value.Symbol}";

                    BatteryCharge.Value =
                        $"{(int)telemetryClient.BatteryCharge.CurrentValue * 100} %";
                    BatteryAmperage.Value = $"{(int)telemetryClient.BatteryCurrent.CurrentValue} A";
                    BatteryVoltage.Value = $"{(int)telemetryClient.BatteryVoltage.CurrentValue} V";
                    BatteryStatus((int)(telemetryClient.BatteryCharge.CurrentValue * 100));
                });
            })
            .DisposeItWith(Disposable);
        StatusText = positionClientEx.IsArmed.Select(_ => _
            ? RS.UavWidgetViewModel_StatusText_Armed
            : RS.UavWidgetViewModel_StatusText_DisArmed).ToBindableReactiveProperty<string>();
        Observable
            .Timer(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1))
            .Subscribe(_ => StatusText.Value = string.Empty)
            .DisposeItWith(Disposable);
    }

    private void BatteryStatus(int procent)
    {
        BatteryStatusBrush.Value.Color = procent switch
        {
            > 70 => GreenColor,
            > 50 => YellowColor,
            > 40 => OrangeColor,
            < 30 => RedColor,
            _ => BatteryStatusBrush.Value.Color,
        };
    }

    private void SpeedAltitudeCheck(int alt, double gs)
    {
        if (gs > DangerHighSpeed && alt < CriticalAltitude)
        {
            StatusText.Value = RS.UavWidgetViewModel_StatusText_PullUp;
            AltitudeStatus.Value.Color = YellowColor;
        }
        else
        {
            StatusText.Value = string.Empty;
            AltitudeStatus.Value.Color = GreenColor;
        }
    }

    private void GnssStatus()
    {
        if (_gnssClient is null)
        {
            return;
        }

        if (_gnssClient.Main.Info.CurrentValue.FixType
            == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
            || _gnssClient.Main.Info.CurrentValue.SatellitesVisible > WarningSateliteAmount.Start.Value
            || _gnssClient.Main.Info.CurrentValue.SatellitesVisible < WarningSateliteAmount.End.Value)
        {
            GnssStatusBrush.Value.Color = OrangeColor;
            return;
        }

        if ((_gnssClient.Main.Info.CurrentValue.FixType
             != Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat
             && _gnssClient.Main.Info.CurrentValue.FixType
             != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed)
            || _gnssClient.Main.Info.CurrentValue.SatellitesVisible < DangerSateliteCount)
        {
            GnssStatusBrush.Value.Color = RedColor;
            return;
        }

        GnssStatusBrush.Value.Color = GreenColor;
    }

    private string LinkQualityStatus(LinkState state)
    {
        LinkQualityStatusBrush.Value.Color = state switch
        {
            Common.LinkState.Connected => GreenColor,
            Common.LinkState.Downgrade => OrangeColor,
            Common.LinkState.Disconnected => RedColor,
            _ => LinkQualityStatusBrush.Value.Color,
        };
        return state.ToString();
    }

    private string GpsFixTypeToString(Mavlink.Common.GpsFixType type)
    {
        return type switch
        {
            Mavlink.Common.GpsFixType.GpsFixType2dFix => RS.GpsFixType_GpsFixType2dFix,
            Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat => RS.GpsFixType_GpsFixTypeRtkFloat,
            Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed => RS.GpsFixType_GpsFixTypeRtkFixed,
            Mavlink.Common.GpsFixType.GpsFixTypeDgps => RS.GpsFixType_GpsFixTypeDgps,
            Mavlink.Common.GpsFixType.GpsFixTypePpp => RS.GpsFixType_GpsFixTypePpp,
            Mavlink.Common.GpsFixType.GpsFixType3dFix => RS.GpsFixType_GpsFixType3dFix,
            Mavlink.Common.GpsFixType.GpsFixTypeStatic => RS.GpsFixType_GpsFixTypeStatic,
            Mavlink.Common.GpsFixType.GpsFixTypeNoGps => RS.GpsFixType_GpsFixTypeNoGps,
            _ => string.Empty,
        };
    }

    private void UpdateStatusText(string text)
    {
        if (StatusText.Value == text)
        {
            return;
        }

        StatusText.Value = text;
        Observable
            .Timer(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1))
            .Subscribe(_ => StatusText.Value = string.Empty)
            .DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public ReactiveCommand TakeOff { get; set; }
    public BindableAsyncCommand AutoMode { get; set; }
    public BindableAsyncCommand Rtl { get; set; }
    public BindableAsyncCommand Land { get; set; }
    public BindableAsyncCommand Guided { get; set; }
    public BindableAsyncCommand StartMission { get; set; }

    public BindableReactiveProperty<MissionProgressViewModel> MissionProgress { get; } = new();

    #region BatteryRtt

    public BindableReactiveProperty<string> BatteryConsumed { get; } = new();
    public BindableReactiveProperty<string> BatteryAmperage { get; } = new();
    public BindableReactiveProperty<string> BatteryCharge { get; } = new();
    public BindableReactiveProperty<string> BatteryVoltage { get; } = new();

    public BindableReactiveProperty<SolidColorBrush> BatteryStatusBrush { get; } =
        new(new SolidColorBrush(GreenColor));

    #endregion

    #region Gnss

    public BindableReactiveProperty<int> SateliteCount { get; } = new();
    public BindableReactiveProperty<string> HdopCount { get; } = new();
    public BindableReactiveProperty<string> VdopCount { get; } = new();
    public BindableReactiveProperty<string> RtkMode { get; } = new();

    public BindableReactiveProperty<SolidColorBrush> GnssStatusBrush { get; } =
        new(new SolidColorBrush(GreenColor));

    #endregion

    public BindableReactiveProperty<string> LinkState { get; } = new();
    public BindableReactiveProperty<string> LinkQuality { get; } = new();

    public BindableReactiveProperty<SolidColorBrush> LinkQualityStatusBrush { get; } =
        new(new SolidColorBrush(GreenColor));

    public BindableReactiveProperty<string> CurrentFlightMode { get; } = new();
    public BindableReactiveProperty<float> VibrationX { get; } = new();
    public BindableReactiveProperty<float> VibrationY { get; } = new();
    public BindableReactiveProperty<float> VibrationZ { get; } = new();
    public BindableReactiveProperty<uint> Clipping0 { get; } = new();
    public BindableReactiveProperty<uint> Clipping1 { get; } = new();
    public BindableReactiveProperty<uint> Clipping2 { get; } = new();
    public BindableReactiveProperty<double> Roll { get; } = new();
    public BindableReactiveProperty<double> Pitch { get; } = new();
    public BindableReactiveProperty<string> Velocity { get; } = new();

    public BindableReactiveProperty<SolidColorBrush> AltitudeStatus { get; } =
        new(new SolidColorBrush(GreenColor));

    public BindableReactiveProperty<string> AltitudeAgl { get; } = new();
    public BindableReactiveProperty<string> AltitudeMsl { get; } = new();
    public BindableReactiveProperty<double> Heading { get; } = new();
    public BindableReactiveProperty<int> HomeAzimuth { get; } = new();
    public BindableReactiveProperty<string> Azimuth { get; } = new();
    public BindableReactiveProperty<string> StatusText { get; } = new();
    public BindableReactiveProperty<bool> IsArmed { get; } = new();
    public BindableReactiveProperty<TimeSpan> ArmedTime { get; } = new();
    public BindableReactiveProperty<IUnit> VelocityUnit { get; } = new();
    public BindableReactiveProperty<IUnit> AltitudeUnit { get; } = new();
    public BindableReactiveProperty<IUnit> BearingUnit { get; } = new();
    public BindableReactiveProperty<IUnit> AngleUnit { get; } = new();
    public BindableReactiveProperty<IUnit> CapacityUnit { get; } = new();

    public IClientDevice Device { get; }

    public WorkspaceDock Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            LinkQuality.Dispose();
            LinkState.Dispose();
            BearingUnit.Dispose();
            AltitudeAgl.Dispose();
            AltitudeMsl.Dispose();
            Heading.Dispose();
            HomeAzimuth.Dispose();
            Azimuth.Dispose();
            StatusText.Dispose();
            IsArmed.Dispose();
            ArmedTime.Dispose();
            VelocityUnit.Dispose();
            AltitudeUnit.Dispose();
            BearingUnit.Dispose();
            AngleUnit.Dispose();
            CapacityUnit.Dispose();
            AltitudeStatus.Dispose();
            CurrentFlightMode.Dispose();
            VibrationX.Dispose();
            VibrationY.Dispose();
            VibrationZ.Dispose();
            Clipping0.Dispose();
            Clipping1.Dispose();
            Clipping2.Dispose();
            Roll.Dispose();
            Pitch.Dispose();
            Velocity.Dispose();
            LinkState.Dispose();
            LinkQuality.Dispose();
            LinkQualityStatusBrush.Dispose();
            GnssStatusBrush.Dispose();
            SateliteCount.Dispose();
            VdopCount.Dispose();
            HdopCount.Dispose();
            RtkMode.Dispose();
            BatteryConsumed.Dispose();
            BatteryCharge.Dispose();
            BatteryVoltage.Dispose();
            BatteryAmperage.Dispose();
            BatteryStatusBrush.Dispose();
        }

        base.Dispose(disposing);
    }
}