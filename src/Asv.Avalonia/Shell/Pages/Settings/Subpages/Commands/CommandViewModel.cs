using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class CommandViewModel : RoutableViewModel
{
    public const string ViewModelBaseId = "hotkey";
    public const string EmptyHotKey = "-";
    private readonly ICommandService _svc;
    private readonly IDialogService _dialogService;

    public CommandViewModel(
        ICommandInfo command,
        ICommandService svc,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(ViewModelBaseId, command.Id), loggerFactory)
    {
        _svc = svc;
        _dialogService = dialogService;
        Base = command;

        var hotkey = svc.GetHotKey(command.Id);
        var editedHotkey = new ReactiveProperty<string?>(
            hotkey == command.DefaultHotKey ? EmptyHotKey : hotkey
        );

        EditedHotKey = new HistoricalStringProperty(
            nameof(EditedHotKey),
            editedHotkey,
            loggerFactory,
            this
        );

        SyncConflict(EditedHotKey.ModelValue.Value);

        EditedHotKey.ModelValue.Subscribe(SyncConflict).DisposeItWith(Disposable);
        svc.OnHotKey.Subscribe(_ => SyncConflict(EditedHotKey.ModelValue.Value))
            .DisposeItWith(Disposable);
        EditedHotKey
            .ViewValue.Subscribe(value =>
                _svc.SetHotKey(
                    Base.Id,
                    value == EmptyHotKey ? null : HotKeyInfo.Parse(value ?? string.Empty)
                )
            )
            .DisposeItWith(Disposable);
    }

    #region Table columns

    public MaterialIconKind Icon => Base.Icon;
    public string Name => Base.Name;
    public string Description => Base.Description;
    public string Source => Base.Source.ModuleName;
    public string? DefaultHotKey => Base.DefaultHotKey;
    public bool IsHotkeyConfigurable => DefaultHotKey is not null;
    public HistoricalStringProperty EditedHotKey { get; }
    public BindableReactiveProperty<bool> HasConflict { get; } = new();

    #endregion

    public Selection NameSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public Selection DescriptionSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public ICommandInfo Base { get; }

    public async Task EditCommand()
    {
        var dialog = _dialogService.GetDialogPrefab<HotKeyCaptureDialogPrefab>();
        var payload = new HotKeyCaptureDialogPayload
        {
            Title = RS.HotKeyViewModel_HotKeyCaptureDialog_Title,
            Message = RS.HotKeyViewModel_HotKeyCaptureDialog_Message,
            CurrentHotKey = Base.DefaultHotKey,
        };

        var hotKey = await dialog.ShowDialogAsync(payload);
        if (hotKey is not null)
        {
            EditedHotKey.ViewValue.Value = hotKey;
        }
    }

    private void SyncConflict(string? value)
    {
        if (value == null)
        {
            HasConflict.Value = false;
            return;
        }

        var duplicateExists = _svc
            .Commands.Where(c => c.Id != Base.Id && EditedHotKey.ModelValue.Value != EmptyHotKey)
            .Select(c => _svc.GetHotKey(c.Id))
            .Any(hk => hk == value);

        HasConflict.Value = duplicateExists;
    }

    public bool Filter(string search, ISearchService searchService)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var descriptionMatch = Selection.Empty;

        var result =
            searchService.Match(Name, search, out var nameMatch)
            || searchService.Match(Description, search, out descriptionMatch);

        if (result)
        {
            NameSelection = nameMatch;
            DescriptionSelection = descriptionMatch;
        }
        else
        {
            ResetSelections();
        }

        return result
            || Name.Contains(search, StringComparison.OrdinalIgnoreCase)
            || Description.Contains(search, StringComparison.OrdinalIgnoreCase)
            || Source.Contains(search, StringComparison.OrdinalIgnoreCase)
            || (DefaultHotKey?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
            || (
                EditedHotKey.ViewValue.Value?.Contains(search, StringComparison.OrdinalIgnoreCase)
                ?? false
            );
    }

    public void ResetSelections()
    {
        NameSelection = Selection.Empty;
        DescriptionSelection = Selection.Empty;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return EditedHotKey;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            HasConflict.Dispose();
            EditedHotKey.Dispose();
        }

        base.Dispose(disposing);
    }
}
