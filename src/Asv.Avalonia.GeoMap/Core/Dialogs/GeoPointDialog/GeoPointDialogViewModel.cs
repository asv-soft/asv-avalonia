using Asv.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.GeoMap;

public class GeoPointDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.geopoint";

    private readonly IUnit _distanceUnit;
    private bool _isSyncingGeoPointProperty;
    private bool _isSyncingCenterGeoPoint;

    public GeoPointDialogViewModel()
        : this(NullLoggerFactory.Instance, NullUnitService.Instance, NullMapService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public GeoPointDialogViewModel(
        ILoggerFactory loggerFactory,
        IUnitService unitService,
        IMapService mapService
    )
        : base(DialogId, loggerFactory)
    {
        var latUnit = unitService.Units[LatitudeUnit.Id];
        var lonUnit = unitService.Units[LongitudeUnit.Id];
        var altUnit = unitService.Units[AltitudeUnit.Id];
        _distanceUnit = unitService.Units[DistanceUnit.Id];

        CurrentProvider = mapService
            .CurrentProvider.ToReadOnlyBindableReactiveProperty<ITileProvider>()
            .DisposeItWith(Disposable);

        var geoPointProperty = new ReactiveProperty<GeoPoint>(GeoPoint.Zero).DisposeItWith(
            Disposable
        );
        GeoPointProperty = new BindableGeoPointProperty(
            nameof(GeoPointProperty),
            geoPointProperty,
            latUnit,
            lonUnit,
            altUnit,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        GeoPointProperty.ForceValidate();
        CenterGeoPoint = new BindableReactiveProperty<GeoPoint>(
            GeoPointProperty.ModelValue.CurrentValue
        ).DisposeItWith(Disposable);

        CenterGeoPoint
            .Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Where(_ => !_isSyncingCenterGeoPoint)
            .Subscribe(value =>
            {
                _isSyncingGeoPointProperty = true;
                if (GeoPointProperty.ModelValue.CurrentValue != value)
                {
                    GeoPointProperty.ModelValue.Value = value;
                }
                _isSyncingGeoPointProperty = false;
            })
            .DisposeItWith(Disposable);

        GeoPointProperty
            .ModelValue.Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Where(_ => !_isSyncingGeoPointProperty)
            .Subscribe(value =>
            {
                _isSyncingCenterGeoPoint = true;
                if (CenterGeoPoint.Value != value)
                {
                    CenterGeoPoint.Value = value;
                }
                _isSyncingCenterGeoPoint = false;
            })
            .DisposeItWith(Disposable);

        StepOptions = new List<double> { 1, 10, 50, 100, 5000, 10000, 50000 };

        var defaultDistanceValue = _distanceUnit.CurrentUnitItem.CurrentValue.Print(StepOptions[0]);
        DistanceProperty = new BindableReactiveProperty<string>(defaultDistanceValue).DisposeItWith(
            Disposable
        );
        DistanceProperty.EnableValidation(ValidateDistancePropertyValue);
        DistanceProperty
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => MoveCommand?.ChangeCanExecute(!DistanceProperty.HasErrors))
            .DisposeItWith(Disposable);

        LonUnitName = lonUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToReadOnlyBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        LatUnitName = latUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToReadOnlyBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AltUnitName = altUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToReadOnlyBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        DistanceUnitName = _distanceUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToReadOnlyBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        GeoPointProperty
            .Longitude.ViewValue.ObserveOnUIThreadDispatcher()
            .Subscribe(_ => RefreshGeoPointValidation())
            .DisposeItWith(Disposable);
        GeoPointProperty
            .Latitude.ViewValue.ObserveOnUIThreadDispatcher()
            .Subscribe(_ => RefreshGeoPointValidation())
            .DisposeItWith(Disposable);
        GeoPointProperty
            .Altitude.ViewValue.ObserveOnUIThreadDispatcher()
            .Subscribe(_ => RefreshGeoPointValidation())
            .DisposeItWith(Disposable);

        MoveCommand = new ReactiveCommand<MoveDirection>(Move).DisposeItWith(Disposable);
    }

    public IReadOnlyList<double> StepOptions { get; }

    public BindableGeoPointProperty GeoPointProperty { get; }
    public BindableReactiveProperty<string> DistanceProperty { get; }

    public IReadOnlyBindableReactiveProperty<string> LonUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> LatUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> AltUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> DistanceUnitName { get; }
    public IReadOnlyBindableReactiveProperty<ITileProvider> CurrentProvider { get; }
    public BindableReactiveProperty<GeoPoint> CenterGeoPoint { get; }

    public ReactiveCommand<MoveDirection> MoveCommand { get; }

    private void RefreshGeoPointValidation()
    {
        var isLonOk = !GeoPointProperty.Longitude.ViewValue.HasErrors;
        var isLatOk = !GeoPointProperty.Latitude.ViewValue.HasErrors;
        var isAltOk = !GeoPointProperty.Altitude.ViewValue.HasErrors;

        IsValid.Value = isLonOk && isLatOk && isAltOk;
    }

    private Exception? ValidateDistancePropertyValue(string? userValue)
    {
        var result = _distanceUnit.CurrentUnitItem.CurrentValue.ValidateValue(userValue);
        return result.IsSuccess
            ? null
            : result.ValidationException?.GetExceptionWithLocalizationOrSelf();
    }

    private void Move(MoveDirection moveDirection)
    {
        var distanceValueInSi = _distanceUnit.CurrentUnitItem.CurrentValue.ParseToSi(
            DistanceProperty.Value
        );

        GeoPointProperty.ModelValue.Value = GeoPointMoveHelper.Step(
            GeoPointProperty.ModelValue.CurrentValue,
            distanceValueInSi,
            moveDirection
        );
    }

    public GeoPoint GetResult()
    {
        return GeoPointProperty.ModelValue.CurrentValue;
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        IsValid
            .ObserveOnUIThreadDispatcher()
            .Subscribe(isValid => dialog.IsPrimaryButtonEnabled = isValid)
            .DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return GeoPointProperty;
    }
}
