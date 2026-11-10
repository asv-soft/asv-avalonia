namespace Asv.Avalonia.GeoMap;

internal static class MapInputStrings
{
    public static string Get(string name) =>
        RS.ResourceManager.GetString($"MapInput_{name}", RS.Culture)!;

    public static string ActionName(MapInputAction action) => Get($"{action}_Header");
}
