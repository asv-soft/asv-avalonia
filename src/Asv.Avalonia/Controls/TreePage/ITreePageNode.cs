using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface ITreePageNode : IViewModel
{
    string? ParentId { get; }
    BindableReactiveProperty<string> Name { get; }
    BindableReactiveProperty<MaterialIconKind> Icon { get; }
    BindableReactiveProperty<string?> Status { get; }
    string? NavigateTo { get; }
}

public class BreadCrumbItem(bool isFirst, ITreePageNode item)
{
    public bool IsFirst { get; } = isFirst;
    public ITreePageNode Item { get; } = item;
}

public class TreePageNode : ViewModelBase, ITreePageNode
{
    public TreePageNode(string id, string? navigateTo, string? parentId = null)
        : base(id)
    {
        NavigateTo = navigateTo;
        ParentId = parentId;
        Name = new BindableReactiveProperty<string>(id);
        Icon = new BindableReactiveProperty<MaterialIconKind>(MaterialIconKind.Tree);
        Status = new BindableReactiveProperty<string?>(null);
    }

    public string? NavigateTo { get; }
    public string? ParentId { get; }
    public BindableReactiveProperty<string> Name { get; }
    public BindableReactiveProperty<MaterialIconKind> Icon { get; }
    public BindableReactiveProperty<string?> Status { get; }
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Name.Dispose();
            Icon.Dispose();
            Status.Dispose();
        }
    }
}