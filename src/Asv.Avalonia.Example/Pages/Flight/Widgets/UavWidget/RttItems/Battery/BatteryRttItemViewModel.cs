using System;
using System.Composition;
using Asv.IO;
using Asv.Mavlink;
using Avalonia.Media;
using R3;

namespace Asv.Avalonia.Example;

[ExportRttItem]
[Shared]
public class BatteryRttItemViewModel : DisposableViewModel, IRttItem
{
    private readonly IDisposable _sub1;
    
    [ImportingConstructor]
    public BatteryRttItemViewModel(IClientDevice device, IMavlinkConnectionService connectionService): base("battery")
    {
        ArgumentNullException.ThrowIfNull(device);
       var telemetry = device.GetMicroservice<TelemetryClientEx>();
       if (telemetry is null)
       {
           return;
       }
       
       _sub1 = connectionService.Router.OnRxMessage.Subscribe(_ =>
       {
           BatteryCharge.Value = $"{(int)telemetry.BatteryCharge.CurrentValue * 100} %";
           BatteryAmperage.Value = $"{(int)telemetry.BatteryCurrent.CurrentValue} A";
           BatteryVoltage.Value = $"{(int)telemetry.BatteryVoltage.CurrentValue} V";
           BatteryTime.Value = $"{telemetry.BatteryVoltage.CurrentValue} min";
           BatteryStatus((int)(telemetry.BatteryCharge.CurrentValue * 100));
       });
    }
    
    private void BatteryStatus(int procent)
    {
        BatteryStatusBrush.Value.Color = procent switch
        {
            > 70 => GreenStatusColor,
            > 50 => YellowStatusColor,
            > 40 => OrangeStatusColor,
            < 30 => RedStatusColor,
            _ => BatteryStatusBrush.Value.Color
        };
    }
    
    public BindableReactiveProperty<SolidColorBrush> BatteryStatusBrush { get; set; } =
        new(new SolidColorBrush(GreenStatusColor));
    
    public static Color GreenStatusColor { get; } = Color.Parse("#21c088");
    public static Color YellowStatusColor { get; } = Color.Parse("#dfc34a");
    public static Color OrangeStatusColor { get; } = Color.Parse("#e48f4d");
    public static Color RedStatusColor { get; } = Color.Parse("#cc5058");
    
    public BindableReactiveProperty<string> BatteryTime { get; set; } = new();
    public BindableReactiveProperty<string> BatteryAmperage { get; set; } = new();
    public BindableReactiveProperty<string> BatteryCharge { get; set; } = new();
    public BindableReactiveProperty<string> BatteryVoltage { get; set; } = new();

    protected override void Dispose(bool disposing)
    {
        _sub1.Dispose();
        base.Dispose(disposing);
    }

    public int Order { get; init; }
}
