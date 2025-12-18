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
    public double Speed { get; set; } = -1;
    public bool IsTurnedOn { get; set; } = false;
    public string StringPropWithoutValidation { get; set; } = string.Empty;
    public string StringPropWithOneValidation { get; set; } = string.Empty;
    public string StringPropWithManyValidations { get; set; } = string.Empty;
    public GeoPoint GeoPointProperty { get; set; } = GeoPoint.ZeroWithAlt;
    public AsvColorKind TagTypeProp { get; set; } = AsvColorKind.Unknown;
    public AsvColorKind AsvColorKindProp { get; set; } = AsvColorKind.Success;
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
        _tagTypeProp = new ReactiveProperty<Enum>(AsvColorKind.Error).DisposeItWith(Disposable);
        _rttBoxStatusProp = new ReactiveProperty<Enum>(AsvColorKind.Success).DisposeItWith(
            Disposable
        );

        IsTurnedOn = new HistoricalBoolProperty(nameof(IsTurnedOn), _isTurnedOn, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        TurnOn = new ReactiveCommand(_ =>
            IsTurnedOn.ViewValue.Value = !IsTurnedOn.ViewValue.Value
        ).DisposeItWith(Disposable);

        Speed = new HistoricalUnitProperty(nameof(Speed), _speed, un, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        StringPropWithoutValidation = new HistoricalStringProperty(
            nameof(StringPropWithoutValidation),
            _stringWithoutValidation,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

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
        ).SetRoutableParent(this).DisposeItWith(Disposable);
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
            nameof(GeoPointProperty),
            _geoPointProperty,
            latUnit,
            lonUnit,
            altUnit,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        GeoPointProperty.ForceValidate();

        TagTypeProp = new HistoricalEnumProperty<AsvColorKind>(
            nameof(TagTypeProp),
            _tagTypeProp,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AsvColorKindProp = new HistoricalEnumProperty<AsvColorKind>(
            nameof(AsvColorKindProp),
            _rttBoxStatusProp,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public ReactiveCommand TurnOn { get; }
    public HistoricalUnitProperty Speed { get; }
    public HistoricalBoolProperty IsTurnedOn { get; }
    public HistoricalStringProperty StringPropWithoutValidation { get; }
    public HistoricalStringProperty StringPropWithOneValidation { get; }
    public HistoricalStringProperty StringPropWithManyValidations { get; }
    public HistoricalGeoPointProperty GeoPointProperty { get; }
    public HistoricalEnumProperty<AsvColorKind> TagTypeProp { get; }
    public HistoricalEnumProperty<AsvColorKind> AsvColorKindProp { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return IsTurnedOn;
        yield return Speed;
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

    private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
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
                        cfg.Speed = Speed.ModelValue.Value;
                        cfg.StringPropWithoutValidation =
                            StringPropWithoutValidation.ViewValue.Value ?? string.Empty;
                        cfg.StringPropWithOneValidation =
                            StringPropWithOneValidation.ViewValue.Value ?? string.Empty;
                        cfg.StringPropWithManyValidations =
                            StringPropWithManyValidations.ViewValue.Value ?? string.Empty;
                        cfg.GeoPointProperty = GeoPointProperty.ModelValue.Value;
                        cfg.TagTypeProp = TagTypeProp.ViewValue.Value;
                        cfg.AsvColorKindProp = AsvColorKindProp.ViewValue.Value;
                    }
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<HistoricalControlsPageViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                    {
                        IsTurnedOn.ModelValue.Value = cfg.IsTurnedOn;
                        if (cfg.Speed >= 0)
                        {
                            Speed.ModelValue.Value = cfg.Speed;
                        }

                        StringPropWithoutValidation.ModelValue.Value =
                            cfg.StringPropWithoutValidation;
                        StringPropWithOneValidation.ModelValue.Value =
                            cfg.StringPropWithOneValidation;
                        StringPropWithManyValidations.ModelValue.Value =
                            cfg.StringPropWithManyValidations;
                        GeoPointProperty.ModelValue.Value = cfg.GeoPointProperty;
                        TagTypeProp.ModelValue.Value = cfg.TagTypeProp;
                        AsvColorKindProp.ModelValue.Value = cfg.AsvColorKindProp;
                    }
                );
                break;
        }

        return ValueTask.CompletedTask;
    }

    public override IExportInfo Source => SystemModule.Instance;
}
