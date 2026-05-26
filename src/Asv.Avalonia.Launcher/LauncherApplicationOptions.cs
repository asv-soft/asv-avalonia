using Asv.Avalonia.Launcher.Orchestration;
using Avalonia.Controls;

namespace Asv.Avalonia.Launcher;

public sealed class LauncherApplicationOptions
{
    public IReadOnlyList<string>? Args { get; set; }
    public TimeSpan SuccessShutdownDelay { get; set; } = TimeSpan.FromMilliseconds(700);
    public bool ShutdownOnSuccess { get; set; } = true;
    public ILauncherOrchestrator? Orchestrator { get; set; }
    public Func<LauncherViewModel, Control>? CreateView { get; set; }
    public Func<LauncherViewModel, Window>? CreateWindow { get; set; }
    public Action<Window>? ConfigureWindow { get; set; }
}
