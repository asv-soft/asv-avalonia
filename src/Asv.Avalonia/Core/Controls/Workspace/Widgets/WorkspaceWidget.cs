using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class WorkspaceWidget : HeadlinedViewModel, IWorkspaceWidget
{
    public WorkspaceWidget(NavId id, ILoggerFactory loggerFactory)
        : base(id, loggerFactory)
    {
        Menu = [];
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public WorkspaceDock Position
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsExpanded
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public bool CanExpand
    {
        get;
        set => SetField(ref field, value);
    } = true;
    public ObservableList<IMenuItem> Menu { get; }
    public MenuTree? MenuView { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        foreach (var item in Menu)
        {
            yield return item;
        }

        foreach (var item in base.GetChildren())
        {
            yield return item;
        }
    }
}
