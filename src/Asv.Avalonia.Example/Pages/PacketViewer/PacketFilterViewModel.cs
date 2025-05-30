using System;
using System.Collections.Generic;
using System.Threading;
using Asv.Avalonia;
using Asv.Avalonia.Example.PacketViewer;
using Asv.Common;
using R3;

public sealed class PacketFilterViewModel : RoutableViewModel
{
    private volatile int _cnt;
    private readonly IncrementalRateCounter _packetRate = new(3);
    private readonly IUnit _unit;

    public BindableReactiveProperty<string> Type { get; }
    public BindableReactiveProperty<string> Source { get; }
    public BindableReactiveProperty<string> MessageRateText { get; }
    public BindableReactiveProperty<bool> IsChecked { get; }

    public PacketFilterViewModel()
        : this(new PacketMessageViewModel(), NullUnitService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PacketFilterViewModel(PacketMessageViewModel pkt, IUnitService unitService)
        : base(pkt.Id.ToString())
    {
        Type = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        Source = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        MessageRateText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(
            Disposable
        );
        IsChecked = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        _unit =
            unitService[VelocityBase.Id]
            ?? throw new ArgumentException(
                $"Unit with ID '{VelocityBase.Id}' not found in IUnitService"
            );
        UpdateRates();

        Type.Value = pkt.Type;
        Source.Value = pkt.Source;
        IsChecked.Value = true;
    }

    public void UpdateRates()
    {
        Interlocked.Increment(ref _cnt);
    }

    public void UpdateRateText()
    {
        var packetRate = Math.Round(_packetRate.Calculate(_cnt), 1);
        MessageRateText.Value = _unit.InternationalSystemUnit.PrintWithUnits(packetRate);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
