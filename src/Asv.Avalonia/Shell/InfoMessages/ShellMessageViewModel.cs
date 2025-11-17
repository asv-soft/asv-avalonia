using System.Windows.Input;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class ShellMessageViewModel : RoutableViewModel
{
    private readonly ShellMessage _message;

    public ShellMessageViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        ReactiveCommand<ShellMessageViewModel> closeCommand,
        ShellMessage message
    )
        : base(id, loggerFactory)
    {
        _message = message;
        CloseCommand = closeCommand;
        if (message.Duration != null)
        {
            Observable
                .Timer(message.Duration.Value)
                .Subscribe(x => CloseCommand.Execute(this))
                .DisposeItWith(Disposable);
        }
    }

    public ReactiveCommand<ShellMessageViewModel> CloseCommand { get; }
    public MaterialIconKind? Icon => _message.Icon;
    public string Title => _message.Title;
    public string Message => _message.Message;
    public ShellErrorState Severity => _message.Severity;
    public string? CommandTitle => _message.CommandTitle;
    public ICommand? Command => _message.Command;
    public object? CommandParam => _message.CommandParam;
    public string? Description => _message.Description;

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
