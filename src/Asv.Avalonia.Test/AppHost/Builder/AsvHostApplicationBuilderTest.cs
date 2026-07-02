using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Asv.XUnit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Asv.Avalonia.Test;

public class AsvHostApplicationBuilderTest
{
    private readonly ILogger _logger;

    public AsvHostApplicationBuilderTest(ITestOutputHelper output)
    {
        _logger = new TestLoggerFactory(
            output,
            TimeProvider.System,
            "composition"
        ).CreateLogger<AsvHostApplicationBuilderTest>();
    }

    [Fact]
    public void Build_WithDefaultSetup_RegistersDefault()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        IServiceCollection? registeredServices = null;

        // Act
        using var host = MockAppHost.Build(
            builder => builder.UseDefault(),
            services =>
            {
                registeredServices = services;
                CompositionServiceLogger.Log(services, _logger);
            },
            fileSystem
        );

        // Assert
        Assert.NotNull(registeredServices);
        Assert.True(registeredServices.IsRegistered<IAppPath>());
        Assert.NotNull(host.Services.GetRequiredService<IAppInfo>());
        Assert.NotNull(host.Services.GetRequiredService<IAppArgsStore>());
        Assert.Same(fileSystem, host.Services.GetRequiredService<IFileSystem>());
        Assert.NotNull(host.Services.GetRequiredService<IExtensionService>());
        Assert.NotNull(host.Services.GetRequiredService<IHotKeyService>());
        Assert.NotNull(host.Services.GetRequiredService<IShellHost>());
        Assert.NotNull(host.Services.GetRequiredService<ViewLocator>());
        Assert.NotNull(host.Services.GetRequiredService<IDialogService>());
        Assert.NotEmpty(host.Services.GetRequiredService<IUnitService>().Units);
    }
}
