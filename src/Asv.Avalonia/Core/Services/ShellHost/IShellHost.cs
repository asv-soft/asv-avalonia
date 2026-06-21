using R3;

namespace Asv.Avalonia;

public interface IShellHost
{
    public delegate void Handler(IShell shell);
    void Init(IShell shell);
    IDisposable ExecuteNowOrWhenShellLoaded(Handler action);
    IShell? Shell { get; }
}
