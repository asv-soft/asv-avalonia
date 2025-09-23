using ObservableCollections;

namespace Asv.Avalonia;

public interface ITreePageViewModel : IDesignTimeTreePage
{
    public ObservableList<ITreePage> Nodes { get; }
}
