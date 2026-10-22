using System.Collections.Immutable;
using System.Reflection;
using Asv.Avalonia.IO;
using Asv.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using R3;
using Xunit;

namespace Asv.Avalonia.Test.IO;

public class PacketCaptureServiceTest
{
    [Fact]
    public void Capture_IdentifiesDeviceAndCopiesVisitorValues()
    {
        using var fixture = new Fixture();
        var device = new TestDeviceId("device-1");
        var calls = 0;
        fixture.Identify = _ =>
        {
            calls++;
            return device;
        };
        using var capture = fixture.CreateCapture();
        var message = new ExampleMessage1 { Value1 = 2, Value3 = "before" };

        fixture.Rx.OnNext(message);
        message.Value1 = 3;
        message.Value3 = "after";
        fixture.Tx.OnNext(message);

        var state = capture.Read();
        Assert.Equal(2, state.Packets.Count);
        Assert.Equal(2, calls);
        var rx = state.Packets[0];
        Assert.Equal(device, rx.Key.DeviceId);
        Assert.Equal("endpoint", rx.Endpoint);
        Assert.Equal("port", rx.Key.PortId);
        Assert.Equal(PacketDirection.Rx, rx.Key.Direction);
        Assert.Equal(PacketDirection.Tx, state.Packets[1].Key.Direction);
        var fields = Assert.Single(rx.Fields).Children;
        Assert.Equal("2", fields.Single(x => x.Name == "Value1").Value);
        Assert.Equal(2d, fields.Single(x => x.Name == "Value1").NumericValue);
        Assert.Null(fields.Single(x => x.Name == "Value3").NumericValue);
        Assert.Equal("before", fields.Single(x => x.Name == "Value3").Value);
        Assert.Equal("after", Assert.Single(state.Packets[1].Fields).Children.Single(x => x.Name == "Value3").Value);
        Assert.Equal(new[] { "[0]", "[1]" }, fields.Single(x => x.Name == "Value5").Children.Select(x => x.Name));
        var nested = fields.Single(x => x.Name == "Value6").Children[0];
        Assert.Equal("struct", nested.Type);
        Assert.Equal("Field1", nested.Children[0].Name);
        Assert.Equal(0, state.Errors);
    }

    [Fact]
    public void Capture_UnknownDeviceAndOverflow_PreservesNewestPackets()
    {
        using var fixture = new Fixture();
        using var capture = fixture.CreateCapture(2);
        for (var i = 0; i < 3; i++)
        {
            fixture.Rx.OnNext(new ExampleMessage1 { Value1 = i });
        }

        var state = capture.Read();
        Assert.Equal(new long[] { 2, 3 }, state.Packets.Select(x => x.Sequence));
        Assert.All(state.Packets, x => Assert.Null(x.Key.DeviceId));
        Assert.Equal(1, state.Evicted);
        capture.Clear();
        Assert.Empty(capture.Read().Packets);
        Assert.Equal(0, capture.Read().Evicted);
        fixture.Rx.OnNext(new ExampleMessage1());
        Assert.Equal(4, Assert.Single(capture.Read().Packets).Sequence);
    }

    [Fact]
    public void Capture_EndpointRemovedAndReadded_DoesNotDuplicateSubscriptions()
    {
        using var fixture = new Fixture();
        using var capture = fixture.CreateCapture();
        fixture.EndpointAdded.OnNext(fixture.Endpoint);
        fixture.Rx.OnNext(new ExampleMessage1());
        Assert.Single(capture.Read().Packets);

        fixture.EndpointRemoved.OnNext(fixture.Endpoint);
        fixture.Rx.OnNext(new ExampleMessage1());
        Assert.Single(capture.Read().Packets);
        Assert.Empty(capture.Read().Sources);

        fixture.EndpointAdded.OnNext(fixture.Endpoint);
        fixture.Rx.OnNext(new ExampleMessage1());
        Assert.Equal(2, capture.Read().Packets.Count);
        capture.Dispose();
        fixture.Rx.OnNext(new ExampleMessage1());
        Assert.Empty(capture.Read().Packets);
    }

