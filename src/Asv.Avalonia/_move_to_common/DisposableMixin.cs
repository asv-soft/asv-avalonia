using R3;

namespace Asv.Avalonia;

public static class DisposableMixin
{
    public static T DisposeItWith<T>(this T src, DisposableBag disposable)
        where T : IDisposable
    {
        disposable.Add(src);
        return src;
    }
}
