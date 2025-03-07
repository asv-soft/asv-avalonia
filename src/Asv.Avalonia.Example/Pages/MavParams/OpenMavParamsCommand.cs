using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenMavParamsCommand(INavigationService nav)
    : OpenPageCommandBase(MavParamsPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{MavParamsPageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open mavlink params",
        Description = "Open mavlink params editor",
        Icon = MavParamsPageViewModel.PageIcon,
        Source = SystemModule.Instance,
        DefaultHotKey = null,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
