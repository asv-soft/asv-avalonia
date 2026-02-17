using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class RestartApplicationCommand : ContextCommand<IRoutable>
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
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        IRoutable context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        var isForce = false;
        if (newValue is BoolArg b)
        {
            isForce = b.Value;
        }

        try
        {
            if (!isForce)
            {
                var reasons = await context.RequestRestartApplicationApproval();
                if (reasons.Count != 0)
                {
                    return null;
                }
            }

            await context.RequestRestart();
            return null;
        }
        catch
        {
            // TODO: add logging later
        }

        return null;
    }
}
