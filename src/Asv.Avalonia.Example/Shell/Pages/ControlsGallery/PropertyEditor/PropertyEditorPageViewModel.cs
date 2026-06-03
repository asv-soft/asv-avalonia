using System;
using System.Collections.Generic;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public class PropertyEditorPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "property_editor_example";
    public const MaterialIconKind PageIcon = MaterialIconKind.PropertyTag;

    public PropertyEditorPageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance,
            DesignTime.UnitService,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        SetParent(DesignTime.Shell);
    }

    public PropertyEditorPageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        IUnitService unit,
        ILoggerFactory loggerFactory
    )
        : base(PageId, context)
    {
        DisplayNameProperty = CreateDisplayNameProperty();
        OperationProfileProperty = CreateOperationProfileProperty();
        ActionButtonProperty = CreateActionButtonProperty();
        AltitudeUnitProperty = CreateUnitProperty(
            "altitude_unit_v2",
            unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
            "Altitude V2",
            "Alt",
            "V2 unit property with a text value and unit selector.",
            MaterialIconKind.Altimeter,
            AsvColorKind.Info3,
            AltitudeUnitValue
        );
        ThrottleUnitProperty = CreateUnitProperty(
            "throttle_unit_v2",
            unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
            "Throttle V2",
            "Thr",
            "V2 unit property using the throttle unit selector.",
            MaterialIconKind.Signal,
            AsvColorKind.Success,
            ThrottleUnitValue
        );
        PropertyEditor = CreatePropertyEditor(
                "editor",
                unit,
                DisplayNameProperty,
                OperationProfileProperty,
                ActionButtonProperty,
                AltitudeUnitProperty,
                ThrottleUnitProperty
            )
            .SetRoutableParent(this);
        PropertyEditorCopy = CreatePropertyEditor(
                "editor_copy",
                unit,
                CreateDisplayNameProperty(),
                CreateOperationProfileProperty(),
                CreateActionButtonProperty(),
                CreateUnitProperty(
                    "altitude_unit_v2_copy",
                    unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
                    "Altitude V2",
                    "Alt",
                    "V2 unit property with a text value and unit selector.",
                    MaterialIconKind.Altimeter,
                    AsvColorKind.Info3,
                    AltitudeUnitValue
                ),
                CreateUnitProperty(
                    "throttle_unit_v2_copy",
                    unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
                    "Throttle V2",
                    "Thr",
                    "V2 unit property using the throttle unit selector.",
                    MaterialIconKind.Signal,
                    AsvColorKind.Success,
                    ThrottleUnitValue
                )
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
        PropertyButtonViewModel actionButtonProperty,
        PropertyUnitViewModel altitudeUnitProperty,
        PropertyUnitViewModel throttleUnitProperty
    )
    {
        return new PropertyEditorViewModel(id)
        {
            ItemsSource =
            {
                displayNameProperty,
                operationProfileProperty,
                actionButtonProperty,
                altitudeUnitProperty,
                throttleUnitProperty,
                new PropertyUnitReactive(
                    $"{id}_lat",
                    unit.GetRequiredUnitOfType<LatitudeUnit>(LatitudeUnit.Id),
                    Latitude
                )
                {
                    Header = "Position",
                    ShortHeader = "Lat",
                    Description = "Latitude description",
                    Icon = MaterialIconKind.Latitude,
                },
                new PropertyUnitReactive(
                    $"{id}_lon",
                    unit[LongitudeUnit.Id] ?? throw new ArgumentNullException(),
                    Longitude
                )
                {
                    Header = "Longitude",
                    ShortHeader = "Lon",
                    Description = "Latitude description",
                    Icon = MaterialIconKind.Latitude,
                },
                new PropertyUnitReactive(
                    $"{id}_alt",
                    unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
                    Altitude
                )
                {
                    Header = "Altitude",
                    ShortHeader = "Alt",
                    Description = "Altitude description",
                    Icon = MaterialIconKind.Altimeter,
                },
                new PropertyGeoPointReactive($"{id}_geo_v2", GeoPoint, unit)
                {
                    Header = "Geo Point",
                    Description = "Geo Point description",
                    Icon = MaterialIconKind.Earth,
                },
                new PropertyUnitReactive(
                    $"{id}_time",
                    unit.GetRequiredUnitOfType<TimeSpanUnit>(TimeSpanUnit.Id),
                    Time
                )
                {
                    Header = "Time",
                    ShortHeader = "Time",
                    Description = "Time description",
                    Icon = MaterialIconKind.Timelapse,
                },
                new PropertyUnitReactive(
                    $"{id}_throttle",
                    unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
                    Throttle
                )
                {
                    Header = "Throttle",
                    ShortHeader = "Throttle",
                    Description = "Throttle description",
                    Icon = MaterialIconKind.Signal,
                },
            },
        };
    }

    private PropertyTextBoxViewModel CreateDisplayNameProperty()
    {
        var property = new PropertyTextBoxReactive("display_name", DisplayName)
        {
            Header = "Display name",
            ShortHeader = "Name",
            Description =
                "Text editor with validation, icon, remote update marker, and menu button.",
            Icon = MaterialIconKind.FormTextbox,
            IconColor = AsvColorKind.Info5,
        };
        property
            .Text.EnableValidation(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new ValidationException("Display name is required");
                }

                return null;
            })
            .AddTo(ref DisposableBag);
        property.Text.ForceValidate();

        return property;
    }

    private PropertyButtonViewModel CreateActionButtonProperty()
    {
        return new PropertyButtonViewModel("action_button", ExecuteActionButton)
        {
            Header = "Run check",
            ShortHeader = "Run",
            Description = "Button property with async command, busy state, and update marker.",
            Icon = MaterialIconKind.PlayCircle,
            IconColor = AsvColorKind.Success,
        };
    }

    private async ValueTask ExecuteActionButton(CancellationToken cancel)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(750), cancel);
        ActionButtonClickCount++;
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

        return property;
    }

    private PropertyComboBoxViewModel CreateOperationProfileProperty()
    {
        var property = new PropertyComboBoxReactive("operation_profile", OperationProfile)
        {
            Header = "Operation profile",
            Description =
                "Combo box with test view models: icon, text-only, and different accent styles.",
            Icon = MaterialIconKind.FormDropdown,
            IconColor = AsvColorKind.Info7,
        };

        var firstItem = AddOperationProfileItem(
            property,
            "manual_mode",
            "Manual",
            "Operator controls every step directly.",
            MaterialIconKind.Hand,
            AsvColorKind.Info1
        );
        AddOperationProfileItem(
            property,
            "guided_mode",
            "Guided",
            "Assisted flow with validation after each step.",
            MaterialIconKind.Compass,
            AsvColorKind.Info5
        );
        AddOperationProfileItem(
            property,
            "survey_mode",
            "Survey",
            "Collects structured measurements for later analysis.",
            MaterialIconKind.MapMarkerRadius,
            AsvColorKind.Success
        );
        AddOperationProfileItem(
            property,
            "silent_mode",
            "Silent",
            "Runs without icon or accent decoration.",
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "inspection_mode",
            "Inspection",
            "Highlights issues and requires operator confirmation.",
            MaterialIconKind.MagnifyScan,
            AsvColorKind.Warning
        );
        AddOperationProfileItem(
            property,
            "minimal_mode",
            "Minimal",
            "Text-only item for compact layouts.",
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "diagnostic_mode",
            "Diagnostics",
            "Shows service information and hardware status.",
            MaterialIconKind.Stethoscope,
            AsvColorKind.Info12
        );
        AddOperationProfileItem(
            property,
            "offline_mode",
            "Offline cache",
            "No image, useful when external data is unavailable.",
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "emergency_mode",
            "Emergency",
            "Critical action style with a warning-colored icon.",
            MaterialIconKind.AlertOctagon,
            AsvColorKind.Error
        );
        AddOperationProfileItem(
            property,
            "custom_mode",
            "Custom profile",
            "Plain item with only header and description.",
            null,
            AsvColorKind.None
        );
        OperationProfile.Value ??= firstItem;

        return property;
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

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return PropertyEditor;
        yield return PropertyEditorCopy;
        foreach (var item in base.GetChildren())
        {
            yield return item;
        }
    }

    public BindableReactiveProperty<double> Altitude { get; } = new();
    public BindableReactiveProperty<double> Latitude { get; } = new();
    public BindableReactiveProperty<double> Longitude { get; } = new();

    public BindableReactiveProperty<string?> DisplayName { get; } = new("Survey mission");
    public BindableReactiveProperty<IHeadlinedViewModel?> OperationProfile { get; } = new();
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

    public PropertyTextBoxViewModel DisplayNameProperty { get; }
    public PropertyComboBoxViewModel OperationProfileProperty { get; }
    public PropertyButtonViewModel ActionButtonProperty { get; }
    public PropertyUnitViewModel AltitudeUnitProperty { get; }
    public PropertyUnitViewModel ThrottleUnitProperty { get; }
    public PropertyEditorViewModel PropertyEditor { get; }
    public PropertyEditorViewModel PropertyEditorCopy { get; }
}
