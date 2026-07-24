# Log Reader Service

## Overview

`ILogReaderService` reads the log files the application has written to disk and streams them back as
`LogMessage` items. The default `LogReaderService` enumerates the `*.logs` files in the configured
folder and deserializes the JSON entries they contain.

These files are produced by the file logging provider (`RegisterLogToFile`), which writes structured
JSON entries into rolling `yyyy-MM-dd_N.logs` files. The reader is the other half of that pair: it
reads from the same folder and expects the same format. The built-in **Log Viewer** page uses this
service as its data source.

The order of the returned messages is not guaranteed — sort them on the consuming side if you need a
strict chronology.

## Usage

Enumerate the messages with `await foreach`; pass a `CancellationToken` to stop a long enumeration
early:

```C#
await foreach (var message in logReaderService.LoadItemsFromLogFile(cancel))
{
    // handle the entry
}
```

## Registration

The service is part of the core services and by default reads from the standard log folder:

```C#
services.RegisterLogViewer();
```

The folder is taken from the `LogViewer` configuration section (`LogReaderOptions`) and resolved
relative to the application data folder; it defaults to `logs`. To override it in code, pass a
`configure` delegate:

```C#
services.RegisterLogViewer(b => b.WithFolder("mylogs"));
```

> The folder must match the one used by `RegisterLogToFile` — otherwise the reader has nothing to read.
> {style="warning"}

In a design-time environment the registration is skipped entirely; `DesignTime.LogReaderService`
provides `NullLogReaderService.Instance`, which returns no messages.

> Despite the name, `RegisterLogViewer` registers only the reader service. The Log Viewer page itself
> is registered separately by `RegisterLogViewerPage` among the default pages.
> {style="note"}

## API {collapsible="true" default-state="collapsed"}

### [ILogReaderService](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Logger/Reader/ILogReaderService.cs)

Reads saved log files and streams their entries.

| Method                               | Return Type                     | Description                                                                          |
|--------------------------------------|---------------------------------|--------------------------------------------------------------------------------------|
| `LoadItemsFromLogFile(cancel)`       | `IAsyncEnumerable<LogMessage>`  | Reads all `*.logs` files from the configured folder and streams the parsed entries.  |

#### `ILogReaderService.LoadItemsFromLogFile`

| Parameter | Type                | Description                           |
|-----------|---------------------|---------------------------------------|
| `cancel`  | `CancellationToken` | A token that cancels the enumeration. |

### [LogMessage](https://github.com/asv-soft/asv-avalonia/blob/main/src/Asv.Avalonia/Core/Services/Logger/Reader/LogMessage.cs)

Represents a single log entry.

| Property      | Type       | Description                             |
|---------------|------------|-----------------------------------------|
| `Timestamp`   | `DateTime` | Time the entry was written.             |
| `LogLevel`    | `LogLevel` | Severity level.                         |
| `Category`    | `string`   | Logger category the entry belongs to.   |
| `Message`     | `string`   | Log message text.                       |
| `Description` | `string?`  | Optional additional details.            |
