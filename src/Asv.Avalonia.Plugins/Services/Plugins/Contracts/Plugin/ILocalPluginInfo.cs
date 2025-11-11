using Avalonia.Media.Imaging;

namespace Asv.Avalonia.Plugins;

public interface ILocalPluginInfo : IPluginSpecification
{
    string Id => $"{SourceUri}|{PackageId}";
    string SourceUri { get; }
    string LocalFolder { get; }
    string Version { get; }
    bool IsUninstalled { get; }
    bool IsLoaded { get; }
    string LoadingError { get; }
    Bitmap? Icon { get; }
}
