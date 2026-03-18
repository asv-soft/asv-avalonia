using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.GeoMap;

public static class SettingsGeoMapMixin
{
    extension(SettingsPageMixin.Builder builder)
    {
        public SettingsPageMixin.Builder UseMapSettings()
        {
            builder.AddSubPage<SettingsMapViewModel, SettingsMapView, SettingsMapTreePageMenu>(
                SettingsMapViewModel.SubPageId
            );

            builder.Parent.Parent.Parent.Services.AddSingleton<IMapService, MapService>();
            builder.Parent.Parent.Parent.Services.AddSingleton<
                IAsyncCommand,
                ChangeMapModeCommand
            >();

            return builder;
        }
    }
}
