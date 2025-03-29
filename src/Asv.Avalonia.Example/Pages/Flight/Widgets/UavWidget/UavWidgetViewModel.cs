using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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

    private readonly GnssClientEx? _deviceGnss;
    private const string WidgetId = "widget-uav";

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

    public UavWidgetViewModel(IDeviceManager service, IClientDevice device, INavigationService navigation, IUnitService unitService, IFlightMode flightContext, ILoggerFactory loggerFactory)
        : base(WidgetId)
    {
        ArgumentNullException.ThrowIfNull(device);
        Device = device;
        Position = WorkspaceDock.Left;
        Icon = DeviceIconMixin.GetIcon(device.Id);
        IconBrush = DeviceIconMixin.GetIconBrush(device.Id);
        AltitudeUnit.Value = unitService.Units.First(pair => pair.Value.UnitId == AltitudeBase.Id).Value.Current.Value;
        VelocityUnit.Value = unitService.Units.First(pair => pair.Value.UnitId == VelocityBase.Id).Value.Current.Value;
        AngleUnit.Value = unitService.Units.First(pair => pair.Value.UnitId == AngleBase.Id).Value.Current.Value;
        BearingUnit.Value = unitService.Units.First(pair => pair.Value.UnitId == BearingBase.Id).Value.Current.Value;
        CapacityUnit.Value = unitService.Units.First(pair => pair.Value.UnitId == CapacityBase.Id).Value.Current.Value;

        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(device.Id.AsString());
        device.WaitUntilConnectAndInit(1000, TimeProvider.System);
        MissionProgress.Value = new MissionProgressViewModel("missionProgress", device, unitService, flightContext, loggerFactory);
        var devicePosition = device.GetMicroservice<PositionClientEx>();
        _deviceGnss = device.GetMicroservice<GnssClientEx>();
        var controlClient = device.GetMicroservice<ControlClient>();
        var missionClient = device.GetMicroservice<MissionClientEx>();
        var telemetryClient = device.GetMicroservice<TelemetryClientEx>();
        var heartbeatClient = device.GetMicroservice<HeartbeatClient>();
        if (controlClient == null || missionClient == null || telemetryClient == null || devicePosition == null ||
            heartbeatClient == null ||
            _deviceGnss == null)
        {
            throw new ArgumentException($"Unable to get one or more services from {Device.Id}");
        }

        ModeClient? modeClient = device switch
        {
            ArduCopterClientDevice => device.GetMicroservice<ArduCopterModeClient>(),
            ArduPlaneClientDevice => device.GetMicroservice<ArduPlaneModeClient>(),
            _ => null
        };

        GoTo = new ReactiveCommand(_ => controlClient.SetAutoMode(CancellationToken.None)).DisposeItWith(Disposable);
        TakeOff = new ReactiveCommand(async (_, _) =>
        {
            var dialog = new SetAltitudeDialogViewModel(navigation);
            var altitude = await dialog.ApplyDialog();
            if (altitude is not null)
            {
                await this.ExecuteCommand(
                    TakeOffCommand.Id,
                    new ActionCommandArg(
                        Device.Id.AsString(),
                        altitude.Value.ToString(CultureInfo.InvariantCulture),
                        CommandParameterActionType.Change));
            }
        }).DisposeItWith(Disposable);
        Rtl = new ReactiveCommand(_ =>
        {
            controlClient.SetGuidedMode(CancellationToken.None);
            controlClient.DoRtl(CancellationToken.None);
        }).DisposeItWith(Disposable);
        Land = new ReactiveCommand(_ =>
        {
            controlClient.SetGuidedMode(CancellationToken.None);
            controlClient.DoLand();
        }).DisposeItWith(Disposable);
        Guided = new ReactiveCommand(_ => controlClient.SetGuidedMode(CancellationToken.None)).DisposeItWith(Disposable);
        StartMission = new ReactiveCommand(_ =>
        {
            controlClient.SetAutoMode();
            missionClient.Base.MissionSetCurrent(0);
        }).DisposeItWith(Disposable);

        heartbeatClient.LinkQuality.Subscribe(_ => Dispatcher.UIThread.InvokeAsync(() => LinkQuality.Value = $"{(int)_ * 100} %")).DisposeItWith(Disposable);
        heartbeatClient.Link.State.Subscribe(state => Dispatcher.UIThread.InvokeAsync(() => LinkState.Value = LinkQualityStatus(state))).DisposeItWith(Disposable);

        service.Router.OnRxMessage.ThrottleFirst(TimeSpan.FromMicroseconds(100)).Subscribe(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (modeClient is not null)
                {
                    CurrentFlightMode.Value = modeClient.CurrentMode.CurrentValue.Name;
                }

                Azimuth.Value = AngleUnit.Value.Print(Heading.Value, "N2");
                AltitudeMsl.Value = devicePosition.Base.GlobalPosition.CurrentValue is null
                    ? RS.Not_Available
                    : AltitudeUnit.Value.Print(devicePosition.Base.GlobalPosition.CurrentValue.Alt / 1000);
                AltitudeAgl.Value = devicePosition.Base.GlobalPosition.CurrentValue is null
                    ? RS.Not_Available
                    : AltitudeUnit.Value.Print(devicePosition.Base.GlobalPosition.CurrentValue.RelativeAlt / 1000);
                Roll.Value = devicePosition.Roll.CurrentValue;
                Pitch.Value = devicePosition.Pitch.CurrentValue;
                Heading.Value = devicePosition.Yaw.CurrentValue;
                Velocity.Value = VelocityUnit.Value.PrintFromSi(_deviceGnss.Main.GroundVelocity.CurrentValue);
                SateliteCount.Value = _deviceGnss.Main.Info.CurrentValue.SatellitesVisible;
                VdopCount.Value = _deviceGnss.Main.Info.CurrentValue.Vdop is null
                    ? RS.Not_Available
                    : $"{_deviceGnss.Main.Info.CurrentValue.Vdop!.Value} VDOP";
                HdopCount.Value = _deviceGnss.Main.Info.CurrentValue.Hdop is null
                    ? RS.Not_Available
                    : $"{_deviceGnss.Main.Info.CurrentValue.Hdop!.Value} HDOP";
                RtkMode.Value = $"{GpsFixTypeToString(_deviceGnss.Main.Info.CurrentValue.FixType)}";
                GnssStatus();
                if (devicePosition.Base.GlobalPosition.CurrentValue != null)
                {
                    SpeedAltitudeCheck(
                        devicePosition.Base.GlobalPosition.CurrentValue.RelativeAlt / 1000,
                        (int)_deviceGnss.Main.GroundVelocity.CurrentValue);
                }

                if (IsArmed.Value != devicePosition.IsArmed.CurrentValue)
                {
                    UpdateStatusText(devicePosition.IsArmed.CurrentValue
                        ? "Armed"
                        : "Disarmed");
                }

                IsArmed.Value = devicePosition.IsArmed.CurrentValue;
                BatteryConsumed.Value = telemetryClient.BatteryCurrent.CurrentValue == 0
                    ? RS.Not_Available
                    : $"{CapacityUnit.CurrentValue.Print(telemetryClient.BatteryCurrent.CurrentValue * devicePosition.ArmedTime.CurrentValue.TotalHours, "N2")}{CapacityUnit.CurrentValue.Symbol}";

                BatteryCharge.Value = $"{(int)telemetryClient.BatteryCharge.CurrentValue * 100} %";
                BatteryAmperage.Value = $"{(int)telemetryClient.BatteryCurrent.CurrentValue} A";
                BatteryVoltage.Value = $"{(int)telemetryClient.BatteryVoltage.CurrentValue} V";

                // BatteryTime.Value = $"{telemetryClient.BatteryVoltage.CurrentValue} min";
                BatteryStatus((int)(telemetryClient.BatteryCharge.CurrentValue * 100));
            });
        }).DisposeItWith(Disposable);
    }

    private void BatteryStatus(int procent)
    {
        BatteryStatusBrush.Value.Color = procent switch
        {
            > 70 => GreenColor,
            > 50 => YellowColor,
            > 40 => OrangeColor,
            < 30 => RedColor,
            _ => BatteryStatusBrush.Value.Color
        };
    }

    private void SpeedAltitudeCheck(int alt, int gs)
    {
        if (gs > 10 && alt < 40)
        {
            StatusText.Value = "Pull Up";
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
        if (_deviceGnss is null)
        {
            return;
        }

        if (_deviceGnss.Main.Info.CurrentValue.FixType == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat ||
            _deviceGnss.Main.Info.CurrentValue.SatellitesVisible > 15 ||
            _deviceGnss.Main.Info.CurrentValue.SatellitesVisible < 20)
        {
            GnssStatusBrush.Value.Color = OrangeColor;
        }
        else if ((_deviceGnss.Main.Info.CurrentValue.FixType != Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat &&
                  _deviceGnss.Main.Info.CurrentValue.FixType != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed) ||
                 _deviceGnss.Main.Info.CurrentValue.SatellitesVisible < 10)
        {
            GnssStatusBrush.Value.Color = RedColor;
        }
        else
        {
            GnssStatusBrush.Value.Color = GreenColor;
        }
    }

    private string LinkQualityStatus(LinkState state)
    {
        LinkQualityStatusBrush.Value.Color = state switch
        {
            Common.LinkState.Connected => GreenColor,
            Common.LinkState.Downgrade => OrangeColor,
            Common.LinkState.Disconnected => RedColor,
            _ => LinkQualityStatusBrush.Value.Color
        };
        return state.ToString();
    }

    private string GpsFixTypeToString(Mavlink.Common.GpsFixType type)
    {
        return type switch
        {
            Mavlink.Common.GpsFixType.GpsFixType2dFix => "2D position Fix",
            Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat => "RTK Float",
            Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed => "Rtk Fixed",
            Mavlink.Common.GpsFixType.GpsFixTypeDgps => "DGPS/SBAS",
            Mavlink.Common.GpsFixType.GpsFixTypePpp => "PPP 3D position",
            Mavlink.Common.GpsFixType.GpsFixType3dFix => "3D position Fix",
            Mavlink.Common.GpsFixType.GpsFixTypeStatic => "Static Fix",
            Mavlink.Common.GpsFixType.GpsFixTypeNoGps => "No GPS connected",
            _ => string.Empty
        };
    }

    private void UpdateStatusText(string text)
    {
        if (StatusText.Value == text)
        {
            return;
        }

        StatusText.Value = text;
        Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(_ => StatusText.Value = string.Empty)
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

    public ReactiveCommand GoTo { get; set; }
    public ReactiveCommand TakeOff { get; set; }
    public ReactiveCommand Rtl { get; set; }
    public ReactiveCommand Land { get; set; }
    public ReactiveCommand Guided { get; set; }
    public ReactiveCommand StartMission { get; set; }

    public BindableReactiveProperty<MissionProgressViewModel> MissionProgress { get; set; } = new();

    #region BatteryRtt

    public BindableReactiveProperty<string> BatteryConsumed { get; set; } = new();
    public BindableReactiveProperty<string> BatteryAmperage { get; set; } = new();
    public BindableReactiveProperty<string> BatteryCharge { get; set; } = new();
    public BindableReactiveProperty<string> BatteryVoltage { get; set; } = new();

    public BindableReactiveProperty<SolidColorBrush> BatteryStatusBrush { get; set; } =
        new(new SolidColorBrush(GreenColor));

    #endregion

    #region Gnss

    public BindableReactiveProperty<int> SateliteCount { get; set; } = new();
    public BindableReactiveProperty<string> HdopCount { get; set; } = new();
    public BindableReactiveProperty<string> VdopCount { get; set; } = new();
    public BindableReactiveProperty<string> RtkMode { get; set; } = new();

    public BindableReactiveProperty<SolidColorBrush> GnssStatusBrush { get; set; } =
        new(new SolidColorBrush(GreenColor));

    #endregion

    public BindableReactiveProperty<string> LinkState { get; set; } = new();
    public BindableReactiveProperty<string> LinkQuality { get; set; } = new();

    public BindableReactiveProperty<SolidColorBrush> LinkQualityStatusBrush { get; set; } =
        new(new SolidColorBrush(GreenColor));

    public BindableReactiveProperty<string> CurrentFlightMode { get; set; } = new();
    public BindableReactiveProperty<float> VibrationX { get; set; } = new();
    public BindableReactiveProperty<float> VibrationY { get; set; } = new();
    public BindableReactiveProperty<float> VibrationZ { get; set; } = new();
    public BindableReactiveProperty<uint> Clipping0 { get; set; } = new();
    public BindableReactiveProperty<uint> Clipping1 { get; set; } = new();
    public BindableReactiveProperty<uint> Clipping2 { get; set; } = new();
    public BindableReactiveProperty<double> Roll { get; set; } = new();
    public BindableReactiveProperty<double> Pitch { get; set; } = new();
    public BindableReactiveProperty<string> Velocity { get; set; } = new();
    public BindableReactiveProperty<SolidColorBrush> AltitudeStatus { get; set; } =
        new(new SolidColorBrush(GreenColor));
    public BindableReactiveProperty<string> AltitudeAgl { get; set; } = new();
    public BindableReactiveProperty<string> AltitudeMsl { get; set; } = new();
    public BindableReactiveProperty<double> Heading { get; set; } = new();
    public BindableReactiveProperty<int> HomeAzimuth { get; set; } = new();
    public BindableReactiveProperty<string> Azimuth { get; set; } = new();
    public BindableReactiveProperty<string> StatusText { get; set; } = new();
    public BindableReactiveProperty<bool> IsArmed { get; set; } = new();
    public BindableReactiveProperty<TimeSpan> ArmedTime { get; set; } = new();
    public BindableReactiveProperty<IUnitItem> VelocityUnit { get; set; } = new();
    public BindableReactiveProperty<IUnitItem> AltitudeUnit { get; set; } = new();
    public BindableReactiveProperty<IUnitItem> BearingUnit { get; set; } = new();
    public BindableReactiveProperty<IUnitItem> AngleUnit { get; set; } = new();
    public BindableReactiveProperty<IUnitItem> CapacityUnit { get; set; } = new();
    
    public IClientDevice Device { get; }

    public WorkspaceDock Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }
}