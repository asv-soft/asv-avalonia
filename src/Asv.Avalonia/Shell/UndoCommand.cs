using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

public class UndoCommand : IAsyncCommand
{
    public const string Id = "global.undo";
    public static ICommandInfo StaticInfo { get; } = new CommandInfo
    {
        CommandId = Id,
        Name = "Undo",
        Description = "Undo the last action",
        Icon = MaterialIconKind.UndoVariant,
        Order = 0,
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
    public ValueTask Execute(IRoutable context, IPersistable? parameter = null, CancellationToken cancel = default)
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