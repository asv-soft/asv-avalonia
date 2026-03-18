using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.GeoMap;

public static class SettingsGeoMapMixin
{
    extension(GeoMapMixin.Builder builder)
    {
        public GeoMapMixin.Builder AddGeoMapSettingsSubPage()
        {
            builder.Parent.Shell.Pages.Settings.AddSubPage<
                SettingsGeoMapViewModel,
                SettingsGeoMapView,
                SettingsGeoMapTreePageMenu
            >(SettingsGeoMapViewModel.PageId);

            return builder;
        }
    }
}
