using Asv.Common;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public sealed class HotKeyEditorItem : ViewModel
{
    private KeyGesture _gesture;
    private bool _hasConflict;

    public HotKeyEditorItem(
        IHotKeyInfo action,
        KeyGesture gesture,
        Func<HotKeyEditorItem, CancellationToken, ValueTask> edit,
        Func<HotKeyEditorItem, CancellationToken, ValueTask> reset,
        Action<HotKeyEditorItem> clear
    )
        : base(action.ActionId)
    {
        Action = action;
        _gesture = gesture;
        GestureText = Format(gesture);
        IsModified = !gesture.Equals(action.DefaultHotKey);
        IsAssigned = gesture.Key != Key.None;
        EditCommand = new ReactiveCommand((_, token) => edit(this, token)).DisposeItWith(
            Disposable
        );
        ResetCommand = new ReactiveCommand((_, token) => reset(this, token)).DisposeItWith(
            Disposable
        );
        ClearCommand = new ReactiveCommand(_ => clear(this)).DisposeItWith(Disposable);
    }

    public IHotKeyInfo Action { get; }
    public KeyGesture Gesture => _gesture;
    private string _gestureText = string.Empty;
    public string GestureText
    {
        get => _gestureText;
        private set => SetField(ref _gestureText, value);
    }
    public string DefaultText => Format(Action.DefaultHotKey);
    private bool _isModified;
    public bool IsModified
    {
        get => _isModified;
        private set => SetField(ref _isModified, value);
    }
    private bool _isAssigned;
    public bool IsAssigned
    {
        get => _isAssigned;
        private set => SetField(ref _isAssigned, value);
    }
    public bool HasConflict
    {
        get => _hasConflict;
        internal set => SetField(ref _hasConflict, value);
    }
    public ReactiveCommand EditCommand { get; }
    public ReactiveCommand ResetCommand { get; }
    public ReactiveCommand ClearCommand { get; }

    internal void SetGesture(KeyGesture gesture)
    {
        if (!SetField(ref _gesture, gesture, nameof(Gesture)))
        {
            return;
        }

        GestureText = Format(gesture);
        IsModified = !gesture.Equals(Action.DefaultHotKey);
        IsAssigned = gesture.Key != Key.None;
    }

    private static string Format(KeyGesture gesture) =>
        gesture.Key == Key.None ? HotKeyEditorText.Unassigned : gesture.ToString();
}
