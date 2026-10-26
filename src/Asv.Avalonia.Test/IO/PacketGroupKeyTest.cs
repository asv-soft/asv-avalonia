using Asv.Avalonia.IO;
using Asv.IO;
using Asv.IO.Device;
using Xunit;

namespace Asv.Avalonia.Test.IO;

public class PacketGroupKeyTest
{
    [Fact]
    public void Selection_SameMessageFromDifferentDevicesRetainsSecondDevice()
    {
        var first = CreateKey(new ExampleDeviceId("example", 1));
        var second = CreateKey(new ExampleDeviceId("example", 2));
        var selected = CreateKey(new ExampleDeviceId("example", 2));
        PacketGroupKey[] rows = [first, second];

        var restored = rows.FirstOrDefault(key => key == selected);

        Assert.NotEqual(first, second);
        Assert.Same(second, restored);
    }

    [Fact]
    public void Equality_SameDeviceFromNewIdentificationMatchesDictionaryEntry()
    {
        var original = CreateKey(new ExampleDeviceId("example", 2));
        var next = CreateKey(new ExampleDeviceId("example", 2));
        var rows = new Dictionary<PacketGroupKey, string> { [original] = "selected-row" };

        Assert.Equal(original, next);
        Assert.Equal(original.GetHashCode(), next.GetHashCode());
        Assert.Equal("selected-row", rows[next]);
    }

    [Fact]
    public void Grouping_DistinguishesDevicesAndCombinesTheirSuccessivePackets()
    {
        PacketGroupKey[] packets =
        [
            CreateKey(new ExampleDeviceId("example", 1)),
            CreateKey(new ExampleDeviceId("example", 2)),
            CreateKey(new ExampleDeviceId("example", 1)),
            CreateKey(new ExampleDeviceId("example", 2)),
        ];

        var groups = packets.GroupBy(key => key).ToArray();

        Assert.Equal(2, groups.Length);
        Assert.All(groups, group => Assert.Equal(2, group.Count()));
        Assert.Equal("example.1", groups[0].Key.DeviceId!.AsString());
        Assert.Equal("example.2", groups[1].Key.DeviceId!.AsString());
    }

    [Fact]
    public void Equality_UnidentifiedDevicesRemainSeparateFromIdentifiedDevices()
    {
        var unidentified = CreateKey(null);
        var next = CreateKey(null);
        var identified = CreateKey(new ExampleDeviceId("example", 1));

        Assert.Equal(unidentified, next);
        Assert.Equal(unidentified.GetHashCode(), next.GetHashCode());
        Assert.NotEqual(unidentified, identified);
        Assert.NotEqual(unidentified, unidentified with { Direction = PacketDirection.Tx });
        Assert.NotEqual(unidentified, unidentified with { EndpointId = "other-endpoint" });
        Assert.NotEqual(unidentified, unidentified with { PortId = "other-port" });
        Assert.NotEqual(unidentified, unidentified with { ProtocolId = "other-protocol" });
        Assert.NotEqual(unidentified, unidentified with { MessageId = "other-message" });
    }

    [Fact]
    public void Equality_SourceAddressDistinguishesFamiliesAndSurvivesDiscovery()
    {
        var unknown = CreateKey(null) with { SourceAddress = new DeviceAddress("example", "1") };
        var identified = CreateKey(new ExampleDeviceId("example", 1)) with { SourceAddress = unknown.SourceAddress };
        var otherFamily = unknown with { SourceAddress = new DeviceAddress("mavlink", "1") };
        Assert.Equal(unknown, identified);
        Assert.Equal(unknown.GetHashCode(), identified.GetHashCode());
        Assert.NotEqual(unknown, otherFamily);
        Assert.NotEqual(unknown, CreateKey(null));
        var groups = new Dictionary<PacketGroupKey, string> { [unknown] = "row" };
        Assert.Equal("row", groups[identified]);
    }

    private static PacketGroupKey CreateKey(DeviceId? device) =>
        new("port", "endpoint", PacketDirection.Rx, "protocol", "message", device);
}
