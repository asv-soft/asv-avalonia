using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public class LanguageProperty : ViewModel
{
    public const string ViewModelId = "language";

    private readonly ILocalizationService _svc;
    private readonly YesOrNoDialogPrefab _dialog;
    private bool _internalChange;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoSink;

    public IEnumerable<ILanguageInfo> Items => _svc.AvailableLanguages;
    public BindableReactiveProperty<ILanguageInfo> SelectedItem { get; }

    public LanguageProperty()
        : this(DesignTime.LocalizationService, NullDialogService.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public LanguageProperty(ILocalizationService svc, IDialogService dialog)
        : base(ViewModelId)
    {
        _svc = svc;
        _dialog = dialog.GetDialogPrefab<YesOrNoDialogPrefab>();
        SelectedItem = new BindableReactiveProperty<ILanguageInfo>(
            svc.CurrentLanguage.CurrentValue
        ).DisposeItWith(Disposable);
        _internalChange = true;
        SelectedItem.SubscribeAwait(OnChangedByUser).DisposeItWith(Disposable);
        svc.CurrentLanguage.Subscribe(OnChangeByModel).DisposeItWith(Disposable);
        _internalChange = false;
        _undoSink = Undo.CreateValueChange<string>("default", ApplyLanguage, ApplyLanguage)
            .DisposeItWith(Disposable);
    }

    private async ValueTask OnChangedByUser(ILanguageInfo userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        var oldValue = _svc.CurrentLanguage.Value.Id;
        if (oldValue == userValue.Id)
        {
            return;
        }

        try
        {
            _internalChange = true;
            ApplyLanguage(userValue.Id);
            _undoSink.Publish(oldValue, userValue.Id);
        }
        finally
        {
            _internalChange = false;
        }

        var dialogPayload = new YesOrNoDialogPayload
        {
            Title = RS.LanguageProperty_RestartDialog_Title,
            Message = RS.LanguageProperty_RestartDialog_Message,
        };

        var isReloadReady = await _dialog.ShowDialogAsync(dialogPayload);

        if (isReloadReady)
        {
            var restrictions = await this.RequestRestartApplicationApproval(cancel);
            if (restrictions.Count == 0)
            {
                await this.RequestRestart(cancel);
            }
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

    private void OnChangeByModel(ILanguageInfo modelValue)
    {
        _internalChange = true;
        SelectedItem.Value = modelValue;
        _internalChange = false;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
