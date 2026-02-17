using Asv.Common;
using Material.Icons;

namespace Asv.Avalonia.Plugins;

public sealed class DeletePluginsSourceCommand : ContextCommand<PluginsSourceViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.settings.sources.source.delete";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.DeletePluginsSourceCommand_CommandInfo_Title,
        Description = RS.DeletePluginsSourceCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Delete,
        DefaultHotKey = null,
        Source = PluginManagerModule.Instance,
    };

    #endregion

    private readonly YesOrNoDialogPrefab _yesOrNoDialogPrefab;

    public DeletePluginsSourceCommand(IDialogService dialogService)
    {
        ArgumentNullException.ThrowIfNull(dialogService);

        _yesOrNoDialogPrefab = dialogService.GetDialogPrefab<YesOrNoDialogPrefab>();
    }

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        PluginsSourceViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        var payload = new YesOrNoDialogPayload
        {
            Title = RS.PluginsSourceViewModel_RemoveDialog_Title,
            Message = RS.PluginsSourceViewModel_RemoveDialog_Message,
        };

        var result = await _yesOrNoDialogPrefab.ShowDialogAsync(payload);

        if (result)
        {
            await context.Rise(new RemovePluginsSourceEvent(context, context.Model));
        }

        return null;
    }
}
