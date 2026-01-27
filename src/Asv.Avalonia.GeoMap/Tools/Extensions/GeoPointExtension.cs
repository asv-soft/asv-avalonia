using Asv.Common;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.GeoMap;

public class GeoPointExtension : MarkupExtension
{
    private double _latitude;
    private double _longitude;

    public double Latitude => _latitude;

    public double Longitude => _longitude;

    public double Altitude { get; set; } = 0.0;

    public GeoPointExtension() { }

    public GeoPointExtension(string latitude, string longitude)
    {
        if (GeoPointLatitude.TryParse(latitude, out _latitude) == false)
        {
            throw new InvalidOperationException($"Invalid latitude format: {latitude}");
        }

        if (GeoPointLongitude.TryParse(longitude, out _longitude) == false)
        {
            throw new InvalidOperationException($"Invalid longitude format: {longitude}");
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return new GeoPoint(_latitude, _longitude, Altitude);
    }
}
