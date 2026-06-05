using System;
using System.Collections.Generic;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public class PropertyEditorPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "property-editor-example";
    public const MaterialIconKind PageIcon = MaterialIconKind.PropertyTag;

    public PropertyEditorPageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance,
            DesignTime.UnitService,
            DesignTime.LoggerFactory,
            DesignTime.DialogService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        SetParent(DesignTime.Shell);
    }

    public PropertyEditorPageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        IUnitService unit,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(PageId, context)
    {
        DisplayNameProperty = CreateDisplayNameProperty();
        OperationProfileProperty = CreateOperationProfileProperty();
        ActionButtonProperty = CreateActionButtonProperty();
        AltitudeUnitProperty = CreateUnitProperty(
            "altitude-unit",
            unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
            "Altitude V2",
            "Alt",
            "V2 unit property with a text value and unit selector.",
            MaterialIconKind.Altimeter,
            AsvColorKind.Info3,
            AltitudeUnitValue
        );
        ThrottleUnitProperty = CreateUnitProperty(
            "throttle-unit",
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
                ThrottleUnitProperty,
                dialogService
            )
            .SetRoutableParent(this);
        PropertyEditorCopy = CreatePropertyEditor(
                "editor-copy",
                unit,
                CreateDisplayNameProperty(),
                CreateOperationProfileProperty(),
                CreateActionButtonProperty(),
                CreateUnitProperty(
                    "altitude-unit",
                    unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
                    "Altitude V2",
                    "Alt",
                    "V2 unit property with a text value and unit selector.",
                    MaterialIconKind.Altimeter,
                    AsvColorKind.Info3,
                    AltitudeUnitValue
                ),
                CreateUnitProperty(
                    "throttle-unit",
                    unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
                    "Throttle V2",
                    "Thr",
                    "V2 unit property using the throttle unit selector.",
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
                CreateActionButtonProperty(),
                CreateUnitProperty(
                    "altitude-unit",
                    unit[AltitudeUnit.Id] ?? throw new ArgumentNullException(),
                    "Altitude V2",
                    "Alt",
                    "V2 unit property with a text value and unit selector.",
                    MaterialIconKind.Altimeter,
                    AsvColorKind.Info3,
                    AltitudeUnitValue
                ),
                CreateUnitProperty(
                    "throttle-unit",
                    unit[ThrottleUnit.Id] ?? throw new ArgumentNullException(),
                    "Throttle V2",
                    "Thr",
                    "V2 unit property using the throttle unit selector.",
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
        PropertyButtonViewModel actionButtonProperty,
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
            actionButtonProperty,
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
        PropertyButtonViewModel actionButtonProperty,
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
            actionButtonProperty,
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
        PropertyButtonViewModel actionButtonProperty,
        PropertyUnitViewModel altitudeUnitProperty,
        PropertyUnitViewModel throttleUnitProperty,
        IDialogService dialogService
    )
        where TEditor : PropertyEditorViewModel
    {
        editor.ItemsSource.Add(displayNameProperty);
        editor.ItemsSource.Add(operationProfileProperty);
        editor.ItemsSource.Add(actionButtonProperty);
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
                    Header = "Position",
                    ShortHeader = "Lat",
                    Description = "Latitude description",
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
                    Header = "Longitude",
                    ShortHeader = "Lon",
                    Description = "Latitude description",
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
                    Header = "Altitude",
                    ShortHeader = "Alt",
                    Description = "Altitude description",
                    Icon = MaterialIconKind.Altimeter,
                }
            )
        );
        editor.ItemsSource.Add(
            AddExampleErrorMenu(
                new PropertyGeoPointReactive("geo-point", GeoPoint, unit, dialogService)
                {
                    Header = "Geo Point",
                    Description = "Geo Point description",
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
                    Header = "Time",
                    ShortHeader = "Time",
                    Description = "Time description",
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
                    Header = "Throttle",
                    ShortHeader = "Throttle",
                    Description = "Throttle description",
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

        return AddExampleErrorMenu(property);
    }

    private PropertyButtonViewModel CreateActionButtonProperty()
    {
        return AddExampleErrorMenu(
            new PropertyButtonViewModel("run-check", ExecuteActionButton)
            {
                Header = "Run check",
                ShortHeader = "Run",
                Description = "Button property with async command, busy state, and update marker.",
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
            Header = "Operation profile",
            Description =
                "Combo box with test view models: icon, text-only, and different accent styles.",
            Icon = MaterialIconKind.FormDropdown,
            IconColor = AsvColorKind.Info7,
        };

        var firstItem = AddOperationProfileItem(
            property,
            "manual",
            "Manual",
            "Operator controls every step directly.",
            MaterialIconKind.Hand,
            AsvColorKind.Info1
        );
        AddOperationProfileItem(
            property,
            "guided",
            "Guided",
            "Assisted flow with validation after each step.",
            MaterialIconKind.Compass,
            AsvColorKind.Info5
        );
        AddOperationProfileItem(
            property,
            "survey",
            "Survey",
            "Collects structured measurements for later analysis.",
            MaterialIconKind.MapMarkerRadius,
            AsvColorKind.Success
        );
        AddOperationProfileItem(
            property,
            "silent",
            "Silent",
            "Runs without icon or accent decoration.",
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "inspection",
            "Inspection",
            "Highlights issues and requires operator confirmation.",
            MaterialIconKind.MagnifyScan,
            AsvColorKind.Warning
        );
        AddOperationProfileItem(
            property,
            "minimal",
            "Minimal",
            "Text-only item for compact layouts.",
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "diagnostics",
            "Diagnostics",
            "Shows service information and hardware status.",
            MaterialIconKind.Stethoscope,
            AsvColorKind.Info12
        );
        AddOperationProfileItem(
            property,
            "offline-cache",
            "Offline cache",
            "No image, useful when external data is unavailable.",
            null,
            AsvColorKind.None
        );
        AddOperationProfileItem(
            property,
            "emergency",
            "Emergency",
            "Critical action style with a warning-colored icon.",
            MaterialIconKind.AlertOctagon,
            AsvColorKind.Error
        );
        AddOperationProfileItem(
            property,
            "custom-profile",
            "Custom profile",
            "Plain item with only header and description.",
            null,
            AsvColorKind.None
        );
        OperationProfile.Value ??= firstItem;

        return AddExampleErrorMenu(property);
    }

    private static TProperty AddExampleErrorMenu<TProperty>(TProperty property)
        where TProperty : PropertyViewModel
    {
        property.Menu.Add(
            CreateSetErrorMenuItem(
                property,
                "set-validation-error",
                "Validation error",
                "Validation error from property menu.",
                MaterialIconKind.AlertCircle,
                0
            )
        );
        property.Menu.Add(
            CreateSetErrorMenuItem(
                property,
                "set-sync-error",
                "Sync error",
                "Synchronization error from property menu.",
                MaterialIconKind.SyncAlert,
                1
            )
        );
        property.Menu.Add(
            CreateSetErrorMenuItem(
                property,
                "set-network-error",
                "Network error",
                "Network error from property menu.",
                MaterialIconKind.CloseNetwork,
                2
            )
        );
        property.Menu.Add(
            new MenuItem("clear-error", "Clear error")
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
        yield return ExtendedPropertyEditor;
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

    public PropertyTextBoxViewModel DisplayNameProperty { get; }
    public PropertyComboBoxViewModel OperationProfileProperty { get; }
    public PropertyButtonViewModel ActionButtonProperty { get; }
    public PropertyUnitViewModel AltitudeUnitProperty { get; }
    public PropertyUnitViewModel ThrottleUnitProperty { get; }
    public PropertyEditorViewModel PropertyEditor { get; }
    public PropertyEditorViewModel PropertyEditorCopy { get; }
    public ExtendedPropertyEditorViewModel ExtendedPropertyEditor { get; }
}
