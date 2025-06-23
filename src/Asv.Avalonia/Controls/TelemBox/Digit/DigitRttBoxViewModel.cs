using Asv.Common;
using Asv.Drones.Gui.Plugin.Afis;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class DigitRttBoxViewModel : RttBoxViewModel
{
    private readonly TimeSpan? _networkErrorTimeout;

    public DigitRttBoxViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
        MeasureUnit = new MeterDistanceUnit();
        Icon = MaterialIconKind.Ruler;
        Header = "Distance";
        Units = MeasureUnit.Symbol;
        FormatString = "## 000.000";
        var sub = new Subject<double>();
        Observable<double> value = sub;
        int index = 0;
        int maxIndex = Enum.GetValues<RttBoxStatus>().Length;
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

                Status = Enum.GetValues<RttBoxStatus>()[index++ % maxIndex];
                ProgressStatus = Enum.GetValues<RttBoxStatus>()[index++ % maxIndex];
                Updated();
            });
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
            units[unitId]?.CurrentUnitItem.CurrentValue
            ?? throw new ArgumentException($"{unitId} unit not found in unit service");
        Units = MeasureUnit.Symbol;
        value
            .ThrottleLastFrame(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(OnValueChanged)
            .DisposeItWith(Disposable);
    }

    protected IUnitItem MeasureUnit { get; }
    public string Units
    {
        get;
        set => SetField(ref field, value);
    }

    public string? FormatString
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ValueString
    {
        get;
        set => SetField(ref field, value);
    }

    protected virtual void OnValueChanged(double value)
    {
        ValueString = MeasureUnit.PrintFromSi(value, FormatString);
        if (_networkErrorTimeout != null)
        {
            Updated();
        }
    }
}
