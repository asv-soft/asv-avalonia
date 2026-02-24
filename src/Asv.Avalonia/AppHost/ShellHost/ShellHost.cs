using Asv.Common;
using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public class ShellHost : AsyncDisposableOnce, IShellHost
{
    private readonly Subject<IShell> _onShellLoaded;

    public ShellHost()
    {
        _onShellLoaded = new Subject<IShell>();
    }

    public IShell? Shell
    {
        get;
        set
        {
            field = value;
            if (field == null)
            {
                return;
            }

            _onShellLoaded.OnNext(field);
        }
    }

    public Observable<IShell> OnShellLoaded => _onShellLoaded;

    public TopLevel? TopLevel { get; set; }
}
