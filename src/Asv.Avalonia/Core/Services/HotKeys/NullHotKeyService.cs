using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public class NullHotKeyService : IHotKeyService
{
    public static IHotKeyService Instance { get; } = new NullHotKeyService();

    public Observable<KeyGesture> OnHotKey => Observable.Empty<KeyGesture>();

    public bool IsHotKeyEnabled { get; set; }

    public KeyGesture? this[string hotKeyId]
    {
        get => null;
        set { }
    }

    public IEnumerable<IHotKeyInfo> Actions => [];
    public Observable<(IHotKeyInfo Action, KeyGesture Gesture)> OnHotKeyGestureChanged =>
        Observable.Empty<(IHotKeyInfo Action, KeyGesture Gesture)>();

    public Observable<bool> ObserveCanExecute(string actionId)
    {
        return Observable.Empty<bool>();
    }
}
