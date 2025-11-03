using System;
using System.Collections.Generic;
using System.Composition;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportControlExamples(PageId)]
public class PropertyEditorPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "property-editor-example";
    public const MaterialIconKind PageIcon = MaterialIconKind.PropertyTag;

    public PropertyEditorPageViewModel()
        : this(DesignTime.UnitService, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        Parent = DesignTime.Shell;
    }

    [ImportingConstructor]
    public PropertyEditorPageViewModel(IUnitService unit, ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
    {
        PropertyEditor = new PropertyEditorViewModel("editor", loggerFactory)
        {
            Parent = this,
            ItemsSource =
            {
                new UnitPropertyViewModel(
                    "lat",
                    Latitude,
                    unit[LatitudeBase.Id] ?? throw new ArgumentNullException(),
                    loggerFactory
                )
                {
                    Parent = this,
                    Header = "Position",
                    ShortName = "Lat",
                    Description = "Latitude description",
                    Icon = MaterialIconKind.Latitude,
                },
                new UnitPropertyViewModel(
                    "lon",
                    Longitude,
                    unit[LongitudeBase.Id] ?? throw new ArgumentNullException(),
                    loggerFactory
                )
                {
                    Parent = this,
                    Header = "Longitude",
                    ShortName = "Lon",
                    Description = "Latitude description",
                    Icon = MaterialIconKind.Latitude,
                },
                new UnitPropertyViewModel(
                    "alt",
                    Altitude,
                    unit[AltitudeBase.Id] ?? throw new ArgumentNullException(),
                    loggerFactory
                )
                {
                    Parent = this,
                    Header = "Altitude",
                    ShortName = "Alt",
                    Description = "Altitude description",
                    Icon = MaterialIconKind.Altimeter,
                },
                new GeoPointPropertyViewModel("geo", GeoPoint, loggerFactory, unit)
                {
                    Parent = this,
                    Header = "Geo Point",
                    Description = "Geo Point description",
                    Icon = MaterialIconKind.Earth,
                },
            },
        };

        GeoPoint.Subscribe(x =>
        {
            Latitude.Value = x.Latitude;
            Longitude.Value = x.Longitude;
            Altitude.Value = x.Altitude;
        });
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return PropertyEditor;
        foreach (var item in base.GetRoutableChildren())
        {
            yield return item;
        }
    }

    public BindableReactiveProperty<double> Altitude { get; } = new();
    public BindableReactiveProperty<double> Latitude { get; } = new();
    public BindableReactiveProperty<double> Longitude { get; } = new();

    public BindableReactiveProperty<GeoPoint> GeoPoint { get; } = new();

    public PropertyEditorViewModel PropertyEditor { get; }

    public override IExportInfo Source => SystemModule.Instance;
}
