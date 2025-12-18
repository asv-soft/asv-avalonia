using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.GeoMap;

public class GeoPointDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.geopoint";

    private readonly IUnit _distanceUnit;
    private readonly MapAnchor<IMapAnchor> _anchor;
    private readonly ReactiveProperty<GeoPoint> _geoPointProperty;
    private bool _internalAnchorChange;

    public GeoPointDialogViewModel()
        : this(NullLoggerFactory.Instance, NullUnitService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public GeoPointDialogViewModel(ILoggerFactory loggerFactory, IUnitService unitService)
        : base(DialogId, loggerFactory)
    {
        var latUnit = unitService.Units[LatitudeBase.Id];
        var lonUnit = unitService.Units[LongitudeBase.Id];
        var altUnit = unitService.Units[AltitudeBase.Id];

        _distanceUnit = unitService.Units[DistanceBase.Id];

        _geoPointProperty = new ReactiveProperty<GeoPoint>(GeoPoint.Zero).DisposeItWith(Disposable);
        GeoPointProperty = new BindableGeoPointProperty(
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

        StepOptions = new List<double> { 1, 10, 50, 100, 5000, 10000, 50000 };

        var defaultDistanceValue = _distanceUnit.CurrentUnitItem.CurrentValue.Print(StepOptions[0]);
        DistanceProperty = new BindableReactiveProperty<string>(defaultDistanceValue).DisposeItWith(
            Disposable
        );
        DistanceProperty.EnableValidation(ValidateDistancePropertyValue);
        DistanceProperty
            .Subscribe(_ => MoveCommand?.ChangeCanExecute(!DistanceProperty.HasErrors))
            .DisposeItWith(Disposable);

        LonUnitName = lonUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        LatUnitName = latUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        AltUnitName = altUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);
        DistanceUnitName = _distanceUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        _anchor = new MapAnchor<IMapAnchor>(nameof(_anchor), loggerFactory)
        {
            Icon = MaterialIconKind.Location,
            Title = RS.GeoPointDialogViewModel_Anchor_Title,
        };
        _anchor.SetRoutableParent(this);

        MapViewModel = new MapViewModel(nameof(MapViewModel), loggerFactory)
            .DisposeItWith(Disposable)
            .SetRoutableParent(this);

        MapViewModel.Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        MapViewModel.Anchors.SetRoutableParent(this).DisposeItWith(Disposable);
        MapViewModel.Anchors.Add(_anchor);

        GeoPointProperty
            .Longitude.ViewValue.Subscribe(_ => RefreshGeoPointValidation())
            .DisposeItWith(Disposable);
        GeoPointProperty
            .Latitude.ViewValue.Subscribe(_ => RefreshGeoPointValidation())
            .DisposeItWith(Disposable);
        GeoPointProperty
            .Altitude.ViewValue.Subscribe(_ => RefreshGeoPointValidation())
            .DisposeItWith(Disposable);

        GeoPointProperty
            .ModelValue.Subscribe(location =>
            {
                if (_anchor.ReactiveLocation.Value.Equals(location) || IsGeoPointInvalid(location))
                {
                    return;
                }

                _internalAnchorChange = true;
                _anchor.ReactiveLocation.Value = location;
                _internalAnchorChange = false;
            })
            .DisposeItWith(Disposable);
        _anchor
            .ReactiveLocation.Subscribe(location =>
            {
                if (_internalAnchorChange)
                {
                    return;
                }

                var isLonChanged =
                    GeoPointProperty.ModelValue.Value.Longitude.ApproximatelyNotEquals(
                        location.Longitude
                    );
                var isLatChanged =
                    GeoPointProperty.ModelValue.Value.Latitude.ApproximatelyNotEquals(
                        location.Latitude
                    );

                if (isLonChanged || isLatChanged)
                {
                    var newLocationWithOldAltitude = location.SetAltitude(
                        GeoPointProperty.ModelValue.Value.Altitude
                    );
                    SetGeoPointLocation(newLocationWithOldAltitude);
                }
            })
            .DisposeItWith(Disposable);

        MoveCommand = new ReactiveCommand<MoveDirection>(Move).DisposeItWith(Disposable);
    }

    public IReadOnlyList<double> StepOptions { get; }

    public MapViewModel MapViewModel { get; }
    public BindableGeoPointProperty GeoPointProperty { get; }
    public BindableReactiveProperty<string> DistanceProperty { get; }

    public IReadOnlyBindableReactiveProperty<string> LonUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> LatUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> AltUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> DistanceUnitName { get; }

    public ReactiveCommand<MoveDirection> MoveCommand { get; }

    private void RefreshGeoPointValidation()
    {
        var isLonOk = !GeoPointProperty.Longitude.ViewValue.HasErrors;
        var isLatOk = !GeoPointProperty.Latitude.ViewValue.HasErrors;
        var isAltOk = !GeoPointProperty.Altitude.ViewValue.HasErrors;

        IsValid.Value = isLonOk && isLatOk && isAltOk;
    }

    private bool IsGeoPointInvalid(GeoPoint geoPoint)
    {
        var lonIsBad = double.IsNaN(geoPoint.Longitude) || double.IsInfinity(geoPoint.Longitude);
        var latIsBad = double.IsNaN(geoPoint.Latitude) || double.IsInfinity(geoPoint.Latitude);
        var altIsBad = double.IsNaN(geoPoint.Altitude) || double.IsInfinity(geoPoint.Altitude);

        return lonIsBad || latIsBad || altIsBad;
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

        var newLocation = GeoPointMoveHelper.Step(
            GeoPointProperty.ModelValue.CurrentValue,
            distanceValueInSi,
            moveDirection
        );
        SetGeoPointLocation(newLocation);
    }

    public GeoPoint GetResult()
    {
        return GeoPointProperty.ModelValue.CurrentValue;
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        IsValid
            .Subscribe(isValid =>
            {
                dialog.IsPrimaryButtonEnabled = isValid;
            })
            .DisposeItWith(Disposable);
    }

    public void SetInitialLocation(GeoPoint location)
    {
        GeoPointProperty.ModelValue.Value = new GeoPoint(
            double.IsNaN(location.Latitude) ? 0 : location.Latitude,
            double.IsNaN(location.Longitude) ? 0 : location.Longitude,
            double.IsNaN(location.Altitude) ? 0 : location.Altitude
        );
    }

    private void SetGeoPointLocation(GeoPoint location)
    {
        GeoPointProperty.Longitude.ViewValue.Value =
            GeoPointProperty.Longitude.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                location.Longitude
            );
        GeoPointProperty.Latitude.ViewValue.Value =
            GeoPointProperty.Latitude.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                location.Latitude
            );
        GeoPointProperty.Altitude.ViewValue.Value =
            GeoPointProperty.Altitude.Unit.CurrentUnitItem.CurrentValue.PrintFromSi(
                location.Altitude
            );
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return MapViewModel;
        yield return GeoPointProperty;
    }
}
