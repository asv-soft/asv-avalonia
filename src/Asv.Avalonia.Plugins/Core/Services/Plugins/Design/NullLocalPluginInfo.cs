using Asv.Common;
using Avalonia.Media.Imaging;

namespace Asv.Avalonia.Plugins;

public sealed class NullLocalPluginInfo : ILocalPluginInfo
{
    public static ILocalPluginInfo Instance { get; } = new NullLocalPluginInfo();

    private NullLocalPluginInfo() { }

    public SemVersion ApiVersion => new(new Version());
    public string PackageId => NavigationId.Empty.ToString();
    public string? Title => "Null Plugin info";
    public string? Description => "Null plugin info for design time";
    public string? Authors => "Asv.Soft";
    public string? Tags => "Tag1 Tag2";
    public bool IsVerified => false;
    public string SourceUri => "uri.uri";
    public string LocalFolder => "folder/folder";
    public string Version => "1.0.0";
    public bool IsUninstalled => false;
    public bool IsLoaded => false;
    public string LoadingError => "NoError";
    public Bitmap? Icon => null;
}
