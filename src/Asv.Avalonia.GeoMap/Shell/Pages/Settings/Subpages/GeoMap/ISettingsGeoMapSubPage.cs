namespace Asv.Avalonia.GeoMap;

public interface ISettingsGeoMapSubPage : ISettingsSubPage
{
    public MapProviderProperty MapProvider { get; }
    public MapModeProperty MapMode { get; }
}
