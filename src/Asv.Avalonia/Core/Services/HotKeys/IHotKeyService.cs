using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public interface IHotKeyService
{
    Observable<KeyGesture> OnHotKey { get; }
    bool IsHotKeyEnabled { get; set; }
    KeyGesture? this[string hotKeyId] { get; set; }
    IEnumerable<IHotKeyInfo> Actions { get; }
    Observable<(IHotKeyInfo Action, KeyGesture Gesture)> OnHotKeyGestureChanged { get; }
    Observable<bool> ObserveCanExecute(string actionId);
}
