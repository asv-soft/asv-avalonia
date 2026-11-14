using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class GeoPointDialogStepConfig
{
    public string? Text { get; set; }
    public string? UnitItemId { get; set; }
}

public sealed class GeoPointDialogStepViewModel : ViewModel
{
    private const string DefaultText = "1";

    private readonly IUnit _unit;
    private bool _isLoading;

    public GeoPointDialogStepViewModel(string id, IUnit unit, string header, MaterialIconKind icon)
        : base(id)
    {
        _unit = unit;
        Header = header;
        Icon = icon;

        UnitItem = new BindableReactiveProperty<IUnitItem>(
            unit.CurrentUnitItem.CurrentValue
        ).DisposeItWith(Disposable);
        Text = new BindableReactiveProperty<string?>(DefaultText).DisposeItWith(Disposable);
        Text.EnableValidation(text => Validate(text, UnitItem.Value));
        ChangeUnitCommand = new ReactiveCommand<IUnitItem>(item =>
            UnitItem.Value = item
        ).DisposeItWith(Disposable);
        UnitItem.Skip(1).Subscribe(_ => Text.ForceValidate()).DisposeItWith(Disposable);

        IsValid = Text.CombineLatest(UnitItem, (text, item) => Validate(text, item) is null);

        Layout
            .Register("Text", Load, Save, IsValid.Skip(1).Where(isValid => isValid && !_isLoading))
            .DisposeItWith(Disposable);
        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);
    }

    public string Header { get; }
    public MaterialIconKind Icon { get; }
    public IEnumerable<IUnitItem> Units => _unit.AvailableUnits.Values;
    public BindableReactiveProperty<IUnitItem> UnitItem { get; }
    public BindableReactiveProperty<string?> Text { get; }
    public ReactiveCommand<IUnitItem> ChangeUnitCommand { get; }
    public Observable<bool> IsValid { get; }

    public double ValueInSi => UnitItem.Value.ParseToSi(Text.Value);

    private GeoPointDialogStepConfig Save()
    {
        return new GeoPointDialogStepConfig
        {
            Text = Text.Value,
            UnitItemId = UnitItem.Value.UnitItemId,
        };
    }

    private void Load(GeoPointDialogStepConfig config)
    {
        if (
            config.UnitItemId is null
            || !_unit.AvailableUnits.TryGetValue(config.UnitItemId, out var unitItem)
            || Validate(config.Text, unitItem) is not null
        )
        {
            return;
        }

        _isLoading = true;
        try
        {
            UnitItem.Value = unitItem;
            Text.Value = config.Text;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private static Exception? Validate(string? text, IUnitItem unitItem)
    {
        var result = unitItem.ValidateValue(text);
        if (!result.IsSuccess)
        {
            return result.ValidationException?.GetExceptionWithLocalizationOrSelf();
        }

        var value = unitItem.ParseToSi(text);
        return value > 0 && double.IsFinite(value)
            ? null
            : new UnitException(RS.GeoPointDialogStep_PositiveRequired);
    }
}
