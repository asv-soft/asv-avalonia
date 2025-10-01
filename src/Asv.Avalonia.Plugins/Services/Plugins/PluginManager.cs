using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Loader;
using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using ZLogger;

namespace Asv.Avalonia.Plugins;

public class PluginManager : IPluginManager
{
    private readonly IList<Assembly> _assemblies;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<PluginManager> _logger;
    private readonly LoggerAdapter _nugetLogger;
    private readonly ReaderWriterLockSlim _repositoriesLock = new();
    private readonly List<SourceRepository> _repositories = [];
    private readonly PluginManagerConfig _localConfig;
    private readonly IConfiguration _userConfig;

    private const string Salt = "Asv.Avalonia";
    private const string PluginStateFileName = "__PLUGIN_STATE__";

    private readonly string _sharedPluginFolder;
    private readonly string _nugetFolder;
    private readonly string _apiPackageId;
    private readonly SourceCacheContext _cache;
    private readonly List<AssemblyLoadContext> _pluginContexts = [];

    public PluginManager(
        IOptions<PluginManagerOptions> options,
        IConfiguration userConfig,
        ILoggerFactory loggerFactory
    )
    {
        _assemblies = new List<Assembly>();
        _apiPackageId = options.Value.ApiPackageId;

        _nugetFolder = options.Value.NugetDirectory;

        _loggerFactory = loggerFactory;
        ApiVersion = SemVersion.Parse(options.Value.ApiVersion);

        _logger = loggerFactory.CreateLogger<PluginManager>();
        _nugetLogger = new LoggerAdapter(_logger);

        _userConfig = userConfig;
        _localConfig = userConfig.Get<PluginManagerConfig>();

        #region Servers

        var servers = _localConfig.Servers;
        var needToSave = false;
        if (servers == null)
        {
            // create default
            servers = options
                .Value.DefaultServers.Select(x => new PluginServerConfig
                {
                    Name = x.Name,
                    SourceUri = x.SourceUri,
                    PasswordHash = x.Password?.EncryptAes(options.Value.Salt),
                    Username = x.Username,
                })
                .ToArray();
            needToSave = true;
        }

        // try find clear text passwords and replace it
        foreach (var server in servers)
        {
            if (
                string.IsNullOrWhiteSpace(server.Password)
                && string.IsNullOrWhiteSpace(server.PasswordHash)
            )
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(server.Password))
            {
                continue;
            }

            _logger.LogWarning($"Replace clear text password for server {server.Name}");
            server.PasswordHash = server.Password.EncryptAes(Salt);
            server.Password = null;
            needToSave = true;
        }

        if (needToSave)
        {
            _localConfig.Servers = servers;
            userConfig.Set(_localConfig);
        }

        foreach (var server in servers)
        {
            if (
                !InternalAddServer(
                    new PluginServer(
                        server.Name,
                        server.SourceUri,
                        server.Username,
                        server.PasswordHash?.DecryptAes(Salt)
                    ),
                    false,
                    false
                )
            )
            {
                _logger.LogWarning($"Error add plugin source server {server.Name}");
            }
        }

        #endregion

        #region Folder creation

        _sharedPluginFolder = options.Value.PluginDirectory;
        if (Directory.Exists(_sharedPluginFolder) == false)
        {
            _logger.LogInformation($"Create plugin folder {_sharedPluginFolder}");
            Directory.CreateDirectory(_sharedPluginFolder);
        }
        else
        {
            _logger.LogInformation($"Found plugin folder {_sharedPluginFolder}");
        }

        if (!Directory.Exists(_nugetFolder))
        {
            _logger.LogInformation($"Create nuget folder {_nugetFolder}");
            Directory.CreateDirectory(_nugetFolder);
        }
        else
        {
            _logger.LogInformation($"Found nuget cache folder {_nugetFolder}");
        }

        var nugetCache = options.Value.NugetCacheDirectory;
        if (!Directory.Exists(nugetCache))
        {
            _logger.LogInformation($"Create nuget folder {nugetCache}");
            Directory.CreateDirectory(nugetCache);
        }
        else
        {
            _logger.LogInformation($"Found nuget cache folder {nugetCache}");
        }

