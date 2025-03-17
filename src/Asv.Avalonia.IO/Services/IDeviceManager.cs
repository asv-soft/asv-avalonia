using System.Composition;
using Asv.IO;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.IO;

public interface IDeviceManager
{
    MaterialIconKind GetIcon(DeviceId id);
    IBrush GetDeviceBrush(DeviceId id);
    IProtocolRouter Router { get; }
    IDeviceExplorer Explorer { get; }
}

[Export(typeof(IDeviceManager))]
[Shared]
public class DeviceManager : IDeviceManager
{
    [ImportingConstructor]
    public DeviceManager(ILoggerFactory loggerFactory)
    {
        var factory = Protocol.Create(builder =>
        {
            builder.SetLog(loggerFactory);
        });

        var router = factory.CreateRouter("ROUTER");

        DeviceExplorer.Create(router, builder => { });
    }

    public MaterialIconKind GetIcon(DeviceId id)
    {
        throw new NotImplementedException();
    }

    public IBrush GetDeviceBrush(DeviceId id)
    {
        throw new NotImplementedException();
    }

    public IProtocolRouter Router { get; }
    public IDeviceExplorer Explorer { get; }
}
