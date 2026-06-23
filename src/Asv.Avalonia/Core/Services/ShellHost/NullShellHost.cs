namespace Asv.Avalonia;

public class NullShellHost : ShellHost
{
    public static IShellHost Instance { get; } = new NullShellHost();

    private NullShellHost() { }

    public override IShell? Shell => DesignTimeShellViewModel.Instance;
}
