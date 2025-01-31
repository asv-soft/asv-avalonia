using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface ISettingsPage : IPage
{
    ObservableList<ITreePageNode> Nodes { get; }
    BindableReactiveProperty<bool> IsCompactMode { get; }
}

public interface ISettingsSubPage : IRoutable
{
    ValueTask Init(ISettingsPage context);
}