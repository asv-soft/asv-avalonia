using Asv.Modeling;
using ObservableCollections;

namespace Asv.Avalonia;

public class TreePageMenu : ObservableTree<ITreePage, NavId>
{
    public TreePageMenu(IReadOnlyObservableList<ITreePage> flatList)
        : base(
            flatList,
            NavId.Empty,
            x => x.Id,
            x => x.ParentId,
            TreePageComparer.Instance,
            (item, list, key, parent, comparer, transform, node) =>
                new TreePageMenuNode(item, list, key, parent, comparer, transform, node)
        ) { }
}
