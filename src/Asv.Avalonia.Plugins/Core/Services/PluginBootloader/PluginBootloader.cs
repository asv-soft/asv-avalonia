using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Asv.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NuGet.Packaging;

namespace Asv.Avalonia.Plugins;

public class PluginBootloaderOptions
{
    public const string SectionName = "Plugins";
    public string RelativeFolder { get; set; } = "plugins";
    public string[]? AdditionalFolderPerPlugin { get; set; }
    public string? ApiPackageName { get; set; } = "Asv.Avalonia.Plugins.Api";
    public string? ApiVersion { get; set; } = "1.0.0";
    public string? PluginAssemblyPrefix { get; set; } = "Asv.Avalonia.Plugins.";
}

public class PluginBootloader : AsyncDisposableOnceBag, IPluginBootloader
{
    private readonly string _apiPackageId;
    private readonly SemVersion _apiVersion;
    private const string PackageFilePostfix = ".nupkg";
    private readonly List<PluginAssemblyLoadContext> _pluginContexts = [];
    private readonly string _assemblyPluginPrefix;
    private readonly List<Assembly> _pluginAssemblies = [];
    private List<ILocalPluginInfo> _info = [];

    public PluginBootloader(
        IOptions<PluginBootloaderOptions> options,
        IHostEnvironment environment)
    {
        _apiPackageId = options.Value.ApiPackageName ?? throw new InvalidOperationException(
            $"ApiPackageName is required in {PluginBootloaderOptions.SectionName} configuration section"
        );
        var apiString = options.Value.ApiVersion ?? throw new InvalidOperationException(
            $"ApiVersion is required in {PluginBootloaderOptions.SectionName} configuration section"
        );
        _apiVersion = SemVersion.Parse(apiString);
        _assemblyPluginPrefix = options.Value.PluginAssemblyPrefix ?? throw new InvalidOperationException(
            $"PluginAssemblyPrefix is required in {PluginBootloaderOptions.SectionName} configuration section"
        );
        
        var relativeFolder = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.Value.RelativeFolder));
        if (Directory.Exists(relativeFolder))
        {
            foreach (
                var dir in Directory.EnumerateDirectories(
                    relativeFolder,
                    "*",
                    SearchOption.TopDirectoryOnly
                )
            )
            {
                ProcessPluginFolder(dir);
            }
        }
        
        if (options.Value.AdditionalFolderPerPlugin != null)
        {
            foreach (var absolutePath in options.Value.AdditionalFolderPerPlugin.Distinct())
            {
                var fullPath = Path.GetFullPath(absolutePath);
                if (!Directory.Exists(fullPath))
                {
                    continue;
                }
                
                ProcessPluginFolder(fullPath);
            }
        }
    }
    
    public SemVersion ApiVersion => _apiVersion;
    public IEnumerable<ILocalPluginInfo> Installed => _info;

    private void ProcessPluginFolder(string folder)
    {
        try
        {
            var info = GetInfo(folder);
            if (info == null)
            {
                return;
            }

            if (info.IsUninstalled)
            {
                Directory.Delete(folder, true);
                return;
            }
            _info.Add(info);
            if (info.ApiVersion.CompareByPrecedence(_apiVersion) != 0)
            {
                PluginState.Edit(folder, x =>
                    {
                        x.IsLoaded = false;
                        x.LoadingError =
                            $"Plugin has different API version {info.ApiVersion} than application {_apiVersion}";
                    }
                );
                return;
            }
            var context = PluginAssemblyLoadContext.Create(folder, _assemblyPluginPrefix, _pluginAssemblies);
            _pluginContexts.Add(context);
        }
        catch (Exception e)
        {
            ExceptionReport.WriteToFile(folder, e, out _);
        }
    }

    private ILocalPluginInfo? GetInfo(string folder)
    {
        var package = Directory
            .EnumerateFiles(folder, $"*{PackageFilePostfix}", SearchOption.TopDirectoryOnly)
            .ToImmutableArray();
        if (package.Length == 0)
        {
            return null;
        }

        if (package.Length > 1)
        {
            throw new Exception($"Find more than one package in folder {folder}: {string.Join(",", package)}");       
        }
        
        var state = PluginState.Read(folder);
        if (state == null)
        {
            state = PluginState.Write(folder, new PluginState
            {
                IsLoaded = false,
                LoadingError = null,
                IsUninstalled = false,
                InstalledFromSourceUri = new Uri(folder).ToString(),
            });       
        }
        Debug.Assert(state != null, nameof(state) + " != null");
        using var reader = new PackageArchiveReader(package[0]);
        return new LocalPluginInfo(reader, folder, state, _apiPackageId);
    }

    public void InitPlugins(IHostApplicationBuilder builder)
    {
        foreach (var assembly in _pluginAssemblies)
        {
            try
            {
                foreach (var pluginAppBuilder in assembly.GetTypes()
                             .Where(t => typeof(IPluginAppBuilder).IsAssignableFrom(t) &&
                                         t is { IsClass: true, IsAbstract: false })
                             .Select(t => Activator.CreateInstance(t) as IPluginAppBuilder)
                             .Where(p => p != null))
                {
                    pluginAppBuilder?.Register(builder);
                }
            }
            catch (Exception e)
            {
                ExceptionReport.WriteToFile(assembly.Location, e, out _);
            }
        }            
    }
}