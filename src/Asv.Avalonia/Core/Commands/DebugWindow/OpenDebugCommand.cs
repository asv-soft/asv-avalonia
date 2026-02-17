using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public class OpenDebugWindowCommand(IServiceProvider factory) : StatelessCommand
{
    #region Static

    public const string Id = $"{BaseId}.open.debug";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenDebugCommand_CommandInfo_Name,
        Description = RS.OpenDebugCommand_CommandInfo_Description,
        Icon = MaterialIconKind.WindowOpenVariant,
        DefaultHotKey = "Ctrl+D",
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<CommandArg?> InternalExecute(
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        var wnd = new DebugWindow
        {
            DataContext = factory.GetService<IDebugWindow>(),
            Topmost = true,
        };
        wnd.Show();
        return ValueTask.FromResult<CommandArg?>(null);
    }
}
