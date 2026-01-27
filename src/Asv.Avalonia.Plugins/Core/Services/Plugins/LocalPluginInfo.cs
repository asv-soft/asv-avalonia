using Asv.Common;
using Avalonia.Media.Imaging;
using NuGet.Packaging;

namespace Asv.Avalonia.Plugins;

public class LocalPluginInfo : ILocalPluginInfo
{
    public LocalPluginInfo(
        PackageArchiveReader reader,
        string pluginFolder,
        PluginState state,
        string apiPackageId
    )
    {
        ArgumentNullException.ThrowIfNull(reader);
        var nuspec = reader.NuspecReader;
        Id = nuspec.GetId();

        // TODO: add IsVerified implementation
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
        var apiPackage = nuspec
            .GetDependencyGroups()
            .SelectMany(x => x.Packages)
            .FirstOrDefault(x => x.Id == apiPackageId);
        if (apiPackage != null && apiPackage.VersionRange.MinVersion != null)
        {
            ApiVersion = SemVersion.Parse(apiPackage.VersionRange.MinVersion.OriginalVersion);
        }
        else
        {
            throw new Exception($"API package {apiPackageId} not found in {Id}");
        }
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
