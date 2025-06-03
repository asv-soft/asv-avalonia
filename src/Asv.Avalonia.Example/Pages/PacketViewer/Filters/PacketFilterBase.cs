using System;
using System.Threading;
using Asv.Common;
using R3;

namespace Asv.Avalonia.Example;

public abstract class PacketFilterBase<TType> : RoutableViewModel
    where TType : PacketFilterBase<TType>
{
    public const string FilterId = $"packet-filter.{nameof(TType)}";

    private readonly IUnit _unit;
    private volatile int _cnt;

    protected const int BaseMovingAverageSize = 3;
    protected virtual IncrementalRateCounter PacketRate => new(BaseMovingAverageSize);

    public PacketFilterBase(IUnitService unitService)
        : base(FilterId)
    {
        Id.ChangeArgs(Guid.NewGuid().ToString());
        MessageRateText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(
            Disposable
        );
        IsChecked = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        _unit =
            unitService[VelocityBase.Id] // TODO: Find out why it is Velocity.
            ?? throw new ArgumentException(
                $"Unit with ID '{VelocityBase.Id}' not found in IUnitService"
            );
        UpdateRates();

        IsChecked.Value = true;
    }

    public void UpdateRates()
    {
        Interlocked.Increment(ref _cnt);
    }

    public void UpdateRateText()
    {
        var packetRate = Math.Round(PacketRate.Calculate(_cnt), 1);
        MessageRateText.Value = _unit.InternationalSystemUnit.PrintWithUnits(packetRate);
    }

    public abstract required BindableReactiveProperty<string> Value { get; init; }
    public required BindableReactiveProperty<string> MessageRateText { get; init; }
    public required BindableReactiveProperty<bool> IsChecked { get; init; }
}
