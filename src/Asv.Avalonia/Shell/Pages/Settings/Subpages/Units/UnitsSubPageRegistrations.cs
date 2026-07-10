namespace Asv.Avalonia;

public static class UnitsSubPageRegistrations
{
    extension(SettingsPageRegistrations.Builder builder)
    {
        public SettingsPageRegistrations.Builder RegisterUnitsSubPage()
        {
            return builder.AddSubPage<
                SettingsUnitsViewModel,
                SettingsUnitsView,
                SettingsUnitTreePageMenu
            >(SettingsUnitsViewModel.PageId);
        }
    }
}
