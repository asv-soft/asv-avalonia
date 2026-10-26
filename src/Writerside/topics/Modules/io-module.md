# IO module

<img src="guide-traffic-inspector.png" dark-src="guide-traffic-inspector.png" alt="Traffic Inspector with source filters, details, and recording controls" thumbnail="true"/>

Open Home → Traffic Inspector. This capture shows the initial inspector state before a matching message is selected; it is not evidence of a connected physical device.

`Asv.Avalonia.IO` connects the shell to protocol ports, device discovery, and inspection tools. It supplies connection settings and dialogs, `IDeviceManager`, the live Packet inspector, and the File inspector.

```c#
using Asv.Avalonia.IO;

builder.RegisterModuleIo();
```

Register this inside your application's existing host configuration, after the framework core and shell. A supplied configuration callback replaces the default registration; call `RegisterDefault()` explicitly if you want to preserve it.

## Supply protocols and device discovery

The module does not make every protocol and device type available automatically. An application supplies `IDeviceManagerExtension` implementations. Each extension can configure protocol parsing and port types, register device factories, supply device icons/colors, and start application-specific behavior.

The example application registers its existing extension as follows:

```c#
using Asv.Avalonia.IO;
using Asv.Avalonia.Example;

builder.RegisterModuleIo();
builder.DeviceManager.RegisterDevice<ExampleDeviceManagerExtension>();
```

`ExampleDeviceManagerExtension` is example-application code, not a type shipped by the IO package. In your own application, substitute your extension implementing these members:

| Member | Responsibility |
|---|---|
| `Configure(IProtocolBuilder)` | Register parsers, message factories, and supported port types |
| `Configure(IDeviceExplorerBuilder)` | Register device factories/discovery behavior |
| `TryGetIcon` / `TryGetDeviceBrush` | Customize discovered-device presentation |
| `Run(IDeviceManager)` | Start extension-specific behavior after manager creation |

Inspect `Asv.Avalonia.Example/Core/Services/DeviceManager/ExampleDeviceManagerExtension.cs` for a complete implementation. It registers the example protocol, several GNSS and MAVLink protocols, TCP/UDP/serial transports, and simulated example devices. Those registrations belong to the example, rather than the IO module defaults.

`IDeviceManager` exposes `Router`, `Explorer`, and `ProtocolFactory`. The module also registers these as `IProtocolRouter`, `IDeviceExplorer`, and `IProtocolFactory`, so consumers can inject the narrower service they need.

## Configure a connection

Open the connection settings page, add a connection, select its transport, and enter the transport-specific address. Connection dialogs cover TCP client, TCP server, UDP, and serial settings. A connection can only operate when its corresponding port type has been registered by the application.

For an application with TCP client support, a router can also be configured directly:

```c#
using Asv.Avalonia.IO;
using Asv.IO;

static void ConnectToSimulator(IDeviceManager manager)
{
    manager.Router.AddPort("tcp://127.0.0.1:8888");
}
```

This adds a runtime port. It does not implement saving connection preferences. The example extension uses the same local endpoint for its simulated devices; avoid adding a duplicate port if that extension already creates it.

## Inspect live traffic

Open **Packet inspector** from the shell. The page groups captured RX/TX messages by their source and message identity, provides filters, shows decoded fields and numeric plots, and exposes recording controls. Available decoded fields depend on the registered protocol and `IPacketFieldSource` implementations.

Capture history is bounded: the defaults retain at most 10,000 packet snapshots and 32 MiB of estimated snapshot data. Old snapshots are evicted as new messages arrive. The visible table is therefore a working inspection window, rather than a permanent traffic archive.

### Record messages

Choose Binary or JSONL in the Recording widget, select **Start recording**, choose a file, then use **Stop** to finish and flush it. Recording includes new RX/TX messages independently of view filters; it does not backfill messages captured before recording started.

| Format | Contents | Intended use |
|---|---|---|
| Binary (`.bin`) | Serialized protocol messages in capture order | Parse again with the application's registered protocol parsers |
| JSONL (`.jsonl`) | One JSON object per line with timestamp, RX/TX direction, port, endpoint, protocol, and message | External analysis with capture metadata |

Binary output is reconstructed from decoded messages through protocol encoders. It is not a byte-for-byte wire trace: malformed/unparsed input and original transport framing are not captured, and timestamps, direction, and endpoint metadata are absent. Messages from multiple endpoints can appear in one file.

Recording uses a bounded writer queue (4,096 entries and 32 MiB of queued bytes). If the disk cannot keep up, recording stops with an error instead of silently producing a complete-looking archive. Check the status and wait for finishing to complete before using the file.

## Inspect a saved file

The **File inspector** feeds file bytes to the application's registered protocol parsers and displays decoded messages and fields. It requires at least one parser; the file extension alone does not register a format.

For a service-side read using the same reader:

```c#
using Asv.Avalonia.IO;

static async Task<int> CountPacketsAsync(
    PacketFileReader reader,
    string path,
    CancellationToken cancellationToken)
{
    var result = await reader.ReadAsync(
        path,
        cancellationToken: cancellationToken);
    return result.State.Packets.Count;
}
```

The reader enforces a maximum of 100,000 messages or 128 MiB of decoded snapshot data. This is a decoded-data limit, not a 128 MiB input-file limit. Use a smaller recording if the limit is reached.

The reader is a binary protocol-stream reader. It does not import the recording widget's JSONL metadata format. Binary files also cannot recover capture timestamps or original RX/TX direction; file snapshots use RX and a file offset. Registration of the matching protocol remains necessary for binary recordings to decode correctly.
