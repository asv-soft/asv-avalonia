using System.Collections;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class PropertyEditorWidgetViewModel : PropertyEditorViewModel, IWorkspaceWidget
{
    public PropertyEditorWidgetViewModel(
        NavigationId id,
        string header,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        Header = header;
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);

        //MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public ObservableList<IMenuItem> Menu { get; } = new();

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public string Header
    {
        get;
        set => SetField(ref field, value);
    }

    public WorkspaceDock Position
    {
        get;
        set => SetField(ref field, value);
    } = WorkspaceDock.Left;

    public bool IsExpanded
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public bool CanExpand
    {
        get;
        set => SetField(ref field, value);
    }

    public MenuTree? MenuView => DesignTime.MenuTree;
}
