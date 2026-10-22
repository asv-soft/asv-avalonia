using Material.Icons;

namespace Asv.Avalonia.IO;

public sealed class PacketDevicePresentation(string name, MaterialIconKind? icon, AsvColorKind color)
{
    public string Name { get; } = name;
    public MaterialIconKind? Icon { get; } = icon;
    public AsvColorKind Color { get; } = color;
}

public sealed class PacketJournalRow(PacketSnapshot packet, PacketDevicePresentation devicePresentation)
{
    public PacketSnapshot Packet { get; } = packet;
    public PacketDevicePresentation DevicePresentation { get; } = devicePresentation;
    public long Sequence => Packet.Sequence;
    public string Time => Packet.Time;
    public string Name => Packet.Name;
    public string Protocol => Packet.Protocol;
    public string Endpoint => string.IsNullOrWhiteSpace(Packet.EndpointName) ? Packet.Endpoint : Packet.EndpointName;
    public string Device => Packet.Device;
    public string Direction => Packet.Direction;
}
