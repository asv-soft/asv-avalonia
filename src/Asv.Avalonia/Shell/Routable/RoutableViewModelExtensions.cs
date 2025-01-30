namespace Asv.Avalonia;

public static class RoutableViewModelExtensions
{
    public static IRoutable GetRoot(this IRoutable src)
    {
        var root = src;
        while (root.Parent != null)
        {
            root = root.Parent;
        }

        return root;
    }

    public static IEnumerable<IRoutable> GetAllToRoot(this IRoutable src)
    {
        var current = src;
        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    public static IEnumerable<IRoutable> GetAllTo(this IRoutable src, IRoutable item)
    {
        var current = src;
        while (current is not null)
        {
            yield return current;
            if (current == item)
            {
                break;
            }

            current = current.Parent;
        }
    }

    public static IEnumerable<IRoutable> GetAllFromRoot(this IRoutable src)
    {
        if (src.Parent != null)
        {
            foreach (var ancestor in src.Parent.GetAllFromRoot())
            {
                yield return ancestor;
            }
        }

        yield return src;
    }

    public static IEnumerable<IRoutable> GetAllFrom(this IRoutable src, IRoutable item)
    {
        if (src.Parent != null && src != item)
        {
            foreach (var ancestor in src.Parent.GetAllFrom(item))
            {
                yield return ancestor;
            }
        }

        yield return src;
    }
}
