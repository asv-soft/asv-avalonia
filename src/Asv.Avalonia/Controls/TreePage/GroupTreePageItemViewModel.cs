﻿using Asv.Avalonia.Tree;
using Asv.Common;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class GroupTreePageItemViewModel : TreeSubpage
{
    public GroupTreePageItemViewModel()
        : base(DesignTime.Id)
    {
        var root = new TreePage(
            "item1",
            "Item 1",
            MaterialIconKind.Abacus,
            NavigationId.Empty,
            NavigationId.Empty
        );
        var flatList = new ObservableList<ITreePage>
        {
            root,
            new TreePage("item2", "Item 2", MaterialIconKind.Abacus, NavigationId.Empty, root.Id),
            new TreePage("item3", "Item 3", MaterialIconKind.Abacus, NavigationId.Empty, root.Id),
        };
        var tree = new ObservableTree<ITreePage, NavigationId>(
            flatList,
            NavigationId.Empty,
            x => x.Id,
            x => x.ParentId,
            TreePageComparer.Instance
        );

        Node = tree.Items[0];
        NavigateCommand = new ReactiveCommand<NavigationId>(x => { }).DisposeItWith(Disposable);
    }

    public GroupTreePageItemViewModel(
        ObservableTreeNode<ITreePage, NavigationId> node,
        Func<NavigationId, ValueTask<IRoutable>> navigateCallback
    )
        : base(NavigationId.Empty)
    {
        Node = node;
        NavigateCommand = new ReactiveCommand<NavigationId>(x => navigateCallback(x)).DisposeItWith(
            Disposable
        );
    }

    public ObservableTreeNode<ITreePage, NavigationId> Node { get; }
    public override IExportInfo Source => SystemModule.Instance;
    public ReactiveCommand<NavigationId> NavigateCommand { get; }
}
