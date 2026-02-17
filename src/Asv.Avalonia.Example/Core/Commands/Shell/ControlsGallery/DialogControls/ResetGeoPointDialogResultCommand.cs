using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Material.Icons;

namespace Asv.Avalonia.Example;

public static class ResetGeoPointDialogResultCommandArg
{
    public const string LonKey = "lon";
    public const string LatKey = "lat";
    public const string AltKey = "alt";

    private const double ZeroLon = 0;
    private const double ZeroLat = 0;
    private const double ZeroAlt = 0;

    public static CommandArg Empty =>
        CommandArg.CreateDictionary(
            new Dictionary<string, CommandArg>
            {
                [LonKey] = CommandArg.Double(ZeroLon),
                [LatKey] = CommandArg.Double(ZeroLat),
                [AltKey] = CommandArg.Double(ZeroAlt),
            }
        );
}

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
                [ResetGeoPointDialogResultCommandArg.LonKey] = CommandArg.CreateDouble(
                    context.GeoPointDialogResult.Longitude.ModelValue.Value
                ),
                [ResetGeoPointDialogResultCommandArg.LatKey] = CommandArg.CreateDouble(
                    context.GeoPointDialogResult.Latitude.ModelValue.Value
                ),
                [ResetGeoPointDialogResultCommandArg.AltKey] = CommandArg.CreateDouble(
                    context.GeoPointDialogResult.Altitude.ModelValue.Value
                ),
            }
        );

        if (
            dictArg[ResetGeoPointDialogResultCommandArg.LonKey] is not DoubleArg lonArg
            || dictArg[ResetGeoPointDialogResultCommandArg.LatKey] is not DoubleArg latArg
            || dictArg[ResetGeoPointDialogResultCommandArg.AltKey] is not DoubleArg altArg
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
