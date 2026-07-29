using Asv.Avalonia.GeoMap;
using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Example;

public class PropertyEditorPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "property-editor-example";
    public const MaterialIconKind PageIcon = MaterialIconKind.PropertyTag;
    private const string AdvancedScope = "advanced";

    public PropertyEditorPageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance,
            DesignTime.UnitService,
            DesignTime.DialogService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        SetParent(DesignTime.Shell);
    }

    public PropertyEditorPageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        IUnitService unit,
        IDialogService dialogService
    )
        : base(PageId, context)
    {
        DisplayNameProperty = CreateDisplayNameProperty();
        OperationProfileProperty = CreateOperationProfileProperty();
        OptimizationModeProperty = CreateOptimizationModeProperty();
        TelemetryEnabledProperty = CreateToggleSwitchProperty(
            "telemetry-enabled",
            TelemetryEnabled
        );

        ActionButtonProperty = CreateActionButtonProperty(
            DisplayNameProperty.Text.Select(static name => !string.IsNullOrWhiteSpace(name))
        );
        ThrottleSliderProperty = CreateSliderProperty("throttle-slider", ThrottleUnitValue);
        AltitudeUnitProperty = CreateUnitProperty(
            "altitude-unit",
            unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
            RS.PropertyEditorPageViewModel_AltitudeUnit_Header,
            RS.PropertyEditorPageViewModel_AltitudeUnit_ShortHeader,
            RS.PropertyEditorPageViewModel_AltitudeUnit_Description,
            MaterialIconKind.Altimeter,
            AsvColorKind.Info3,
            AltitudeUnitValue
        );
        ThrottleUnitProperty = CreateUnitProperty(
            "throttle-unit",
            unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
            RS.PropertyEditorPageViewModel_ThrottleUnit_Header,
            RS.PropertyEditorPageViewModel_ThrottleUnit_ShortHeader,
            RS.PropertyEditorPageViewModel_ThrottleUnit_Description,
            MaterialIconKind.Signal,
            AsvColorKind.Success,
            ThrottleUnitValue
        );
        PropertyEditor = CreatePropertyEditor(
                "editor",
                unit,
                DisplayNameProperty,
                OperationProfileProperty,
                OptimizationModeProperty,
                ActionButtonProperty,
                TelemetryEnabledProperty,
                ThrottleSliderProperty,
                AltitudeUnitProperty,
                ThrottleUnitProperty,
                dialogService
            )
            .SetRoutableParent(this);
        ConfigureLeftEditorScopes();
        PropertyEditorCopy = CreatePropertyEditor(
                "editor-copy",
                unit,
                CreateDisplayNameProperty(),
                CreateOperationProfileProperty(),
                CreateOptimizationModeProperty(),
                CreateActionButtonProperty(),
                CreateToggleSwitchProperty("telemetry-enabled", TelemetryEnabled),
                CreateSliderProperty("throttle-slider", ThrottleUnitValue),
                CreateUnitProperty(
                    "altitude-unit",
                    unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
                    RS.PropertyEditorPageViewModel_AltitudeUnit_Header,
                    RS.PropertyEditorPageViewModel_AltitudeUnit_ShortHeader,
                    RS.PropertyEditorPageViewModel_AltitudeUnit_Description,
                    MaterialIconKind.Altimeter,
                    AsvColorKind.Info3,
                    AltitudeUnitValue
                ),
                CreateUnitProperty(
                    "throttle-unit",
                    unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
                    RS.PropertyEditorPageViewModel_ThrottleUnit_Header,
                    RS.PropertyEditorPageViewModel_ThrottleUnit_ShortHeader,
                    RS.PropertyEditorPageViewModel_ThrottleUnit_Description,
                    MaterialIconKind.Signal,
                    AsvColorKind.Success,
                    ThrottleUnitValue
                ),
                dialogService
            )
            .SetRoutableParent(this);
        ExtendedPropertyEditor = CreateExtendedPropertyEditor(
                "editor-extended",
                unit,
                CreateDisplayNameProperty(),
                CreateOperationProfileProperty(),
                CreateOptimizationModeProperty(),
                CreateActionButtonProperty(),
                CreateToggleSwitchProperty("telemetry-enabled", TelemetryEnabled),
                CreateSliderProperty("throttle-slider", ThrottleUnitValue),
                CreateUnitProperty(
                    "altitude-unit",
                    unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
                    RS.PropertyEditorPageViewModel_AltitudeUnit_Header,
                    RS.PropertyEditorPageViewModel_AltitudeUnit_ShortHeader,
                    RS.PropertyEditorPageViewModel_AltitudeUnit_Description,
                    MaterialIconKind.Altimeter,
                    AsvColorKind.Info3,
                    AltitudeUnitValue
                ),
                CreateUnitProperty(
                    "throttle-unit",
                    unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
                    RS.PropertyEditorPageViewModel_ThrottleUnit_Header,
                    RS.PropertyEditorPageViewModel_ThrottleUnit_ShortHeader,
                    RS.PropertyEditorPageViewModel_ThrottleUnit_Description,
                    MaterialIconKind.Signal,
                    AsvColorKind.Success,
                    ThrottleUnitValue
                ),
                dialogService
            )
            .SetRoutableParent(this);

        GeoPoint.Subscribe(x =>
        {
            Latitude.Value = x.Latitude;
            Longitude.Value = x.Longitude;
            Altitude.Value = x.Altitude;
        });
    }

    private PropertyEditorViewModel CreatePropertyEditor(
        string id,
        IUnitService unit,
        PropertyTextBoxViewModel displayNameProperty,
        PropertyComboBoxViewModel operationProfileProperty,
        PropertyToggleButtonGroupViewModel optimizationModeProperty,
        PropertyButtonViewModel actionButtonProperty,
        PropertyToggleSwitchViewModel toggleSwitchProperty,
        PropertySliderViewModel throttleSliderProperty,
        PropertyUnitViewModel altitudeUnitProperty,
        PropertyUnitViewModel throttleUnitProperty,
        IDialogService dialogService
    )
    {
        return FillPropertyEditor(
            new PropertyEditorViewModel(id),
            unit,
            displayNameProperty,
            operationProfileProperty,
            optimizationModeProperty,
            actionButtonProperty,
            toggleSwitchProperty,
            throttleSliderProperty,
            altitudeUnitProperty,
            throttleUnitProperty,
            dialogService
        );
    }

    private ExtendedPropertyEditorViewModel CreateExtendedPropertyEditor(
        string id,
        IUnitService unit,
        PropertyTextBoxViewModel displayNameProperty,
        PropertyComboBoxViewModel operationProfileProperty,
        PropertyToggleButtonGroupViewModel optimizationModeProperty,
        PropertyButtonViewModel actionButtonProperty,
        PropertyToggleSwitchViewModel toggleSwitchProperty,
        PropertySliderViewModel throttleSliderProperty,
        PropertyUnitViewModel altitudeUnitProperty,
        PropertyUnitViewModel throttleUnitProperty,
        IDialogService dialogService
    )
    {
        return FillPropertyEditor(
            new ExtendedPropertyEditorViewModel(id),
            unit,
            displayNameProperty,
            operationProfileProperty,
            optimizationModeProperty,
            actionButtonProperty,
            toggleSwitchProperty,
            throttleSliderProperty,
            altitudeUnitProperty,
            throttleUnitProperty,
            dialogService
        );
    }

    private TEditor FillPropertyEditor<TEditor>(
        TEditor editor,
        IUnitService unit,
        PropertyTextBoxViewModel displayNameProperty,
        PropertyComboBoxViewModel operationProfileProperty,
        PropertyToggleButtonGroupViewModel optimizationModeProperty,
        PropertyButtonViewModel actionButtonProperty,
        PropertyToggleSwitchViewModel toggleSwitchProperty,
        PropertySliderViewModel throttleSliderProperty,
        PropertyUnitViewModel altitudeUnitProperty,
        PropertyUnitViewModel throttleUnitProperty,
        IDialogService dialogService
    )
        where TEditor : PropertyEditorViewModel
    {
        editor.ItemsSource.Add(displayNameProperty);
        editor.ItemsSource.Add(operationProfileProperty);
        editor.ItemsSource.Add(optimizationModeProperty);
        editor.ItemsSource.Add(actionButtonProperty);
        editor.ItemsSource.Add(toggleSwitchProperty);
        editor.ItemsSource.Add(throttleSliderProperty);
        editor.ItemsSource.Add(altitudeUnitProperty);
        editor.ItemsSource.Add(throttleUnitProperty);
        editor.ItemsSource.Add(
            AddExampleErrorMenu(
                new PropertyUnitReactive(
                    "latitude",
                    unit.GetRequiredUnitOfType<LatitudeUnit>(LatitudeUnit.Id),
                    Latitude
                )
                {
                    Header = RS.PropertyEditorPageViewModel_Latitude_Header,
                    ShortHeader = RS.PropertyEditorPageViewModel_Latitude_ShortHeader,
                    Description = RS.PropertyEditorPageViewModel_Latitude_Description,
                    Icon = MaterialIconKind.Latitude,
                }
            )
        );
        editor.ItemsSource.Add(
            AddExampleErrorMenu(
                new PropertyUnitReactive(
                    "longitude",
                    unit[LongitudeUnit.Id] ?? throw new ArgumentNullException(),
                    Longitude
                )
                {
                    Header = RS.PropertyEditorPageViewModel_Longitude_Header,
                    ShortHeader = RS.PropertyEditorPageViewModel_Longitude_ShortHeader,
                    Description = RS.PropertyEditorPageViewModel_Latitude_Description,
                    Icon = MaterialIconKind.Latitude,
                }
            )
        );
        editor.ItemsSource.Add(
            AddExampleErrorMenu(
                new PropertyUnitReactive(
                    "altitude",
                    unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
                    Altitude
                )
                {
                    Header = RS.PropertyEditorPageViewModel_Altitude_Header,
                    ShortHeader = RS.PropertyEditorPageViewModel_Altitude_ShortHeader,
                    Description = RS.PropertyEditorPageViewModel_Altitude_Description,
                    Icon = MaterialIconKind.Altimeter,
                }
            )
        );
        editor.ItemsSource.Add(
            AddExampleErrorMenu(
                new PropertyGeoPointReactive("geo-point", GeoPoint, unit, dialogService)
                {
                    Header = RS.PropertyEditorPageViewModel_GeoPoint_Header,
                    Description = RS.PropertyEditorPageViewModel_GeoPoint_Description,
                    Icon = MaterialIconKind.Earth,
                }
            )
        );
        editor.ItemsSource.Add(
            AddExampleErrorMenu(
                new PropertyUnitReactive(
                    "time",
                    unit.GetRequiredUnitOfType<TimeSpanUnit>(TimeSpanUnit.Id),
                    Time
                )
                {
                    Header = RS.PropertyEditorPageViewModel_Time_Header,
                    ShortHeader = RS.PropertyEditorPageViewModel_Time_ShortHeader,
                    Description = RS.PropertyEditorPageViewModel_Time_Description,
                    Icon = MaterialIconKind.Timelapse,
                }
            )
        );
        editor.ItemsSource.Add(
            AddExampleErrorMenu(
                new PropertyUnitReactive(
                    "throttle",
                    unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
                    Throttle
                )
                {
                    Header = RS.PropertyEditorPageViewModel_Throttle_Header,
                    ShortHeader = RS.PropertyEditorPageViewModel_Throttle_ShortHeader,
                    Description = RS.PropertyEditorPageViewModel_Throttle_Description,
                    Icon = MaterialIconKind.Signal,
                }
            )
        );

        return editor;
    }

    private PropertyTextBoxViewModel CreateDisplayNameProperty()
    {
        var property = new PropertyTextBoxReactive("display-name", DisplayName)
        {
            Header = RS.PropertyEditorPageViewModel_DisplayName_Header,
            ShortHeader = RS.PropertyEditorPageViewModel_DisplayName_ShortHeader,
            Description = RS.PropertyEditorPageViewModel_DisplayName_Description,
            Icon = MaterialIconKind.FormTextbox,
            IconColor = AsvColorKind.Info5,
        };
        property
            .Text.EnableValidation(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new ValidationException(
                        RS.PropertyEditorPageViewModel_DisplayName_Required
                    );
                }

                return null;
            })
            .AddTo(ref DisposableBag);
        property.Text.ForceValidate();

        return AddExampleErrorMenu(property);
    }

    private PropertyButtonViewModel CreateActionButtonProperty(Observable<bool>? canExecute = null)
    {
        return AddExampleErrorMenu(
            new PropertyButtonViewModel("run-check", ExecuteActionButton, canExecute)
            {
                Header = RS.PropertyEditorPageViewModel_ActionButton_Header,
                ShortHeader = RS.PropertyEditorPageViewModel_ActionButton_ShortHeader,
                Description = RS.PropertyEditorPageViewModel_ActionButton_Description,
                Icon = MaterialIconKind.PlayCircle,
                IconColor = AsvColorKind.Success,
            }
        );
    }

    private async ValueTask ExecuteActionButton(CancellationToken cancel)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(750), cancel);
        ActionButtonClickCount++;
    }

    private static PropertyToggleSwitchViewModel CreateToggleSwitchProperty(
        string id,
        ReactiveProperty<bool> model
    )
    {
        return AddExampleErrorMenu(
            new PropertyToggleSwitchReactive(id, model)
            {
                Header = RS.PropertyEditorPageViewModel_ToggleSwitch_Header,
                ShortHeader = RS.PropertyEditorPageViewModel_ToggleSwitch_ShortHeader,
                Description = RS.PropertyEditorPageViewModel_ToggleSwitch_Description,
                Icon = MaterialIconKind.ToggleSwitch,
                IconColor = AsvColorKind.Info5,
            }
        );
    }

    private static PropertySliderViewModel CreateSliderProperty(
        string id,
        ReactiveProperty<double> model
    )
    {
        return AddExampleErrorMenu(
            new PropertySliderReactive(id, model, 0, 100)
            {
                Header = RS.PropertyEditorPageViewModel_ThrottleSlider_Header,
                ShortHeader = RS.PropertyEditorPageViewModel_ThrottleSlider_ShortHeader,
                Description = RS.PropertyEditorPageViewModel_ThrottleSlider_Description,
                Icon = MaterialIconKind.Signal,
                IconColor = AsvColorKind.Success,
                TickFrequency = 5,
                SmallChange = 1,
                LargeChange = 10,
                IsSnapToTickEnabled = true,
                Units = "%",
                ValueFormat = "0",
            }
        );
    }

    private static PropertyUnitViewModel CreateUnitProperty(
        string id,
        IUnit unit,
        string header,
        string shortName,
        string description,
        MaterialIconKind icon,
        AsvColorKind iconColor,
        ReactiveProperty<double> model
    )
    {
        var property = new PropertyUnitReactive(id, unit, model)
        {
            Header = header,
            ShortHeader = shortName,
            Description = description,
            Icon = icon,
            IconColor = iconColor,
        };

        return AddExampleErrorMenu(property);
    }

    private PropertyComboBoxViewModel CreateOperationProfileProperty()
    {
        var property = new PropertyComboBoxReactive("operation-profile", OperationProfile)
        {
            Header = RS.PropertyEditorPageViewModel_OperationProfile_Header,
            Description = RS.PropertyEditorPageViewModel_OperationProfile_Description,
            Icon = MaterialIconKind.FormDropdown,
            IconColor = AsvColorKind.Info7,
        };

        var firstItem = AddOperationProfileItem(
            property,
            "manual",
            RS.PropertyEditorPageViewModel_Profile_Manual_Header,
            RS.PropertyEditorPageViewModel_Profile_Manual_Description,
            MaterialIconKind.Hand,
            AsvColorKind.Info1
        );
        AddOperationProfileItem(
            property,
            "guided",
            RS.PropertyEditorPageViewModel_Profile_Guided_Header,
            RS.PropertyEditorPageViewModel_Profile_Guided_Description,
            MaterialIconKind.Compass,
            AsvColorKind.Info5
        );
        AddOperationProfileItem(
            property,
            "survey",
            RS.PropertyEditorPageViewModel_Profile_Survey_Header,
            RS.PropertyEditorPageViewModel_Profile_Survey_Description,
            MaterialIconKind.MapMarkerRadius,
            AsvColorKind.Success
        );
        AddOperationProfileItem(
            property,
            "silent",
            RS.PropertyEditorPageViewModel_Profile_Silent_Header,
            RS.PropertyEditorPageViewModel_Profile_Silent_Description,
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "inspection",
            RS.PropertyEditorPageViewModel_Profile_Inspection_Header,
            RS.PropertyEditorPageViewModel_Profile_Inspection_Description,
            MaterialIconKind.MagnifyScan,
            AsvColorKind.Warning
        );
        AddOperationProfileItem(
            property,
            "minimal",
            RS.PropertyEditorPageViewModel_Profile_Minimal_Header,
            RS.PropertyEditorPageViewModel_Profile_Minimal_Description,
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "diagnostics",
            RS.PropertyEditorPageViewModel_Profile_Diagnostics_Header,
            RS.PropertyEditorPageViewModel_Profile_Diagnostics_Description,
            MaterialIconKind.Stethoscope,
            AsvColorKind.Info12
        );
        AddOperationProfileItem(
            property,
            "offline-cache",
            RS.PropertyEditorPageViewModel_Profile_OfflineCache_Header,
            RS.PropertyEditorPageViewModel_Profile_OfflineCache_Description,
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "emergency",
            RS.PropertyEditorPageViewModel_Profile_Emergency_Header,
            RS.PropertyEditorPageViewModel_Profile_Emergency_Description,
            MaterialIconKind.AlertOctagon,
            AsvColorKind.Error
        );
        AddOperationProfileItem(
            property,
            "custom-profile",
            RS.PropertyEditorPageViewModel_Profile_Custom_Header,
            RS.PropertyEditorPageViewModel_Profile_Custom_Description,
            null,
            AsvColorKind.None
        );
        OperationProfile.Value ??= firstItem;

        return AddExampleErrorMenu(property);
    }

    private PropertyToggleButtonGroupViewModel CreateOptimizationModeProperty()
    {
        var property = new PropertyToggleButtonGroupReactive("optimization-mode", OptimizationMode)
        {
            Header = RS.PropertyEditorPageViewModel_OptimizationMode_Header,
            ShortHeader = RS.PropertyEditorPageViewModel_OptimizationMode_ShortHeader,
            Description = RS.PropertyEditorPageViewModel_OptimizationMode_Description,
            Icon = MaterialIconKind.Tune,
            IconColor = AsvColorKind.Success,
        };

        var firstItem = AddSelectionItem(
            property,
            "speed",
            RS.PropertyEditorPageViewModel_Mode_Speed_Header,
            RS.PropertyEditorPageViewModel_Mode_Speed_Description,
            MaterialIconKind.Signal,
            AsvColorKind.Success
        );
        AddSelectionItem(
            property,
            "quality",
            RS.PropertyEditorPageViewModel_Mode_Quality_Header,
            RS.PropertyEditorPageViewModel_Mode_Quality_Description,
            MaterialIconKind.CheckCircle,
            AsvColorKind.Success
        );
        AddSelectionItem(
            property,
            "balanced",
            RS.PropertyEditorPageViewModel_Mode_Balanced_Header,
            RS.PropertyEditorPageViewModel_Mode_Balanced_Description,
            MaterialIconKind.Tune,
            AsvColorKind.Success
        );
        OptimizationMode.Value ??= firstItem;

        return AddExampleErrorMenu(property);
    }

    private static TProperty AddExampleErrorMenu<TProperty>(TProperty property)
        where TProperty : PropertyViewModel
    {
        property.Menu.Add(
            CreateSetErrorMenuItem(
                property,
                "set-validation-error",
                RS.PropertyEditorPageViewModel_ErrorMenu_Validation_Header,
                RS.PropertyEditorPageViewModel_ErrorMenu_Validation_Message,
                MaterialIconKind.AlertCircle,
                0
            )
        );
        property.Menu.Add(
            CreateSetErrorMenuItem(
                property,
                "set-sync-error",
                RS.PropertyEditorPageViewModel_ErrorMenu_Sync_Header,
                RS.PropertyEditorPageViewModel_ErrorMenu_Sync_Message,
                MaterialIconKind.SyncAlert,
                1
            )
        );
        property.Menu.Add(
            CreateSetErrorMenuItem(
                property,
                "set-network-error",
                RS.PropertyEditorPageViewModel_ErrorMenu_Network_Header,
                RS.PropertyEditorPageViewModel_ErrorMenu_Network_Message,
                MaterialIconKind.CloseNetwork,
                2
            )
        );
        property.Menu.Add(
            new MenuItem("clear-error", RS.PropertyEditorPageViewModel_ErrorMenu_Clear_Header)
            {
                Icon = MaterialIconKind.Restore,
                Order = 3,
                Command = new ReactiveCommand(_ => property.ErrorMessage = null),
            }
        );

        return property;
    }

    private static MenuItem CreateSetErrorMenuItem(
        PropertyViewModel property,
        string id,
        string header,
        string message,
        MaterialIconKind icon,
        int order
    )
    {
        return new MenuItem(id, header)
        {
            Icon = icon,
            Order = order,
            Command = new ReactiveCommand(_ =>
            {
                property.ErrorIcon = icon;
                property.ErrorMessage = message;
            }),
        };
    }

    private void ConfigureLeftEditorScopes()
    {
        AltitudeUnitProperty.DisplayScopes.Add(AdvancedScope);
        ThrottleUnitProperty.DisplayScopes.Add(AdvancedScope);

        foreach (var property in PropertyEditor.ItemsSource)
        {
            switch (property.Id.TypeId)
            {
                case "geo-point":
                case "time":
                case "throttle":
                    property.DisplayScopes.Add(AdvancedScope);
                    break;
            }
        }
    }

    private static IHeadlinedViewModel AddOperationProfileItem(
        PropertyComboBoxViewModel property,
        string id,
        string header,
        string description,
        MaterialIconKind? icon,
        AsvColorKind iconColor
    )
    {
        var item = new HeadlinedViewModel(id)
        {
            Header = header,
            Description = description,
            Icon = icon,
            IconColor = iconColor,
        };
        property.ItemsSource.Add(item);
        return item;
    }

    private static IHeadlinedViewModel AddSelectionItem(
        PropertyComboBoxViewModel property,
        string id,
        string header,
        string description,
        MaterialIconKind? icon,
        AsvColorKind iconColor
    )
    {
        var item = new HeadlinedViewModel(id)
        {
            Header = header,
            Description = description,
            Icon = icon,
            IconColor = iconColor,
        };
        property.ItemsSource.Add(item);
        return item;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return PropertyEditor;
        yield return PropertyEditorCopy;
        yield return ExtendedPropertyEditor;
        foreach (var item in base.GetChildren())
        {
            yield return item;
        }
    }

    public BindableReactiveProperty<double> Altitude { get; } = new();
    public BindableReactiveProperty<double> Latitude { get; } = new();
    public BindableReactiveProperty<double> Longitude { get; } = new();

    public BindableReactiveProperty<string?> DisplayName { get; } =
        new(RS.PropertyEditorPageViewModel_DisplayName_DefaultValue);
    public BindableReactiveProperty<IHeadlinedViewModel?> OperationProfile { get; } = new();
    public BindableReactiveProperty<IHeadlinedViewModel?> OptimizationMode { get; } = new();
    public BindableReactiveProperty<bool> TelemetryEnabled { get; } = new(true);
    public BindableReactiveProperty<double> AltitudeUnitValue { get; } = new(1250);
    public BindableReactiveProperty<double> ThrottleUnitValue { get; } = new(65);

    public BindableReactiveProperty<GeoPoint> GeoPoint { get; } = new();
    public BindableReactiveProperty<double> Time { get; } = new();
    public BindableReactiveProperty<double> Throttle { get; } = new();

    public int ActionButtonClickCount
    {
        get;
        private set => SetField(ref field, value);
    }

    public bool ShowPropertyHeaders
    {
        get;
        set
        {
            if (SetField(ref field, value) == false)
            {
                return;
            }

            PropertyEditor.ShowHeader = value;
            PropertyEditorCopy.ShowHeader = value;
            ExtendedPropertyEditor.ShowHeader = value;
        }
    }

    public bool ShowLeftEditorAdvancedScope
    {
        get;
        set
        {
            if (SetField(ref field, value) == false)
            {
                return;
            }

            if (value)
            {
                PropertyEditor.DisplayScopes.Add(AdvancedScope);
            }
            else
            {
                PropertyEditor.DisplayScopes.Remove(AdvancedScope);
            }
        }
    }

    public PropertyTextBoxViewModel DisplayNameProperty { get; }
    public PropertyComboBoxViewModel OperationProfileProperty { get; }
    public PropertyToggleButtonGroupViewModel OptimizationModeProperty { get; }
    public PropertyButtonViewModel ActionButtonProperty { get; }
    public PropertyToggleSwitchViewModel TelemetryEnabledProperty { get; }
    public PropertySliderViewModel ThrottleSliderProperty { get; }
    public PropertyUnitViewModel AltitudeUnitProperty { get; }
    public PropertyUnitViewModel ThrottleUnitProperty { get; }
    public PropertyEditorViewModel PropertyEditor { get; }
    public PropertyEditorViewModel PropertyEditorCopy { get; }
    public ExtendedPropertyEditorViewModel ExtendedPropertyEditor { get; }
}
