namespace Asv.Avalonia;

public class GotFocusEvent(IRoutable source) : AsyncRoutedEvent(source) { }

public static class GotFocusEventMixin
{
    /// <summary>
    /// Raises a <see cref="GotFocusEvent"/> for the specified routable element.
    /// This signals that the element has received focus.
    /// </summary>
    /// <param name="src">The routable element that received focus.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public static ValueTask RaiseFocusEvent(this IRoutable src)
    {
        return src.Rise(new GotFocusEvent(src));
    }
}
