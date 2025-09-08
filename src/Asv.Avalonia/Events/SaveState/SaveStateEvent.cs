namespace Asv.Avalonia;

public class SaveStateEvent(IRoutable source) : AsyncRoutedEvent(source, RoutingStrategy.Bubble) { }

public static class SaveStateMixin
{
    public static ValueTask RequestSaveState(this IRoutable src)
    {
        return src.Rise(new SaveStateEvent(src));
    }
}
