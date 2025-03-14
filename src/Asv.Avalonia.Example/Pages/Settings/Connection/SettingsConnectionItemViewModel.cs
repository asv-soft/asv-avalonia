using System;
using Asv.Common;
using Asv.IO;
using R3;

namespace Asv.Avalonia.Example;

public class SettingsConnectionItemViewModel : DisposableViewModel
{

    public SettingsConnectionItemViewModel(
        string name,
        IProtocolPort portInfo,
        IMavlinkConnectionService service
    )
        : base(portInfo.Id)
    {
        Name.Value = name;
        ConnectionString.Value = portInfo.Config.AsUri().ToString();
        RemovePort = new ReactiveCommand(_ => service.RemovePort(portInfo));
        IsEnabled = new BindableReactiveProperty<bool>(portInfo.IsEnabled.CurrentValue);
        ConnectionString.Value = portInfo.Id;
        RxPacketsAmount.Value = portInfo.Statistic.RxMessages;
        TxPacketsAmount.Value = portInfo.Statistic.TxMessages;
        RxPacketsErrorsAmount.Value = portInfo.Statistic.RxError;
        TxPacketsErrorsAmount.Value = portInfo.Statistic.TxError;
        IsEnabled.Value = portInfo.IsEnabled.CurrentValue;
        Status.Value = portInfo.Status.CurrentValue;
        Port.Value = portInfo;
        
        service
            .Router.OnRxMessage.ThrottleFirst(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                ConnectionString.Value = portInfo.Config.AsUri().ToString();
                RxPacketsAmount.Value = portInfo.Statistic.RxMessages;
                TxPacketsAmount.Value = portInfo.Statistic.TxMessages;
                RxPacketsErrorsAmount.Value = portInfo.Statistic.RxError;
                TxPacketsErrorsAmount.Value = portInfo.Statistic.TxError;
                IsEnabled.Value = portInfo.IsEnabled.CurrentValue;
                Status.Value = portInfo.Status.CurrentValue;
                Port.Value = portInfo;
            })
            .DisposeItWith(Disposable);
        service
            .Router.PortAdded.ThrottleFirst(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                ConnectionString.Value = portInfo.Config.AsUri().ToString();
                RxPacketsAmount.Value = portInfo.Statistic.RxMessages;
                TxPacketsAmount.Value = portInfo.Statistic.TxMessages;
                RxPacketsErrorsAmount.Value = portInfo.Statistic.RxError;
                TxPacketsErrorsAmount.Value = portInfo.Statistic.TxError;
                IsEnabled.Value = portInfo.IsEnabled.CurrentValue;
                Status.Value = portInfo.Status.CurrentValue;
                Port.Value = portInfo;
            })
            .DisposeItWith(Disposable);
        service
            .Router.PortRemoved.ThrottleFirst(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                ConnectionString.Value = portInfo.Config.AsUri().ToString();
                RxPacketsAmount.Value = portInfo.Statistic.RxMessages;
                TxPacketsAmount.Value = portInfo.Statistic.TxMessages;
                RxPacketsErrorsAmount.Value = portInfo.Statistic.RxError;
                TxPacketsErrorsAmount.Value = portInfo.Statistic.TxError;
                IsEnabled.Value = portInfo.IsEnabled.CurrentValue;
                Status.Value = portInfo.Status.CurrentValue;
                Port.Value = portInfo;
            })
            .DisposeItWith(Disposable);

        IsEnabled
            .Subscribe(b =>
            {
                if (b)
                {
                    service.EnablePort(portInfo);
                }
                else
                {
                    service.DisablePort(portInfo);
                }
            })
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string> Name { get; set; } = new();
    public BindableReactiveProperty<IProtocolPort> Port { get; set; } = new();
    public BindableReactiveProperty<string> ConnectionString { get; init; } = new();
    public BindableReactiveProperty<uint> RxPacketsAmount { get; set; } = new();
    public BindableReactiveProperty<uint> RxPacketsErrorsAmount { get; set; } = new();
    public BindableReactiveProperty<uint> TxPacketsErrorsAmount { get; set; } = new();
    public BindableReactiveProperty<uint> TxPacketsAmount { get; set; } = new();
    public BindableReactiveProperty<ProtocolPortStatus> Status { get; set; } = new();
    public BindableReactiveProperty<bool> IsEnabled { get; set; }
    public ReactiveCommand RemovePort { get; set; }
    public ReactiveCommand EditPortCommand { get; set; }
}