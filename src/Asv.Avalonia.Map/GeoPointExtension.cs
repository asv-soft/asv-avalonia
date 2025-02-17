using Asv.Common;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Map;

public class GeoPointExtension : MarkupExtension
{
    private readonly double _lon;
    private readonly double _lat;
    public double Altitude { get; set; } = 0.0;

    public GeoPointExtension(string latitude, string longitude)
    {
        if (GeoPointLatitude.TryParse(latitude, out _lat) == false)
        {
            throw new InvalidOperationException($"Invalid latitude format: {latitude}");
        }
        if (GeoPointLongitude.TryParse(longitude, out _lon) == false)
        {
            throw new InvalidOperationException($"Invalid longitude format: {longitude}");
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new GeoPoint(_lat, _lon, Altitude);
    }
}
