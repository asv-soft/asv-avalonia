using Asv.Common;
using NuGet.Packaging.Core;

namespace Asv.Avalonia.Plugins;

public sealed class NullPluginSearchInfo : IPluginSearchInfo
{
    public static IPluginSearchInfo Instance { get; } = new NullPluginSearchInfo();

    private NullPluginSearchInfo() { }

    public SemVersion ApiVersion => new(new Version());
    public string PackageId => NavigationId.Empty.ToString();
    public string? Title => "Null Plugin info";
    public string? Description => "Null plugin info for design time";
    public string? Authors => "https://github.com/asv-soft";
    public string? Tags => "Tag3 Tag4";
    public bool IsVerified => true;
    public IPluginServerInfo Source => NullPluginServerInfo.Instance;
    public IEnumerable<PackageDependency> Dependencies => [];
    public string LastVersion => "1.0.0";
    public long? DownloadCount => 99;
}
