using Avalonia.Controls.ApplicationLifetimes;

namespace Asv.Avalonia;

public class MobileShellViewModel : ShellViewModel
{
    public MobileShellViewModel(
        ISingleViewApplicationLifetime lifetime,
        IContainerHost containerHost
    )
        : base(containerHost)
    {
        // do nothing
    }
}
