using Material.Icons;

namespace Asv.Avalonia.GeoMap;

public sealed class ChangeMaxZoomCommand : StatelessCommand<IntArg>
{
    #region Static

    public const string Id = $"{BaseId}.map.change.max-zoom";
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeMaxZoomCommand_CommandInfo_Name,
        Description = RS.ChangeMaxZoomCommand_CommandInfo_Description,
        Icon = MaterialIconKind.MagnifyPlusOutline,
        DefaultHotKey = null,
    };

    #endregion

    private readonly IMapService _svc;

    public ChangeMaxZoomCommand(IMapService svc)
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
        var oldValue = _svc.MaxZoom.Value;

        if (value < _svc.MinZoom.Value)
        {
            throw new CommandException("Max zoom cannot be less than min zoom");
        }

        _svc.MaxZoom.Value = value;

        return ValueTask.FromResult<IntArg?>(new IntArg(oldValue));
    }
}
