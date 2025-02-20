using System.ComponentModel;
using Asv.Common;
using Asv.IO;
using R3;

namespace Asv.Avalonia;

public enum ConnectionType
{
    TCP,
    UDP,
    Serial,
}

public class SettingsConnectionItemViewModel: DisposableViewModel
{
    public SettingsConnectionItemViewModel(IProtocolPort portInfo, IMavlinkConnectionService service) : base("1")
    {
        IsEnabled.Subscribe(b =>
        {
            if (b)
            {
                service.EnablePort(portInfo);
            }
            else
            {
                service.DisablePort(portInfo);
            }
        });
        
        service.Router.OnRxMessage.ThrottleLast(TimeSpan.FromSeconds(1)).Subscribe(_ =>
        {
            Name.Value = portInfo.TypeInfo.Name;
            ConnectionString.Value = portInfo.TypeInfo.Scheme;
            RxPacketsAmount.Value = portInfo.Statistic.RxMessages;
            TxPacketsAmount.Value = portInfo.Statistic.TxMessages;
            RxPacketsErrorsAmount.Value = portInfo.Statistic.RxError;
            TxPacketsErrorsAmount.Value = portInfo.Statistic.TxError;
            IsEnabled.Value = portInfo.IsEnabled.CurrentValue;
        }).DisposeItWith(Disposable);
    }
    
    public BindableReactiveProperty<string> Name { get; set; } = new();
    public BindableReactiveProperty<string> ConnectionString { get; init; } = new();
    public BindableReactiveProperty<uint> RxPacketsAmount { get; set; } = new();
    public BindableReactiveProperty<uint> RxPacketsErrorsAmount { get; set; } = new();
    public BindableReactiveProperty<uint> TxPacketsErrorsAmount { get; set; } = new();
    public BindableReactiveProperty<uint> TxPacketsAmount { get; set; } = new();
    public BindableReactiveProperty<bool> IsEnabled { get; set; } = new(true);
}