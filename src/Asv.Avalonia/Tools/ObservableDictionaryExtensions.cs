using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public static class ObservableDictionaryExtensions
{
    public static Observable<ReactiveNode<T>?> TransformToTree<T, TKey>(
        this IReadOnlyObservableDictionary<TKey, T> source,
        Func<T, TKey> getKey,
        Func<T, TKey?> getParentKey,
        Func<IReadOnlyObservableDictionary<TKey, T>, TKey?> getPivotKey
    )
        where TKey : notnull
    {
        return Observable.Merge(
            source
                .ObserveAdd()
                .Select(_ => BuildTree(source, getKey, getParentKey, getPivotKey(source))),
            source
                .ObserveRemove()
                .Select(_ => BuildTree(source, getKey, getParentKey, getPivotKey(source))),
            source
                .ObserveReplace()
                .Select(_ => BuildTree(source, getKey, getParentKey, getPivotKey(source)))
        );
    }

    private static ReactiveNode<T>? BuildTree<T, TKey>(
        IReadOnlyObservableDictionary<TKey, T> source,
        Func<T, TKey> getKey,
        Func<T, TKey?> getParentKey,
        TKey? pivotKey
    )
        where TKey : notnull
    {
        if (pivotKey == null || !source.TryGetValue(pivotKey, out _))
        {
            return null;
        }

        var nodes = source.ToDictionary(
            kv => getKey(kv.Value),
            kv => new ReactiveNode<T>(kv.Value)
        );

        foreach (var node in nodes.Values)
        {
            var parentKey = getParentKey(node.Item.Value);
            if (parentKey != null && nodes.TryGetValue(parentKey, out var parentNode))
            {
                parentNode.Children.Add(node);
            }
        }

        return nodes.GetValueOrDefault(pivotKey);
    }
}

public class ReactiveNode<T>(T item)
{
    public ReactiveProperty<T> Item { get; } = new(item);
    public ObservableList<ReactiveNode<T>> Children { get; } = [];
}
