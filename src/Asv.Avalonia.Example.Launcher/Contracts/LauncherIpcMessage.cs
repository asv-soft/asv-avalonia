using System;

namespace Asv.Avalonia.Example.Launcher.Contracts;

public sealed class LauncherIpcMessage
{
    public string ProtocolVersion { get; init; } = LauncherIpcConstants.ProtocolVersion;
    public string SessionToken { get; init; } = string.Empty;
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
    public LauncherSignal Signal { get; init; } = new(LauncherSignalType.Progress);
}
