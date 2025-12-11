using System.Composition;

namespace Asv.Avalonia;

[ExportCommand]
[method: ImportingConstructor]
public class OpenSettingsCommand(INavigationService nav)
    : OpenPageCommandBase(SettingsPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{SettingsPageViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenSettingsCommand_CommandInfo_Name,
        Description = RS.OpenSettingsCommand_CommandInfo_Description,
        Icon = SettingsPageViewModel.PageIcon,
        IconColor = SettingsPageViewModel.PageIconColor,
        DefaultHotKey = "Ctrl+Shift+S",
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;

    protected override bool InternalCanExecute(StringArg arg) => true;
}
