namespace Asv.Avalonia.Launcher.Api;

public sealed class LauncherIpcMessage
{
    public string ProtocolVersion { get; init; } = LauncherIpcConstants.ProtocolVersion;
    public string SessionToken { get; init; } = string.Empty;
    public DateTimeOffset TimestampUtc { get; init; }
    public LauncherSignal Signal { get; init; } = new(LauncherSignalType.Progress);
}
