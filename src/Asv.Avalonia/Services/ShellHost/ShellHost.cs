using Asv.Common;
using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public class ShellHost : IShellHost, IDisposable
{
    private readonly Subject<(IShell, TopLevel)> _onShellLoaded = new();

    public void Init(IShell shell, TopLevel topLevel)
    {
        Shell = shell;
        TopLevel = topLevel;
    }

    public IDisposable ExecuteNowOrWhenShellLoaded(IShellHost.Handler action)
    {
        if (Shell != null && TopLevel != null)
        {
            action(Shell, TopLevel);
            return Disposable.Empty;
        }

        return _onShellLoaded.Subscribe(t => action(t.Item1, t.Item2));
    }

    public IShell? Shell { get; private set; }

    public TopLevel? TopLevel { get; set; }


    public void Dispose()
    {
        _onShellLoaded.Dispose();
    }
}
