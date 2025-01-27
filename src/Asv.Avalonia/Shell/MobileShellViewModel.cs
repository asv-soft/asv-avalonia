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
        throw new System.NotImplementedException();
    }

    protected override void InternalAddPageToMainTab(IPage export)
    {
        throw new NotImplementedException();
    }
}
