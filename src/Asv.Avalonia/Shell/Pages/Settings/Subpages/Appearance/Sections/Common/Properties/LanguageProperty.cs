using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class LanguageProperty : PropertyComboBoxViewModel
{
    public const string ViewModelId = "language";

    private readonly ILocalizationService _svc;
    private readonly YesOrNoDialogPrefab _dialog;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoSink;

    public LanguageProperty()
        : this(DesignTime.LocalizationService, NullDialogService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public LanguageProperty(ILocalizationService svc, IDialogService dialog)
        : base(ViewModelId, false)
    {
        _svc = svc;
        _dialog = dialog.GetDialogPrefab<YesOrNoDialogPrefab>();

        Header = RS.SettingsAppearanceView_AppLanguage_Title;
        Description = RS.ChangeLanguageCommand_CommandInfo_Description;
        Icon = MaterialIconKind.Translate;
        IconColor = AsvColorKind.Info6;

        foreach (var language in svc.AvailableLanguages)
        {
            ItemsSource.Add(new LanguageOptionViewModel(language));
        }

        _undoSink = Undo.CreateValueChange<string>("default", ApplyLanguage, ApplyLanguage)
            .DisposeItWith(Disposable);

        svc.CurrentLanguage.Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(value => OnChangeByModel(value))
            .DisposeItWith(Disposable);
        OnChangeByModel(svc.CurrentLanguage.Value);
    }

    protected override async ValueTask ApplyFromUser(
        IHeadlinedViewModel item,
        CancellationToken cancel
    )
    {
        if (item is not LanguageOptionViewModel option)
        {
            return;
        }

        var oldValue = _svc.CurrentLanguage.Value?.Id;
        if (oldValue == option.Language.Id)
        {
            return;
        }

        ApplyLanguage(option.Language.Id);

        if (oldValue is not null)
        {
            _undoSink.Publish(oldValue, option.Language.Id);
        }

        var dialogPayload = new YesOrNoDialogPayload
        {
            Title = RS.LanguageProperty_RestartDialog_Title,
            Message = RS.LanguageProperty_RestartDialog_Message,
        };

        var isReloadReady = await _dialog.ShowDialogAsync(dialogPayload);
        if (!isReloadReady)
        {
            return;
        }

        var restrictions = await this.RequestRestartApplicationApproval(cancel);
        if (restrictions.Count == 0)
        {
            await this.RequestRestart(cancel);
        }
    }

    private void ApplyLanguage(string languageId)
    {
        var language = _svc.AvailableLanguages.FirstOrDefault(x => x.Id == languageId);
        if (language is null)
        {
            return;
        }

        _svc.CurrentLanguage.Value = language;
    }

    private void OnChangeByModel(ILanguageInfo? modelValue)
    {
        ApplyValueFromModel(FindLanguage(modelValue?.Id));
    }

    private LanguageOptionViewModel? FindLanguage(string? languageId)
    {
        return ItemsSource
                .OfType<LanguageOptionViewModel>()
                .FirstOrDefault(x => x.Language.Id == languageId)
            ?? ItemsSource.OfType<LanguageOptionViewModel>().FirstOrDefault();
    }

    private sealed class LanguageOptionViewModel : HeadlinedViewModel
    {
        public LanguageOptionViewModel(ILanguageInfo language)
            : base(language.Id)
        {
            Language = language;
            Header = language.DisplayName;
            Icon = MaterialIconKind.Translate;
            IconColor = GetIconColor(language.Id);
        }

        public ILanguageInfo Language { get; }

        private static AsvColorKind GetIconColor(string languageId)
        {
            return languageId switch
            {
                "en" => AsvColorKind.Info3,
                "ru" => AsvColorKind.Info5,
                _ => AsvColorKind.Info6,
            };
        }
    }
}
