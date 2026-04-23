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
        : base(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        var root = new TreePage(
            "item1",
            "Item 1",
            MaterialIconKind.Abacus,
            NavId.Empty,
            NavId.Empty,
            DesignTime.LoggerFactory
        )
        {
            Description = "This is a description for Item 1",
            IconColor = AsvColorKind.Error | AsvColorKind.Blink,
        };
        var flatList = new ObservableList<ITreePage>
        {
            root,
            new TreePage(
                "item2",
                "Item 2",
                MaterialIconKind.Abacus,
                NavId.Empty,
                root.Id,
                DesignTime.LoggerFactory
            )
            {
                Description = "This is a description for Item 2",
                IconColor = AsvColorKind.Error | AsvColorKind.Blink,
            },
            new TreePage(
                "item3",
                "Item 3",
                MaterialIconKind.Abacus,
                NavId.Empty,
                root.Id,
                DesignTime.LoggerFactory
            )
            {
                Description = "This is a description for Item 3",
            },
        };
        var tree = new ObservableTree<ITreePage, NavId>(
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
        ObservableTreeNode<ITreePage, NavId> node,
        Func<NavId, ValueTask<IViewModel>> navigateCallback,
        ILoggerFactory loggerFactory
    )
        : base(NavId.GenerateRandom(), loggerFactory)
    {
        Node = node;
        NavigateCommand = new ReactiveCommand<NavId>(x => navigateCallback(x)).DisposeItWith(
            Disposable
        );
    }

    public ObservableTreeNode<ITreePage, NavId> Node { get; }
    public ReactiveCommand<NavId> NavigateCommand { get; }
}
