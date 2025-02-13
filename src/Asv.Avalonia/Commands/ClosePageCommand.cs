using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public class ClosePageCommand : IAsyncCommand
{
    public const string Id = "cmd.page.close";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Close page",
        Description = RS.OpenDebugCommand_CommandInfo_Description,
        Icon = MaterialIconKind.CloseBold,
        DefaultHotKey = KeyGesture.Parse("Alt+F4"),
        Order = 0,
        IsEditable = true,
        Source = AppHost.Instance.AppInfo.Name,
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
            return page.TryCloseAsync();
        }

        return ValueTask.CompletedTask;
    }
}

[ExportCommand]
[Shared]
public class ClosePageCommandFactory : ICommandFactory
{
    public ICommandInfo Info => ClosePageCommand.StaticInfo;

    public IAsyncCommand Create()
    {
        return new ClosePageCommand();
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context.FindParentOfType<IShell>()?.SelectedPage.Value;
        return target != null;
    }
}
