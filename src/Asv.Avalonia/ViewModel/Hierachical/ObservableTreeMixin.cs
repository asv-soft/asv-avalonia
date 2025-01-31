using ObservableCollections;

namespace Asv.Avalonia;

public static class ObservableTreeMixin
{
    public static ObservableTree<T, TKey> ToObservableTree<T, TKey>(this IReadOnlyObservableList<T> source, Func<T, TKey> keySelector, Func<T, TKey?> parentSelector)
        where TKey : notnull
    {
        return new ObservableTree<T, TKey>(source,keySelector, parentSelector);
    }
}