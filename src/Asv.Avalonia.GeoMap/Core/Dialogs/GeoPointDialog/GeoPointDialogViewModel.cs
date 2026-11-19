using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

public class GeoPointDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}-geopoint";

    private readonly ReactiveProperty<double> _altitude;
    private readonly ReactiveProperty<double> _latitude;
    private readonly ReactiveProperty<double> _longitude;
    private bool _isSyncingCenterGeoPoint;
    private bool _isSyncingFields;

    public GeoPointDialogViewModel()
        : this(NullUnitService.Instance, NullMapService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public GeoPointDialogViewModel(IUnitService unitService, IMapService mapService)
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

        var altitudeUnit = unitService.GetRequiredUnitOfType<AltitudeUnit>(AltitudeUnit.Id);

        LatitudeProperty = new PropertyUnitReactive(
            "latitude",
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
            "longitude",
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
            "altitude",
            altitudeUnit,
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

        FieldsEditor = new PropertyEditorViewModel("fields")
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        FieldsEditor.ItemsSource.Add(LatitudeProperty);
        FieldsEditor.ItemsSource.Add(LongitudeProperty);
        FieldsEditor.ItemsSource.Add(AltitudeProperty);

        HorizontalStep = new GeoPointDialogStepViewModel(
            "horizontal-step",
            unitService.GetRequiredUnitOfType<DistanceUnit>(DistanceUnit.Id),
            RS.GeoPointDialogViewModel_HorizontalStep,
            Material.Icons.MaterialIconKind.ArrowAll
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        VerticalStep = new GeoPointDialogStepViewModel(
            "vertical-step",
            altitudeUnit,
            RS.GeoPointDialogViewModel_VerticalStep,
            Material.Icons.MaterialIconKind.ArrowUpDown
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        MoveCommand = HorizontalStep
            .IsValid.ToReactiveCommand<MoveDirection>(direction => Move(direction, HorizontalStep))
            .DisposeItWith(Disposable);
        MoveVerticalCommand = VerticalStep
            .IsValid.ToReactiveCommand<MoveDirection>(direction => Move(direction, VerticalStep))
            .DisposeItWith(Disposable);

        CenterGeoPoint = new BindableReactiveProperty<GeoPoint>(GeoPoint.Zero).DisposeItWith(
            Disposable
        );

        CenterGeoPoint
            .Skip(1)
            .Where(_ => !_isSyncingCenterGeoPoint)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                ApplyValueToFields(
                    new GeoPoint(
                        CenterGeoPoint.Value.Latitude,
                        CenterGeoPoint.Value.Longitude,
                        ReadUnitPropertyValue(AltitudeProperty, _altitude.Value)
                    )
                );
            })
            .DisposeItWith(Disposable);

        _latitude.Skip(1).Subscribe(_ => ApplyFieldsToCenter()).DisposeItWith(Disposable);
        _longitude.Skip(1).Subscribe(_ => ApplyFieldsToCenter()).DisposeItWith(Disposable);
        _altitude.Skip(1).Subscribe(_ => ApplyFieldsToCenter()).DisposeItWith(Disposable);

        TrackValidation(LatitudeProperty);
        TrackValidation(LongitudeProperty);
        TrackValidation(AltitudeProperty);
        RefreshGeoPointValidation();
    }

    public PropertyEditorViewModel FieldsEditor { get; }
    public PropertyUnitViewModel LatitudeProperty { get; }
    public PropertyUnitViewModel LongitudeProperty { get; }
    public PropertyUnitViewModel AltitudeProperty { get; }
    public GeoPointDialogStepViewModel HorizontalStep { get; }
    public GeoPointDialogStepViewModel VerticalStep { get; }
    public IReadOnlyBindableReactiveProperty<ITileProvider> CurrentProvider { get; }
    public BindableReactiveProperty<GeoPoint> CenterGeoPoint { get; }

    public ReactiveCommand<MoveDirection> MoveCommand { get; }
    public ReactiveCommand<MoveDirection> MoveVerticalCommand { get; }

    public void ApplyValueToFields(GeoPoint value)
    {
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

        ApplyValueToCenter(value);
    }

    public GeoPoint GetResult()
    {
        return new GeoPoint(
            ReadPendingUnitPropertyValue(LatitudeProperty, _latitude.Value),
            ReadPendingUnitPropertyValue(LongitudeProperty, _longitude.Value),
            ReadPendingUnitPropertyValue(AltitudeProperty, _altitude.Value)
        );
    }

    private void RefreshGeoPointValidation()
    {
        var isLonOk = !LongitudeProperty.Text.HasErrors;
        var isLatOk = !LatitudeProperty.Text.HasErrors;
        var isAltOk = !AltitudeProperty.Text.HasErrors;

        IsValid.Value = isLonOk && isLatOk && isAltOk;
    }

    private void TrackValidation(PropertyUnitViewModel property)
    {
        Observable
            .FromEventHandler<System.ComponentModel.DataErrorsChangedEventArgs>(
                h => property.Text.ErrorsChanged += h,
                h => property.Text.ErrorsChanged -= h
            )
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => RefreshGeoPointValidation())
            .DisposeItWith(Disposable);
    }

    private void Move(MoveDirection moveDirection, GeoPointDialogStepViewModel step)
    {
        ApplyValueToFields(GeoPointMoveHelper.Step(GetResult(), step.ValueInSi, moveDirection));
    }

    private void ApplyFieldsToCenter()
    {
        if (_isSyncingFields)
        {
            return;
        }

        ApplyValueToCenter(new GeoPoint(_latitude.Value, _longitude.Value, _altitude.Value));
    }

    private void ApplyValueToCenter(GeoPoint value)
    {
        _isSyncingCenterGeoPoint = true;
        try
        {
            CenterGeoPoint.Value = value;
        }
        finally
        {
            _isSyncingCenterGeoPoint = false;
        }
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

    private static double ReadUnitPropertyValue(PropertyUnitViewModel property, double fallback)
    {
        if (property.Text.HasErrors)
        {
            return fallback;
        }

        return property.Unit.CurrentUnitItem.CurrentValue.ParseToSi(property.Text.Value);
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
        yield return HorizontalStep;
        yield return VerticalStep;
    }
}
