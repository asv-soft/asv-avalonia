using System.Collections;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class PropertyEditorWidgetViewModel : PropertyEditorViewModel, IWorkspaceWidget
{
    public PropertyEditorWidgetViewModel(
        NavId id,
        string header,
        ILoggerFactory loggerFactory
    )
        : base(id, loggerFactory)
    {
        Header = header;
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

    public ObservableList<IMenuItem> Menu { get; } = new();

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind IconColor
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
    } = true;

    public MenuTree? MenuView { get; }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

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
