using Material.Icons;

namespace Asv.Avalonia.GeoMap;

public sealed class ChangeMinZoomCommand : StatelessCommand<IntArg>
{
    #region Static

    public const string Id = $"{BaseId}.map.change.min-zoom";
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeMinZoomCommand_CommandInfo_Name,
        Description = RS.ChangeMinZoomCommand_CommandInfo_Description,
        Icon = MaterialIconKind.MagnifyMinusOutline,
        DefaultHotKey = null,
    };

    #endregion

    private readonly IMapService _svc;

    public ChangeMinZoomCommand(IMapService svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
    }

    public override ICommandInfo Info => StaticInfo;

    protected override bool InternalCanExecute(IntArg arg)
    {
        return arg.Value is >= IZoomService.MinZoomLevel and <= IZoomService.MaxZoomLevel;
    }

    protected override ValueTask<IntArg?> InternalExecute(IntArg newValue, CancellationToken cancel)
    {
        var value = (int)newValue.Value;
        var oldValue = _svc.MinZoom.Value;

        if (value > _svc.MaxZoom.Value)
        {
            throw new CommandException("Min zoom cannot be greater than max zoom");
        }

        _svc.MinZoom.Value = value;

        return ValueTask.FromResult<IntArg?>(new IntArg(oldValue));
    }
}
