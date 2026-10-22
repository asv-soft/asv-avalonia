using Asv.Modeling;
using Material.Icons;

namespace Asv.Avalonia;

public static class HotKeysSubPageRegistrations
{
    extension(SettingsPageRegistrations.Builder builder)
    {
        public SettingsPageRegistrations.Builder RegisterHotKeysSubPage() =>
            builder.AddSubPage<
                SettingsHotKeysViewModel,
                SettingsHotKeysView,
                SettingsHotKeysTreePageMenu
            >(SettingsHotKeysViewModel.PageId);
    }
}

public sealed class SettingsHotKeysTreePageMenu()
    : TreePageMenuItem(
        SettingsHotKeysViewModel.PageId,
        HotKeyEditorText.Title,
        MaterialIconKind.KeyboardSettings,
        new NavId(SettingsHotKeysViewModel.PageId),
        NavId.Empty
    ) { }
