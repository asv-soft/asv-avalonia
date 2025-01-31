using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;

namespace Asv.Avalonia;

public class DesktopShellViewModel : ShellViewModel
{
    private readonly ICommandService _commandService;

    public DesktopShellViewModel(
        IClassicDesktopStyleApplicationLifetime lifetime,
        ICommandService commandService,
        IContainerHost containerHost
    )
        : base(containerHost)
    {
        _commandService = commandService;
        lifetime.MainWindow = new ShellWindow { DataContext = this };
        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(OnKeyDownCustom, handledEventsToo: true);
    }

    private void OnKeyDownCustom(TopLevel arg1, KeyEventArgs arg2)
    {
        if (KeyGesture.Parse("Ctrl+Z").Matches(arg2))
        {
            arg2.Handled = true;
            if (_commandService.CanExecuteCommand(UndoCommand.Id, SelectedControl.CurrentValue, out var target))
            {
                if (target != null)
                {
                    target.Rise(new ExecuteCommandEvent(target, UndoCommand.Id, null));
                }
            }
        }
        
        if (KeyGesture.Parse("Ctrl+T").Matches(arg2))
        {
            arg2.Handled = true;
            if (_commandService.CanExecuteCommand(ChangeThemeCommand.Id, SelectedControl.CurrentValue, out var target))
            {
                if (target != null)
                {
                    target.Rise(new ExecuteCommandEvent(target, ChangeThemeCommand.Id, null));
                }
            }
        }
    }

    protected override ValueTask CloseAsync(CancellationToken cancellationToken)
    {
        if (Application.Current != null && Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }

        return ValueTask.CompletedTask;
    }

}
