using System.Windows.Input;
using R3;

namespace Asv.Avalonia;

internal static class EditHistoryMenuBinding
{
    public static void BindHistoryCommand(
        this EditUndoMenu menu,
        IShellHost shellHost,
        Func<IPage, ICommand?> commandSelector,
        CompositeDisposable contextDispose
    )
    {
        BindHistoryCommandCore(menu, shellHost, commandSelector, contextDispose);
    }

    public static void BindHistoryCommand(
        this EditRedoMenu menu,
        IShellHost shellHost,
        Func<IPage, ICommand?> commandSelector,
        CompositeDisposable contextDispose
    )
    {
        BindHistoryCommandCore(menu, shellHost, commandSelector, contextDispose);
    }

    private static void BindHistoryCommandCore(
        MenuItem menu,
        IShellHost shellHost,
        Func<IPage, ICommand?> commandSelector,
        CompositeDisposable contextDispose
    )
    {
        ArgumentNullException.ThrowIfNull(menu);
        ArgumentNullException.ThrowIfNull(shellHost);
        ArgumentNullException.ThrowIfNull(commandSelector);
        ArgumentNullException.ThrowIfNull(contextDispose);

        shellHost
            .ExecuteNowOrWhenShellLoaded(shell =>
                shell
                    .SelectedPage.DistinctUntilChanged()
                    .Subscribe(page => menu.Command = page is null ? null : commandSelector(page))
                    .AddTo(contextDispose)
            )
            .AddTo(contextDispose);
    }
}
