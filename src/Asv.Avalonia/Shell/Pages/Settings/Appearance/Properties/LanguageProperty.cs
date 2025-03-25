using System.Composition;
using R3;

namespace Asv.Avalonia;

public class LanguageProperty : RoutableViewModel
{
    private readonly ILocalizationService _svc;
    private readonly IDialogService _dialogService;
    private bool _internalChange;
    public const string ViewModelId = "language.current";

    public IEnumerable<ILanguageInfo> Items => _svc.AvailableLanguages;
    public BindableReactiveProperty<ILanguageInfo> SelectedItem { get; }

    public LanguageProperty()
        : this(DesignTime.LocalizationService, null!)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public LanguageProperty(ILocalizationService svc, IDialogService dialogService)
        : base(ViewModelId)
    {
        _svc = svc;
        _dialogService = dialogService;
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
        var newValue = new StringCommandParameter(userValue.Id);
        await this.ExecuteCommand(ChangeLanguageCommand.Id, newValue);
        _internalChange = false;

        if (_dialogService.IsImplementedShowYesNoDialogDialog)
        {
            var isReloadReady = await _dialogService.ShowYesNoDialog(
                RS.LanguageProperty_RestartDialog_Title,
                RS.LanguageProperty_RestartDialog_Message
            );

            if (isReloadReady)
            {
                await this.ExecuteCommand(RestartApplicationCommand.Id);
            }
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
