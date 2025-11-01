using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public sealed class HistoricalControlsPageViewModelConfig
{
    public string Speed { get; set; } = string.Empty;
    public bool IsTurnedOn { get; set; } = false;
    public string StringPropWithoutValidation { get; set; } = string.Empty;
    public string StringPropWithOneValidation { get; set; } = string.Empty;
    public string StringPropWithManyValidations { get; set; } = string.Empty;
    public GeoPoint GeoPointProperty { get; set; } = GeoPoint.ZeroWithAlt;
    public TagType TagTypeProp { get; set; } = TagType.Unknown;
    public RttBoxStatus RttBoxStatusProp { get; set; } = RttBoxStatus.Normal;
}

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

    private HistoricalControlsPageViewModelConfig? _config;

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
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        TurnOn = new ReactiveCommand(_ =>
            IsTurnedOn.ViewValue.Value = !IsTurnedOn.ViewValue.Value
        ).DisposeItWith(Disposable);

        Speed = new HistoricalUnitProperty(
            nameof(Speed),
            _speed,
            un,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        StringPropWithoutValidation = new HistoricalStringProperty(
            nameof(StringPropWithoutValidation),
            _stringWithoutValidation,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        StringPropWithOneValidation = new HistoricalStringProperty(
            nameof(StringPropWithOneValidation),
            _stringWithOneValidation,
            loggerFactory,
            this,
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
        ).DisposeItWith(Disposable);
        StringPropWithOneValidation.ForceValidate();

        StringPropWithManyValidations = new HistoricalStringProperty(
            nameof(StringPropWithManyValidations),
            _stringWithManyValidations,
            loggerFactory,
            this,
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
        ).DisposeItWith(Disposable);
        StringPropWithManyValidations.ForceValidate();

        GeoPointProperty = new HistoricalGeoPointProperty(
            nameof(GeoPointProperty),
            _geoPointProperty,
            latUnit,
            lonUnit,
            altUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        GeoPointProperty.ForceValidate();

        TagTypeProp = new HistoricalEnumProperty<TagType>(
            nameof(TagTypeProp),
            _tagTypeProp,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        RttBoxStatusProp = new HistoricalEnumProperty<RttBoxStatus>(
            nameof(RttBoxStatusProp),
            _rttBoxStatusProp,
            loggerFactory,
            this
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

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case SaveLayoutEvent saveLayoutEvent:
                if (_config is null)
                {
                    break;
                }

                this.HandleSaveLayout(
                    saveLayoutEvent,
                    _config,
                    cfg =>
                    {
                        cfg.IsTurnedOn = IsTurnedOn.ViewValue.Value;
                        cfg.Speed = Speed.ViewValue.Value ?? string.Empty;
                        cfg.StringPropWithoutValidation =
                            StringPropWithoutValidation.ViewValue.Value ?? string.Empty;
                        cfg.StringPropWithOneValidation =
                            StringPropWithOneValidation.ViewValue.Value ?? string.Empty;
                        cfg.StringPropWithManyValidations =
                            StringPropWithManyValidations.ViewValue.Value ?? string.Empty;
                        cfg.GeoPointProperty = GeoPointProperty.ModelValue.Value;
                        cfg.TagTypeProp = TagTypeProp.ViewValue.Value;
                        cfg.RttBoxStatusProp = RttBoxStatusProp.ViewValue.Value;
                    }
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<HistoricalControlsPageViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                    {
                        IsTurnedOn.ViewValue.Value = cfg.IsTurnedOn;
                        Speed.ViewValue.Value = cfg.Speed;
                        StringPropWithoutValidation.ViewValue.Value =
                            cfg.StringPropWithoutValidation;
                        StringPropWithOneValidation.ViewValue.Value =
                            cfg.StringPropWithOneValidation;
                        StringPropWithManyValidations.ViewValue.Value =
                            cfg.StringPropWithManyValidations;
                        GeoPointProperty.ModelValue.Value = cfg.GeoPointProperty;
                        TagTypeProp.ModelValue.Value = cfg.TagTypeProp;
                        RttBoxStatusProp.ModelValue.Value = cfg.RttBoxStatusProp;
                    }
                );
                break;
        }

        return base.InternalCatchEvent(e);
    }

    public override IExportInfo Source => SystemModule.Instance;
}
