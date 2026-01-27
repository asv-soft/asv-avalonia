using Asv.Cfg;
using Asv.Common;
using Avalonia.Media;
using DotNext;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class UnitPropertyViewModel
    : HistoricalUnitProperty,
        IPropertyViewModel,
        ISupportCancel,
        ISupportRefresh
{
    private double _lastValue;

    public UnitPropertyViewModel()
        : this(
            DesignTime.Id.ToString(),
            new BindableReactiveProperty<double>(),
            new AltitudeBase(
                DesignTime.Configuration,
                [new MeterAltitudeUnit(), new FeetAltitudeUnit()]
            ),
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Parent = DesignTime.Shell;
        Header = "Altitude";
        Description = "Altitude description";
        Icon = MaterialIconKind.Altimeter;
        Observable
            .Timer(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5))
            .Subscribe(x =>
            {
                ShortName = Random.Shared.NextBoolean() ? "Alt" : null;
                Icon = Random.Shared.NextBoolean() ? DesignTime.RandomImage : null;
                ViewValue.Value = Random.Shared.NextBoolean()
                    ? "err value"
                    : Random.Shared.Next<short>().ToString();
            });
        ChangeUnitCommand = new ReactiveCommand<IUnitItem>(x => { });
    }

    public UnitPropertyViewModel(
        string id,
        ReactiveProperty<double> modelValue,
        IUnit unit,
        ILoggerFactory loggerFactory,
        string? format = null
    )
        : base(id, modelValue, unit, loggerFactory, format)
    {
        ChangeUnitCommand = new ReactiveCommand<IUnitItem>(item =>
            ChangeMeasureUnitCommand.ExecuteCommand(this, unit, item).SafeFireAndForget()
        ).DisposeItWith(Disposable);
        CurrentUnit = unit
            .CurrentUnitItem.ToBindableReactiveProperty(unit.CurrentUnitItem.Value)
            .DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<IUnitItem> CurrentUnit { get; }
    public ReactiveCommand<IUnitItem> ChangeUnitCommand { get; }

    public void CommitValue()
    {
        IsInEditMode = false;
        this.ExecuteCommand(ChangeDoublePropertyCommand.Id, new DoubleArg(_lastValue))
            .SafeFireAndForget();
    }

    protected override ValueTask ApplyValueToModel(double value, CancellationToken cancel)
    {
        IsInEditMode = true;
        _lastValue = value;
        return ValueTask.CompletedTask;
    }

    protected override void OnChangeByModel(double modelValue)
    {
        IsInEditMode = false;
        base.OnChangeByModel(modelValue);
    }

    public bool IsInEditMode
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ShortName
    {
        get;
        set => SetField(ref field, value);
    }

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }
    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }
    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }
    public string? Description
    {
        get;
        set => SetField(ref field, value);
    }
    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    }
    public int Order
    {
        get;
        set => SetField(ref field, value);
    }

    public void Cancel()
    {
        IsInEditMode = false;
        OnChangeByModel(ModelValue.CurrentValue);
    }

    public void Refresh()
    {
        IsInEditMode = false;
        OnChangeByModel(ModelValue.CurrentValue);
    }
}
