using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
public class OpenSettingsCommandFactory : ICommandFactory
{
    public ICommandInfo Info => OpenSettingsCommand.StaticInfo;

    public IAsyncCommand Create()
    {
        return new OpenSettingsCommand();
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context.FindParentOfType<IShell>();
        return target != null;
    }
}

public class OpenSettingsCommand : IAsyncCommand
{
    public const string Id = "cmd.open.settings";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Settings",
        Description = "Open settings",
        Icon = MaterialIconKind.Cog,
        DefaultHotKey = KeyGesture.Parse("Ctrl+F1"),
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

    public async ValueTask Execute(
        IRoutable context,
        IPersistable? parameter = null,
        CancellationToken cancel = default
    )
    {
        var shell = context.FindParentOfType<IShell>();
        if (shell != null)
        {
            await shell.Navigate(SettingsPageViewModel.PageId);
        }
    }
}
