using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Asv.Cfg;
using Asv.Common;
using Asv.IO;
using Asv.Mavlink;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using ZLogger;

namespace Asv.Avalonia.Example;

public class MavlinkConnectionsConfig
{
    public Dictionary<string, ConnectionPortConfigItem> Items { get; set; } = new();
}

[Export(typeof(IMavlinkConnectionService))]
[Shared]
public class MavlinkConnectionService : AsyncDisposableOnce, IMavlinkConnectionService
{
    private IConfiguration _cfg;
    private ILogger<MavlinkConnectionService> _logger;
    private IProtocolFactory _protocol;
    private IDialogService _dialog;

    [ImportingConstructor]
    public MavlinkConnectionService(
        IConfiguration cfg,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
    {
        _cfg = cfg;
        _dialog = dialogService;
        _logger = loggerFactory.CreateLogger<MavlinkConnectionService>();
        _protocol = Protocol.Create(builder =>
        {
            builder.RegisterMavlinkV2Protocol();
            builder.SetLog(loggerFactory);
            builder.SetTimeProvider(TimeProvider.System);
            builder.SetMetrics(new DefaultMeterFactory());
        });

        CreateRouter();

        LoadFromConfig();

        CreateDeviceExplorer();
    }

    private void CreateRouter()
    {
        Router = _protocol.CreateRouter("Router");
    }

    private void CreateDeviceExplorer()
    {
        var seq = new PacketSequenceCalculator();
        DevicesExplorer = DeviceExplorer.Create(
            Router,
            builder =>
            {
                builder.SetLog(_protocol.LoggerFactory);
                builder.SetMetrics(_protocol.MeterFactory);
                builder.SetTimeProvider(_protocol.TimeProvider);
                builder.SetConfig(
                    new ClientDeviceBrowserConfig()
                    {
                        DeviceTimeoutMs = 30_000,
                        DeviceCheckIntervalMs = 3_000,
                    }
                );

                builder.Factories.RegisterDefaultDevices(
                    new MavlinkIdentity(254, 254),
                    seq,
                    new InMemoryConfiguration()
                );
            }
        );
    }

    public void DisablePort(IProtocolPort port)
    {
        if (port.IsEnabled.CurrentValue)
        {
            Router.Ports.First(protocolPort => protocolPort == port).Disable(); // executes only when value is different for avoid reconnecting
        }

        UpdateConfig();
    }

    public void EnablePort(IProtocolPort port)
    {
        if (!port.IsEnabled.CurrentValue)
        {
            Router.Ports.First(protocolPort => protocolPort == port).Enable(); // executes only when value is different for avoid reconnecting
        }

        UpdateConfig();
    }

    public async Task RemovePort(IProtocolPort port, bool withDialog = true)
    {
        if (withDialog)
        {
            var res = await _dialog.ShowYesNoDialog(
                $"{port.TypeInfo.Name} delete requested",
                "Delete that connection?"
            ); //TODO: localization
            if (res)
            {
                var remove = Connections.First(valuePair => valuePair.Value == port);
                Config.Items.Remove(remove.Key);
                Connections.Remove(remove.Key);
                Router.RemovePort(port);
            }
        }
        else
        {
            var remove = Connections.First(valuePair => valuePair.Value == port);
            Config.Items.Remove(remove.Key);
            Connections.Remove(remove.Key);
            Router.RemovePort(port);
        }

        _logger.ZLogInformation($"Removed port {port.Id}");
        UpdateConfig();
    }

    public ValueTask EditPort(IProtocolPort port)
    {
        UpdateConfig();
        return ValueTask.CompletedTask;
    }

    private void LoadFromConfig()
    {
        var currentCfg = _cfg.Get<MavlinkConnectionsConfig>();

        foreach (var configItem in currentCfg.Items)
        {
            AddConnection(
                configItem.Key,
                configItem.Value.ConnectionString,
                configItem.Value.IsEnabled
            );
        }
    }

    public void AddConnection(string portName, string connectionString, bool isEnabled = true)
    {
        var port = Router.AddPort(connectionString);
        if (!isEnabled)
        {
            DisablePort(port);
        }

        Config.Items.Add(
            portName,
            new ConnectionPortConfigItem
            {
                ConnectionString = connectionString,
                IsEnabled = isEnabled,
            }
        );
        Connections.Add(portName, port);

        UpdateConfig();
        _logger.ZLogInformation($"Added Connection for {connectionString}");
    }

    private void UpdateConfig()
    {
        _cfg.Set(nameof(MavlinkConnectionsConfig), Config);
    }

    public IProtocolRouter Router { get; set; }
    public ObservableDictionary<string, IProtocolPort> Connections { get; set; } = new();
    private MavlinkConnectionsConfig Config { get; set; } = new();
    public IDeviceExplorer DevicesExplorer { get; set; }
    public IExportInfo Source => SystemModule.Instance;
}
