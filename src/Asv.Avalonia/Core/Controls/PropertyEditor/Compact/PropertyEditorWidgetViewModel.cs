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
    public PropertyEditorWidgetViewModel()
        : this(DesignTime.Id.TypeId, "Property editor", DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        Icon = MaterialIconKind.Tune;
        IconColor = AsvColorKind.Info5;
        Position = WorkspaceDock.Left;
        ShowHeader = true;
        ItemsSource.Add(new PropertyTextBoxDesign());
        ItemsSource.Add(new PropertyComboBoxDesign());
        ItemsSource.Add(new PropertyToggleButtonGroupDesign());
        ItemsSource.Add(new PropertyUnitDesign());
        ItemsSource.Add(new PropertyToggleSwitchDesign());
        ItemsSource.Add(new PropertyButtonViewModel());
    }

    public PropertyEditorWidgetViewModel(string id, string header, ILoggerFactory loggerFactory)
        : base(id)
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

    public int Order { get; set; }
}
