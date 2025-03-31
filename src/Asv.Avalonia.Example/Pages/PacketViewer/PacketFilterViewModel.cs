using System;
using System.Threading;
using Asv.Avalonia;
using Asv.Avalonia.Example.PacketViewer;
using Asv.Common;
using R3;

public class PacketFilterViewModel : IDisposable
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
        : this(null!, NullUnitService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PacketFilterViewModel(PacketMessageViewModel pkt, IUnitService unitService)
    {
        Type = new BindableReactiveProperty<string>(string.Empty);
        Source = new BindableReactiveProperty<string>(string.Empty);
        MessageRateText = new BindableReactiveProperty<string>(string.Empty);
        IsChecked = new BindableReactiveProperty<bool>(false);
        _disposables.Add(Type);
        _disposables.Add(Source);
        _disposables.Add(MessageRateText);
        _disposables.Add(IsChecked);
        _unit =
            unitService[VelocityBase.Id]
            ?? throw new ArgumentException(
                $"Unit with ID '{VelocityBase.Id}' not found in IUnitService"
            );
        UpdateRates();

        if (pkt != null)
        {
            Type.Value = pkt.Type;
            Source.Value = pkt.Source;
            IsChecked.Value = true;
        }
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
