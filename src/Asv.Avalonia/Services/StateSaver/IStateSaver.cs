using R3;

namespace Asv.Avalonia;

public interface IStateSaver { }

public interface IStateSaver<out TConfig> : IStateSaver
{
    TConfig Config { get; }

    public IDisposable Add<T>(
        Observable<T> source,
        Action<T, TConfig> applyToConfig,
        bool saveImmediately = true,
        bool skipInitial = true
    );
}
