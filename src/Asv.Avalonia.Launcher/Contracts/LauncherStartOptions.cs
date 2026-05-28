namespace Asv.Avalonia.Launcher.Contracts;

public sealed class LauncherStartOptions
{
    public string TargetPath { get; init; } = string.Empty;
    public IReadOnlyList<string> TargetArgs { get; init; } = [];
    public TimeSpan StartupTimeout { get; init; }
    public string PipeName { get; init; } = string.Empty;
    public string SessionToken { get; init; } = string.Empty;
}
