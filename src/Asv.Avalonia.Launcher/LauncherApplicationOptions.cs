using Avalonia.Controls;
using Material.Icons;

namespace Asv.Avalonia.Launcher;

public sealed class LauncherApplicationOptions
{
    public IReadOnlyList<string>? Args { get; set; }
    public TimeSpan SuccessShutdownDelay { get; set; } = TimeSpan.FromMilliseconds(700);
    public bool ShutdownOnSuccess { get; set; } = true;
    public Uri? IconSource { get; set; }
    public MaterialIconKind? IconKind { get; set; } = MaterialIconKind.Abacus;
    public string Title { get; set; } = "ASV Launcher";
    public string Description { get; set; } = "Application startup";
    public string Footer { get; set; } = "made by ASV.SOFT";
    public Func<LauncherViewModel, Control>? CreateView { get; set; }
    public Action<Window>? ConfigureWindow { get; set; }
}
