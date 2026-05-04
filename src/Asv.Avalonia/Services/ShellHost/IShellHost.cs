using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public interface IShellHost
{
    public delegate void Handler(IShell shell, TopLevel topLevel);
    void Init(IShell shell, TopLevel topLevel);
    IDisposable ExecuteNowOrWhenShellLoaded(Handler action);
    IShell? Shell { get; }
    TopLevel? TopLevel { get; }
}
