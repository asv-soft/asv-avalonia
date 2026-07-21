using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class GroupTreePageItemViewModel : TreeSubpage
{
    public GroupTreePageItemViewModel()
        : base(DesignTime.Id.TypeId, default)
    {
        DesignTime.ThrowIfNotDesignMode();
        var root = new TreePageMenuItem(
            "item1",
            "Item 1",
            MaterialIconKind.Abacus,
            NavId.Empty,
            NavId.Empty
        )
        {
            Description = "This is a description for Item 1",
            IconColor = AsvColorKind.Error | AsvColorKind.Blink,
        };
        var flatList = new ObservableList<ITreePageMenuItem>
        {
            root,
            new TreePageMenuItem("item2", "Item 2", MaterialIconKind.Abacus, NavId.Empty, root.Id)
            {
                Description = "This is a description for Item 2",
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
            },
            new TreePageMenuItem("item3", "Item 3", MaterialIconKind.Abacus, NavId.Empty, root.Id)
            {
                Description = "This is a description for Item 3",
            },
        };
        var tree = new ObservableTree<ITreePageMenuItem, NavId>(
            flatList,
            NavId.Empty,
            x => x.Id,
            x => x.ParentId,
            TreePageComparer.Instance
        );

        Node = tree.Items[0];
        NavigateCommand = new ReactiveCommand<NavId>(x => { }).DisposeItWith(Disposable);
    }

    public GroupTreePageItemViewModel(
        ObservableTreeNode<ITreePageMenuItem, NavId> node,
        Func<NavId, CancellationToken, ValueTask<IViewModel>> navigateCallback
    )
        : base(NavId.GenerateRandomAsString(), default)
    {
        Node = node;
        NavigateCommand = new ReactiveCommand<NavId>(
            async (x, cancel) => await navigateCallback(x, cancel)
        ).DisposeItWith(Disposable);
    }

    public ObservableTreeNode<ITreePageMenuItem, NavId> Node { get; }
    public ReactiveCommand<NavId> NavigateCommand { get; }
}
