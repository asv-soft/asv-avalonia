using System;
using System.Collections.Generic;
using System.Composition;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class HistoricalControlsPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "historical_controls";
    public const MaterialIconKind PageIcon = MaterialIconKind.History;

    private readonly ReactiveProperty<bool> _isTurnedOn;
    private readonly ReactiveProperty<double> _speed;
    private readonly ReactiveProperty<string?> _stringWithManyValidations;
    private readonly ReactiveProperty<string?> _stringWithOneValidation;
    private readonly ReactiveProperty<string?> _stringWithoutValidation;
    private readonly ReactiveProperty<GeoPoint> _geoPointProperty;
    private readonly ReactiveProperty<Enum> _tagTypeProp;
    private readonly ReactiveProperty<Enum> _rttBoxStatusProp;

    public HistoricalControlsPageViewModel()
        : this(DesignTime.UnitService, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public HistoricalControlsPageViewModel(IUnitService unit, ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
    {
        var un = unit.Units[VelocityBase.Id];
        var latUnit = unit.Units[LatitudeBase.Id];
        var lonUnit = unit.Units[LongitudeBase.Id];
        var altUnit = unit.Units[AltitudeBase.Id];

        _speed = new ReactiveProperty<double>(double.NaN).DisposeItWith(Disposable);
        _isTurnedOn = new ReactiveProperty<bool>().DisposeItWith(Disposable);
        _stringWithoutValidation = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _stringWithOneValidation = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _stringWithManyValidations = new ReactiveProperty<string?>().DisposeItWith(Disposable);
        _geoPointProperty = new ReactiveProperty<GeoPoint>().DisposeItWith(Disposable);
        _tagTypeProp = new ReactiveProperty<Enum>(TagType.Error).DisposeItWith(Disposable);
        _rttBoxStatusProp = new ReactiveProperty<Enum>(RttBoxStatus.Normal).DisposeItWith(
            Disposable
        );

        IsTurnedOn = new HistoricalBoolProperty(
            nameof(IsTurnedOn),
            _isTurnedOn,
            loggerFactory
        ).DisposeItWith(Disposable);

        TurnOn = new ReactiveCommand(_ =>
            IsTurnedOn.ViewValue.Value = !IsTurnedOn.ViewValue.Value
        ).DisposeItWith(Disposable);

        Speed = new HistoricalUnitProperty(
            nameof(Speed),
            _speed,
            un,
            loggerFactory){
                Parent = this,
            }.DisposeItWith(Disposable);

        StringPropWithoutValidation = new HistoricalStringProperty(
            nameof(StringPropWithoutValidation),
            _stringWithoutValidation,
            loggerFactory
        )
        {
            Parent = this,
        }.DisposeItWith(Disposable);

        StringPropWithOneValidation = new HistoricalStringProperty(
            nameof(StringPropWithOneValidation),
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
        ){
            Parent = this,
        }.DisposeItWith(Disposable);
        StringPropWithOneValidation.ForceValidate();

        StringPropWithManyValidations = new HistoricalStringProperty(
            nameof(StringPropWithManyValidations),
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
                        return new ValidationResult
                        {
                            IsSuccess = false,
                            ValidationException = new ValidationException(
                                "Value shouldn't contain \'s\'"
                            ),
                        };
                    }

                    return ValidationResult.Success;
                },
            ]
        ){
            Parent = this,
        }.DisposeItWith(Disposable);
        StringPropWithManyValidations.ForceValidate();

        GeoPointProperty = new HistoricalGeoPointProperty(
            nameof(GeoPointProperty),
            _geoPointProperty,
            latUnit,
            lonUnit,
            altUnit,
            loggerFactory
        ).DisposeItWith(Disposable);
        GeoPointProperty.ForceValidate();

        TagTypeProp = new HistoricalEnumProperty<TagType>(
            nameof(TagTypeProp),
            _tagTypeProp,
            loggerFactory
        ).DisposeItWith(Disposable);

        RttBoxStatusProp = new HistoricalEnumProperty<RttBoxStatus>(
            nameof(RttBoxStatusProp),
            _rttBoxStatusProp,
            loggerFactory
        ).DisposeItWith(Disposable);
    }

    public ReactiveCommand TurnOn { get; }
    public HistoricalUnitProperty Speed { get; }
    public HistoricalBoolProperty IsTurnedOn { get; }
    public HistoricalStringProperty StringPropWithoutValidation { get; }
    public HistoricalStringProperty StringPropWithOneValidation { get; }
    public HistoricalStringProperty StringPropWithManyValidations { get; }
    public HistoricalGeoPointProperty GeoPointProperty { get; }
    public HistoricalEnumProperty<TagType> TagTypeProp { get; }
    public HistoricalEnumProperty<RttBoxStatus> RttBoxStatusProp { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return IsTurnedOn;
        yield return Speed;
        yield return StringPropWithoutValidation;
        yield return StringPropWithOneValidation;
        yield return StringPropWithManyValidations;
        yield return GeoPointProperty;
        yield return TagTypeProp;
        yield return RttBoxStatusProp;

        foreach (var child in base.GetRoutableChildren())
        {
            yield return child;
        }
    }

    public override IExportInfo Source => SystemModule.Instance;
}
