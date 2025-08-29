using R3;

namespace Asv.Avalonia;

public class NullStateFactory<TConfig> : IStateSaver<TConfig>
{
    public TConfig Config { get; }

    public IDisposable StartTracking<T>(
        Observable<T> source,
        Action<T, TConfig> applyToConfig,
        bool skipInitial = true
    )
    {
        return Disposable.Empty;
    }
}
