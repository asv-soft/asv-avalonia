using Asv.Common;
using Asv.Modeling;
using Material.Icons;
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
        );
        var flatList = new ObservableList<ITreePageMenuItem>
        {
            root,
            new TreePageMenuItem(
                "item2",
                "Short item",
                MaterialIconKind.Folder,
                NavId.Empty,
                root.Id
            )
            {
                IconColor = AsvColorKind.Info1,
            },
            new TreePageMenuItem(
                "item3",
                "Item with description",
                MaterialIconKind.FileDocument,
                NavId.Empty,
                root.Id
            )
            {
                Description = "A short description that fits on one line.",
                IconColor = AsvColorKind.Info2,
            },
            new TreePageMenuItem(
                "item4",
                "A longer item title to check text trimming",
                MaterialIconKind.Cloud,
                NavId.Empty,
                root.Id
            )
            {
                Description =
                    "A longer description that demonstrates text wrapping and trimming when the available width is limited.",
                IconColor = AsvColorKind.Info3,
            },
            new TreePageMenuItem(
                "item5",
                "Item without description",
                MaterialIconKind.Puzzle,
                NavId.Empty,
                root.Id
            )
            {
                IconColor = AsvColorKind.Success,
            },
            new TreePageMenuItem(
                "item6",
                "Another item",
                MaterialIconKind.Cog,
                NavId.Empty,
                root.Id
            )
            {
                Description = "Another short description.",
                IconColor = AsvColorKind.Warning,
            },
            new TreePageMenuItem(
                "item7",
                "VeryLongItemNameWithoutSpacesToCheckTrimming",
                MaterialIconKind.Tag,
                NavId.Empty,
                root.Id
            )
            {
                Description = "Description below a long single-word title.",
                IconColor = AsvColorKind.Error,
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
