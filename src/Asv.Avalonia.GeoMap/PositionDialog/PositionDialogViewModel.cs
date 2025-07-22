using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.GeoMap;

public class PositionDialogViewModel : DialogViewModelBase
{
    public const string DialogId = "dialog.position";

    private readonly MapAnchor<IMapAnchor> _anchor;
    private readonly ReactiveProperty<double> _distanceProperty;
    private readonly ReactiveProperty<GeoPoint> _geoPointProperty;
    private bool _internalChange;

    public PositionDialogViewModel()
        : this(NullLoggerFactory.Instance, NullUnitService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public PositionDialogViewModel(ILoggerFactory loggerFactory, IUnitService unitService)
        : base(DialogId, loggerFactory)
    {
        var latUnit = unitService.Units[LatitudeBase.Id];
        var lonUnit = unitService.Units[LongitudeBase.Id];
        var altUnit = unitService.Units[AltitudeBase.Id];
        var distanceUnit = unitService.Units[DistanceBase.Id];

        _geoPointProperty = new ReactiveProperty<GeoPoint>(GeoPoint.Zero).DisposeItWith(Disposable);
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

        CurrentLocation = GeoPointProperty
            .ModelValue.ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);

        StepOptions = new List<double> { 1, 10, 50, 100, 5000, 10000, 50000 };

        _distanceProperty = new ReactiveProperty<double>().DisposeItWith(Disposable);
        DistanceProperty = new HistoricalUnitProperty(
            nameof(DistanceProperty),
            _distanceProperty,
            distanceUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        DistanceProperty.SetViewValue(StepOptions[0]);

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
        DistanceUnitName = distanceUnit
            .CurrentUnitItem.Select(item => item.Symbol)
            .ToBindableReactiveProperty<string>()
            .DisposeItWith(Disposable);

        _anchor = new MapAnchor<IMapAnchor>(nameof(_anchor), loggerFactory)
        {
            Icon = MaterialIconKind.Location,
            Title = RS.PositionDialogViewModel_Point,
        };
        _anchor.SetRoutableParent(this);

        MapViewModel = new MapViewModel(nameof(MapViewModel), loggerFactory)
            .DisposeItWith(Disposable)
            .SetRoutableParent(this);

        MapViewModel.Anchors.DisposeRemovedItems().DisposeItWith(Disposable);
        MapViewModel.Anchors.SetRoutableParent(this).DisposeItWith(Disposable);
        MapViewModel.Anchors.Add(_anchor);

        GeoPointProperty
            .Longitude.ViewValue.Subscribe(_ => RefreshGeoPointValidationAndData())
            .DisposeItWith(Disposable);
        GeoPointProperty
            .Latitude.ViewValue.Subscribe(_ => RefreshGeoPointValidationAndData())
            .DisposeItWith(Disposable);
        GeoPointProperty
            .Altitude.ViewValue.Subscribe(_ => RefreshGeoPointValidationAndData())
            .DisposeItWith(Disposable);

        GeoPointProperty
            .ModelValue.Subscribe(location =>
            {
                if (_anchor.Location.Value.Equals(location) || _internalChange)
                {
                    return;
                }

                _internalChange = true;
                _anchor.Location.Value = location;
                _internalChange = false;
            })
            .DisposeItWith(Disposable);
        _anchor
            .Location.Subscribe(location =>
            {
                if (_internalChange)
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
                    _internalChange = true;
                    GeoPointProperty.ModelValue.Value = location.SetAltitude(
                        GeoPointProperty.Altitude.ModelValue.Value
                    );
                    _internalChange = false;
                }
            })
            .DisposeItWith(Disposable);

        MoveCommand = new ReactiveCommand<MoveDirection>(Move).DisposeItWith(Disposable);
        ChangeAltCommand = new ReactiveCommand<MoveDirection>(ChangeAlt).DisposeItWith(Disposable);
    }

    public IReadOnlyList<double> StepOptions { get; }

    public MapViewModel MapViewModel { get; }
    public HistoricalGeoPointProperty GeoPointProperty { get; }
    public HistoricalUnitProperty DistanceProperty { get; }

    public IReadOnlyBindableReactiveProperty<GeoPoint> CurrentLocation { get; }
    public IReadOnlyBindableReactiveProperty<string> LonUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> LatUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> AltUnitName { get; }
    public IReadOnlyBindableReactiveProperty<string> DistanceUnitName { get; }

    public ReactiveCommand<MoveDirection> MoveCommand { get; }
    public ReactiveCommand<MoveDirection> ChangeAltCommand { get; }

    private void RefreshGeoPointValidationAndData()
    {
        var isLonOk = !GeoPointProperty.Longitude.ViewValue.HasErrors;
        var isLatOk = !GeoPointProperty.Latitude.ViewValue.HasErrors;
        var isAltOk = !GeoPointProperty.Altitude.ViewValue.HasErrors;

        IsValid.Value = isLonOk && isLatOk && isAltOk;

        if (!IsValid.CurrentValue || _internalChange)
        {
            return;
        }

        var newLon = GetUnitPropertyValueInSi(GeoPointProperty.Longitude);
        var newLat = GetUnitPropertyValueInSi(GeoPointProperty.Latitude);
        var newAlt = GetUnitPropertyValueInSi(GeoPointProperty.Altitude);

        if (
            GeoPointProperty.ModelValue.Value.Longitude.ApproximatelyEquals(newLon)
            && GeoPointProperty.ModelValue.Value.Latitude.ApproximatelyEquals(newLat)
            && GeoPointProperty.ModelValue.Value.Altitude.ApproximatelyEquals(newAlt)
        )
        {
            return;
        }

        GeoPointProperty.ModelValue.Value = new GeoPoint(newLat, newLon, newAlt);
    }

    private double GetUnitPropertyValueInSi(HistoricalUnitProperty property)
    {
        var valueRaw = property.ViewValue.CurrentValue;
        if (valueRaw is null)
        {
            return 0;
        }
        var value = property.Unit.CurrentUnitItem.CurrentValue.ParseToSi(valueRaw);
        return double.IsNaN(value) ? 0 : value;
    }

    private double GetSelectedDistanceInSi()
    {
        return GetUnitPropertyValueInSi(DistanceProperty);
    }

    private void ChangeAlt(MoveDirection moveDirection)
    {
        var deltaAltInSi = moveDirection switch
        {
            MoveDirection.Up => GetSelectedDistanceInSi(),
            MoveDirection.Down => -GetSelectedDistanceInSi(),
            _ => 0,
        };

        GeoPointProperty.ModelValue.Value = GeoPointProperty.ModelValue.Value.AddAltitude(
            deltaAltInSi
        );
    }

    private void Move(MoveDirection moveDirection)
    {
        GeoPointProperty.ModelValue.Value = GeoPointMoveHelper.Step(
            GeoPointProperty.ModelValue.CurrentValue,
            GetSelectedDistanceInSi(),
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
            .Subscribe(isValid =>
            {
                dialog.IsPrimaryButtonEnabled = isValid;
            })
            .DisposeItWith(Disposable);
    }

    public void SetInitialCoordinates(GeoPoint point)
    {
        GeoPointProperty.ModelValue.Value = point;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return DistanceProperty;
        yield return MapViewModel;
        yield return GeoPointProperty;
    }
}
