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
        DisplayNameProperty
            .Text.EnableValidation(value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new ValidationException("Display name is required");
                }

                return null;
            })
            .AddTo(ref DisposableBag);
        DisplayNameProperty.Text.ForceValidate();
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
            1250
        );
        ThrottleUnitProperty = CreateUnitProperty(
            "throttle_unit_v2",
            unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
            "Throttle V2",
            "Thr",
            "V2 unit property using the throttle unit selector.",
            MaterialIconKind.Signal,
            AsvColorKind.Success,
            65
        );
        PropertyEditor = new PropertyEditorViewModel("editor")
        {
            ItemsSource =
            {
                DisplayNameProperty,
                OperationProfileProperty,
                ActionButtonProperty,
                AltitudeUnitProperty,
                ThrottleUnitProperty,
                new PropertyUnitReactive(
                    "lat",
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
                    "lon",
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
                    "alt",
                    unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
                    Altitude
                )
                {
                    Header = "Altitude",
                    ShortHeader = "Alt",
                    Description = "Altitude description",
                    Icon = MaterialIconKind.Altimeter,
                },
                new PropertyGeoPointReactive("geo_v2", GeoPoint, unit)
                {
                    Header = "Geo Point",
                    Description = "Geo Point description",
                    Icon = MaterialIconKind.Earth,
                },
                new PropertyUnitReactive(
                    "time",
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
                    "throttle",
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
        }.SetRoutableParent(this);

        GeoPoint.Subscribe(x =>
        {
            Latitude.Value = x.Latitude;
            Longitude.Value = x.Longitude;
            Altitude.Value = x.Altitude;
        });
    }

    private static PropertyTextBoxViewModel CreateDisplayNameProperty()
    {
        var property = new PropertyTextBoxReactive(
            "display_name",
            new ReactiveProperty<string?>("Survey mission")
        )
        {
            Header = "Display name",
            ShortHeader = "Name",
            Description =
                "Text editor with validation, icon, remote update marker, and menu button.",
            Icon = MaterialIconKind.FormTextbox,
            IconColor = AsvColorKind.Info5,
        };

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
        double value
    )
    {
        var property = new PropertyUnitReactive(id, unit, new ReactiveProperty<double>(value))
        {
            Header = header,
            ShortHeader = shortName,
            Description = description,
            Icon = icon,
            IconColor = iconColor,
        };

        return property;
    }

    private static PropertyComboBoxViewModel CreateOperationProfileProperty()
    {
        var selectedProfile = new ReactiveProperty<IHeadlinedViewModel?>();
        var property = new PropertyComboBoxReactive("operation_profile", selectedProfile)
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
        selectedProfile.Value = firstItem;

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
        foreach (var item in base.GetChildren())
        {
            yield return item;
        }
    }

    public BindableReactiveProperty<double> Altitude { get; } = new();
    public BindableReactiveProperty<double> Latitude { get; } = new();
    public BindableReactiveProperty<double> Longitude { get; } = new();

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
}
