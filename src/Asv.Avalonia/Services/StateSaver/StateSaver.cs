using Asv.Cfg;
using R3;

namespace Asv.Avalonia;

public class StateSaver<TConfig> : IStateSaver<TConfig>
    where TConfig : new()
{
    private readonly IConfiguration _configuration;

    public TConfig Config { get; }

    internal StateSaver(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Config = configuration.Get<TConfig>();
    }

    public IDisposable Add<T>(
        Observable<T> source,
        Action<T, TConfig> applyToConfig,
        bool saveImmediately = true,
        bool skipInitial = true
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(applyToConfig);

        var first = true;
        var subscription = source.Subscribe(value =>
        {
            if (skipInitial && first)
            {
                first = false;
                return;
            }
            first = false;

            applyToConfig(value, Config);

            if (saveImmediately)
            {
                _configuration.Set(Config);
            }
        });

        return subscription;
    }
}
