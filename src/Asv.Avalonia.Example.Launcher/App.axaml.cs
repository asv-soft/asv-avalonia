using Asv.Avalonia.Launcher;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example.Launcher;

public partial class App : LauncherApp
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void ConfigureLauncher(LauncherApplicationOptions options)
    {
        options.Args = Program.StartupArgs;
    }
}
