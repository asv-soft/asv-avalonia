using R3;

namespace Asv.Avalonia;

public interface IRoutable : IViewModel
{
    IRoutable? Parent { get; set; }
    ValueTask Rise(AsyncRoutedEvent e);
    ValueTask<IRoutable> Navigate(string id);
}

public abstract class AsyncRoutedEvent(IRoutable source)
{
    public IRoutable Source { get; } = source;
    public bool IsHandled { get; set; }

    public virtual AsyncRoutedEvent Clone()
    {
        return (AsyncRoutedEvent)MemberwiseClone();
    }
}
