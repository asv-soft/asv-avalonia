﻿using Asv.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;

namespace Asv.Avalonia.Plugins;

internal class PluginSearchInfo : IPluginSearchInfo
{
    public PluginSearchInfo(
        IPackageSearchMetadata packageSearchMetadata,
        SourceRepository repository,
        SourcePackageDependencyInfo dependencyInfo,
        string apiPackageName
    )
    {
        Authors = packageSearchMetadata.Authors;
        Title = packageSearchMetadata.Identity.Id;
        LastVersion = packageSearchMetadata.Identity.Version.ToString();
        Source = new SourceInfo(repository);
        PackageId = packageSearchMetadata.Identity.Id;
        Description = packageSearchMetadata.Description;
        Tags = packageSearchMetadata.Tags;
        DownloadCount = packageSearchMetadata.DownloadCount;
        Dependencies = dependencyInfo.Dependencies;

        var apiPackage = dependencyInfo.Dependencies.FirstOrDefault(x => x.Id == apiPackageName);
        if (apiPackage == null)
        {
            throw new Exception(
                $"Plugin {packageSearchMetadata.Identity.Id} does not contain API package as dependency"
            );
        }

        ApiVersion =
            apiPackage.VersionRange.MinVersion?.ToNormalizedString()
            ?? throw new InvalidOperationException("Api version not found in plugin dependencies");
        IsVerified =
            Authors.Contains("https://github.com/asv-soft")
            && Source.SourceUri.Contains("https://nuget.pkg.github.com/asv-soft/index.json");
    }

    public bool IsVerified { get; set; }
    public IPluginServerInfo Source { get; }
    public SemVersion ApiVersion { get; }
    public string PackageId { get; }
    public string? Title { get; }
    public string? Authors { get; }
    public string LastVersion { get; }
    public string Description { get; }
    public long? DownloadCount { get; }
    public string? Tags { get; }
    public IEnumerable<PackageDependency> Dependencies { get; set; }
}
