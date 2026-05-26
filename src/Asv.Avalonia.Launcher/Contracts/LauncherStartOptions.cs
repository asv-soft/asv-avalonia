namespace Asv.Avalonia.Launcher.Contracts;

public sealed class LauncherStartOptions
{
    public string TargetPath { get; init; } = string.Empty;
    public IReadOnlyList<string> TargetArgs { get; init; } = [];
    public TimeSpan StartupTimeout { get; init; } = TimeSpan.FromSeconds(60);
    public string PipeName { get; init; } = string.Empty;
    public string SessionToken { get; init; } = string.Empty;
}
