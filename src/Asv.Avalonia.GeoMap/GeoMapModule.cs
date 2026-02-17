using Avalonia.Controls;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.GeoMap;

public sealed class GeoMapModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.GeoMap";
    public static IExportInfo Instance { get; } = new GeoMapModule();

    private GeoMapModule() { }

    public string ModuleName => Name;
}