    [Fact]
    public void Capture_IdentificationFailure_RetainsUnknownPacketAndContinues()
    {
        using var fixture = new Fixture();
        fixture.Identify = _ => throw new InvalidOperationException("identity failure");
        using var capture = fixture.CreateCapture();
        fixture.Rx.OnNext(new ExampleMessage1());
        var state = capture.Read();
        Assert.Null(Assert.Single(state.Packets).Key.DeviceId);
        Assert.Equal("identity failure", state.Packets[0].InspectionError);
        Assert.Equal(1, state.Errors);
        fixture.Rx.OnNext(new ExampleMessage1());
        Assert.Equal(2, capture.Read().Packets.Count);
    }

    [Fact]
    public void Capture_ByteLimit_DropsOversizedPacket()
    {
        using var fixture = new Fixture();
        using var capture = fixture.CreateCapture(bytes: 1024);
        fixture.Rx.OnNext(new ExampleMessage1 { Value3 = new string('x', 4000) });
        Assert.Empty(capture.Read().Packets);
        Assert.Equal(1, capture.Read().Evicted);
    }

    [Fact]
    public void Read_UsesCompactNamesAndDescriptionsWithoutChangingIds()
    {
        using var fixture = new Fixture();
        using var capture = fixture.CreateCapture();
        var state = capture.Read();
        var port = Assert.Single(state.Ports);
        Assert.Equal("port", port.Id);
        Assert.Equal(fixture.PortDisplayName, port.DisplayName);
        Assert.Equal(fixture.PortDescription, port.Description);
        var source = Assert.Single(state.Sources);
        Assert.Equal("port", source.PortId);
        Assert.Equal("endpoint", source.EndpointId);
        Assert.Equal(fixture.PortDisplayName, source.PortName);
        Assert.Equal(fixture.PortDescription, source.PortDescription);
        Assert.Equal(fixture.EndpointDisplayName, source.EndpointName);
        Assert.Equal(fixture.EndpointDescription, source.EndpointDescription);
    }

    [Fact]
    public void Capture_RemovedSourceRetainsPresentationInHistory()
    {
        using var fixture = new Fixture();
        using var capture = fixture.CreateCapture();
        fixture.Rx.OnNext(new ExampleMessage1());
        fixture.EndpointRemoved.OnNext(fixture.Endpoint);
        fixture.PortRemoved.OnNext(fixture.Port);
        var state = capture.Read();
        Assert.Empty(state.Sources);
        Assert.Empty(state.Ports);
        var packet = Assert.Single(state.Packets);
        Assert.Equal("port", packet.Key.PortId);
        Assert.Equal("endpoint", packet.Endpoint);
        Assert.Equal(fixture.PortDisplayName, packet.PortName);
        Assert.Equal(fixture.PortDescription, packet.PortDescription);
        Assert.Equal(fixture.EndpointDisplayName, packet.EndpointName);
        Assert.Equal(fixture.EndpointDescription, packet.EndpointDescription);
    }

    [Fact]
    public void Read_ReflectsRenamingWhilePacketMetadataRemainsImmutable()
    {
        using var fixture = new Fixture();
        using var capture = fixture.CreateCapture();
        fixture.Rx.OnNext(new ExampleMessage1());
        fixture.PortDisplayName = "Renamed port";
        fixture.PortDescription = "Updated port details";
        fixture.EndpointDisplayName = "Renamed endpoint";
        fixture.EndpointDescription = "Updated endpoint details";
        var state = capture.Read();
        Assert.Equal("Renamed port", Assert.Single(state.Ports).DisplayName);
        var source = Assert.Single(state.Sources);
        Assert.Equal("Renamed port", source.PortName);
        Assert.Equal("Updated port details", source.PortDescription);
        Assert.Equal("Renamed endpoint", source.EndpointName);
        Assert.Equal("Updated endpoint details", source.EndpointDescription);
        Assert.Equal("port", source.PortId);
        Assert.Equal("endpoint", source.EndpointId);
        var packet = Assert.Single(state.Packets);
        Assert.Equal("TCP · localhost:1234", packet.PortName);
        Assert.Equal("127.0.0.1:5678", packet.EndpointName);
    }

