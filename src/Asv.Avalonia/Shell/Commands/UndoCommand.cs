using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class UndoCommand : IAsyncCommand
{
    public const string Id = "global.undo";
    public static ICommandInfo StaticInfo { get; } =
        new CommandInfo
        {
            Id = Id,
            Name = RS.UndoCommand_CommandInfo_Name,
            Description = RS.UndoCommand_CommandInfo_Description,
            Icon = MaterialIconKind.UndoVariant,
            DefaultHotKey = KeyGesture.Parse("Ctrl+Z"),
            Order = 0,
            IsEditable = true,
        };

    public IPersistable Save()
    {
        throw new NotImplementedException();
    }

    public void Restore(IPersistable state)
    {
        throw new NotImplementedException();
    }

    public ICommandInfo Info => StaticInfo;

    public ValueTask Execute(
        IRoutable context,
        IPersistable? parameter = null,
        CancellationToken cancel = default
    )
    {
        if (context is IPage page)
        {
            return page.History.UndoAsync(cancel);
        }

        return ValueTask.CompletedTask;
    }
}

[ExportCommand]
[Shared]
public class UndoCommandFactory : ICommandFactory
{
    public ICommandInfo Info => UndoCommand.StaticInfo;

    public IAsyncCommand Create()
    {
        return new UndoCommand();
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context.GetAllToRoot().FirstOrDefault(x => x is IPage);
        return target != null;
    }
}
