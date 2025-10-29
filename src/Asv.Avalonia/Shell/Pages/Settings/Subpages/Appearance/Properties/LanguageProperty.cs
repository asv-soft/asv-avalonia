using System.Composition;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class LanguageProperty : RoutableViewModel
{
    public const string ViewModelId = "language.current";

    private readonly ILocalizationService _svc;
    private readonly YesOrNoDialogPrefab _dialog;
    private bool _internalChange;

    public IEnumerable<ILanguageInfo> Items => _svc.AvailableLanguages;
    public BindableReactiveProperty<ILanguageInfo> SelectedItem { get; }

    public LanguageProperty()
        : this(DesignTime.LocalizationService, NullDialogService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public LanguageProperty(
        ILocalizationService svc,
        IDialogService dialog,
        ILoggerFactory loggerFactory
    )
        : base(ViewModelId, loggerFactory)
    {
        _svc = svc;
        _dialog = dialog.GetDialogPrefab<YesOrNoDialogPrefab>();
        SelectedItem = new BindableReactiveProperty<ILanguageInfo>(
            svc.CurrentLanguage.CurrentValue
        );
        _internalChange = true;
        _sub1 = SelectedItem.SubscribeAwait(OnChangedByUser);
        _sub2 = svc.CurrentLanguage.Subscribe(OnChangeByModel);
        _internalChange = false;
    }

    private async ValueTask OnChangedByUser(ILanguageInfo userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _internalChange = true;
        var newValue = new StringArg(userValue.Id);
        await this.ExecuteCommand(ChangeLanguageFreeCommand.Id, newValue, cancel: cancel);
        _internalChange = false;

        var dialogPayload = new YesOrNoDialogPayload
        {
            Title = RS.LanguageProperty_RestartDialog_Title,
            Message = RS.LanguageProperty_RestartDialog_Message,
        };

        var isReloadReady = await _dialog.ShowDialogAsync(dialogPayload);

        if (isReloadReady)
        {
            await this.ExecuteCommand(RestartApplicationCommand.Id, cancel: cancel);
        }
    }

    private void OnChangeByModel(ILanguageInfo modelValue)
    {
        _internalChange = true;
        SelectedItem.Value = modelValue;
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
