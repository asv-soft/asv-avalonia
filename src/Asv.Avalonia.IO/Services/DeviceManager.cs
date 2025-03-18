using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.Metrics;
using Asv.IO;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

[Export(typeof(IDeviceManager))]
[Shared]
public class DeviceManager : IDeviceManager
{
    private readonly ImmutableArray<IDeviceManagerExtension> _extensions;

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
        ILoggerFactory loggerFactory,
        IMeterFactory meterFactory,
        TimeProvider timeProvider,
        [ImportMany] IEnumerable<IDeviceManagerExtension> extensions
    )
    {
        _extensions = [.. extensions];
        var factory = Protocol.Create(builder =>
        {
            builder.SetLog(loggerFactory);
            builder.SetMetrics(meterFactory);
            builder.SetTimeProvider(timeProvider);
            foreach (var extension in _extensions)
            {
                extension.Configure(builder);
            }
        });

        Router = factory.CreateRouter("ROUTER");
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
}
