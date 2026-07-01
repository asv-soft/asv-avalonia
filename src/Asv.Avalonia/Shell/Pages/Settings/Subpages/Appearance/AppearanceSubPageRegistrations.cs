namespace Asv.Avalonia;

public static class AppearanceSubPageRegistrations
{
    extension(SettingsPageRegistrations.Builder builder)
    {
        public SettingsPageRegistrations.Builder RegisterAppearanceSubPage()
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                CommonAppearanceSettingsSectionViewModel,
                CommonAppearanceSettingsSectionView
            >();
            builder.AppBuilder.Extensions.Register<
                SettingsAppearanceViewModel,
                SettingsAppearanceExtension
            >();
            builder.AppBuilder.Extensions.Register<
                ISettingsAppearanceSubPage,
                SettingsAppearanceExtension
            >();

            builder.AppBuilder.Settings.AddSubPage<
                SettingsAppearanceViewModel,
                SettingsAppearanceView,
                AppearanceSettingTreePageMenu
            >(SettingsAppearanceViewModel.PageId);

            return builder;
        }
    }
}
