namespace Asv.Avalonia;

public class BreadCrumbItem(bool isFirst, ITreePageMenuItem item)
{
    public bool IsFirst { get; } = isFirst;
    public ITreePageMenuItem Item { get; } = item;
}
