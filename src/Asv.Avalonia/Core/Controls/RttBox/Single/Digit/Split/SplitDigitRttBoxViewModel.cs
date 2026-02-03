using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class SplitDigitRttBoxViewModel : DigitRttBoxViewModel
{
    private readonly TimeSpan? _networkErrorTimeout;

    public SplitDigitRttBoxViewModel()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SplitDigitRttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IUnitService units,
        string unitId,
        Observable<double> value,
        TimeSpan? networkErrorTimeout
    )
        : base(id, loggerFactory, units, unitId, value, networkErrorTimeout)
    {
        _networkErrorTimeout = networkErrorTimeout;
    }

    public int? FractionDigits { get; init; }

    public string? FracString
    {
        get;
        private set => SetField(ref field, value);
    }

    protected override void OnValueChanged(double value)
    {
        if (FractionDigits is null)
        {
            MeasureUnit.CurrentUnitItem.Value.PrintSplitString(
                value,
                FormatString,
                out var intFormat,
                out var fracFormat
            );
            ValueString = intFormat;
            FracString = fracFormat;
        }
        else
        {
            MeasureUnit.CurrentUnitItem.Value.PrintSplitString(
                value,
                FormatString,
                FractionDigits.Value,
                out var intFormat,
                out var fracFormat
            );
            ValueString = intFormat;
            FracString = fracFormat;
        }

        if (_networkErrorTimeout is not null)
        {
            Updated();
        }
    }
}
