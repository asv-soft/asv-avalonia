namespace Asv.Avalonia;

public class DesignTimeShellViewModel : ShellViewModel
{
    public DesignTimeShellViewModel()
        : base(NullContainerHost.Instance) { }

    protected override void InternalAddPageToMainTab(IPage export)
    {
        // ignore
    }
}
