using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.GeoMap;

public static class SettingsGeoMapRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public PagesRegistrations.Builder RegisterGeoMapSettingsSubPage()
        {
            builder.AppBuilder.Settings.AddSubPage<
                SettingsGeoMapViewModel,
                SettingsGeoMapView,
                SettingsGeoMapTreePageMenu
            >(SettingsGeoMapViewModel.PageId);

            return builder;
        }
    }
}
