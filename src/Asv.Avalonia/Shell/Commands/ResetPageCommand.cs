using System.Composition;
using Material.Icons;

namespace Asv.Avalonia.Example.Commands;

[ExportCommand]
[Shared]
public class ResetPageCommand : ContextCommand<IResettable>
{
    #region Static

    public const string Id = $"{BaseId}.page.reset";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ResetPageCommand_CommandInfo_Name,
        Description = RS.ResetPageCommand_CommandInfo_Description,
        Icon = MaterialIconKind.LockReset,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<CommandArg?> InternalExecute(
        IResettable context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        var oldValue = context.Reset(newValue);
        return ValueTask.FromResult<CommandArg?>(oldValue);
    }
}
