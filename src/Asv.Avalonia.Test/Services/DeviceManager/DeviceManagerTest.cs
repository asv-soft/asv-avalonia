using System.Collections.Immutable;
using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.IO.Device;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Asv.Avalonia.Test;

public class DeviceManagerTest : IDisposable
{
    private readonly InMemoryConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ServiceProvider _services;
    private readonly DeviceManager _manager;

    private static readonly ImmutableArray<AsvColorKind> AllowedDeviceColors =
    [
        AsvColorKind.Error,
        AsvColorKind.Warning,
        AsvColorKind.Success,
        AsvColorKind.Info1,
        AsvColorKind.Info2,
        AsvColorKind.Info3,
        AsvColorKind.Info4,
        AsvColorKind.Info5,
        AsvColorKind.Info6,
        AsvColorKind.Info7,
        AsvColorKind.Info8,
        AsvColorKind.Info9,
        AsvColorKind.Info10,
        AsvColorKind.Info11,
        AsvColorKind.Info12,
        AsvColorKind.Info13,
        AsvColorKind.Info14,
        AsvColorKind.Info15,
        AsvColorKind.Info16,
        AsvColorKind.Info17,
        AsvColorKind.Info18,
        AsvColorKind.Info19,
        AsvColorKind.Info20,
    ];

    public DeviceManagerTest()
    {
        _configuration = new InMemoryConfiguration();
        _configuration.Set(new DeviceManagerConfig { Connections = [] });
        _loggerFactory = LoggerFactory.Create(_ => { });
        _services = new ServiceCollection().AddMetrics().BuildServiceProvider();
        _manager = new DeviceManager(
            _configuration,
            _loggerFactory,
            _services.GetRequiredService<System.Diagnostics.Metrics.IMeterFactory>(),
            TimeProvider.System,
            []
        );
    }

    [Fact]
    public void GetDeviceColor_AllPossibleDeviceIDs_ReturnsOnlyAllowedColors()
    {
        // Arrange
        var realColors = new List<AsvColorKind>();

        // Act
        for (var id = byte.MinValue; ; id++)
        {
            realColors.Add(_manager.GetDeviceColor(new ExampleDeviceId("example", id)));

            if (id == byte.MaxValue)
            {
                break;
            }
        }

        // Assert
        foreach (var color in realColors)
        {
            Assert.Contains(color, AllowedDeviceColors);
        }
    }

    public void Dispose()
    {
        _manager.Dispose();
        _services.Dispose();
        _loggerFactory.Dispose();
        _configuration.Dispose();
    }
}
