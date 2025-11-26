using System;
using System.Composition;
using Asv.Avalonia;
using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.IO;
using Asv.IO.Device;
using Avalonia.Media;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Example;

[Export(typeof(IDeviceManagerExtension))]
[Shared]
public class ExampleDeviceManagerExtension : IDeviceManagerExtension
{
    [ImportingConstructor]
    public ExampleDeviceManagerExtension() { }

    public void Configure(IProtocolBuilder builder)
    {
        builder.Protocols.RegisterExampleProtocol();
        builder.PortTypes.RegisterTcpClientPort();
        builder.PortTypes.RegisterTcpServerPort();
        builder.PortTypes.RegisterSerialPort();
        builder.PortTypes.RegisterUdpPort();
    }

    public void Configure(IDeviceExplorerBuilder builder)
    {
        builder.Factories.Register(
            new ExampleDeviceFactory(new ExampleDeviceConfig { SelfId = 255 })
        );
    }

    public bool TryGetIcon(DeviceId id, out MaterialIconKind? icon)
    {
        icon = null;
        return false;
    }

    public bool TryGetDeviceBrush(DeviceId id, out AsvColorKind brush)
    {
        brush = AsvColorKind.None;
        return false;
    }

    public void Run(IDeviceManager deviceManager)
    {
        // Implement virtual server with two example devices
        deviceManager.Router.AddPort("tcps://127.0.0.1:8888");
        var protocol = Protocol.Create(builder =>
        {
            builder.Protocols.RegisterExampleProtocol();
            builder.PortTypes.RegisterTcpClientPort();
            builder.PortTypes.RegisterTcpServerPort();
            builder.PortTypes.RegisterSerialPort();
            builder.PortTypes.RegisterUdpPort();
        });
        var server = protocol.CreateRouter("Example");
        server.AddPort("tcp://127.0.0.1:8888");

        const byte senderId = 1;
        const byte sender2Id = 2;

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(x =>
            {
                server.Send(new ExampleMessage1 { SenderId = senderId });
                server.Send(new ExampleMessage2 { SenderId = senderId });
                server.Send(new ExampleMessage3 { SenderId = senderId });
            });

        Observable
            .Timer(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1))
            .Subscribe(x =>
            {
                server.Send(new ExampleMessage1 { SenderId = sender2Id });
                server.Send(new ExampleMessage2 { SenderId = sender2Id });
                server.Send(new ExampleMessage3 { SenderId = sender2Id });
            });
    }
}
