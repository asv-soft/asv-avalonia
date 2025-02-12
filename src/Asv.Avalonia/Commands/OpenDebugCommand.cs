using System.Composition;
using Avalonia;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class OpenDebugCommandFactory : ICommandFactory
{
    private readonly ExportFactory<IDebugWindow> _factory;

    [ImportingConstructor]
    public OpenDebugCommandFactory(ExportFactory<IDebugWindow> factory)
    {
        _factory = factory;
    }

    public ICommandInfo Info => OpenDebugCommand.StaticInfo;

    public IAsyncCommand Create()
    {
        return new OpenDebugCommand(_factory);
    }

    public bool CanExecute(IRoutable context, out IRoutable? target)
    {
        target = context;
        return true;
    }
}

public class OpenDebugCommand(ExportFactory<IDebugWindow> factory) : IAsyncCommand
{
    public const string Id = "cmd.open.debug";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenDebugCommand_CommandInfo_Name,
        Description = RS.OpenDebugCommand_CommandInfo_Description,
        Icon = MaterialIconKind.WindowOpenVariant,
        DefaultHotKey = KeyGesture.Parse("Ctrl+D"),
        Order = 0,
        IsEditable = true,
        Source = "Asv.Avalonia",
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
        var wnd = new DebugWindow() { DataContext = factory.CreateExport().Value };
        wnd.Topmost = true;
        wnd.Show();
        return ValueTask.CompletedTask;
    }
}
