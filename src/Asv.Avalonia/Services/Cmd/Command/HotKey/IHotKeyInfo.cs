using Avalonia.Input;
using R3;

namespace Asv.Avalonia;

public interface IHotKeyInfo : IDisposable
{
    KeyGesture? DefaultHotKey { get; init; }
    ReactiveProperty<KeyGesture?> CustomHotKey { get; init; }
    bool IsDisposed { get; }
}
