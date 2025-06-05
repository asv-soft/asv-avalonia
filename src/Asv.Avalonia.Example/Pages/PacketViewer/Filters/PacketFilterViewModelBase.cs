using System;
using System.Threading;
using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

public abstract class PacketFilterViewModelBase<TFilter> : RoutableViewModel
    where TFilter : PacketFilterViewModelBase<TFilter>
{
    public const string FilterId = $"packet-filter.{nameof(TFilter)}";
    private const int BaseMovingAverageSize = 3;

    private readonly IUnit _unit;
    private volatile int _cnt;

    protected virtual IncrementalRateCounter PacketRate => new(BaseMovingAverageSize);

    public PacketFilterViewModelBase(IUnitService unitService)
        : base(FilterId)
    {
        Id.ChangeArgs(Guid.NewGuid().ToString());
        MessageRateText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(
            Disposable
        );
        IsChecked = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        _unit = unitService.Units[FrequencyBase.Id];
        IncreaseRatesCounterSafe();

        IsChecked.Value = true;

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ => UpdateRateText())
            .DisposeItWith(Disposable);
    }

    public void IncreaseRatesCounterSafe()
    {
        Interlocked.Increment(ref _cnt);
    }

    private void UpdateRateText()
    {
        var packetRate = Math.Round(PacketRate.Calculate(_cnt), 1);
        MessageRateText.Value = _unit.CurrentUnitItem.Value.PrintWithUnits(packetRate, "F2");
    }

    public abstract BindableReactiveProperty<string> FilterValue { get; }

    public BindableReactiveProperty<string> MessageRateText { get; }
    public BindableReactiveProperty<bool> IsChecked { get; }
}
