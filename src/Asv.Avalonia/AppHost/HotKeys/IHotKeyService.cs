using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public interface IHotKeyService
{
    Observable<KeyGesture> OnHotKey { get; }
    bool IsHotKeyEnabled { get; set; }
    KeyGesture? this[string hotKeyId] { get; set; }
    IEnumerable<IHotKeyInfo> Actions { get; }
}

public class NullActionsService : IHotKeyService
{
    public static IHotKeyService Instance { get; } = new NullActionsService();

    public Observable<KeyGesture> OnHotKey => Observable.Empty<KeyGesture>();

    public bool IsHotKeyEnabled { get; set; }

    public KeyGesture? this[string hotKeyId]
    {
        get => null;
        set { }
    }

    public IEnumerable<IHotKeyInfo> Actions => [];
}
