using Asv.IO;

namespace Asv.Avalonia.IO;

public enum PacketDirection
{
    Rx,
    Tx,
}

public sealed record PacketFieldSnapshot
{
    public PacketFieldSnapshot(
        string name,
        string type,
        string value,
        string? units,
        string? description,
        IReadOnlyList<PacketFieldSnapshot> children
    )
    {
        Name = name;
        Type = type;
        Value = value;
        Units = units;
        Description = description;
        Children = children;
    }

    public Field? Definition { get; init; }
    public IFieldType? DataType { get; init; }
    public string CreateDetailsMarkdown(string path) => PacketFieldDetails.CreateMarkdown(this, path);

    public string Name { get; init; }
    public string Type { get; init; }
    public double? NumericValue { get; init; }
    public string Value { get; init; }
    public string? Units { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<PacketFieldSnapshot> Children { get; init; }
}

public sealed record PacketGroupKey
{
    public PacketGroupKey(
        string portId,
        string endpointId,
        PacketDirection direction,
        string protocolId,
        string messageId,
        DeviceId? deviceId
    )
    {
        PortId = portId;
        EndpointId = endpointId;
        Direction = direction;
        ProtocolId = protocolId;
        MessageId = messageId;
        DeviceId = deviceId;
    }

    public string PortId { get; init; }
    public string EndpointId { get; init; }
    public PacketDirection Direction { get; init; }
    public string ProtocolId { get; init; }
    public string MessageId { get; init; }
    public DeviceId? DeviceId { get; init; }
    public DeviceAddress? SourceAddress { get; init; }

    // A source stays in the same group before and after discovery resolves its identity.
    public bool Equals(PacketGroupKey? other) => other is not null
        && PortId == other.PortId
        && EndpointId == other.EndpointId
        && Direction == other.Direction
        && ProtocolId == other.ProtocolId
        && MessageId == other.MessageId
        && SourceAddress == other.SourceAddress
        && (SourceAddress.HasValue || (DeviceId?.DeviceClass == other.DeviceId?.DeviceClass
            && DeviceId?.AsString() == other.DeviceId?.AsString()));

    public override int GetHashCode() => HashCode.Combine(
        PortId,
        EndpointId,
        Direction,
        ProtocolId,
        MessageId,
        SourceAddress,
        SourceAddress.HasValue ? null : DeviceId?.DeviceClass,
        SourceAddress.HasValue ? null : DeviceId?.AsString());
}

public sealed record PacketSnapshot
{
    public PacketSnapshot(
        long sequence,
        DateTimeOffset timestamp,
        long monotonicTimestamp,
        PacketGroupKey key,
        string portName,
        string protocolName,
        string name,
        IReadOnlyList<PacketFieldSnapshot> fields,
        string? inspectionError
    )
    {
        Sequence = sequence;
        Timestamp = timestamp;
        MonotonicTimestamp = monotonicTimestamp;
        Key = key;
        PortName = portName;
        ProtocolName = protocolName;
        Name = name;
        Fields = fields;
        InspectionError = inspectionError;
    }

    public long? FileEndOffset { get; init; }
    public long Sequence { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public long MonotonicTimestamp { get; init; }
    public PacketGroupKey Key { get; init; }
    public string PortName { get; init; }
    public string? PortDescription { get; init; }
    public string? EndpointName { get; init; }
    public string? EndpointDescription { get; init; }
    public string ProtocolName { get; init; }
    public string Name { get; init; }
    public IReadOnlyList<PacketFieldSnapshot> Fields { get; init; }
    public string? InspectionError { get; init; }

    public string Endpoint => Key.EndpointId;
    public string Protocol => ProtocolName;
    public string MessageId => Key.MessageId;
    public string Device => Key.DeviceId?.ToString()
        ?? (Key.SourceAddress is { } address ? $"{address.Family}/{address.Value}" : RS.PacketInspector_Unidentified);
    public string Direction => FileEndOffset != null ? string.Empty : Key.Direction == PacketDirection.Rx ? "RX" : "TX";
    public string Time => FileEndOffset != null ? string.Empty : Timestamp.ToLocalTime().ToString("HH:mm:ss.fff");
    internal long EstimatedBytes => 256L + ((Name.Length + PortName.Length + Endpoint.Length
            + (Key.SourceAddress?.Family.Length ?? 0) + (Key.SourceAddress?.Value.Length ?? 0)
            + (PortDescription?.Length ?? 0) + (EndpointName?.Length ?? 0) + (EndpointDescription?.Length ?? 0)) * 2L)
        + EstimateFields(Fields);

    private static long EstimateFields(IReadOnlyList<PacketFieldSnapshot> fields) =>
        fields.Sum(x => 128L + ((x.Name.Length + x.Type.Length + x.Value.Length
            + (x.Units?.Length ?? 0) + (x.Description?.Length ?? 0)) * 2L) + EstimateFields(x.Children));
}

/// <summary>Supplies visitable sections without coupling the inspector to a protocol.</summary>
public interface IPacketFieldSource
{
    IEnumerable<KeyValuePair<string, IVisitable>> GetSections(IProtocolMessage message);
}

/// <summary>Supports visitable messages and public visitable-valued properties.</summary>
public sealed class DefaultPacketFieldSource : IPacketFieldSource
{
    public IEnumerable<KeyValuePair<string, IVisitable>> GetSections(IProtocolMessage message)
    {
        if (message is IVisitable visitable)
        {
            yield return new(message.Name, visitable);
            yield break;
        }

        foreach (var property in message.GetType().GetProperties())
        {
            if (property.GetIndexParameters().Length == 0 && property.GetMethod is { IsPublic: true }
                && typeof(IVisitable).IsAssignableFrom(property.PropertyType)
                && property.GetValue(message) is IVisitable section)
            {
                yield return new(property.Name, section);
            }
        }
    }
}
