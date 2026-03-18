using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.GeoMap;

public static class SettingsGeoMapMixin
{
    extension(SettingsPageMixin.Builder builder)
    {
        public SettingsPageMixin.Builder UseMapSettings()
        {
            builder.Parent.Parent.Parent.Services.AddSingleton<IMapService, MapService>();
            builder.Parent.Parent.Parent.Services.AddSingleton<
                IAsyncCommand,
                ChangeMapModeCommand
            >();

            builder.Parent.Parent.Parent.ViewLocator.RegisterViewFor<
                GeoMapAppearanceSettingsSectionViewModel,
                GeoMapAppearanceSettingsSectionView
            >();
            builder.Parent.Parent.Parent.Extensions.Register<
                ISettingsAppearanceSubPage,
                SettingsAppearanceExtension
            >();

            return builder;
        }
    }
}
