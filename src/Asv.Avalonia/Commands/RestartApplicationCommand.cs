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
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<ICommandParameter?> InternalExecute(
        IRoutable context,
        ICommandParameter newValue,
        CancellationToken cancel
    )
    {
        var isForce = false;
        if (newValue is BoolCommandParameter b)
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
