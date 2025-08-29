using Asv.Cfg;
using R3;

namespace Asv.Avalonia;

public class StateSaver<TConfig> : IStateSaver<TConfig> // TODO: try ta make static
    where TConfig : new()
{
    private readonly IConfiguration _configuration;

    public TConfig Config { get; }

    internal StateSaver(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        Config = configuration.Get<TConfig>();
    }

    public IDisposable StartTracking<T>(
        Observable<T> source,
        Action<T, TConfig> applyToConfig,
        bool skipInitial = true
    )
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(applyToConfig);

        var first = true;
        return source.Subscribe(value =>
        {
            if (skipInitial && first)
            {
                first = false;
                return;
            }
            first = false;

            applyToConfig(value, Config);

            _configuration.Set(Config);
        });
    }
}
