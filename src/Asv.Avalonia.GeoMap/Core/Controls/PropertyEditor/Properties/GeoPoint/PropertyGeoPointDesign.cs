using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using AvaloniaRS = Asv.Avalonia.RS;

namespace Asv.Avalonia.GeoMap;

public class PropertyGeoPointDesign : PropertyGeoPointViewModel
{
    public PropertyGeoPointDesign()
        : base(NavId.GenerateRandomAsString(), DesignTime.UnitService, DesignTime.DialogService)
    {
        DesignTime.ThrowIfNotDesignMode();
        Header = AvaloniaRS.GeoPointPropertyViewModel_Title;
        Description = AvaloniaRS.GeoPointPropertyViewModel_Description;
        Icon = MaterialIconKind.Earth;
        ApplyValueFromModel(new GeoPoint(56.8389, 60.6057, 250));
    }

    protected override ValueTask ApplyFromUser(GeoPoint value, CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }
}
