using Asv.Modeling;

namespace Asv.Avalonia;

public static class GoToMixin
{
    public static ValueTask GoTo(this IViewModel sender, NavPath path)
    {
        return sender.Events.Rise(new GoToEvent(sender, path));
    }
}