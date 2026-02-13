using System.Composition.Hosting;
using System.Diagnostics.Metrics;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class SystemModule : IExportInfo
{
    public const string Name = "System";
    public static IExportInfo Instance { get; } = new SystemModule();

    private SystemModule() { }

    public string ModuleName => Name;
}

public static class ContainerConfigurationMixin
{
    public static ContainerConfiguration WithDependenciesFromSystemModule(
        this ContainerConfiguration containerConfiguration
    )
    {
        containerConfiguration.WithExport(TimeProvider.System);
        if (Design.IsDesignMode)
        {
            containerConfiguration
                .WithExport(DesignTime.Configuration)
                .WithExport(NullLoggerFactory.Instance)
                .WithExport(NullAppPath.Instance)
                .WithExport(NullAppInfo.Instance)
                .WithExport<IMeterFactory>(new DefaultMeterFactory())
                .WithExport(NullLogReaderService.Instance)
                .WithExport(NullUnitService.Instance);

            return containerConfiguration.WithAssemblies([typeof(SystemModule).Assembly]);
        }

        containerConfiguration.WithExport(AppHost.Instance.GetService<IUnitService>());

        var exceptionTypes = new List<Type>();
        if (AppHost.Instance.GetServiceOrDefault<IConfiguration>() is { } configuration)
        {
            containerConfiguration.WithExport(configuration);
        }

        if (AppHost.Instance.GetServiceOrDefault<ILoggerFactory>() is { } loggerFactory)
        {
            containerConfiguration.WithExport(loggerFactory);
        }

        if (AppHost.Instance.GetServiceOrDefault<IAppPath>() is { } appPath)
        {
            containerConfiguration.WithExport(appPath);
        }

        if (AppHost.Instance.GetServiceOrDefault<IAppInfo>() is { } appInfo)
        {
            containerConfiguration.WithExport(appInfo);
        }

        if (AppHost.Instance.GetServiceOrDefault<IMeterFactory>() is { } meterFactory)
        {
            containerConfiguration.WithExport(meterFactory);
        }

        if (AppHost.Instance.GetServiceOrDefault<ISoloRunFeature>() is { } soloRunFeature)
        {
            containerConfiguration.WithExport(soloRunFeature);
        }

        if (AppHost.Instance.GetService<IOptions<LoggerOptions>>().Value.ViewerEnabled ?? false)
        {
            containerConfiguration.WithExport(
                AppHost.Instance.GetService<IOptions<LogToFileOptions>>().Value
            );
        }
        else
        {
            exceptionTypes.AddRange([
                typeof(LogViewerViewModel),
                typeof(LogViewerView),
                typeof(HomePageLogViewerExtension),
                typeof(OpenLogViewerCommand),
                typeof(ILogReaderService),
                typeof(LogReaderService),
            ]);
        }

        var systemTypes = typeof(SystemModule).Assembly.GetTypes().Except(exceptionTypes);

        return containerConfiguration.WithParts(systemTypes);
    }
}
