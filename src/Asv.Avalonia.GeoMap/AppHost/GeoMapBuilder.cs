using Microsoft.Extensions.Options;

namespace Asv.Avalonia.GeoMap;

public class GeoMapBuilder
{
    internal OptionsBuilder<GeoMapOptions> Build(OptionsBuilder<GeoMapOptions> options)
    {
        return options.Configure(config =>
        {
            config.IsTurnedOn = true;
        });
    }
}
