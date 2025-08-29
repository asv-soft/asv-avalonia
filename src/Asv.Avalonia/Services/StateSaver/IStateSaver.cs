using R3;

namespace Asv.Avalonia;

public interface IStateSaver { }

public interface IStateSaver<out TConfig> : IStateSaver
{
    TConfig Config { get; }

    public IDisposable StartTracking<T>(
        Observable<T> source,
        Action<T, TConfig> applyToConfig,
        bool skipInitial = true
    );
}
