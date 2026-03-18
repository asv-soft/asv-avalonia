using ObservableCollections;

namespace Asv.Avalonia;

public interface ISettingsAppearanceSubPage : ISettingsSubPage
{
    public ObservableList<ISettingsAppearanceSection> Sections { get; }
}
