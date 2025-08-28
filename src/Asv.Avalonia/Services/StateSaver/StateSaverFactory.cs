using System.Composition;
using Asv.Cfg;

namespace Asv.Avalonia;

[Export(typeof(IStateSaverFactory))]
[Shared]
public class StateSaverFactory : IStateSaverFactory
{
    private readonly IConfiguration _configuration;

    [ImportingConstructor]
    public StateSaverFactory(IConfiguration cfg)
    {
        _configuration = cfg;
    }

    public IStateSaver<TConfig> Create<TConfig>()
        where TConfig : new()
    {
        return new StateSaver<TConfig>(_configuration);
    }
}