        #endregion

        _cache = new SourceCacheContext
        {
            GeneratedTempFolder = "nuget_cache",
            SessionId = Guid.Empty,
            DirectDownload = true,
            NoCache = true,
            MaxAge = DateTimeOffset.MaxValue,
        };

        // load all plugins
        foreach (
            var dir in Directory.EnumerateDirectories(
                _sharedPluginFolder,
                "*",
                SearchOption.TopDirectoryOnly
            )
        )
        {
            if (!TryGetLocalPluginInfoByFolder(dir, out var info))
            {
                _logger.LogWarning($"Error read plugin info from {dir}. Delete it");
                Directory.Delete(dir, true);
                continue;
            }

            if (info == null)
            {
                continue;
            }

            if (info.IsUninstalled)
            {
                _logger.LogInformation(
                    $"Plugin {info.PackageId} is marked as uninstalled. Delete it"
                );
                Directory.Delete(info.LocalFolder, true);
                continue;
            }

            // check API version
            if (info.ApiVersion.CompareByPrecedence(ApiVersion) != 0)
            {
                _logger.LogWarning(
                    $"Plugin {info.PackageId} {info.Version} has different API version {info.ApiVersion} than application {ApiVersion}"
                );
                SetPluginStateByFolder(
                    info.LocalFolder,
                    x =>
                    {
                        x.IsLoaded = false;
                        x.LoadingError =
                            $"Plugin has different API version {info.ApiVersion} than application {ApiVersion}";
                    }
                );
                continue;
            }

            try
            {
                _logger.LogInformation(
                    $"Load plugin {info.PackageId} {info.Version} {info.LocalFolder}"
                );
                _pluginContexts.Add(
                    new PluginAssemblyLoadContext(
                        info.LocalFolder,
                        options.Value.NugetPluginPrefix,
                        _assemblies,
                        _loggerFactory
                    )
                );
                SetPluginStateByFolder(
                    info.LocalFolder,
                    x =>
                    {
                        x.IsLoaded = true;
                        x.LoadingError = null;
                    }
                );
            }
            catch (Exception e)
            {
                _logger.LogError(
                    e,
                    $"Error load plugin {info.PackageId} {info.Version} {info.LocalFolder}"
                );
                SetPluginStateByFolder(
                    info.LocalFolder,
                    x =>
                    {
                        x.IsLoaded = false;
                        x.LoadingError = e.Message;
                    }
                );
            }
        }

