using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Asv.Avalonia.Map;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Avalonia.Threading;
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
    private PositionClientEx? _devicePosition;
    private GnssClientEx? _deviceGnss;
    private ControlClient? _controlClient;
    public const string WidgetId = "widget-uav";

    public UavWidgetViewModel()
        : base(WidgetId)
    {
        DesignTime.ThrowIfNotDesignMode();
        InitArgs("1");
        Altitude.Value = 300;
        Velocity.Value = 15;
        HomeAzimuth.Value = -115;

    }

    [ImportingConstructor]
    public UavWidgetViewModel(IMavlinkConnectionService service, IClientDevice device)
        : base(WidgetId)
    {
        ArgumentNullException.ThrowIfNull(device);
       
        Device = device;
        Position = WorkspaceDock.Left;
        Icon = DeviceIconMixin.GetIcon(device.Id);
        IconBrush = DeviceIconMixin.GetIconBrush(device.Id);
        device.Name.Subscribe(x => Header = x).DisposeItWith(Disposable);
        InitArgs(device.Id.AsString());
        device.WaitUntilConnectAndInit(1000, TimeProvider.System);
        _devicePosition = device.GetMicroservice<PositionClientEx>();
        _deviceGnss = device.GetMicroservice<GnssClientEx>();
        _controlClient = device.GetMicroservice<ControlClient>();
        var telemetryClient = device.GetMicroservice<TelemetryClientEx>();
        GoTo = new ReactiveCommand(_ =>
        {
            _controlClient!.SetAutoMode(CancellationToken.None);
        });
        TakeOff = new ReactiveCommand(_ =>
        {
            _controlClient!.SetGuidedMode(CancellationToken.None);
            _controlClient!.TakeOff(100, CancellationToken.None);
        });
        Rtl = new ReactiveCommand(_ =>
        {
            _controlClient!.SetGuidedMode(CancellationToken.None);
            _controlClient!.DoRtl(CancellationToken.None);
        });
        
        service.Router.OnRxMessage.Subscribe(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Altitude.Value = _devicePosition!.AltitudeAboveHome.CurrentValue;
                Roll.Value = _devicePosition!.Roll.CurrentValue;
                Pitch.Value = _devicePosition!.Pitch.CurrentValue;
                Heading.Value = _devicePosition!.Yaw.CurrentValue;
                HomeAzimuth.Value = 0.0;
                Velocity.Value = _deviceGnss!.Main.GroundVelocity.CurrentValue;
                UpdateStatusText(_devicePosition.IsArmed.CurrentValue
                    ? "Armed"
                    : "Disarmed");
            
                BatteryCharge.Value = telemetryClient!.BatteryCharge.CurrentValue;
                BatteryAmperage.Value = telemetryClient!.BatteryCurrent.CurrentValue;
                BatteryVoltage.Value = telemetryClient!.BatteryVoltage.CurrentValue;
                if (_ is not VibrationPacket vibration)
                {
                    return;
                }

                VibrationX.Value = vibration.Payload.VibrationX;
                VibrationY.Value = vibration.Payload.VibrationY;
                VibrationZ.Value = vibration.Payload.VibrationZ;
                Clipping0.Value = vibration.Payload.Clipping0;
                Clipping1.Value = vibration.Payload.Clipping1;
                Clipping2.Value = vibration.Payload.Clipping2;
            });
        });
    }

    private void UpdateStatusText(string text)
    {
        StatusText.Value = text;
        R3.Observable.Timer(TimeSpan.FromSeconds(10)).Subscribe(_ => StatusText.Value = string.Empty)
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
    
    #region BatteryRtt

    public BindableReactiveProperty<string> BatteryTime { get; set; } = new();
    public BindableReactiveProperty<double> BatteryAmperage { get; set; } = new();
    public BindableReactiveProperty<double> BatteryCharge { get; set; } = new();
    public BindableReactiveProperty<double> BatteryVoltage { get; set; } = new(); 

    #endregion
    
    public BindableReactiveProperty<float> VibrationX { get; set; } = new();

    public BindableReactiveProperty<float> VibrationY { get; set; } = new();

    public BindableReactiveProperty<float> VibrationZ { get; set; } = new();

    public BindableReactiveProperty<uint> Clipping0 { get; set; } = new();

    public BindableReactiveProperty<uint> Clipping1 { get; set; } = new();

    public BindableReactiveProperty<uint> Clipping2 { get; set; } = new();

    public BindableReactiveProperty<double> Roll { get; set; } = new();

    public BindableReactiveProperty<double> Pitch { get; set; } = new();

    public BindableReactiveProperty<double> Velocity { get; set; } = new();

    public BindableReactiveProperty<double> Altitude { get; set; } = new();

    public BindableReactiveProperty<double> Heading { get; set; } = new();

    public BindableReactiveProperty<double?> HomeAzimuth { get; set; } = new();

    public BindableReactiveProperty<string> StatusText { get; set; } = new();

    public BindableReactiveProperty<string> MissionDistance { get; set; } = new();

    public BindableReactiveProperty<bool> IsArmed { get; set; } = new();

    public BindableReactiveProperty<TimeSpan> ArmedTime { get; set; } = new();


    public IClientDevice Device { get; }


    public WorkspaceDock Position
    {
        get => _position;
        set => SetField(ref _position, value);
    }
}