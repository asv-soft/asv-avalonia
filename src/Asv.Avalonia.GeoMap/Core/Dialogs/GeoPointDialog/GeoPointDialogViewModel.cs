using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.GeoMap;

public class GeoPointDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}-geopoint";

    private const double DefaultDistance = 1;

    private readonly ReactiveProperty<double> _altitude;
    private readonly ReactiveProperty<double> _distance;
    private readonly ReactiveProperty<double> _latitude;
    private readonly ReactiveProperty<double> _longitude;
    private GeoPoint _currentValue = GeoPoint.Zero;
    private bool _isSyncingCenterGeoPoint;
    private bool _isSyncingFields;
    private bool _isSyncingLayout;

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
        : base(DialogId)
    {
        ArgumentNullException.ThrowIfNull(unitService);
        ArgumentNullException.ThrowIfNull(mapService);

        CurrentProvider = mapService
            .CurrentProvider.ToReadOnlyBindableReactiveProperty<ITileProvider>()
            .DisposeItWith(Disposable);

        _latitude = new ReactiveProperty<double>(GeoPoint.Zero.Latitude).DisposeItWith(Disposable);
        _longitude = new ReactiveProperty<double>(GeoPoint.Zero.Longitude).DisposeItWith(
            Disposable
        );
        _altitude = new ReactiveProperty<double>(GeoPoint.Zero.Altitude).DisposeItWith(Disposable);

        var distanceUnit = unitService.GetRequiredUnitOfType<DistanceUnit>(DistanceUnit.Id);
        _distance = new ReactiveProperty<double>(DefaultDistance).DisposeItWith(Disposable);

        LatitudeProperty = new PropertyUnitReactive(
            nameof(LatitudeProperty),
            unitService.GetRequiredUnitOfType<LatitudeUnit>(LatitudeUnit.Id),
            _latitude,
            format: "F7"
        )
        {
            Header = Avalonia.RS.Latitude_Name,
            ShortHeader = Avalonia.RS.Latitude_ShortName,
            Icon = Material.Icons.MaterialIconKind.Latitude,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        LongitudeProperty = new PropertyUnitReactive(
            nameof(LongitudeProperty),
            unitService.GetRequiredUnitOfType<LongitudeUnit>(LongitudeUnit.Id),
            _longitude,
            format: "F7"
        )
        {
            Header = Avalonia.RS.Longitude_Name,
            ShortHeader = Avalonia.RS.Longitude_ShortName,
            Icon = Material.Icons.MaterialIconKind.Longitude,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        AltitudeProperty = new PropertyUnitReactive(
            nameof(AltitudeProperty),
            unitService.GetRequiredUnitOfType<AltitudeUnit>(AltitudeUnit.Id),
            _altitude,
            format: "F2"
        )
        {
            Header = Avalonia.RS.Altitude_Name,
            ShortHeader = Avalonia.RS.Altitude_ShortName,
            Icon = Material.Icons.MaterialIconKind.Altimeter,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        DistanceProperty = new PropertyUnitReactive(
            nameof(DistanceProperty),
            distanceUnit,
            _distance,
            format: "F0"
        )
        {
            Header = RS.GeoPointDialogViewModel_Step,
            ShortHeader = RS.GeoPointDialogViewModel_Step,
            Description = Avalonia.RS.Distance_Description,
            Icon = Material.Icons.MaterialIconKind.Ruler,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        FieldsEditor = new PropertyEditorViewModel($"{DialogId}-fields")
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        FieldsEditor.ItemsSource.Add(LatitudeProperty);
        FieldsEditor.ItemsSource.Add(LongitudeProperty);
        FieldsEditor.ItemsSource.Add(AltitudeProperty);
        FieldsEditor.ItemsSource.Add(DistanceProperty);

        MoveCommand = new ReactiveCommand<MoveDirection>(Move).DisposeItWith(Disposable);

        CenterGeoPoint = new BindableReactiveProperty<GeoPoint>(_currentValue).DisposeItWith(
            Disposable
        );

        CenterGeoPoint
            .Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Where(_ => !_isSyncingCenterGeoPoint)
            .Subscribe(value =>
            {
                ApplyValueToFields(
                    new GeoPoint(
                        value.Latitude,
                        value.Longitude,
                        ReadUnitPropertyValue(AltitudeProperty, _currentValue.Altitude)
                    )
                );
            })
            .DisposeItWith(Disposable);

        _latitude.Skip(1).Subscribe(_ => ApplyLatitudeToCenter()).DisposeItWith(Disposable);
        _longitude.Skip(1).Subscribe(_ => ApplyLongitudeToCenter()).DisposeItWith(Disposable);
        _altitude.Skip(1).Subscribe(_ => ApplyAltitudeFromField()).DisposeItWith(Disposable);

        TrackValidation(LatitudeProperty, false);
        TrackValidation(LongitudeProperty, false);
        TrackValidation(AltitudeProperty, false);
        TrackValidation(DistanceProperty, true);
        RefreshGeoPointValidation();
        RegisterDistanceLayout();
    }

    public PropertyEditorViewModel FieldsEditor { get; }
    public PropertyUnitViewModel LatitudeProperty { get; }
    public PropertyUnitViewModel LongitudeProperty { get; }
    public PropertyUnitViewModel AltitudeProperty { get; }
    public PropertyUnitViewModel DistanceProperty { get; }
    public IReadOnlyBindableReactiveProperty<ITileProvider> CurrentProvider { get; }
    public BindableReactiveProperty<GeoPoint> CenterGeoPoint { get; }

    public ReactiveCommand<MoveDirection> MoveCommand { get; }

    private void RefreshGeoPointValidation()
    {
        var isLonOk = !LongitudeProperty.Text.HasErrors;
        var isLatOk = !LatitudeProperty.Text.HasErrors;
        var isAltOk = !AltitudeProperty.Text.HasErrors;

        IsValid.Value = isLonOk && isLatOk && isAltOk;
    }

    private void TrackValidation(PropertyUnitViewModel property, bool affectsMoveCommand)
    {
        Observable
            .FromEventHandler<System.ComponentModel.DataErrorsChangedEventArgs>(
                h => property.Text.ErrorsChanged += h,
                h => property.Text.ErrorsChanged -= h
            )
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                RefreshGeoPointValidation();
                if (affectsMoveCommand)
                {
                    MoveCommand.ChangeCanExecute(!property.Text.HasErrors);
                }
            })
            .DisposeItWith(Disposable);
    }

    private void Move(MoveDirection moveDirection)
    {
        ApplyValueToFields(
            GeoPointMoveHelper.Step(GetResult(), GetCurrentDistance(), moveDirection)
        );
    }

    private void ApplyLatitudeToCenter()
    {
        if (_isSyncingFields)
        {
            return;
        }

        ApplyCurrentValueToCenter(
            new GeoPoint(_latitude.Value, _currentValue.Longitude, _currentValue.Altitude)
        );
    }

    private void ApplyLongitudeToCenter()
    {
        if (_isSyncingFields)
        {
            return;
        }

        ApplyCurrentValueToCenter(
            new GeoPoint(_currentValue.Latitude, _longitude.Value, _currentValue.Altitude)
        );
    }

    private void ApplyAltitudeFromField()
    {
        if (_isSyncingFields)
        {
            return;
        }

        _currentValue = new GeoPoint(
            _currentValue.Latitude,
            _currentValue.Longitude,
            _altitude.Value
        );
        RefreshGeoPointValidation();
    }

    public void ApplyValueToFields(GeoPoint value)
    {
        _currentValue = value;
        _isSyncingFields = true;
        try
        {
            _latitude.Value = value.Latitude;
            _longitude.Value = value.Longitude;
            _altitude.Value = value.Altitude;
        }
        finally
        {
            _isSyncingFields = false;
        }

        ApplyCurrentValueToCenter(value);
        RefreshGeoPointValidation();
    }

    private void ApplyCurrentValueToCenter(GeoPoint value)
    {
        _currentValue = value;
        _isSyncingCenterGeoPoint = true;
        try
        {
            CenterGeoPoint.Value = value;
        }
        finally
        {
            _isSyncingCenterGeoPoint = false;
        }

        RefreshGeoPointValidation();
    }

    public GeoPoint GetResult()
    {
        CommitPendingFieldValues();
        CommitPendingDistanceValue();
        return _currentValue;
    }

    private void CommitPendingFieldValues()
    {
        _currentValue = new GeoPoint(
            ReadPendingUnitPropertyValue(LatitudeProperty, _currentValue.Latitude),
            ReadPendingUnitPropertyValue(LongitudeProperty, _currentValue.Longitude),
            ReadPendingUnitPropertyValue(AltitudeProperty, _currentValue.Altitude)
        );
    }

    private static double ReadPendingUnitPropertyValue(
        PropertyUnitViewModel property,
        double fallback
    )
    {
        if (property.IsSync)
        {
            return fallback;
        }

        return ReadUnitPropertyValue(property, fallback);
    }

    private double GetCurrentDistance()
    {
        var distance = ReadUnitPropertyValue(DistanceProperty, _distance.Value);
        if (IsValidDistance(distance))
        {
            _distance.Value = distance;
        }

        return distance;
    }

    private void CommitPendingDistanceValue()
    {
        if (DistanceProperty.IsSync)
        {
            return;
        }

        var distance = ReadUnitPropertyValue(DistanceProperty, _distance.Value);
        if (IsValidDistance(distance))
        {
            _distance.Value = distance;
        }
    }

    private void RegisterDistanceLayout()
    {
        var distanceLayout = Layout.Register<double>(
            nameof(DistanceProperty),
            (value, _) =>
            {
                if (IsValidDistance(value))
                {
                    _isSyncingLayout = true;
                    try
                    {
                        _distance.Value = value;
                    }
                    finally
                    {
                        _isSyncingLayout = false;
                    }
                }

                return ValueTask.CompletedTask;
            }
        );

        var distanceLayoutSave = _distance
            .Skip(1)
            .Where(value => !_isSyncingLayout && IsValidDistance(value))
            .SubscribeAwait(
                (value, cancel) => distanceLayout.SaveAsync(value, cancel),
                AwaitOperation.Drop
            );

        R3.Disposable.Combine(distanceLayout, distanceLayoutSave).DisposeItWith(Disposable);
        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);
    }

    private static double ReadUnitPropertyValue(PropertyUnitViewModel property, double fallback)
    {
        if (property.Text.HasErrors)
        {
            return fallback;
        }

        try
        {
            return property.Unit.CurrentUnitItem.CurrentValue.ParseToSi(property.Text.Value);
        }
        catch
        {
            return fallback;
        }
    }

    private static bool IsValidDistance(double value)
    {
        return value is > 0 && double.IsFinite(value);
    }

    public override void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        IsValid
            .ObserveOnUIThreadDispatcher()
            .Subscribe(isValid => dialog.IsPrimaryButtonEnabled = isValid)
            .DisposeItWith(Disposable);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return FieldsEditor;
    }
}
