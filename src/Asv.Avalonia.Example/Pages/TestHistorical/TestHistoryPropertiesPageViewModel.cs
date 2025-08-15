using System;
using System.Collections.Generic;
using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public sealed class TestHistoryPropertiesPageViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class TestHistoryPropertiesPageViewModel
    : PageViewModel<TestHistoryPropertiesPageViewModel, TestInfoBoxPageViewModelConfig>
{
    public const string PageId = "test.history.properties";
    public const MaterialIconKind PageIcon = MaterialIconKind.TestTube;

    private readonly ReactiveProperty<GeoPoint> _geoPointProperty;
    private readonly ReactiveProperty<double> _speed;
    private readonly ReactiveProperty<bool> _isTurnedOn;
    private readonly ReactiveProperty<string?> _stringWithoutValidation;
    private readonly ReactiveProperty<string?> _stringWithOneValidation;
    private readonly ReactiveProperty<string?> _stringWithManyValidations;

    public TestHistoryPropertiesPageViewModel()
        : this(
            DesignTime.UnitService,
            DesignTime.CommandService,
            DesignTime.Configuration,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public TestHistoryPropertiesPageViewModel(
        IUnitService unit,
        ICommandService commandService,
        IConfiguration cfg,
        ILoggerFactory loggerFactory
    )
        : base(PageId, commandService, cfg, loggerFactory)
    {
        Title = "Test History Properties";
        var un = unit.Units[VelocityBase.Id];
        var latUnit = unit.Units[LatitudeBase.Id];
        var lonUnit = unit.Units[LongitudeBase.Id];
        var altUnit = unit.Units[AltitudeBase.Id];
        _speed = new ReactiveProperty<double>(double.NaN);
        _isTurnedOn = new ReactiveProperty<bool>();
        _stringWithoutValidation = new ReactiveProperty<string?>();
        _stringWithOneValidation = new ReactiveProperty<string?>();
        _stringWithManyValidations = new ReactiveProperty<string?>();
        _geoPointProperty = new ReactiveProperty<GeoPoint>();

        IsTurnedOn = new HistoricalBoolProperty(nameof(Speed), _isTurnedOn, loggerFactory, this);

        TurnOn = new ReactiveCommand(_ => IsTurnedOn.ViewValue.Value = !IsTurnedOn.ViewValue.Value);

        Speed = new HistoricalUnitProperty(nameof(Speed), _speed, un, loggerFactory, this);
        StringPropWithoutValidation = new HistoricalStringProperty(
            nameof(StringPropWithoutValidation),
            _stringWithoutValidation,
            loggerFactory,
            this
        );

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
        );
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
        );
        StringPropWithManyValidations.ForceValidate();

        GeoPointProperty = new HistoricalGeoPointProperty(
            nameof(GeoPointProperty),
            _geoPointProperty,
            latUnit,
            lonUnit,
            altUnit,
            loggerFactory,
            this
        );
        GeoPointProperty.ForceValidate();
    }

    public ReactiveCommand TurnOn { get; }
    public HistoricalUnitProperty Speed { get; }
    public HistoricalBoolProperty IsTurnedOn { get; }
    public HistoricalStringProperty StringPropWithoutValidation { get; }
    public HistoricalStringProperty StringPropWithOneValidation { get; }
    public HistoricalStringProperty StringPropWithManyValidations { get; }
    public HistoricalGeoPointProperty GeoPointProperty { get; }

    protected override TestHistoryPropertiesPageViewModel GetContext()
    {
        return this;
    }

    protected override void AfterLoadExtensions() { }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return IsTurnedOn;
        yield return Speed;
        yield return StringPropWithoutValidation;
        yield return StringPropWithOneValidation;
        yield return StringPropWithManyValidations;
        yield return GeoPointProperty;
    }

    public override IExportInfo Source => SystemModule.Instance;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _speed.Dispose();
            _isTurnedOn.Dispose();
            _geoPointProperty.Dispose();
            _stringWithoutValidation.Dispose();
            _stringWithOneValidation.Dispose();
            _stringWithManyValidations.Dispose();

            TurnOn.Dispose();
            Speed.Dispose();
            StringPropWithoutValidation.Dispose();
            StringPropWithOneValidation.Dispose();
            StringPropWithManyValidations.Dispose();
            GeoPointProperty.Dispose();
        }

        base.Dispose(disposing);
    }
}
