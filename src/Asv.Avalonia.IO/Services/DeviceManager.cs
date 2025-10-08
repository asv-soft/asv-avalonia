using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.Metrics;
using Asv.Cfg;
using Asv.Common;
using Asv.IO;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public class DeviceManagerConfig
{
    public string[] Connections { get; set; } = ["tcp://172.16.0.1:7341#name=GBS"];
}

[Export(typeof(IDeviceManager))]
[Shared]
public class DeviceManager : IDeviceManager, IDisposable, IAsyncDisposable
{
    private readonly IConfiguration _cfgSvc;
    private readonly ImmutableArray<IDeviceManagerExtension> _extensions;
    private IDisposable? _sub1;
    private readonly DeviceManagerConfig _config;

    private static readonly ImmutableSolidColorBrush[] DeviceColors =
    [
        new(Color.Parse("#AA00FF")),
        new(Color.Parse("#6200EA")),
        new(Color.Parse("#304FFE")),
        new(Color.Parse("#2962FF")),
        new(Color.Parse("#0091EA")),
        new(Color.Parse("#00B8D4")),
        new(Color.Parse("#00BFA5")),
    ];

    [ImportingConstructor]
    public DeviceManager(
        IConfiguration cfgSvc,
        ILoggerFactory loggerFactory,
        IMeterFactory meterFactory,
        TimeProvider timeProvider,
        [ImportMany] IEnumerable<IDeviceManagerExtension> extensions
    )
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(meterFactory);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(extensions);
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _extensions = [.. extensions];
        ProtocolFactory = Protocol.Create(builder =>
        {
            builder.SetLog(loggerFactory);
            builder.SetMetrics(meterFactory);
            builder.SetTimeProvider(timeProvider);
            foreach (var extension in _extensions)
            {
                extension.Configure(builder);
            }
        });

        Router = ProtocolFactory.CreateRouter("ROUTER");
        Explorer = DeviceExplorer.Create(
            Router,
            builder =>
            {
                builder.SetLog(loggerFactory);
                builder.SetMetrics(meterFactory);
                builder.SetTimeProvider(timeProvider);
                builder.SetDefaultConfig();
                foreach (var extension in _extensions)
                {
                    extension.Configure(builder);
                }
            }
        );
        _config = _cfgSvc.Get<DeviceManagerConfig>();
        Task.Factory.StartNew(LoadPortsAtBackground, null, TaskCreationOptions.LongRunning)
            .SafeFireAndForget();
        foreach (var extension in _extensions)
        {
            extension.Run(this);
        }
    }

    public IProtocolFactory ProtocolFactory { get; }

    private void LoadPortsAtBackground(object? obj)
    {
        foreach (var cs in _config.Connections)
        {
            Router.AddPort(cs);
        }

        // this is needed to save config after port changes
        _sub1 = Router
            .PortUpdated.Merge(Router.PortAdded)
            .Merge(Router.PortRemoved)
            .Subscribe(_ => SaveConfig());
    }

    private void SaveConfig()
    {
        _config.Connections = Router.Ports.Select(x => x.Config.AsUri().ToString()).ToArray();
        _cfgSvc.Set(_config);
    }

    public MaterialIconKind? GetIcon(DeviceId id)
    {
        foreach (var extension in _extensions)
        {
            if (extension.TryGetIcon(id, out var icon))
            {
                return icon;
            }
        }

        return MaterialIconKind.Navigation;
    }

    public IBrush? GetDeviceBrush(DeviceId id)
    {
        foreach (var extension in _extensions)
        {
            if (extension.TryGetDeviceBrush(id, out var brush))
            {
                return brush;
            }
        }

        return DeviceColors[Math.Abs(id.GetHashCode()) % DeviceColors.Length];
    }

    public IProtocolRouter Router { get; }
    public IDeviceExplorer Explorer { get; }

    public void Dispose()
    {
        _sub1?.Dispose();
        Router.Dispose();
        Explorer.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_sub1 != null)
        {
            await CastAndDispose(_sub1);
        }

        await Router.DisposeAsync();
        await Explorer.DisposeAsync();

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
            {
                await resourceAsyncDisposable.DisposeAsync();
            }
            else
            {
                resource.Dispose();
            }
        }
    }
}
