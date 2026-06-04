using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public sealed class HistoricalControlsPageViewModelConfig
{
    public double Speed { get; set; } = -1;
    public double Time { get; set; } = -1;
    public bool IsTurnedOn { get; set; } = false;
    public string StringPropWithoutValidation { get; set; } = string.Empty;
    public string StringPropWithOneValidation { get; set; } = string.Empty;
    public string StringPropWithManyValidations { get; set; } = string.Empty;
    public GeoPoint GeoPointProperty { get; set; } = GeoPoint.ZeroWithAlt;
    public AsvColorKind TagTypeProp { get; set; } = AsvColorKind.Unknown;
    public AsvColorKind AsvColorKindProp { get; set; } = AsvColorKind.Success;
}

public class HistoricalControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "historical-controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.History;

    private readonly ReactiveProperty<bool> _isTurnedOn;
    private readonly ReactiveProperty<double> _speed;
    private readonly ReactiveProperty<double> _time;
    private readonly ReactiveProperty<string?> _stringWithManyValidations;
    private readonly ReactiveProperty<string?> _stringWithOneValidation;
    private readonly ReactiveProperty<string?> _stringWithoutValidation;
    private readonly ReactiveProperty<GeoPoint> _geoPointProperty;
    private readonly ReactiveProperty<Enum> _tagTypeProp;
    private readonly ReactiveProperty<Enum> _rttBoxStatusProp;
    private readonly Subject<Unit> _layoutChanged = new();

    public HistoricalControlsPageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance,
            DesignTime.UnitService,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public HistoricalControlsPageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        IUnitService unit,
        ILoggerFactory loggerFactory
    )
        : base(PageId, context)
    {
        _layoutChanged.DisposeItWith(Disposable);
        var speedUnit = unit.GetRequiredUnitOfType<VelocityUnit>(VelocityUnit.Id);
        var timeUnit = unit.GetRequiredUnitOfType<TimeSpanUnit>(TimeSpanUnit.Id);

        _speed = new ReactiveProperty<double>(double.NaN).DisposeItWith(Disposable);
        _time = new ReactiveProperty<double>(0).DisposeItWith(Disposable);
        _isTurnedOn = new ReactiveProperty<bool>().DisposeItWith(Disposable);
        _stringWithoutValidation = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _stringWithOneValidation = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _stringWithManyValidations = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _geoPointProperty = new ReactiveProperty<GeoPoint>().DisposeItWith(Disposable);
        _tagTypeProp = new ReactiveProperty<Enum>(AsvColorKind.Error).DisposeItWith(Disposable);
        _rttBoxStatusProp = new ReactiveProperty<Enum>(AsvColorKind.Success).DisposeItWith(
            Disposable
        );

        IsTurnedOn = new HistoricalBoolProperty("is-turned-on", _isTurnedOn)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        TurnOn = new ReactiveCommand(_ =>
            IsTurnedOn.ViewValue.Value = !IsTurnedOn.ViewValue.Value
        ).DisposeItWith(Disposable);

        Speed = new HistoricalUnitProperty<VelocityUnit>("speed", _speed, speedUnit, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Time = new HistoricalUnitProperty<TimeSpanUnit>("time", _time, timeUnit, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        ReadOnlyTime = new HistoricalUnitProperty<TimeSpanUnit>(
            "read-only-time",
            _time,
            timeUnit,
            loggerFactory,
            "00"
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        StringPropWithoutValidation = new HistoricalStringProperty(
            "string-without-validation",
            _stringWithoutValidation,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        StringPropWithOneValidation = new HistoricalStringProperty(
            "string-with-one-validation",
            _stringWithOneValidation,
            loggerFactory,
            [
                v =>
                {
                    if (string.IsNullOrWhiteSpace(v))
                    {
                        return ValidationResult.FailAsNullOrWhiteSpace;
                    }

                    return ValidationResult.Success;
                },
            ]
        ).SetRoutableParent(this).DisposeItWith(Disposable);
        StringPropWithOneValidation.ForceValidate();

        StringPropWithManyValidations = new HistoricalStringProperty(
            "string-with-many-validations",
            _stringWithManyValidations,
            loggerFactory,
            [
                v =>
                {
                    if (string.IsNullOrWhiteSpace(v))
                    {
                        return ValidationResult.FailAsNullOrWhiteSpace;
                    }

                    return ValidationResult.Success;
                },
                v =>
                {
                    if (v?.Contains('s', StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        return ValidationResult.FailFromErrorMessage(
                            "Property should not contain 's'",
                            RS.HistoricalControlsPageViewModel_StringPropWithManyValidations_ValidationError_ShouldNotContainS
                        );
                    }

                    return ValidationResult.Success;
                },
            ]
        ).SetRoutableParent(this).DisposeItWith(Disposable);
        StringPropWithManyValidations.ForceValidate();

        GeoPointProperty = new HistoricalGeoPointProperty(
            "geo-point",
            _geoPointProperty,
            unit,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        GeoPointProperty.ForceValidate();

        TagTypeProp = new HistoricalEnumProperty<AsvColorKind>("tag-type", _tagTypeProp)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AsvColorKindProp = new HistoricalEnumProperty<AsvColorKind>(
            "asv-color-kind",
            _rttBoxStatusProp
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        TrackLayout(_speed);
        TrackLayout(_time);
        TrackLayout(_isTurnedOn);
        TrackLayout(_stringWithoutValidation);
        TrackLayout(_stringWithOneValidation);
        TrackLayout(_stringWithManyValidations);
        TrackLayout(_geoPointProperty);
        TrackLayout(_tagTypeProp);
        TrackLayout(_rttBoxStatusProp);
        Layout
            .Register(
                nameof(HistoricalControlsPageViewModel),
                LoadLayout,
                SaveLayout,
                _layoutChanged
            )
            .DisposeItWith(Disposable);
    }

    public ReactiveCommand TurnOn { get; }
    public HistoricalUnitProperty<VelocityUnit> Speed { get; }
    public HistoricalUnitProperty<TimeSpanUnit> Time { get; }
    public HistoricalUnitProperty<TimeSpanUnit> ReadOnlyTime { get; }
    public HistoricalBoolProperty IsTurnedOn { get; }
    public HistoricalStringProperty StringPropWithoutValidation { get; }
    public HistoricalStringProperty StringPropWithOneValidation { get; }
    public HistoricalStringProperty StringPropWithManyValidations { get; }
    public HistoricalGeoPointProperty GeoPointProperty { get; }
    public HistoricalEnumProperty<AsvColorKind> TagTypeProp { get; }
    public HistoricalEnumProperty<AsvColorKind> AsvColorKindProp { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return IsTurnedOn;
        yield return Speed;
        yield return Time;
        yield return StringPropWithoutValidation;
        yield return StringPropWithOneValidation;
        yield return StringPropWithManyValidations;
        yield return GeoPointProperty;
        yield return TagTypeProp;
        yield return AsvColorKindProp;

        foreach (var child in base.GetChildren())
        {
            yield return child;
        }
    }

    private void TrackLayout<T>(Observable<T> observable)
    {
        observable
            .Skip(1)
            .Subscribe(_ => _layoutChanged.OnNext(Unit.Default))
            .DisposeItWith(Disposable);
    }

    private HistoricalControlsPageViewModelConfig SaveLayout()
    {
        var speed = Speed.ModelValue.Value;
        var time = Time.ModelValue.Value;
        return new HistoricalControlsPageViewModelConfig
        {
            IsTurnedOn = IsTurnedOn.ViewValue.Value,
            Speed = double.IsFinite(speed) ? speed : -1,
            Time = double.IsFinite(time) ? time : -1,
            StringPropWithoutValidation =
                StringPropWithoutValidation.ViewValue.Value ?? string.Empty,
            StringPropWithOneValidation =
                StringPropWithOneValidation.ViewValue.Value ?? string.Empty,
            StringPropWithManyValidations =
                StringPropWithManyValidations.ViewValue.Value ?? string.Empty,
            GeoPointProperty = GeoPointProperty.ModelValue.Value,
            TagTypeProp = TagTypeProp.ViewValue.Value,
            AsvColorKindProp = AsvColorKindProp.ViewValue.Value,
        };
    }

    private void LoadLayout(HistoricalControlsPageViewModelConfig config)
    {
        IsTurnedOn.ModelValue.Value = config.IsTurnedOn;
        if (config.Speed >= 0)
        {
            Speed.ModelValue.Value = config.Speed;
        }

        if (config.Time >= 0)
        {
            Time.ModelValue.Value = config.Time;
        }

        StringPropWithoutValidation.ModelValue.Value = config.StringPropWithoutValidation;
        StringPropWithOneValidation.ModelValue.Value = config.StringPropWithOneValidation;
        StringPropWithManyValidations.ModelValue.Value = config.StringPropWithManyValidations;
        GeoPointProperty.ModelValue.Value = config.GeoPointProperty;
        TagTypeProp.ModelValue.Value = config.TagTypeProp;
        AsvColorKindProp.ModelValue.Value = config.AsvColorKindProp;
    }
}
