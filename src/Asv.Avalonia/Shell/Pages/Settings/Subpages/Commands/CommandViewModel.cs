using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class CommandViewModel : RoutableViewModel
{
    public const string ViewModelBaseId = "hotkey";
    public const string EmptyHotKey = "-";

    private readonly ICommandInfo _base;
    private readonly ICommandService _svc;
    private readonly IDialogService _dialogService;
    private readonly ISearchService _searchService;

    public CommandViewModel(
        ICommandInfo command,
        ICommandService svc,
        ISearchService searchService,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base(new NavigationId(ViewModelBaseId, command.Id), loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(svc);
        ArgumentNullException.ThrowIfNull(searchService);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _svc = svc;
        _dialogService = dialogService;
        _base = command;
        _searchService = searchService;

        var hotkey = svc.GetHotKey(command.Id);
        var editedHotkey = new ReactiveProperty<string?>(
            hotkey == command.DefaultHotKey ? EmptyHotKey : hotkey
        ).DisposeItWith(Disposable);

        EditedHotKey = new HistoricalStringProperty(
            nameof(EditedHotKey),
            editedHotkey,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        HasConflict = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsSelected = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        SyncConflict(EditedHotKey.ModelValue.Value);

        EditedHotKey.ModelValue.Subscribe(SyncConflict).DisposeItWith(Disposable);
        svc.OnHotKey.Subscribe(_ => SyncConflict(EditedHotKey.ModelValue.Value))
            .DisposeItWith(Disposable);
        EditedHotKey
            .ViewValue.Subscribe(value =>
                _svc.SetHotKey(
                    _base.Id,
                    value == EmptyHotKey || string.IsNullOrWhiteSpace(value)
                        ? null
                        : HotKeyInfo.Parse(value)
                )
            )
            .DisposeItWith(Disposable);
    }

    #region Table columns

    public MaterialIconKind Icon => _base.Icon;
    public string Name => _base.Name;
    public string CommandId => _base.Id;
    public string Description => _base.Description;
    public string Source => _base.Source.ModuleName;
    public string? DefaultHotKey => _base.DefaultHotKey;
    public bool IsHotkeyConfigurable => DefaultHotKey is not null;
    public HistoricalStringProperty EditedHotKey { get; }
    public BindableReactiveProperty<bool> HasConflict { get; }

    #endregion

    public BindableReactiveProperty<bool> IsSelected { get; }

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

    public Selection SourceSelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public Selection DefaultHotKeySelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public Selection EditedHotKeySelection
    {
        get;
        private set => SetField(ref field, value);
    }

    public async Task EditCommand()
    {
        var dialog = _dialogService.GetDialogPrefab<HotKeyCaptureDialogPrefab>();
        var payload = new HotKeyCaptureDialogPayload
        {
            Title = RS.HotKeyViewModel_HotKeyCaptureDialog_Title,
            Message = RS.HotKeyViewModel_HotKeyCaptureDialog_Message,
            CurrentHotKey = _base.DefaultHotKey,
        };

        var hotKey = await dialog.ShowDialogAsync(payload);
        if (hotKey is not null)
        {
            EditedHotKey.ViewValue.Value = hotKey;
        }
    }

    private void SyncConflict(string? value)
    {
        if (value is null)
        {
            HasConflict.Value = false;
            return;
        }

        var duplicateExists = _svc
            .Commands.Where(c => c.Id != _base.Id && EditedHotKey.ModelValue.Value != EmptyHotKey)
            .Select(c => _svc.GetHotKey(c.Id))
            .Any(hk => hk == value);

        HasConflict.Value = duplicateExists;
    }

    public bool Filter(string search)
    {
        var isNameMatch = _searchService.Match(Name, search, out var nameMatch);
        var isDescriptionMatch = _searchService.Match(
            Description,
            search,
            out var descriptionMatch
        );
        var isSourceMatch = _searchService.Match(Source, search, out var sourceMatch);
        var isDefaultHotkeyMatch = _searchService.Match(
            DefaultHotKey,
            search,
            out var defaultHotkeyMatch
        );
        var isEditedHotkeyMatch = _searchService.Match(
            EditedHotKey.ViewValue.Value,
            search,
            out var editedHotkeyMatch
        );

        NameSelection = nameMatch;
        DescriptionSelection = descriptionMatch;
        SourceSelection = sourceMatch;
        DefaultHotKeySelection = defaultHotkeyMatch;
        EditedHotKeySelection = editedHotkeyMatch;

        return isNameMatch
            || isDescriptionMatch
            || isSourceMatch
            || isDefaultHotkeyMatch
            || isEditedHotkeyMatch;
    }

    internal void SetHotKey(string? hotKey)
    {
        if (string.IsNullOrWhiteSpace(hotKey))
        {
            _svc.SetHotKey(_base.Id, null);
            EditedHotKey.ModelValue.Value = EmptyHotKey;
        }
        else
        {
            _svc.SetHotKey(_base.Id, HotKeyInfo.Parse(hotKey));
            EditedHotKey.ModelValue.Value = hotKey;
        }
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return EditedHotKey;
    }
}
