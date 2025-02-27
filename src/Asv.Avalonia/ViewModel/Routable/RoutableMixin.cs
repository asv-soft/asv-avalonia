namespace Asv.Avalonia;

/// <summary>
/// Provides extension methods for working with <see cref="IRoutable"/> components,
/// including navigation, hierarchy traversal, and event propagation.
/// </summary>
public static class RoutableMixin
{
    public static string[] GetPathToRoot(this IRoutable src)
    {
        return src.GetHierarchyFromRoot().Select(x => x.Id).ToArray();
    }

    /// <summary>
    /// Navigates through the specified path, resolving each step sequentially.
    /// </summary>
    /// <param name="src">The starting <see cref="IRoutable"/> element.</param>
    /// <param name="path">An ordered segment of path identifiers.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> resolving to the final <see cref="IRoutable"/> reached in the path.
    /// </returns>
    public static async ValueTask<IRoutable> NavigateByPath(
        this IRoutable src,
        ArraySegment<string> path
    )
    {
        while (true)
        {
            if (path.Count == 0)
            {
                return src;
            }

            var first = path[0];
            var item = await src.Navigate(first);
            src = item;
            path = path[1..];
        }
    }

    /// <summary>
    /// Navigates through the specified path, resolving each step sequentially.
    /// </summary>
    /// <param name="src">The starting <see cref="IRoutable"/> element.</param>
    /// <param name="path">An ordered array of path identifiers.</param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> resolving to the final <see cref="IRoutable"/> reached in the path.
    /// </returns>
    public static async ValueTask<IRoutable> NavigateByPath(this IRoutable src, string[] path)
    {
        return await src.NavigateByPath(new ArraySegment<string>(path));
    }

    /// <summary>
    /// Retrieves the root <see cref="IRoutable"/> element in the hierarchy.
    /// </summary>
    /// <param name="src">The starting <see cref="IRoutable"/> element.</param>
    /// <returns>The root element in the hierarchy.</returns>
    public static IRoutable GetRoot(this IRoutable src)
    {
        var root = src;
        while (root.Parent != null)
        {
            root = root.Parent;
        }

        return root;
    }

    /// <summary>
    /// Enumerates all ancestors from the current element up to the root.
    /// </summary>
    /// <param name="src">The starting <see cref="IRoutable"/> element.</param>
    /// <returns>An enumerable collection of ancestors up to the root.</returns>
    public static IEnumerable<IRoutable> GetAncestorsToRoot(this IRoutable? src)
    {
        var current = src;
        while (current is not null)
        {
            yield return current;
            current = current.Parent;
        }
    }

    /// <summary>
    /// Enumerates all elements from the current element up to the specified target element.
    /// </summary>
    /// <param name="src">The starting <see cref="IRoutable"/> element.</param>
    /// <param name="target">The target <see cref="IRoutable"/> element.</param>
    /// <returns>An enumerable collection of elements leading to the target.</returns>
    public static IEnumerable<IRoutable> GetAncestorsTo(this IRoutable src, IRoutable target)
    {
        var current = src;
        while (current is not null)
        {
            yield return current;
            if (current == target)
            {
                break;
            }

            current = current.Parent;
        }
    }

    /// <summary>
    /// Enumerates all elements from the root down to the current element.
    /// </summary>
    /// <param name="src">The starting <see cref="IRoutable"/> element.</param>
    /// <returns>An enumerable collection of elements from the root to the current element.</returns>
    public static IEnumerable<IRoutable> GetHierarchyFromRoot(this IRoutable src)
    {
        if (src.Parent != null)
        {
            foreach (var ancestor in src.Parent.GetHierarchyFromRoot())
            {
                yield return ancestor;
            }
        }

        yield return src;
    }

    /// <summary>
    /// Enumerates all elements from the specified target element down to the current element.
    /// </summary>
    /// <param name="src">The starting <see cref="IRoutable"/> element.</param>
    /// <param name="target">The target <see cref="IRoutable"/> element.</param>
    /// <returns>An enumerable collection of elements from the target down to the current element.</returns>
    public static IEnumerable<IRoutable> GetHierarchyFrom(this IRoutable src, IRoutable target)
    {
        if (src.Parent != null && src != target)
        {
            foreach (var ancestor in src.Parent.GetHierarchyFrom(target))
            {
                yield return ancestor;
            }
        }

        yield return src;
    }

    /// <summary>
    /// Finds the first parent of the current element that matches the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IRoutable"/> to search for.</typeparam>
    /// <param name="src">The starting <see cref="IRoutable"/> element.</param>
    /// <returns>The first matching parent of type <typeparamref name="T"/>, or <c>null</c> if none is found.</returns>
    public static T? FindParentOfType<T>(this IRoutable? src)
        where T : IRoutable
    {
        var current = src;
        while (current is not null)
        {
            if (current is T result)
            {
                return result;
            }

            current = current.Parent;
        }

        return default;
    }
}
