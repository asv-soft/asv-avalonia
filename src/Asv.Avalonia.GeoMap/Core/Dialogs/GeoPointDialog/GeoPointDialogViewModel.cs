using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.GeoMap;

public class GeoPointDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}-geopoint";

    private sealed class StepLayout
    {
        public string? Text { get; init; }
        public string? UnitItemId { get; init; }
    }

    private readonly ReactiveProperty<double> _altitude;
    private readonly ReactiveProperty<double> _latitude;
    private readonly ReactiveProperty<double> _longitude;
    private bool _isSyncingCenterGeoPoint;
    private bool _isSyncingFields;
    private bool _isLoadingStep;

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
        FieldsEditor = new PropertyEditorViewModel("fields")
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

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
        };
        FieldsEditor.ItemsSource.Add(LatitudeProperty);

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
        };
        FieldsEditor.ItemsSource.Add(LongitudeProperty);

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
        };
        FieldsEditor.ItemsSource.Add(AltitudeProperty);

        StepViewModel = new PropertyUnitStepViewModel(
            "step",
            unitService.GetRequiredUnitOfType<DistanceUnit>(DistanceUnit.Id),
            validator: stepInSi =>
                stepInSi > 0 && double.IsFinite(stepInSi)
                    ? null
                    : new UnitException(RS.GeoPointDialogStep_PositiveRequired)
        )
        {
            Header = RS.GeoPointDialogViewModel_Step,
            ShortHeader = RS.GeoPointDialogViewModel_Step,
            Icon = Material.Icons.MaterialIconKind.ArrowAll,
        };
        FieldsEditor.ItemsSource.Add(StepViewModel);

        MoveCommand = StepViewModel
            .ObservePropertyChanged(property => property.HasValidationErrors)
            .Select(hasErrors => !hasErrors)
            .ToReactiveCommand<MoveDirection>(Move)
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

        Observable
            .CombineLatest(
                LatitudeProperty.ObservePropertyChanged(property => property.HasValidationErrors),
                LongitudeProperty.ObservePropertyChanged(property => property.HasValidationErrors),
                AltitudeProperty.ObservePropertyChanged(property => property.HasValidationErrors),
                (latitudeHasErrors, longitudeHasErrors, altitudeHasErrors) =>
                    !latitudeHasErrors && !longitudeHasErrors && !altitudeHasErrors
            )
            .Subscribe(isValid => IsValid.Value = isValid)
            .DisposeItWith(Disposable);

        RegisterStepLayout();
    }

    public PropertyEditorViewModel FieldsEditor { get; }
    public PropertyUnitViewModel LatitudeProperty { get; }
    public PropertyUnitViewModel LongitudeProperty { get; }
    public PropertyUnitViewModel AltitudeProperty { get; }
    public PropertyUnitStepViewModel StepViewModel { get; }
    public IReadOnlyBindableReactiveProperty<ITileProvider> CurrentProvider { get; }
    public BindableReactiveProperty<GeoPoint> CenterGeoPoint { get; }

    public ReactiveCommand<MoveDirection> MoveCommand { get; }

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

    private void Move(MoveDirection moveDirection)
    {
        if (StepViewModel.TryGetValueInSi(out var stepInSi))
        {
            ApplyValueToFields(GeoPointMoveHelper.Step(GetResult(), stepInSi, moveDirection));
        }
    }

    private void RegisterStepLayout()
    {
        var layout = Layout.Register<StepLayout>("Step", LoadStepLayout).DisposeItWith(Disposable);
        StepViewModel
            .Text.CombineLatest(
                StepViewModel.UnitItem,
                (text, unitItem) => new StepLayout { Text = text, UnitItemId = unitItem.UnitItemId }
            )
            .Skip(1)
            .Where(_ => !_isLoadingStep && !StepViewModel.HasValidationErrors)
            .SubscribeAwait(layout.SaveAsync)
            .DisposeItWith(Disposable);
        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);
    }

    private void LoadStepLayout(StepLayout step)
    {
        _isLoadingStep = true;
        try
        {
            StepViewModel.TrySetValue(step.Text, step.UnitItemId);
        }
        finally
        {
            _isLoadingStep = false;
        }
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
    }
}
