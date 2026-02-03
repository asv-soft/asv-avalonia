using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class DigitRttBoxViewModel : SingleRttBoxViewModel
{
    private readonly TimeSpan? _networkErrorTimeout;

    public DigitRttBoxViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        MeasureUnit = DesignTime.UnitService.Units[DistanceBase.Id];
        Icon = MaterialIconKind.Ruler;
        Header = "Distance";
        UnitSymbol = MeasureUnit.CurrentUnitItem.CurrentValue.Symbol;
        FormatString = "## 000.000";
        var sub = new Subject<double>().DisposeItWith(Disposable);
        Observable<double> value = sub;
        int index = 0;
        int maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                if (Random.Shared.NextDouble() > 0.8)
                {
                    IsNetworkError = true;
                    return;
                }

                Progress = Random.Shared.NextDouble();
                if (Random.Shared.NextDouble() > 0.9)
                {
                    sub.OnNext(double.NaN);
                }
                else
                {
                    sub.OnNext(Random.Shared.Next(-6553500, 6553500) / 100.0);
                }

                Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                Updated();
            })
            .DisposeItWith(Disposable);
        value
            .ThrottleLastFrame(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(OnValueChanged)
            .DisposeItWith(Disposable);
    }

    public DigitRttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IUnitService units,
        string unitId,
        Observable<double> value,
        TimeSpan? networkErrorTimeout
    )
        : base(id, loggerFactory, networkErrorTimeout)
    {
        _networkErrorTimeout = networkErrorTimeout;
        MeasureUnit =
            units[unitId]
            ?? throw new ArgumentException($"{unitId} unit not found in unit service");
        UnitSymbol = MeasureUnit.CurrentUnitItem.CurrentValue.Symbol;
        MeasureUnit
            .CurrentUnitItem.ObserveOnUIThreadDispatcher()
            .Subscribe(s => UnitSymbol = s.Symbol)
            .DisposeItWith(Disposable);

        value
            .ThrottleLastFrame(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(OnValueChanged)
            .DisposeItWith(Disposable);
    }

    public string? FormatString
    {
        get;
        set => SetField(ref field, value);
    }

    public new string? ValueString
    {
        get;
        protected set => SetField(ref field, value);
    }

    public new string? UnitSymbol
    {
        get;
        private set => SetField(ref field, value);
    }

    protected IUnit MeasureUnit { get; }

    protected virtual void OnValueChanged(double value)
    {
        ValueString = MeasureUnit.CurrentUnitItem.Value.PrintFromSi(value, FormatString);
        if (_networkErrorTimeout is not null)
        {
            Updated();
        }
    }
}
