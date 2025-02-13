using Asv.Common;
using Avalonia.Media.Imaging;
using NuGet.Packaging;

namespace Asv.Avalonia;

public class LocalPluginInfo : ILocalPluginInfo
{
    public LocalPluginInfo(PackageArchiveReader reader, string pluginFolder, PluginState state)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var nuspec = reader.NuspecReader;
        Id = nuspec.GetId();
        Title = nuspec.GetTitle();
        Version = nuspec.GetVersion().ToFullString();
        LocalFolder = pluginFolder;
        PackageId = nuspec.GetId();
        Description = nuspec.GetDescription();
        Authors = nuspec.GetAuthors();
        Tags = nuspec.GetTags();
        SourceUri = state.InstalledFromSourceUri;
        IsUninstalled = state.IsUninstalled;
        IsLoaded = state.IsLoaded;
        LoadingError = state.LoadingError;
    }

    public string? SourceUri { get; }
    public string PackageId { get; }
    public string LocalFolder { get; }
    public string Title { get; }
    public string? Description { get; }
    public string? Authors { get; }
    public string? Tags { get; }
    public bool IsVerified { get; }
    public string Id { get; }
    public string Version { get; }
    public SemVersion ApiVersion { get; }
    public bool IsUninstalled { get; }
    public bool IsLoaded { get; }
    public string? LoadingError { get; }
    public Bitmap? Icon { get; }
}
