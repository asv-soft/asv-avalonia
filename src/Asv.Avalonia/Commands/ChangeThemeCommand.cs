using System.Composition;
using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeThemeCommand : NoContextCommand
{
    #region Static

    public const string Id = $"{BaseId}.theme.change";
    internal static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeThemeCommand_CommandInfo_Name,
        Description = RS.ChangeThemeCommand_CommandInfo_Description,
        Icon = MaterialIconKind.ThemeLightDark,
        DefaultHotKey = KeyGesture.Parse("Ctrl+T"),
        Source = SystemModule.Instance,
    };

    #endregion
    private readonly IThemeService _svc;

    [ImportingConstructor]
    public ChangeThemeCommand(IThemeService svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
    }

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<ICommandArg?> InternalExecute(
        ICommandArg newValue,
        CancellationToken cancel
    )
    {
        var oldValue = _svc.CurrentTheme.Value.Id;
        if (newValue is StringCommandArg memento)
        {
            // execute with parameter
            var theme = _svc.Themes.FirstOrDefault(x => x.Id == memento.Value);
            if (theme != null)
            {
                _svc.CurrentTheme.Value = theme;
            }
        }
        else
        {
            // execute without parameter
            var temp = _svc.Themes.ToList();
            var index = temp.IndexOf(_svc.CurrentTheme.Value);
            index++;
            if (index >= temp.Count)
            {
                index = 0;
            }

            _svc.CurrentTheme.Value = temp[index];
        }

        return ValueTask.FromResult<ICommandArg?>(new StringCommandArg(oldValue));
    }
}
