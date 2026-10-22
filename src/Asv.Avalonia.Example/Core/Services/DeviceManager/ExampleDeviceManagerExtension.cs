using Asv.Avalonia.IO;
using Asv.Cfg;
using Asv.IO;
using Asv.IO.Device;
using Material.Icons;
using R3;

namespace Asv.Avalonia.Example;

public class ExampleDeviceManagerExtension(IConfiguration cfgSvc) : IDeviceManagerExtension
{
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
        var serverUri = new Uri("tcps://127.0.0.1:8888");
        var serverPortExists = cfgSvc
            .Get<DeviceManagerConfig>()
            .Connections.Any(cs =>
                Uri.TryCreate(cs, UriKind.Absolute, out var uri)
                && uri.Scheme == serverUri.Scheme
                && uri.Host == serverUri.Host
                && uri.Port == serverUri.Port
            );
        if (!serverPortExists)
        {
            deviceManager.Router.AddPort(serverUri);
        }
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
        long firstSequence = 0;
        long secondSequence = 0;

        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                var sequence = firstSequence++;
                server.Send(CreateMessage1(senderId, sequence));
                server.Send(CreateMessage2(senderId, sequence));
                server.Send(new ExampleMessage3 { SenderId = senderId });
            });

        Observable
            .Timer(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                var sequence = secondSequence++;
                server.Send(CreateMessage1(sender2Id, sequence));
                server.Send(CreateMessage2(sender2Id, sequence));
                server.Send(new ExampleMessage3 { SenderId = sender2Id });
            });
    }

    private static ExampleMessage1 CreateMessage1(byte senderId, long sequence)
    {
        var sample = (int)(sequence % 1000);
        var message = new ExampleMessage1
        {
            SenderId = senderId,
            Value1 = 1 + ((sample + senderId) % 3),
            Value2 = (ushort)(1000 + (senderId * 100) + sample),
            Value3 = $"D{senderId}:{sample:D4}",
        };
        message.Value4[0] = (senderId == 1 ? 1 : -1) * (500 + sample);
        message.Value5[0] = $"sensor-{senderId}";
        message.Value5[1] = sample % 2 == 0 ? "active" : "standby";
        for (var i = 0; i < message.Value6.Length; i++)
        {
            PopulateSubObject(message.Value6[i], senderId, sample, i);
        }
        PopulateSubObject(message.Value7, senderId, sample, 3);
        for (var i = 0; i < 2 + (sample % 2); i++)
        {
            var item = new SubObject();
            PopulateSubObject(item, senderId, sample, i + 4);
            message.Value8.Add(item);
        }
        foreach (var character in $"D{senderId}-{sample % 10}")
        {
            message.Value9.Add(character);
        }
        return message;
    }

    private static ExampleMessage2 CreateMessage2(byte senderId, long sequence)
    {
        var sample = (int)(sequence % 1000);
        var phase = (sequence * 0.15) + senderId;
        return new ExampleMessage2
        {
            SenderId = senderId,
            Value1 = (senderId * 10000) + sample,
            Value2 = (ushort)(1200 + (senderId * 50) + (sample % 100)),
            Value3 = $"Device-{senderId}: sample {sample:D4}",
            Value4 = -((senderId * 100) + sample),
            Value5 = (float)Math.Round(20 + (senderId * 10) + (3 * Math.Sin(phase)), 3),
            Value6 = Math.Round(0.5 + (senderId * 0.1) + (0.05 * Math.Cos(phase)), 6),
            Value7 = (sample + senderId) % 2 == 0,
            Value8 = -5000000000L + (senderId * 100000L) + sample,
            Value9 = 9000000000UL + ((ulong)senderId * 100000UL) + (ulong)sample,
        };
    }

    private static void PopulateSubObject(SubObject item, byte senderId, int sample, int channel)
    {
        item.Field1 = (sbyte)(-60 + (senderId * 10) + channel + (sample % 20));
        item.Field2 = (byte)(20 + (senderId * 10) + channel + (sample % 20));
        item.Field3 = (short)(-1000 + (senderId * 100) + (channel * 10) - sample);
        item.Field4 = (ushort)(2000 + (senderId * 100) + (channel * 10) + sample);
    }

}
