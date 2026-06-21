using R3;

namespace Asv.Avalonia;

public class ShellHost : IShellHost, IDisposable
{
    private readonly Subject<IShell> _onShellLoaded = new();

    public void Init(IShell shell)
    {
        Shell = shell;
        _onShellLoaded.OnNext(shell);
    }

    public IDisposable ExecuteNowOrWhenShellLoaded(IShellHost.Handler action)
    {
        if (Shell != null)
        {
            action(Shell);
            return Disposable.Empty;
        }

        return _onShellLoaded.Subscribe(shell => action(shell));
    }

    public virtual IShell? Shell { get; private set; }

    public void Dispose()
    {
        _onShellLoaded.Dispose();
    }
}
