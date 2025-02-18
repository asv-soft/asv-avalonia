namespace Asv.Avalonia;

public class BreadCrumbItem(bool isFirst, ITreePage item)
{
    public bool IsFirst { get; } = isFirst;
    public ITreePage Item { get; } = item;
}
