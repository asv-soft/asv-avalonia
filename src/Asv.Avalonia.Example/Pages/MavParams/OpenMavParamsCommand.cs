using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenMavParamsCommand(INavigationService nav)
    : OpenPageCommandBase(MavParamsPageViewModelViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{MavParamsPageViewModelViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open mavlink params",
        Description = "Open mavlink params editor",
        Icon = MavParamsPageViewModelViewModel.PageIcon,
        Source = SystemModule.Instance,
        DefaultHotKey = null,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
