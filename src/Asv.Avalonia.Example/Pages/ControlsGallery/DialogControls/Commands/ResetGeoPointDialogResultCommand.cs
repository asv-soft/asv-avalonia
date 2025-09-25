using System.Collections.Generic;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Material.Icons;

namespace Asv.Avalonia.Example.Commands;

[ExportCommand]
[Shared]
public class ResetGeoPointDialogResultCommand : ContextCommand<DialogControlsPageViewModel>
{
    #region Static

    public const string Id = $"{BaseId}.example.dialogs.geopoint.reset";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ResetGeoPointDialogResultCommand_CommandInfo_Name,
        Description = RS.ResetGeoPointDialogResultCommand_CommandInfo_Description,
        Icon = MaterialIconKind.LockReset,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<CommandArg?> InternalExecute(
        DialogControlsPageViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is not DictArg dictArg)
        {
            throw new CommandArgMismatchException(typeof(DictArg));
        }

        var oldGeopointValue = CommandArg.CreateDictionary(
            new Dictionary<string, CommandArg>
            {
                ["lon"] = CommandArg.CreateDouble(
                    context.GeoPointDialogResult.Longitude.ModelValue.Value
                ),
                ["lat"] = CommandArg.CreateDouble(
                    context.GeoPointDialogResult.Latitude.ModelValue.Value
                ),
                ["alt"] = CommandArg.CreateDouble(
                    context.GeoPointDialogResult.Altitude.ModelValue.Value
                ),
            }
        );

        if (
            dictArg["lon"] is not DoubleArg lonArg
            || dictArg["lat"] is not DoubleArg latArg
            || dictArg["alt"] is not DoubleArg altArg
        )
        {
            throw new CommandArgMismatchException(typeof(StringArg));
        }

        context.GeoPointDialogResult.ModelValue.Value = new GeoPoint(
            latArg.Value,
            lonArg.Value,
            altArg.Value
        );

        return ValueTask.FromResult<CommandArg?>(oldGeopointValue);
    }
}
