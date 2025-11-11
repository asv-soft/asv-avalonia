using NuGet.Packaging.Core;

namespace Asv.Avalonia.Plugins;

public interface IPluginSearchInfo : IPluginSpecification
{
    string Id => $"{Source.SourceUri}|{PackageId}";
    IPluginServerInfo Source { get; }
    IEnumerable<PackageDependency> Dependencies { get; }
    string LastVersion { get; }
    long? DownloadCount { get; }
}
