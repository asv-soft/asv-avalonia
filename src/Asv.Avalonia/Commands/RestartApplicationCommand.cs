using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class RestartApplicationCommand : ContextCommand<IRoutable>
{
    #region Static

    public const string Id = $"{BaseId}.application.restart";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.RestartApplicationCommand_Info_Name,
        Description = RS.RestartApplicationCommand_Info_Description,
        Icon = MaterialIconKind.Restart,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ICommandArg?> InternalExecute(
        IRoutable context,
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var isForce = false;
        if (newValue is BoolCommandArg b)
        {
            isForce = b.Value;
        }

        if (!isForce)
        {
            var reasons = await context.RequestRestartApplicationApproval();
            if (reasons.Count != 0)
            {
                // TODO: Think about a better approach to handle restrictions
            }
        }

        await context.RequestRestart();

        return null;
    }
}
