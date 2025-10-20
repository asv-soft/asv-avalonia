using Asv.Common;
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
            NavigationId.Empty,
            NavigationId.Empty,
            NullLayoutService.Instance,
            DesignTime.LoggerFactory
        )
        {
            Description = "This is a description for Item 1",
        };
        var flatList = new ObservableList<ITreePage>
        {
            root,
            new TreePage(
                "item2",
                "Item 2",
                MaterialIconKind.Abacus,
                NavigationId.Empty,
                root.Id,
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
            {
                Description = "This is a description for Item 2",
            },
            new TreePage(
                "item3",
                "Item 3",
                MaterialIconKind.Abacus,
                NavigationId.Empty,
                root.Id,
                NullLayoutService.Instance,
                DesignTime.LoggerFactory
            )
            {
                Description = "This is a description for Item 3",
            },
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
        Func<NavigationId, ValueTask<IRoutable>> navigateCallback,
        ILoggerFactory loggerFactory
    )
        : base(NavigationId.GenerateRandom(), loggerFactory)
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
