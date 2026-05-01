using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public interface IShellHost
{
    void Init(IShell shell, TopLevel topLevel);
    IDisposable ExecuteNowOrWhenShellLoaded(Action<IShell, TopLevel> action);
    IShell? Shell { get; }
    TopLevel? TopLevel { get; }
}
