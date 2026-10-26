using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia.IO;

internal static class PacketJsonLines
{
    public static string Serialize(DateTimeOffset? timestamp, PacketDirection? direction,
        string port, string endpoint, IProtocolMessage message) => JsonConvert.SerializeObject(new
        {
            timestamp,
            direction = direction.HasValue ? direction == PacketDirection.Rx ? "RX" : "TX" : null,
            port,
            endpoint,
            protocol = message.Protocol.Id,
            message,
        }, Formatting.None);
}
