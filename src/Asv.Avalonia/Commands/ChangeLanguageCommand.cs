using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class ChangeLanguageCommand : NoContextCommand
{
    #region Static

    public const string Id = $"{BaseId}.settings.language.change";

    private static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.ChangeLanguageCommand_CommandInfo_Name,
        Description = RS.ChangeLanguageCommand_CommandInfo_Description,
        Icon = MaterialIconKind.Translate,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion

    private readonly ILocalizationService _svc;

    [ImportingConstructor]
    public ChangeLanguageCommand(ILocalizationService svc)
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
        if (newValue is StringCommandArg themeName)
        {
            // execute with parameter
            var oldValue = _svc.CurrentLanguage.Value.Id;
            var language = _svc.AvailableLanguages.FirstOrDefault(x => x.Id == themeName.Value);
            if (language is not null)
            {
                _svc.CurrentLanguage.Value = language;
            }

            return ValueTask.FromResult<ICommandArg?>(new StringCommandArg(oldValue));
        }
        else
        {
            // execute without parameter
            var oldValue = _svc.CurrentLanguage.Value.Id;
            var temp = _svc.AvailableLanguages.ToList();
            var index = temp.IndexOf(_svc.CurrentLanguage.Value);
            index++;
            if (index >= temp.Count)
            {
                index = 0;
            }

            _svc.CurrentLanguage.Value = temp[index];
            return ValueTask.FromResult<ICommandArg?>(new StringCommandArg(oldValue));
        }
    }
}
