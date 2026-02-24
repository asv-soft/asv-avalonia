using Avalonia.Controls;

namespace Asv.Avalonia;

public class NullShellHost : ShellHost
{
    public static IShellHost Instance { get; } = new NullShellHost();

    private NullShellHost() { }

    public IShell? Shell
    {
        get => DesignTimeShellViewModel.Instance;
        set
        {
            // do nothing
        }
    }

    public TopLevel? TopLevel { get; set; }
}
