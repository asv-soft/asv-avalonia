using System.IO.Compression;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;

namespace Asv.Avalonia;

public static class NugetHelper
{
    public static NuGetFramework DefaultFramework = NuGetFramework.ParseFolder("net8.0");

    public static async Task<IReadOnlyList<SourcePackageDependencyInfo>> ListAllDependencies(
        IEnumerable<SourceRepository> repositories,
        SourceRepository repository,
        SourceCacheContext cache,
        PackageIdentity packageId,
        NuGetFramework framework,
        ILogger logger,
        CancellationToken cancel
    )
    {
        var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(
            cancel
        );
        var dependencyInfo = await dependencyInfoResource.ResolvePackage(
            packageId,
            framework,
            cache,
            logger,
            cancel
        );
        var packages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
        await ListAllPackageDependencies(
            dependencyInfo,
            repositories,
            DefaultFramework,
            cache,
            logger,
            packages,
            cancel
        );
        var targets = new[] { packageId };
        var resolver = new PackageResolver();
        var context = new PackageResolverContext(
            DependencyBehavior.Lowest,
            targets.Select(p => p.Id),
            [],
            [],
            targets,
            packages,
            [],
            logger
        );
        var result = new HashSet<PackageIdentity>(
            resolver.Resolve(context, cancel),
            PackageIdentityComparer.Default
        );
        return packages.Where(package => result.Contains(package)).ToList();
    }

    private static async Task ListAllPackageDependencies(
        SourcePackageDependencyInfo package,
        IEnumerable<SourceRepository> repositories,
        NuGetFramework framework,
        SourceCacheContext cache,
        ILogger logger,
        ISet<SourcePackageDependencyInfo> dependencies,
        CancellationToken cancellationToken
    )
    {
        if (dependencies.Contains(package))
        {
            return;
        }

        var sourceRepositories = repositories as SourceRepository[] ?? repositories.ToArray();
        foreach (var repository in sourceRepositories)
        {
            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(
                cancellationToken
            );
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                package,
                framework,
                cache,
                logger,
                cancellationToken
            );

            if (dependencyInfo == null)
            {
                continue;
            }

            if (!dependencies.Add(dependencyInfo))
            {
                continue;
            }

            foreach (var dependency in dependencyInfo.Dependencies)
            {
                await ListAllPackageDependencies(
                    new SourcePackageDependencyInfo(
                        dependency.Id,
                        dependency.VersionRange.MinVersion,
                        [],
                        true,
                        repository
                    ),
                    sourceRepositories,
                    framework,
                    cache,
                    logger,
                    dependencies,
                    cancellationToken
                );
            }
        }
    }

    public static async Task<string> DownloadPackage(
        PackageIdentity identity,
        SourceRepository repository,
        SourceCacheContext cache,
        string nugetPluginFolder,
        ILogger logger,
        CancellationToken cancel
    )
    {
        var findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>(
            cancel
        );
        var packageFolder = Path.Combine(nugetPluginFolder, $"{identity.Id}.{identity.Version}");
        if (Directory.Exists(packageFolder))
        {
            return packageFolder;
        }

        var packageFile = Path.Combine(
            nugetPluginFolder,
            $"{identity.Id}.{identity.Version}.nupkg"
        );

        if (!File.Exists(packageFile))
        {
            await using var file = File.OpenWrite(packageFile);
            await findPackageByIdResource.CopyNupkgToStreamAsync(
                identity.Id,
                identity.Version,
                file,
                cache,
                logger,
                cancel
            );
        }

        ZipFile.ExtractToDirectory(File.OpenRead(packageFile), packageFolder);

        return packageFolder;
    }

    public static void CopyNugetFiles(string pluginFolder, string nugetFolder)
    {
        using var packageFolderReader = new PackageFolderReader(nugetFolder);
        var libs = packageFolderReader.GetLibItems();
        var platform = libs.Where(_ => _.TargetFramework.Platform == ".NETCoreApp")
            .MaxBy(_ => _.TargetFramework.Version);
        if (platform == null)
        {
            return;
        }

        foreach (var file in platform.Items)
        {
            File.Copy(
                Path.Combine(nugetFolder, file),
                Path.Combine(pluginFolder, Path.GetFileName(file)),
                true
            );
        }
    }

    public static FrameworkSpecificGroup? GetPlatform(
        PackageReaderBase dependencyPackageArchiveReader
    )
    {
        return dependencyPackageArchiveReader
            .GetLibItems()
            .Where(_ => _.TargetFramework.Framework == DefaultFramework.Framework)
            .MaxBy(_ => _.TargetFramework.Version);
    }
}
