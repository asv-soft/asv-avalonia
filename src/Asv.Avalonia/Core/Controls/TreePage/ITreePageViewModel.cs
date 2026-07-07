using ObservableCollections;

namespace Asv.Avalonia;

public interface ITreePageViewModel : IDesignTimeTreePage
{
    public ObservableList<ITreePageMenuItem> Nodes { get; }
}
