using Asv.Common;
using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia;

public class PropertyGeoPointDesign : PropertyGeoPointViewModel
{
    public PropertyGeoPointDesign()
        : base(NavId.GenerateRandomAsString(), DesignTime.UnitService)
    {
        DesignTime.ThrowIfNotDesignMode();
        Header = RS.GeoPointPropertyViewModel_Title;
        Description = RS.GeoPointPropertyViewModel_Description;
        Icon = MaterialIconKind.Earth;
        ApplyValueFromModel(new GeoPoint(56.8389, 60.6057, 250));
    }

    protected override ValueTask ApplyFromUser(GeoPoint value, CancellationToken cancel)
    {
        return ValueTask.CompletedTask;
    }
}
