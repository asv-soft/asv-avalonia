using System.Composition.Hosting;
using System.Diagnostics.Metrics;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
        if (Design.IsDesignMode)
        {
            containerConfiguration
                .WithExport(TimeProvider.System)
                .WithExport(DesignTime.Configuration)
                .WithExport(NullLoggerFactory.Instance)
                .WithExport(NullAppPath.Instance)
                .WithExport(NullAppInfo.Instance)
                .WithExport<IMeterFactory>(new DefaultMeterFactory())
                .WithExport(NullLogReaderService.Instance);

            return containerConfiguration.WithAssemblies([typeof(SystemModule).Assembly]);
        }

        var exceptionTypes = new List<Type>();
        if (AppHost.Instance.GetServiceOrDefault<TimeProvider>() is { } timeProvider)
        {
            containerConfiguration.WithExport(timeProvider);
        }

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

        if (AppHost.Instance.GetServiceOrDefault<ILogReaderService>() is { } logReader)
        {
            containerConfiguration.WithExport(logReader);
        }
        else
        {
            exceptionTypes.AddRange(
                [
                    typeof(LogViewerViewModel),
                    typeof(LogViewerView),
                    typeof(HomePageLogViewerExtension),
                    typeof(OpenLogViewerCommand),
                ]
            );
        }

        var systemTypes = typeof(SystemModule).Assembly.GetTypes().Except(exceptionTypes);

        return containerConfiguration.WithParts(systemTypes);
    }
}