    private sealed class TestDeviceId(string name) : DeviceId(name)
    {
        public override string AsString() => DeviceClass;
    }

    public class InterfaceStub : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => Handler(targetMethod!, args);
    }

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var result = DispatchProxy.Create<T, InterfaceStub>();
        ((InterfaceStub)(object)result).Handler = handler;
        return result;
    }

    private sealed class Fixture : IDisposable
    {
        public Subject<IProtocolMessage> Rx { get; } = new();
        public Subject<IProtocolMessage> Tx { get; } = new();
        public Subject<Exception> Errors { get; } = new();
        public Subject<IProtocolEndpoint> EndpointAdded { get; } = new();
        public Subject<IProtocolEndpoint> EndpointRemoved { get; } = new();
        public Subject<IProtocolPort> PortAdded { get; } = new();
        public Subject<IProtocolPort> PortRemoved { get; } = new();
        public Func<IProtocolMessage, DeviceId?> Identify { get; set; } = _ => null;
        public string PortDisplayName { get; set; } = "TCP · localhost:1234";
        public string PortDescription { get; set; } = "tcp://localhost:1234/";
        public string EndpointDisplayName { get; set; } = "127.0.0.1:5678";
        public string EndpointDescription { get; set; } = "TCP · 127.0.0.1:1234 → 127.0.0.1:5678";
        public IProtocolPort Port { get; }
        public IProtocolEndpoint Endpoint { get; }
        private readonly IDeviceManager _manager;

        public Fixture()
        {
            Endpoint = Stub<IProtocolEndpoint>((method, _) => method.Name switch
            {
                "get_Id" => "endpoint",
                "get_DisplayName" => EndpointDisplayName,
                "get_Description" => EndpointDescription,
                "get_IsDisposed" => false,
                "get_OnRxMessage" => Rx,
                "get_OnTxMessage" => Tx,
                "get_OnRxError" or "get_OnTxError" => Errors,
                _ => throw new NotSupportedException(method.Name),
            });
            Port = Stub<IProtocolPort>((method, _) => method.Name switch
            {
                "get_Id" => "port",
                "get_DisplayName" => PortDisplayName,
                "get_Description" => PortDescription,
                "get_Config" => new ProtocolPortConfig(new Uri("tcp://localhost:1234")),
                "get_IsDisposed" => false,
                "get_Endpoints" => ImmutableArray.Create(Endpoint),
                "get_EndpointAdded" => EndpointAdded,
                "get_EndpointRemoved" => EndpointRemoved,
                _ => throw new NotSupportedException(method.Name),
            });
            var router = Stub<IProtocolRouter>((method, _) => method.Name switch
            {
                "get_Ports" => ImmutableArray.Create(Port),
                "get_AvailableProtocols" => ImmutableArray<ProtocolInfo>.Empty,
                "get_PortAdded" => PortAdded,
                "get_PortRemoved" => PortRemoved,
                _ => throw new NotSupportedException(method.Name),
            });
            _manager = Stub<IDeviceManager>((method, args) =>
            {
                if (method.Name == "TryIdentify")
                {
                    var device = Identify((IProtocolMessage)args![0]!);
                    args[1] = device;
                    return device != null;
                }
                return method.Name switch
                {
                    "get_Router" => router,
                    _ => throw new NotSupportedException(method.Name),
                };
            });
        }

        public PacketCaptureService CreateCapture(int maximum = 10, long bytes = 1024 * 1024) => new(
            _manager, TimeProvider.System, [new DefaultPacketFieldSource()],
            Options.Create(new PacketInspectorOptions { MaximumPackets = maximum, MaximumBytes = bytes }),
            NullLogger<PacketCaptureService>.Instance);

        public void Dispose()
        {
            Rx.Dispose();
            Tx.Dispose();
            Errors.Dispose();
            EndpointAdded.Dispose();
            EndpointRemoved.Dispose();
            PortAdded.Dispose();
            PortRemoved.Dispose();
        }
    }
}