        PluginsAssemblies = _assemblies.ToImmutableList();
    }

    public SemVersion ApiVersion { get; }

    public IReadOnlyList<Assembly> PluginsAssemblies { get; }

    #region Servers

    public IReadOnlyList<IPluginServerInfo> Servers
    {
        get
        {
            try
            {
                _repositoriesLock.EnterReadLock();
                return _repositories.Select(x => new SourceInfo(x)).ToImmutableList();
            }
            finally
            {
                _repositoriesLock.ExitReadLock();
            }
        }
    }

    public void AddServer(PluginServer server)
    {
        InternalAddServer(server, true, true);
    }

    private bool InternalAddServer(PluginServer server, bool throwExceptions, bool saveToConfig)
    {
        try
        {
            _repositoriesLock.EnterWriteLock();

            if (string.IsNullOrWhiteSpace(server.Name))
            {
                ArgumentNullException.ThrowIfNull(server.Name, nameof(server.Name));
            }

            if (string.IsNullOrWhiteSpace(server.SourceUri))
            {
                ArgumentNullException.ThrowIfNull(server.SourceUri, nameof(server.SourceUri));
            }

            if (_repositories.Any(_ => _.PackageSource.Source == server.SourceUri))
            {
                _logger.LogWarning($"Server source {server.SourceUri} already exists");
                return false;
            }

            SourceRepository repo;
            if (string.IsNullOrWhiteSpace(server.Username))
            {
                repo = Repository.Factory.GetCoreV3(
                    new PackageSource(server.SourceUri, server.Name)
                );
                _logger.LogInformation($"Add plugin server source {server.SourceUri}");
            }
            else
            {
                _logger.LogInformation(
                    $"Add plugin server source {server.SourceUri} with credentials"
                );
                repo = Repository.Factory.GetCoreV3(
                    new PackageSource(server.SourceUri, server.Name)
                    {
                        Credentials = new PackageSourceCredential(
                            server.SourceUri,
                            server.Username,
                            server.Password ?? string.Empty,
                            true,
                            null
                        ),
                    }
                );
            }

            _repositories.Add(repo);
            if (!saveToConfig)
            {
                return true;
            }

            _localConfig.Servers = _repositories
                .Select(x => new PluginServerConfig
                {
                    Name = x.PackageSource.Name,
                    SourceUri = x.PackageSource.Source,
                    PasswordHash = x.PackageSource.Credentials?.PasswordText.EncryptAes(Salt),
                    Username = x.PackageSource.Credentials?.Username,
                })
                .ToArray();
            _userConfig.Set(_localConfig);

            return true;
        }
        catch when (!throwExceptions) { }
        finally
        {
            _repositoriesLock.ExitWriteLock();
        }

        return false;
    }

    public void RemoveServer(IPluginServerInfo info)
    {
        try
        {
            _repositoriesLock.EnterWriteLock();
            var repository = _repositories.FirstOrDefault(_ =>
                _.PackageSource.Source == info.SourceUri
            );
            if (repository == null)
            {
                _logger.LogWarning($"Server source {info.SourceUri} not found");
                return;
            }

            _repositories.Remove(repository);
            _localConfig.Servers = _repositories
                .Select(x => new PluginServerConfig
                {
                    Name = x.PackageSource.Name,
                    SourceUri = x.PackageSource.Source,
                    PasswordHash = x.PackageSource.Credentials?.PasswordText.EncryptAes(Salt),
                    Username = x.PackageSource.Credentials?.Username,
                })
                .ToArray();
            _userConfig.Set(_localConfig);
        }
        finally
        {
            _repositoriesLock.ExitWriteLock();
        }
    }

    #endregion

    #region Plugin management

    public async Task<IReadOnlyList<IPluginSearchInfo>> Search(
        SearchQuery query,
        CancellationToken cancel
    )
    {
        ArgumentNullException.ThrowIfNull(query);
        _repositoriesLock.EnterReadLock();
        var repositories =
            query.Sources.Count == 0
                ? _repositories.ToArray()
                : _repositories
                    .Where(r => query.Sources.Contains(r.PackageSource.Source))
                    .ToArray();
        _repositoriesLock.ExitReadLock();

        var result = new List<IPluginSearchInfo>();

        foreach (var repository in repositories)
        {
            try
            {
                var resource = await repository.GetResourceAsync<PackageSearchResource>(cancel);
                var filter = new SearchFilter(query.IncludePrerelease);
                var packages = await resource.SearchAsync(
                    query.Name,
                    filter,
                    query.Skip,
                    query.Take,
                    _nugetLogger,
                    cancel
                );

                foreach (var package in packages)
                {
                    try
                    {
                        var dependencyInfoResource =
                            await repository.GetResourceAsync<DependencyInfoResource>(cancel);
                        var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                            package.Identity,
                            NugetHelper.DefaultFramework,
                            _cache,
                            _nugetLogger,
                            cancel
                        );
                        if (
                            !dependencyInfo.Dependencies.Any(x =>
                                string.Equals(
                                    x.Id,
                                    _apiPackageId,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            )
                        )
                        {
                            continue;
                        }

                        result.Add(
                            new PluginSearchInfo(package, repository, dependencyInfo, _apiPackageId)
                        );
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(
                            $"Error create plugin search info from {package.Identity.Id} {e.Message}"
                        );
                        continue;
                    }

                    if (result.Count >= query.Take)
                    {
                        break;
                    }
                }

                if (result.Count >= query.Take)
                {
                    break;
                }
            }
            catch (Exception e)
            {
                _logger.ZLogError(e, $"Error search in {repository.PackageSource.Source}");
            }
        }

        return result;
    }

    public async Task<IReadOnlyList<string>> ListPluginVersions(
        SearchQuery query,
        string pluginId,
        CancellationToken cancel
    )
    {
        _repositoriesLock.EnterReadLock();
        var repositories =
            query.Sources.Count == 0
                ? _repositories.ToArray()
                : _repositories
                    .Where(r => query.Sources.Contains(r.PackageSource.Source))
                    .ToArray();
        _repositoriesLock.ExitReadLock();

        var versions = new HashSet<NuGetVersion>(VersionComparer.VersionRelease);

        foreach (var repository in repositories)
        {
            try
            {
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancel);
                var all = await resource.GetAllVersionsAsync(
                    pluginId,
                    new SourceCacheContext(),
                    new LoggerAdapter(_logger),
                    cancel
                );

                foreach (var v in all)
                {
                    if (query.IncludePrerelease || !v.IsPrerelease)
                    {
                        versions.Add(v);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error search in {repository.PackageSource.Source}");
            }
        }

        return versions
            .OrderByDescending(v => v, VersionComparer.VersionRelease)
            .Select(v => v.ToFullString())
            .ToList();
    }

    public async Task Install(
        IPluginServerInfo source,
        string packageId,
        string version,
        IProgress<ProgressMessage>? progress,
        CancellationToken cancel
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(packageId);
        ArgumentNullException.ThrowIfNull(version);
        var downloadVersion = NuGetVersion.Parse(version);
        if (TryGetLocalPluginInfoById(packageId, out var info))
        {
            Debug.Assert(info != null, nameof(info) + " != null");
            var localVersion = NuGetVersion.Parse(info.Version);
            throw new Exception(
                $"Local version {localVersion} of {packageId} is exists. Remove it first."
            );
        }

        var currentPluginFolder = Path.Combine(_sharedPluginFolder, packageId);

        try
        {
            Directory.CreateDirectory(currentPluginFolder);
            var repository = _repositories.FirstOrDefault(_ =>
                _.PackageSource.Source == source.SourceUri
            );
            if (repository == null)
            {
                throw new Exception($"Source {source.SourceUri} not found");
            }

            var packageIdentity = new PackageIdentity(packageId, downloadVersion);

            var packageFile = Path.Combine(
                currentPluginFolder,
                $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg"
            );
            var findPackageByIdResource =
                await repository.GetResourceAsync<FindPackageByIdResource>(cancel);

            var tmpFile = Path.Combine(
                currentPluginFolder,
                $"{packageIdentity.Id}.{packageIdentity.Version}.nupkg.tmp"
            );

            _cache.DirectDownload = true;
            await using (
                var fs = new FileStream(
                    tmpFile,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true
                )
            )
            {
                var ok = await findPackageByIdResource.CopyNupkgToStreamAsync(
                    packageIdentity.Id,
                    packageIdentity.Version,
                    fs,
                    _cache,
                    _nugetLogger,
                    cancel
                );

                await fs.FlushAsync(cancel);

                if (!ok)
                {
                    TryDelete(tmpFile);

                    throw new Exception(
                        $"Failed to download package {packageIdentity.Id} {packageIdentity.Version} "
                            + $"from source {source.SourceUri}. CopyNupkgToStreamAsync returned false."
                    );
                }
            }
            try
            {
                using var validationReader = new PackageArchiveReader(tmpFile);

                // Attempt to read identity forces zip central directory read (quick integrity check)
                _ = await validationReader.GetIdentityAsync(cancel);
            }
            catch (Exception ex)
            {
                TryDelete(tmpFile);
                throw new Exception(
                    $"Downloaded package is invalid: {packageIdentity.Id} {packageIdentity.Version}. "
                        + $"Source: {source.SourceUri}. Details: {ex.Message}",
                    ex
                );
            }

            TryDelete(packageFile);
            File.Move(tmpFile, packageFile, overwrite: true);
            using var packageArchiveReader = new PackageArchiveReader(packageFile);
            var platform = NugetHelper.GetPlatform(packageArchiveReader);
            if (platform == null)
            {
                throw new Exception($"Not found {NugetHelper.NetCoreAppGroup} platform in package");
            }

            foreach (var file in platform.Items)
            {
                packageArchiveReader.ExtractFile(
                    file,
                    Path.Combine(currentPluginFolder, Path.GetFileName(file)),
                    _nugetLogger
                );
            }

            // now we need to load all dependencies
            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(
                cancel
            );
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                packageIdentity,
                NugetHelper.DefaultFramework,
                _cache,
                _nugetLogger,
                cancel
            );

            _repositoriesLock.EnterReadLock();
            var repositories = _repositories.ToArray();
            _repositoriesLock.ExitReadLock();

            var dependencies = new HashSet<SourcePackageDependencyInfo>(
                PackageIdentityComparer.Default
            );

            await ListAllPackageDependencies(dependencyInfo, repositories, dependencies, cancel);
            foreach (var identity in dependencies)
            {
                // if base application contains this package we don't need to download it
                // TODO: There is a potential problem with the version of NuGet packages in the base application
                if (NugetHelper.IncludedPackages.Contains(identity.Id))
                {
                    continue;
                }

                // if self package we don't need to download it
                if (identity.Equals(packageIdentity))
                {
                    continue;
                }

                var dependencyPackageFile = Path.Combine(
                    _nugetFolder,
                    $"{identity.Id}.{identity.Version}.nupkg"
                );

                // if we already have this package we don't need to download it
                if (!File.Exists(dependencyPackageFile))
                {
                    var dependencyFindPackageByIdResource =
                        await identity.Source.GetResourceAsync<FindPackageByIdResource>(cancel);
                    await using var file = File.OpenWrite(dependencyPackageFile);
                    await dependencyFindPackageByIdResource.CopyNupkgToStreamAsync(
                        identity.Id,
                        identity.Version,
                        file,
                        _cache,
                        _nugetLogger,
                        cancel
                    );
                }

                using var dependencyPackageArchiveReader = new PackageArchiveReader(
                    dependencyPackageFile
                );
                var dependencyPlatform = NugetHelper.GetPlatform(dependencyPackageArchiveReader);
                if (dependencyPlatform == null)
                {
                    _logger.LogWarning(
                        $"Not found  {NugetHelper.NetCoreAppGroup} platform in package {identity.Id}"
                    );
                    continue;
                }

                foreach (var file in dependencyPlatform.Items)
                {
                    dependencyPackageArchiveReader.ExtractFile(
                        file,
                        Path.Combine(currentPluginFolder, Path.GetFileName(file)),
                        _nugetLogger
                    );
                }
            }

            SetPluginStateById(
                packageId,
                x =>
                {
                    x.IsLoaded = false;
                    x.IsUninstalled = false;
                    x.LoadingError = null;
                    x.InstalledFromSourceUri = source.SourceUri;
                }
            );
        }
        catch (Exception) when (Directory.Exists(currentPluginFolder))
        {
            try
            {
                Directory.Delete(currentPluginFolder, true);
            }
            catch
            {
                // ignore
            }

            throw;
        }

        return;

        static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    public async Task InstallManually(
        string from,
        IProgress<ProgressMessage>? progress,
        CancellationToken cancel
    )
    {
        ArgumentNullException.ThrowIfNull(from);
        if (!File.Exists(from))
        {
            throw new FileNotFoundException($"File {from} not found");
        }

        await using var nugetFile = File.OpenRead(from);
        var packageReader = new PackageArchiveReader(nugetFile);
        var packageIdentity = await packageReader.GetIdentityAsync(cancel);

        if (TryGetLocalPluginInfoById(packageIdentity.Id, out var info))
        {
            Debug.Assert(info != null, nameof(info) + " != null");
            var localVersion = NuGetVersion.Parse(info.Version);
            throw new Exception(
                $"Local version {localVersion} of {packageIdentity.Id} exists. Remove it first."
            );
        }

        var currentPluginFolder = Path.Combine(_sharedPluginFolder, packageIdentity.Id);

        try
        {
            Directory.CreateDirectory(currentPluginFolder);

            var plugin = Path.Combine(currentPluginFolder, Path.GetFileName(from));
            if (!File.Exists(plugin))
            {
                File.Copy(from, plugin);
            }

            ExtractContentFolder(plugin, currentPluginFolder);

            var platform = NugetHelper.GetPlatform(packageReader);
            if (platform == null)
            {
                throw new Exception($"Not found {NugetHelper.NetCoreAppGroup} platform in package");
            }

            foreach (var file in platform.Items)
            {
                packageReader.ExtractFile(
                    file,
                    Path.Combine(currentPluginFolder, Path.GetFileName(file)),
                    _nugetLogger
                );
            }

            var nuspecReader = packageReader.NuspecReader;

            // now we need to load all dependencies
            var dependencyInfoResource = nuspecReader.GetDependencyGroups();

            _repositoriesLock.EnterReadLock();
            var repositories = _repositories.ToArray();
            _repositoriesLock.ExitReadLock();

            var dependencies = await ListAllPackageDependencies(
                dependencyInfoResource,
                repositories,
                cancel
            );
            foreach (var identity in dependencies)
            {
                // if base application contains this package we don't need to download it
                // TODO: There is a potential problem with the version of NuGet packages in the base application
                if (NugetHelper.IncludedPackages.Contains(identity.Id))
                {
                    continue;
                }

                // if self package we don't need to download it
                if (identity.Equals(packageIdentity))
                {
                    continue;
                }

                var dependencyPackageFile = Path.Combine(
                    _nugetFolder,
                    $"{identity.Id}.{identity.Version}.nupkg"
                );

                // if we already have this package we don't need to download it
                if (File.Exists(dependencyPackageFile) == false)
                {
                    var dependencyFindPackageByIdResource =
                        await identity.Source.GetResourceAsync<FindPackageByIdResource>(cancel);
                    await using var file = File.OpenWrite(dependencyPackageFile);
                    await dependencyFindPackageByIdResource.CopyNupkgToStreamAsync(
                        identity.Id,
                        identity.Version,
                        file,
                        _cache,
                        _nugetLogger,
                        cancel
                    );
                }

                using var dependencyPackageArchiveReader = new PackageArchiveReader(
                    dependencyPackageFile
                );
                var dependencyPlatform = NugetHelper.GetPlatform(dependencyPackageArchiveReader);
                if (dependencyPlatform == null)
                {
                    _logger.LogWarning(
                        $"Not found  {NugetHelper.NetCoreAppGroup} platform in package {identity.Id}"
                    );
                    continue;
                }

                foreach (var file in dependencyPlatform.Items)
                {
                    dependencyPackageArchiveReader.ExtractFile(
                        file,
                        Path.Combine(currentPluginFolder, Path.GetFileName(file)),
                        _nugetLogger
                    );
                }
            }

            SetPluginStateById(
                packageIdentity.Id,
                x =>
                {
                    x.IsLoaded = false;
                    x.IsUninstalled = false;
                    x.LoadingError = null;
                    x.InstalledFromSourceUri = "N/A";
                }
            );
        }
        catch (Exception)
        {
            if (Directory.Exists(currentPluginFolder))
            {
                try
                {
                    Directory.Delete(currentPluginFolder, true);
                }
                catch
                {
                    // ignore
                }
            }

            throw;
        }
    }

    private static void ExtractContentFolder(string nugetPackage, string to)
    {
        if (!File.Exists(nugetPackage))
        {
            return;
        }

        if (!Directory.Exists(to))
        {
            return;
        }

        var archive = ZipFile.OpenRead(nugetPackage);

        foreach (var entry in archive.Entries)
        {
            if (
                !entry
                    .FullName.Trim()
                    .StartsWith(@"content", StringComparison.InvariantCultureIgnoreCase)
            )
            {
                continue;
            }

            var filePath = Path.Combine(to, entry.FullName);
            var fileDir = Path.GetDirectoryName(filePath);

            if (fileDir is null)
            {
                continue;
            }

            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }

            if (!File.Exists(filePath))
            {
                entry.ExtractToFile(filePath, true);
            }
        }
    }

    private async Task<ISet<SourcePackageDependencyInfo>> ListAllPackageDependencies(
        IEnumerable<PackageDependencyGroup> dependencyGroups,
        SourceRepository[] repositories,
        CancellationToken cancellationToken
    )
    {
        var dependencies = new HashSet<SourcePackageDependencyInfo>(
            PackageIdentityComparer.Default
        );
        var packages = await ConvertToSourcePackageDependencyInfo(
            dependencyGroups,
            repositories,
            cancellationToken
        );

        foreach (var package in packages)
        {
            await ListAllPackageDependencies(
                package,
                repositories,
                dependencies,
                cancellationToken
            );
        }

        return dependencies;
    }

    private async Task<ISet<SourcePackageDependencyInfo>> ConvertToSourcePackageDependencyInfo(
        IEnumerable<PackageDependencyGroup> dependencyGroups,
        SourceRepository[] repositories,
        CancellationToken cancel
    )
    {
        var dependenciesSet = new HashSet<SourcePackageDependencyInfo>(
            PackageIdentityComparer.Default
        );
        var packageDependencyGroups = dependencyGroups.ToList();

        foreach (var repository in repositories)
        {
            foreach (var group in packageDependencyGroups)
            {
                foreach (var package in group.Packages)
                {
                    var version = package.VersionRange.MinVersion;
                    if (version is null)
                    {
                        continue;
                    }

                    var packageByIdSearcher =
                        await repository.GetResourceAsync<FindPackageByIdResource>(cancel);
                    var isPackage = await packageByIdSearcher.DoesPackageExistAsync(
                        package.Id,
                        version,
                        _cache,
                        _nugetLogger,
                        cancel
                    );

                    if (!isPackage)
                    {
                        continue;
                    }

                    var dependencyInfo = new SourcePackageDependencyInfo(
                        package.Id,
                        version,
                        [],
                        true,
                        repository
                    );

                    if (dependenciesSet.Contains(dependencyInfo))
                    {
                        continue;
                    }

                    if (NugetHelper.IncludedPackages.Contains(dependencyInfo.Id))
                    {
                        continue;
                    }

                    dependenciesSet.Add(dependencyInfo);
                }
            }
        }

        return dependenciesSet;
    }

    private async Task ListAllPackageDependencies(
        SourcePackageDependencyInfo package,
        SourceRepository[] repositories,
        ISet<SourcePackageDependencyInfo> dependencies,
        CancellationToken cancellationToken
    )
    {
        if (dependencies.Contains(package))
        {
            return;
        }

        foreach (var repository in repositories)
        {
            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>(
                cancellationToken
            );
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                package,
                NugetHelper.DefaultFramework,
                _cache,
                _nugetLogger,
                cancellationToken
            );
            if (dependencyInfo == null)
            {
                continue;
            }

            if (NugetHelper.IncludedPackages.Contains(dependencyInfo.Id))
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
                    repositories,
                    dependencies,
                    cancellationToken
                );
            }
        }
    }

    public void Uninstall(ILocalPluginInfo plugin)
    {
        // we cannot remove plugin folder because it used by application
        // so we set state PluginStateEnum.MarkedAsUninstalled
        // and application will remove folder on next start
        SetPluginStateById(plugin.PackageId, x => x.IsUninstalled = true);
    }

    public void CancelUninstall(ILocalPluginInfo pluginInfo)
    {
        SetPluginStateById(pluginInfo.PackageId, x => x.IsUninstalled = false);
    }

    public IEnumerable<ILocalPluginInfo> Installed
    {
        get
        {
            foreach (var folder in Directory.EnumerateDirectories(_sharedPluginFolder))
            {
                if (!TryGetLocalPluginInfoByFolder(folder, out var info))
                {
                    continue;
                }

                if (info != null)
                {
                    yield return info;
                }
            }
        }
    }

    public bool IsInstalled(string packageId, out ILocalPluginInfo? info)
    {
        return TryGetLocalPluginInfoById(packageId, out info);
    }

    private bool TryGetLocalPluginInfoByFolder(string pluginFolder, out ILocalPluginInfo? info)
    {
        info = null;
        var package = Directory
            .EnumerateFiles(pluginFolder, "*.nupkg", SearchOption.TopDirectoryOnly)
            .ToImmutableArray();
        if (package.Length == 0)
        {
            return false;
        }

        if (package.Length > 1)
        {
            _logger.LogWarning($"Find more than one package in folder {pluginFolder}");
        }

        if (TryGetPluginStateByFolder(pluginFolder, out var state) == false)
        {
            state = new PluginState
            {
                IsLoaded = false,
                LoadingError = null,
                IsUninstalled = false,
                InstalledFromSourceUri = new Uri(pluginFolder).ToString(),
            };
            SetPluginStateByFolder(pluginFolder, x => x.CopyFrom(state));
        }

        Debug.Assert(state != null, nameof(state) + " != null");
        using var reader = new PackageArchiveReader(package[0]);
        try
        {
            info = new LocalPluginInfo(reader, pluginFolder, state, _apiPackageId);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error read nuspec from {package[0]}");
            return false;
        }
    }

    private bool TryGetLocalPluginInfoById(string packageId, out ILocalPluginInfo? info)
    {
        info = null;
        ArgumentNullException.ThrowIfNull(packageId);
        var pluginFolder = Path.Combine(_sharedPluginFolder, packageId);
        return Directory.Exists(pluginFolder)
            && TryGetLocalPluginInfoByFolder(pluginFolder, out info);
    }

    private bool SetPluginStateById(string packageId, Action<PluginState> edit)
    {
        ArgumentNullException.ThrowIfNull(packageId);
        var pluginFolder = Path.Combine(_sharedPluginFolder, packageId);
        return Directory.Exists(pluginFolder) && SetPluginStateByFolder(pluginFolder, edit);
    }

    private bool SetPluginStateByFolder(string pluginFolder, Action<PluginState> edit)
    {
        if (TryGetPluginStateByFolder(pluginFolder, out var state) == false)
        {
            state = new PluginState();
        }

        if (state == null)
        {
            return false;
        }

        try
        {
            var stateFilePath = Path.Combine(pluginFolder, PluginStateFileName);
            if (File.Exists(stateFilePath))
            {
                File.Delete(stateFilePath);
            }

            edit(state);
            File.WriteAllText(stateFilePath, JsonConvert.SerializeObject(state));
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error to write plugin {pluginFolder} state file");
            return false;
        }
    }

    private bool TryGetPluginStateById(string packageId, out PluginState? state)
    {
        state = null;
        ArgumentNullException.ThrowIfNull(packageId);
        var pluginFolder = Path.Combine(_sharedPluginFolder, packageId);
        return Directory.Exists(pluginFolder) && TryGetPluginStateByFolder(pluginFolder, out state);
    }

    private static bool TryGetPluginStateByFolder(string pluginFolder, out PluginState? state)
    {
        state = null;
        var stateFilePath = Path.Combine(pluginFolder, PluginStateFileName);
        if (!File.Exists(stateFilePath))
        {
            return false;
        }

        try
        {
            state = JsonConvert.DeserializeObject<PluginState>(File.ReadAllText(stateFilePath));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

public class PluginManagerConfig
{
    public string MainDependenceName { get; set; }
    public PluginServerConfig[]? Servers { get; set; }
}

public class PluginServerConfig
{
    public string Name { get; set; }
    public string SourceUri { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PasswordHash { get; set; }
}
