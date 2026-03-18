using Material.Icons;

namespace Asv.Avalonia.GeoMap;

public sealed class ChangeMapModeCommand : StatelessCommand<StringArg>
{
    #region Static

    public const string Id = $"{BaseId}.settings.map.mode";
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Change map mode",
        Description = "Command that changes map mode",
        Icon = MaterialIconKind.MapOutline,
        DefaultHotKey = null,
    };

    #endregion
    private readonly IMapService _svc;

    public ChangeMapModeCommand(IMapService svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
    }

    public override ICommandInfo Info => StaticInfo;

    protected override bool InternalCanExecute(StringArg arg)
    {
        return Enum.TryParse<MapModeType>(arg.Value, out _);
    }

    protected override ValueTask<StringArg?> InternalExecute(
        StringArg newValue,
        CancellationToken cancel
    )
    {
        var oldValue = _svc.Mode.Value;

        if (!Enum.TryParse<MapModeType>(newValue.Value, out var mode))
        {
            throw new ArgumentException("Invalid mode");
        }

        _svc.Mode.Value = mode;

        return ValueTask.FromResult<StringArg?>(new StringArg(oldValue.ToString()));
    }
}
