using System.Composition;
using Avalonia.Controls.ApplicationLifetimes;

namespace Asv.Avalonia;

[Export(ShellId, typeof(IShell))]
public class MobileShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.mobile";

    [ImportingConstructor]
    public MobileShellViewModel(IContainerHost containerHost)
        : base(containerHost, ShellId)
    {
        // do nothing
    }
}
