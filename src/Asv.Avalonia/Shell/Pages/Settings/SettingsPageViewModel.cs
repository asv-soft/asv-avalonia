using System.Composition;
using R3;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsPageViewModel))]
public class SettingsPageView : TreePageView
{
    
}

[ExportPage(PageId)]
public class SettingsPageViewModel : TreePageViewModel<ISettingsPage>, ISettingsPage
{
    public const string PageId = "settings";

    public SettingsPageViewModel()
        : base(PageId, DesignTime.CommandService, NullContainerHost.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        /*Nodes.Add(new TreePageNode("node1", () => new SettingsAppearanceViewModel() ));
        Nodes.Add(new TreePageNode("node2", () => new SettingsAppearanceViewModel() ));
        Nodes.Add(new TreePageNode("node3", () => new SettingsAppearanceViewModel() ));
        Nodes.Add(new TreePageNode("node4", () => new SettingsAppearanceViewModel() ));
        Nodes.Add(new TreePageNode("node5", () => new SettingsAppearanceViewModel() ));
        Nodes.Add(new TreePageNode("node1.1", () => new SettingsAppearanceViewModel(), "node1" ));
        Nodes.Add(new TreePageNode("node1.2", () => new SettingsAppearanceViewModel(), "node1" ));
        Nodes.Add(new TreePageNode("node1.3", () => new SettingsAppearanceViewModel(), "node1" ));
        Nodes.Add(new TreePageNode("node1.4", () => new SettingsAppearanceViewModel(), "node1" ));
        Nodes.Add(new TreePageNode("node1.5", () => new SettingsAppearanceViewModel(), "node1" ));
        Nodes.Add(new TreePageNode("node1.1.1", () => new SettingsAppearanceViewModel(), "node1.1" ));
        Nodes.Add(new TreePageNode("node1.1.2", () => new SettingsAppearanceViewModel(), "node1.1" ));*/
    }

    [ImportingConstructor]
    public SettingsPageViewModel(ICommandService svc, IContainerHost host)
        : base(PageId, svc, host)
    {
        
    }
}