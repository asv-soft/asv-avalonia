using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using Avalonia.Threading;
using ObservableCollections;
using R3;
using Observable = R3.Observable;

namespace Asv.Avalonia.Example;

public interface IUavFlightWidget : IMapWidget
{
    IClientDevice Device { get; }
}

public class UavWidgetViewModel : ExtendableHeadlinedViewModel<IUavFlightWidget>, IUavFlightWidget
{
    private WorkspaceDock _position;

    private readonly INavigationService _navigationService;

    private readonly PositionClientEx? _devicePosition;
    private readonly GnssClientEx? _deviceGnss;
    private readonly ControlClient? _controlClient;
    public const string WidgetId = "widget-uav";

    private static readonly Color _greenColor = Color.Parse("#21c088");
    private static readonly Color _orangeColor = Color.Parse("#e48f4d");
    private static readonly Color _redColor = Color.Parse("#cc5058");
    private static readonly Color _yellowColor = Color.Parse("#dfc34a");

    public UavWidgetViewModel()
        : base(WidgetId)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs("1");
        AltitudeStatus.Value.Color = _greenColor;
        BatteryStatusBrush.Value.Color = _greenColor;
        GnssStatusBrush.Value.Color = _greenColor;
       
        HomeAzimuth.Value = -115;
    }

    public UavWidgetViewModel(IMavlinkConnectionService service, IClientDevice device,
        [Import] INavigationService navigation, [Import] IUnitService unitService)
        : base(WidgetId)
    {
        ArgumentNullException.ThrowIfNull(device);
        _navigationService = navigation;
        Device = device;
        Position = WorkspaceDock.Left;
        Icon = DeviceIconMixin.GetIcon(device.Id);
        IconBrush = DeviceIconMixin.GetIconBrush(device.Id);
        AltitudeUnit.Value = unitService.Units.First(_ => _.Value.UnitId == AltitudeBase.Id).Value.Current.Value;
        VelocityUnit.Value = unitService.Units.First(_ => _.Value.UnitId == VelocityBase.Id).Value.Current.Value;
        AngleUnit.Value = unitService.Units.First(_ => _.Value.UnitId == AngleBase.Id).Value.Current.Value;
        BearingUnit.Value = unitService.Units.First(_ => _.Value.UnitId == BearingBase.Id).Value.Current.Value;
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(device.Id.AsString());
        device.WaitUntilConnectAndInit(1000, TimeProvider.System);
        MissionProgress.Value = new MissionProgressViewModel("missionPorgress", device, unitService);
        _devicePosition = device.GetMicroservice<PositionClientEx>();
        _deviceGnss = device.GetMicroservice<GnssClientEx>();
        _controlClient = device.GetMicroservice<ControlClient>();
        var missionClient = device.GetMicroservice<MissionClientEx>();
        var telemetryClient = device.GetMicroservice<TelemetryClientEx>();
        ModeClient? modeClient = null;
        if (device is ArduCopterClientDevice)
        {
            modeClient = device.GetMicroservice<ArduCopterModeClient>();
        }
        else if (device is ArduPlaneClientDevice)
        {
            modeClient = device.GetMicroservice<ArduPlaneModeClient>();
        }

        GoTo = new ReactiveCommand(_ => _controlClient!.SetAutoMode(CancellationToken.None));
        TakeOff = new ReactiveCommand(async (_, ct) =>
        {
            if (_controlClient != null)
            {
                var dialog = new SetAltitudeDialogViewModel(navigation);
                var altitude = await dialog.ApplyDialog();
                if (altitude is not null) 
                {
                    var persist =
                        new Persistable<KeyValuePair<DeviceId, double>>(
                            new KeyValuePair<DeviceId, double>(Device.Id, altitude.Value));
                    await this.ExecuteCommand(TakeOffCommand.Id, persist);
                }
            }
        });
        Rtl = new ReactiveCommand(_ =>
        {
            _controlClient!.SetGuidedMode(CancellationToken.None);
            _controlClient.DoRtl(CancellationToken.None);
        });
        Land = new ReactiveCommand(_ =>
        {
            _controlClient!.SetGuidedMode(CancellationToken.None);
            _controlClient.DoLand();
        });
        Guided = new ReactiveCommand(_ => _controlClient!.SetGuidedMode(CancellationToken.None));
        StartMission = new ReactiveCommand(_ =>
        {
            _controlClient!.SetAutoMode();
            missionClient!.Base.MissionSetCurrent(0);
        });

        service.Router.OnRxMessage.ThrottleFirst(TimeSpan.FromMicroseconds(100)).Subscribe(_ =>
        {
            if (_deviceGnss == null || _devicePosition == null || telemetryClient == null)
            {
                return;
            }

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (modeClient is not null)
                {
                    CurrentFlightMode.Value = modeClient.CurrentMode.CurrentValue.Name;
                }

                Azimuth.Value = AngleUnit.Value.PrintFromSi(Heading.Value);
                AltitudeMsl.Value = AltitudeUnit.Value.PrintFromSi(_devicePosition.Base.GlobalPosition.CurrentValue!.Alt);
                AltitudeAgl.Value = AltitudeUnit.Value.PrintFromSi(_devicePosition.Base.GlobalPosition.CurrentValue!.RelativeAlt);
                Roll.Value = _devicePosition.Roll.CurrentValue;
                Pitch.Value = _devicePosition.Pitch.CurrentValue;
                Heading.Value = _devicePosition.Yaw.CurrentValue;
                VerticalSpeed.Value = _devicePosition.Base.Attitude.CurrentValue!.Pitch;
                Velocity.Value = VelocityUnit.Value.PrintFromSi(_deviceGnss.Main.GroundVelocity.CurrentValue);
                SateliteCount.Value = _deviceGnss.Main.Info.CurrentValue.SatellitesVisible;
                VdopCount.Value = $"{_deviceGnss.Main.Info.CurrentValue.Vdop!.Value} VDOP";
                HdopCount.Value = $"{_deviceGnss.Main.Info.CurrentValue.Hdop!.Value} HDOP";
                RtkMode.Value = $"{GpsFixTypeToString(_deviceGnss.Main.Info.CurrentValue.FixType)}";
                GnssStatus();

                if (IsArmed.Value != _devicePosition.IsArmed.CurrentValue)
                {
                    UpdateStatusText(_devicePosition.IsArmed.CurrentValue
                        ? "Armed"
                        : "Disarmed");
                }

                IsArmed.Value = _devicePosition.IsArmed.CurrentValue;
                BatteryCharge.Value = $"{(int)telemetryClient.BatteryCharge.CurrentValue * 100} %";
                BatteryAmperage.Value = $"{(int)telemetryClient.BatteryCurrent.CurrentValue} A";
                BatteryVoltage.Value = $"{(int)telemetryClient.BatteryVoltage.CurrentValue} V";
                BatteryTime.Value = $"{telemetryClient.BatteryVoltage.CurrentValue} min";
                BatteryStatus((int)(telemetryClient.BatteryCharge.CurrentValue * 100));
            });
        });
    }

    private static double GetHomeAzimuthPosition(double? value, double headingValue)
    {
        if (value == null)
        {
            return -100;
        }

        var distance = (headingValue - value.Value) % 360;
        if (distance < -180)
        {
            distance += 360;
        }
        else if (distance > 179)
        {
            distance -= 360;
        }

        return distance;
    }

    private void BatteryStatus(int procent)
    {
        BatteryStatusBrush.Value.Color = procent switch
        {
            > 70 => _greenColor,
            > 50 => _yellowColor,
            > 40 => _orangeColor,
            < 30 => _redColor,
            _ => BatteryStatusBrush.Value.Color
        };
    }

    private void SpeedAltitudeCheck(int alt, int gs)
    {
        if (gs > 10 && alt < 40)
        {
            StatusText.Value = "Pull Up";
            AltitudeStatus.Value.Color = _yellowColor;
        }
        else
        {
            StatusText.Value = string.Empty;
            AltitudeStatus.Value.Color = _greenColor;
        }
    }

    private void GnssStatus()
    {
        if (_deviceGnss!.Main.Info.CurrentValue.FixType == Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat ||
            _deviceGnss!.Main.Info.CurrentValue.SatellitesVisible > 15 ||
            _deviceGnss!.Main.Info.CurrentValue.SatellitesVisible < 20)
        {
            GnssStatusBrush.Value.Color = _orangeColor;
        }
        else if ((_deviceGnss!.Main.Info.CurrentValue.FixType != Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat &&
                  _deviceGnss!.Main.Info.CurrentValue.FixType != Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed) ||
                 _deviceGnss!.Main.Info.CurrentValue.SatellitesVisible < 10)
        {
            GnssStatusBrush.Value.Color = _redColor;
        }
        else
        {
            GnssStatusBrush.Value.Color = _greenColor;
        }
    }

    private string GpsFixTypeToString(Mavlink.Common.GpsFixType type)
    {
        switch (type)
        {
            case Mavlink.Common.GpsFixType.GpsFixType2dFix:
                return "2D position Fix";
            case Mavlink.Common.GpsFixType.GpsFixTypeRtkFloat:
                return "RTK Float";
            case Mavlink.Common.GpsFixType.GpsFixTypeRtkFixed:
                return "Rtk Fixed";
            case Mavlink.Common.GpsFixType.GpsFixTypeDgps:
                return "DGPS/SBAS";
            case Mavlink.Common.GpsFixType.GpsFixTypePpp:
                return "PPP 3D position";
            case Mavlink.Common.GpsFixType.GpsFixType3dFix:
                return "3D position Fix";
            case Mavlink.Common.GpsFixType.GpsFixTypeStatic:
                return "Static Fix";
            case Mavlink.Common.GpsFixType.GpsFixTypeNoGps:
                return "No GPS connected";
        }

        return string.Empty;
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

    public NotifyCollectionChangedSynchronizedViewList<IRttItem> RttItemsList { get; set; }
    public Observable<List<IRttItem>> RttItems;

    public ReactiveCommand GoTo { get; set; }
    public ReactiveCommand TakeOff { get; set; }
    public ReactiveCommand Rtl { get; set; }
    public ReactiveCommand Land { get; set; }
    public ReactiveCommand Guided { get; set; }
    public ReactiveCommand StartMission { get; set; }
    
    public BindableReactiveProperty<MissionProgressViewModel> MissionProgress { get; set; } = new();

    public BindableReactiveProperty<bool> IsBusy { get; set; } = new();

    #region BatteryRtt

    public BindableReactiveProperty<string> BatteryTime { get; set; } = new();
    public BindableReactiveProperty<string> BatteryAmperage { get; set; } = new();
    public BindableReactiveProperty<string> BatteryCharge { get; set; } = new();
    public BindableReactiveProperty<string> BatteryVoltage { get; set; } = new();

    public BindableReactiveProperty<SolidColorBrush> BatteryStatusBrush { get; set; } =
        new(new SolidColorBrush(_greenColor));

    #endregion

    #region Gnss

    public BindableReactiveProperty<int> SateliteCount { get; set; } = new();
    public BindableReactiveProperty<string> HdopCount { get; set; } = new();
    public BindableReactiveProperty<string> VdopCount { get; set; } = new();
    public BindableReactiveProperty<string> RtkMode { get; set; } = new();

    public BindableReactiveProperty<SolidColorBrush> GnssStatusBrush { get; set; } =
        new(new SolidColorBrush(_greenColor));

    #endregion

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
        new(new SolidColorBrush(_greenColor));

    public BindableReactiveProperty<string> AltitudeAgl { get; set; } = new();
    public BindableReactiveProperty<string> AltitudeMsl { get; set; } = new();

    public BindableReactiveProperty<double> Heading { get; set; } = new();
    public BindableReactiveProperty<double> VerticalSpeed { get; set; } = new();

    public BindableReactiveProperty<int> HomeAzimuth { get; set; } = new();
    public BindableReactiveProperty<string> Azimuth { get; set; } = new();

    public BindableReactiveProperty<string> StatusText { get; set; } = new();

    public BindableReactiveProperty<string> MissionDistance { get; set; } = new();

    public BindableReactiveProperty<bool> IsArmed { get; set; } = new();

    public BindableReactiveProperty<TimeSpan> ArmedTime { get; set; } = new();

    public BindableReactiveProperty<IUnitItem> VelocityUnit { get; set; } = new();
    public BindableReactiveProperty<IUnitItem> AltitudeUnit { get; set; } = new();
    public BindableReactiveProperty<IUnitItem> BearingUnit { get; set; } = new();
    public BindableReactiveProperty<IUnitItem> AngleUnit { get; set; } = new();


    public IClientDevice Device { get; }


    public WorkspaceDock Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }
}