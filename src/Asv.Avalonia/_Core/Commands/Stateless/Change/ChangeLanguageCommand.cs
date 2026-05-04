using Material.Icons;

namespace Asv.Avalonia;

public class ChangeLanguageFreeCommand : StatelessCommand
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
    };

    #endregion

    private readonly ILocalizationService _svc;

    public ChangeLanguageFreeCommand(ILocalizationService svc)
    {
        ArgumentNullException.ThrowIfNull(svc);
        _svc = svc;
    }

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<CommandArg?> InternalExecute(
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        if (newValue is StringArg themeName)
        {
            var oldValue = _svc.CurrentLanguage.Value.Id;
            var language = _svc.AvailableLanguages.FirstOrDefault(x => x.Id == themeName.Value);
            if (language is not null)
            {
                _svc.CurrentLanguage.Value = language;
            }

            return ValueTask.FromResult<CommandArg?>(new StringArg(oldValue));
        }
        else
        {
            var oldValue = _svc.CurrentLanguage.Value.Id;
            var temp = _svc.AvailableLanguages.ToList();
            var index = temp.IndexOf(_svc.CurrentLanguage.Value);
            index++;
            if (index >= temp.Count)
            {
                index = 0;
            }

            _svc.CurrentLanguage.Value = temp[index];
            return ValueTask.FromResult<CommandArg?>(new StringArg(oldValue));
        }
    }
}
