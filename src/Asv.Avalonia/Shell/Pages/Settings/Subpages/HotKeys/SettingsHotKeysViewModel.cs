using System.Collections.ObjectModel;
using System.Globalization;
using Asv.Common;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public sealed class SettingsHotKeysViewModel : SettingsSubPage
{
    public const string PageId = "hotkeys";
    private readonly IHotKeyService _hotKeys;
    private readonly IDialogService _dialogs;
    private readonly HotKeyEditorItem[] _items;
    private Dictionary<string, KeyGesture> _saved = [];
    private string? _searchText;
    private int _filterIndex;
    private bool _applying;
    private bool _externalChanges;
    private bool _hasChanges;
    private bool _hasConflicts;
    private string _status = string.Empty;
    private string _count = string.Empty;

    public SettingsHotKeysViewModel()
        : this(
            NullTreeSubPageContext<SettingsPageViewModel>.Instance,
            DesignTime.HotKeyService,
            DesignTime.DialogService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsHotKeysViewModel(
        ITreeSubPageContext<ISettingsPage> context,
        IHotKeyService hotKeys,
        IDialogService dialogs
    )
        : base(PageId, context)
    {
        _hotKeys = hotKeys;
        _dialogs = dialogs;
        _items = hotKeys
            .Actions.OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(action =>
                new HotKeyEditorItem(
                    action,
                    hotKeys[action.ActionId] ?? action.DefaultHotKey,
                    EditAsync,
                    ResetAsync,
                    item => SetGesture(item, new KeyGesture(Key.None))
                ).DisposeItWith(Disposable)
            )
            .ToArray();
        foreach (var item in _items)
        {
            item.SetParent(this);
        }

        ApplyCommand = new ReactiveCommand(_ => Apply()).DisposeItWith(Disposable);
        CancelCommand = new ReactiveCommand(_ => Reload()).DisposeItWith(Disposable);
        ResetAllCommand = new ReactiveCommand(_ => ResetAll()).DisposeItWith(Disposable);
        hotKeys
            .OnHotKeyGestureChanged.ObserveOnUIThreadDispatcher()
            .Subscribe(_ =>
            {
                if (_applying || IsDisposed)
                {
                    return;
                }

                if (HasChanges)
                {
                    _externalChanges = true;
                    Update();
                }
                else
                {
                    Reload();
                }
            })
            .DisposeItWith(Disposable);
        Reload();
    }

    public ObservableCollection<HotKeyEditorItem> Items { get; } = [];
    public string[] Filters { get; } =
    [
        HotKeyEditorText.All,
        HotKeyEditorText.Modified,
        HotKeyEditorText.Unassigned,
        HotKeyEditorText.Conflicts,
    ];
    public string? SearchText
    {
        get => _searchText;
        set
        {
            if (SetField(ref _searchText, value))
            {
                UpdateFilter();
            }
        }
    }
    public int FilterIndex
    {
        get => _filterIndex;
        set
        {
            if (SetField(ref _filterIndex, value))
            {
                UpdateFilter();
            }
        }
    }
    public bool HasChanges
    {
        get => _hasChanges;
        private set => SetField(ref _hasChanges, value);
    }
    public bool HasConflicts
    {
        get => _hasConflicts;
        private set => SetField(ref _hasConflicts, value);
    }
    private bool _canApply;
    public bool CanApply
    {
        get => _canApply;
        private set => SetField(ref _canApply, value);
    }
    private bool _canCancel;
    public bool CanCancel
    {
        get => _canCancel;
        private set => SetField(ref _canCancel, value);
    }
    private bool _isEmpty;
    public bool IsEmpty
    {
        get => _isEmpty;
        private set => SetField(ref _isEmpty, value);
    }
    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }
    public string Count
    {
        get => _count;
        private set => SetField(ref _count, value);
    }
    public ReactiveCommand ApplyCommand { get; }
    public ReactiveCommand CancelCommand { get; }
    public ReactiveCommand ResetAllCommand { get; }

    private async ValueTask EditAsync(HotKeyEditorItem item, CancellationToken token)
    {
        using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token, DisposeCancel);
        token = lifetime.Token;
        var wasEnabled = _hotKeys.IsHotKeyEnabled;
        _hotKeys.IsHotKeyEnabled = false;
        try
        {
            var gesture = await _dialogs
                .GetDialogPrefab<HotKeyCaptureDialogPrefab>()
                .ShowDialogAsync(
                    new HotKeyCaptureDialogPayload
                    {
                        Title = HotKeyEditorText.Capture,
                        Message = string.Format(
                            CultureInfo.CurrentCulture,
                            HotKeyEditorText.CaptureHint,
                            item.Action.Name
                        ),
                        CancellationToken = token,
                    }
                );
            token.ThrowIfCancellationRequested();
            if (gesture is not null)
            {
                await AssignAsync(item, gesture, token);
            }
        }
        finally
        {
            _hotKeys.IsHotKeyEnabled = wasEnabled;
        }
    }

    private ValueTask ResetAsync(HotKeyEditorItem item, CancellationToken token) =>
        AssignAsync(item, item.Action.DefaultHotKey, token);

    /// <summary>
    /// Updates the draft. A conflicting shortcut is left unchanged unless replacement is explicit.
    /// </summary>
    public bool TryAssign(HotKeyEditorItem item, KeyGesture gesture, bool replaceConflicts = false)
    {
        ArgumentNullException.ThrowIfNull(item);
        ArgumentNullException.ThrowIfNull(gesture);
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (!_items.Contains(item))
        {
            throw new ArgumentException(
                "The command does not belong to this editor.",
                nameof(item)
            );
        }

        var conflicts = FindConflicts(item, gesture);
        if (conflicts.Length != 0 && !replaceConflicts)
        {
            return false;
        }

        foreach (var conflict in conflicts)
        {
            conflict.SetGesture(new KeyGesture(Key.None));
        }

        SetGesture(item, gesture);
        return true;
    }

    private HotKeyEditorItem[] FindConflicts(HotKeyEditorItem item, KeyGesture gesture) =>
        _items
            .Where(x => x != item && gesture.Key != Key.None && x.Gesture.Equals(gesture))
            .ToArray();

    private async ValueTask AssignAsync(
        HotKeyEditorItem item,
        KeyGesture gesture,
        CancellationToken token
    )
    {
        token.ThrowIfCancellationRequested();
        if (IsDisposed || TryAssign(item, gesture))
        {
            return;
        }

        var confirmed = await _dialogs.ShowYesOrNoDialogAsync(
            HotKeyEditorText.Replace,
            string.Format(
                CultureInfo.CurrentCulture,
                HotKeyEditorText.ConflictMessage,
                gesture,
                string.Join(", ", FindConflicts(item, gesture).Select(x => x.Action.Name))
            )
        );
        token.ThrowIfCancellationRequested();
        if (confirmed == true && !IsDisposed)
        {
            TryAssign(item, gesture, replaceConflicts: true);
        }
    }

    private void SetGesture(HotKeyEditorItem item, KeyGesture gesture)
    {
        item.SetGesture(gesture);
        Update();
    }

    private void ResetAll()
    {
        foreach (var item in _items)
        {
            item.SetGesture(item.Action.DefaultHotKey);
        }

        Update();
    }

    private void Apply()
    {
        if (!CanApply)
        {
            return;
        }

        _applying = true;
        try
        {
            _hotKeys.ApplyHotKeys(
                _items.ToDictionary(x => x.Action.ActionId, x => (KeyGesture?)x.Gesture)
            );
            Reload();
        }
        finally
        {
            _applying = false;
        }
    }

    private void Reload()
    {
        foreach (var item in _items)
        {
            item.SetGesture(_hotKeys[item.Action.ActionId] ?? item.Action.DefaultHotKey);
        }

        _saved = _items.ToDictionary(x => x.Action.ActionId, x => x.Gesture);
        _externalChanges = false;
        Update();
    }

    private void Update()
    {
        foreach (var item in _items)
        {
            item.HasConflict =
                item.IsAssigned && _items.Any(x => x != item && x.Gesture.Equals(item.Gesture));
        }

        HasChanges = _items.Any(x =>
            !_saved.GetValueOrDefault(x.Action.ActionId, x.Gesture).Equals(x.Gesture)
        );
        HasConflicts = _items.Any(x => x.HasConflict);
        Status =
            _externalChanges ? HotKeyEditorText.ExternalChanges
            : HasConflicts ? HotKeyEditorText.Conflicts
            : HasChanges ? HotKeyEditorText.Pending
            : HotKeyEditorText.Saved;
        CanApply = HasChanges && !HasConflicts && !_externalChanges;
        CanCancel = HasChanges || _externalChanges;
        UpdateFilter();
    }

    private void UpdateFilter()
    {
        Items.Clear();
        var query =
            SearchText?.Replace(" ", string.Empty, StringComparison.Ordinal) ?? string.Empty;
        foreach (var item in _items)
        {
            var text =
                $"{item.Action.Name} {item.Action.Description} {item.Action.ActionId} {item.GestureText}";
            if (
                !text.Replace(" ", string.Empty, StringComparison.Ordinal)
                    .Contains(query, StringComparison.CurrentCultureIgnoreCase)
            )
            {
                continue;
            }

            if (
                FilterIndex switch
                {
                    1 => !item.IsModified,
                    2 => item.IsAssigned,
                    3 => !item.HasConflict,
                    _ => false,
                }
            )
            {
                continue;
            }

            Items.Add(item);
        }

        Count = string.Format(
            CultureInfo.CurrentCulture,
            HotKeyEditorText.Count,
            Items.Count,
            _items.Length
        );
        IsEmpty = Items.Count == 0;
    }

    public override IEnumerable<IViewModel> GetChildren() => _items.Concat(base.GetChildren());
}
