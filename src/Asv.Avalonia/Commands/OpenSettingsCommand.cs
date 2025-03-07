using System.Composition;
using Avalonia.Input;
using Material.Icons;

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
        Name = "Settings",
        Description = "Open settings",
        Icon = SettingsPageViewModel.PageIcon,
        DefaultHotKey = KeyGesture.Parse("Ctrl+F1"),
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
