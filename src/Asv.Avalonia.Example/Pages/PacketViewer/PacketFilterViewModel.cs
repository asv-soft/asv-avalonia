using System;
using System.Threading;
using Asv.Avalonia;
using Asv.Avalonia.Example.PacketViewer;
using Asv.Common;
using R3;

public class PacketFilterViewModel
{
    private volatile int _cnt;
    private readonly IncrementalRateCounter _packetRate = new(3);
    private readonly IUnit _unit;
    private readonly CompositeDisposable _disposables = new();

    public BindableReactiveProperty<string> Type { get; }
    public BindableReactiveProperty<string> Source { get; }
    public BindableReactiveProperty<string> MessageRateText { get; }
    public BindableReactiveProperty<bool> IsChecked { get; }

    public PacketFilterViewModel()
    {
        Type = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(_disposables);
        Source = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(_disposables);
        MessageRateText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(_disposables);
        IsChecked = new BindableReactiveProperty<bool>(false).DisposeItWith(_disposables);
        UpdateRates();
    }

    public PacketFilterViewModel(PacketMessageViewModel pkt, IUnitService unitService)
        : this()
    {
        _unit = unitService["velocity"] ?? throw new ArgumentException("Unit not found");
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
    
    public void Dispose()
    {
        _disposables.Dispose();
    }
}