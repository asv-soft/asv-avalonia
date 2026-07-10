using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Test;

internal static class MockAppHost
{
    public static IHost Build(
        Action<IHostApplicationBuilder> configure,
        Action<IServiceCollection>? inspectServices = null,
        IFileSystem? fileSystem = null
    )
    {
        ArgumentNullException.ThrowIfNull(configure);

        fileSystem ??= new MockFileSystem();
        var builder = new MockHostApplicationBuilder(fileSystem);

        configure(builder);
        builder.ExecutePostConfigureCallbacks();
        inspectServices?.Invoke(builder.Services);

        return new MockHost(builder.Services.BuildServiceProvider());
    }

    private sealed class MockHostApplicationBuilder : IHostApplicationBuilder
    {
        public MockHostApplicationBuilder(IFileSystem fileSystem)
        {
            Services.AddLogging();
            Services.AddMetrics();
            Services.AddSingleton(fileSystem);
            Services.AddSingleton(Environment);
            Services.AddSingleton<IConfiguration>(Configuration);

            Environment.ApplicationName =
                typeof(MockAppHost).Assembly.GetName().Name ?? string.Empty;
            Environment.EnvironmentName = "Test";
            Environment.ContentRootPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "asv-avalonia-composition-tests"
            );
            Environment.ContentRootFileProvider = new NullFileProvider();
        }

        public IDictionary<object, object> Properties { get; } = new Dictionary<object, object>();
        public IConfigurationManager Configuration { get; } = new ConfigurationManager();
        public IHostEnvironment Environment { get; } = new MockHostEnvironment();
        public ILoggingBuilder Logging => new MockLoggingBuilder(Services);
        public IMetricsBuilder Metrics => new MockMetricsBuilder(Services);
        public IServiceCollection Services { get; } = new ServiceCollection();

        public void ConfigureContainer<TContainerBuilder>(
            IServiceProviderFactory<TContainerBuilder> factory,
            Action<TContainerBuilder>? configure = null
        )
            where TContainerBuilder : notnull
        {
            throw new NotSupportedException("Custom containers are not supported by test host.");
        }
    }

    private sealed class MockHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class MockLoggingBuilder(IServiceCollection services) : ILoggingBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    private sealed class MockMetricsBuilder(IServiceCollection services) : IMetricsBuilder
    {
        public IServiceCollection Services { get; } = services;
    }

    private sealed class MockHost(IServiceProvider services) : IHost
    {
        public IServiceProvider Services { get; } = services;

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (Services is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
