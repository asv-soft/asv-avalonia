using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;
public class ConnectionPortItem
{
    public string Name { get; set; }
    public string ConnectionString { get; set; }

    public bool IsEnabled { get; set; }
}

public class MavlinkConnectionsConfig
{
    public List<ConnectionPortItem> Items { get; set; }
}

[Export(typeof(IMavlinkConnectionService))]
[Shared]
public class MavlinkConnectionService : AsyncDisposableOnce, IMavlinkConnectionService
{
    private IConfiguration _cfg;
    private ILogger<MavlinkConnectionService> _logger;
    private IProtocolFactory _protocol;
    private MavlinkConnectionsConfig _connectionsConfig;

    [ImportingConstructor]
    public MavlinkConnectionService(IConfiguration cfg, ILoggerFactory loggerFactory)
    {
        _cfg = cfg;
        _logger = loggerFactory.CreateLogger<MavlinkConnectionService>();
        _protocol = Protocol.Create(builder =>
        {
            builder.RegisterMavlinkV2Protocol();
            builder.SetLog(loggerFactory);
            builder.SetTimeProvider(TimeProvider.System);
            builder.SetMetrics(new DefaultMeterFactory());
        });
        CreateRouter();
        if (Router != null)
        {
            Router.AddPort("tcp://127.0.0.1:5762"); // TODO: Remove after tests
        }
        
       // CreateDeviceExplorer();
    }

    private void CreateRouter()
    {
        Router = _protocol.CreateRouter("Router");
    }

    private void CreateDeviceExplorer()
    {
        var seq = new PacketSequenceCalculator();
        DevicesExplorer = DeviceExplorer.Create(Router, builder =>
        {
            builder.SetLog(_protocol.LoggerFactory);
            builder.SetMetrics(_protocol.MeterFactory);
            builder.SetTimeProvider(_protocol.TimeProvider);
            builder.SetConfig(new ClientDeviceBrowserConfig()
            {
                DeviceTimeoutMs = 1000,
                DeviceCheckIntervalMs = 1000,
            });
            _connectionsConfig = _cfg.Get<MavlinkConnectionsConfig>();

            foreach (var port in _connectionsConfig.Items)
            {
                Router.AddPort(port.ConnectionString);
            }

            builder.Factories.RegisterDefaultDevices(new MavlinkIdentity(254, 254), seq, new InMemoryConfiguration());
        });
    }

    public ValueTask AddPort(string connectionString)
    {
        try
        {
            Router.AddPort(connectionString);
        }
        catch (Exception e)
        {
            _logger.ZLogError($"Unable add connection at  :{connectionString}");

            return ValueTask.FromException(e);
        }

        return ValueTask.CompletedTask;
    }

    public void DisablePort(IProtocolPort port)
    {
        Router.Ports.First(protocolPort => protocolPort == port).Disable();
    }
    
    public void EnablePort(IProtocolPort port)
    {
        Router.Ports.First(protocolPort => protocolPort == port).Enable();
    }

    public void RemovePort(IProtocolPort port)
    {
        Router.Ports.Remove(port); //TODO: YesNoDialog
    }

    public ValueTask EditPort(IProtocolPort port)
    {
        return ValueTask.CompletedTask;
    }

    public IProtocolRouter Router { get; set; }
    public IDeviceExplorer DevicesExplorer { get; set; }
    public IExportInfo Source => SystemModule.Instance;
}