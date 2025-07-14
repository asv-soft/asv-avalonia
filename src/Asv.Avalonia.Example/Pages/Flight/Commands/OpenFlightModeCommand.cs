using System.Composition;
using Avalonia.Input;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenFlightModeCommand(INavigationService nav)
    : OpenPageCommandBase(FlightPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.flight";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open flight mode",
        Description = "Command that opens flight mode map",
        Icon = FlightPageViewModel.PageIcon,
        DefaultHotKey = "Ctrl+F2",
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
