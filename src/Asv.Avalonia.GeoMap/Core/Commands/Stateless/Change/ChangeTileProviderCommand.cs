using Material.Icons;

namespace Asv.Avalonia.GeoMap;

public sealed class ChangeTileProviderCommand(IMapService mapService) : StatelessCommand<StringArg>
{
    #region Static

    public const string Id = $"{BaseId}.map.change.tile-provider";
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeTileProviderCommand_CommandInfo_Name,
        Description = RS.ChangeTileProviderCommand_CommandInfo_Description,
        Icon = MaterialIconKind.LayersOutline,
        DefaultHotKey = null,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override bool InternalCanExecute(StringArg arg)
    {
        return mapService.AvailableProviders.Any(x => x.Info.Id == arg.Value);
    }

    protected override ValueTask<StringArg?> InternalExecute(
        StringArg newValue,
        CancellationToken cancel
    )
    {
        var oldValue = mapService.CurrentProvider.Value.Info.Id;

        var provider = mapService.AvailableProviders.FirstOrDefault(x =>
            x.Info.Id == newValue.Value
        );
        if (provider != null)
        {
            mapService.CurrentProvider.Value = provider;
        }

        return ValueTask.FromResult<StringArg?>(new StringArg(oldValue));
    }
}
