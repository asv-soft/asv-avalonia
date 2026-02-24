using Avalonia.Controls;
using R3;

namespace Asv.Avalonia;

public interface IShellHost
{
    IShell? Shell { get; set; }
    Observable<IShell> OnShellLoaded { get; }
    TopLevel? TopLevel { get; set; }
}
