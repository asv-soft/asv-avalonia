using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class ObservableTreeNode<T, TKey> : AsyncDisposableOnce
    where TKey : notnull
{
    private readonly Func<T, TKey?> _parentSelector;
    private readonly ObservableList<ObservableTreeNode<T, TKey>> _itemSource;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly IReadOnlyObservableList<T> _source;
    private readonly Func<T, TKey> _keySelector;

    public ObservableTreeNode(
        T baseItem,
        IReadOnlyObservableList<T> source,
        Func<T, TKey> keySelector,
        Func<T, TKey?> parentSelector,
        ObservableTreeNode<T, TKey>? parentNode = null
    )
    {
        _source = source;
        _keySelector = keySelector;
        _parentSelector = parentSelector;
        ParentNode = parentNode;
        _itemSource = new ObservableList<ObservableTreeNode<T, TKey>>();
        Items = _itemSource.ToNotifyCollectionChangedSlim();
        Key = keySelector(baseItem);
        Base = baseItem;
        foreach (var item in source)
        {
            TryAdd(item);
        }

        _sub1 = source.ObserveAdd().Subscribe(x => TryAdd(x.Value));
        _sub2 = source.ObserveRemove().Subscribe(TryRemove);
    }

    public TKey Key { get; }
    public T Base { get; }
    public ObservableTreeNode<T, TKey>? ParentNode { get; }

    private void TryRemove(CollectionRemoveEvent<T> e)
    {
        var parent = _parentSelector(e.Value);
        if (parent != null && parent.Equals(Key))
        {
            var node = _itemSource.FirstOrDefault(x => x.Key.Equals(e.Value));
            if (node != null)
            {
                _itemSource.Remove(node);
            }
        }
    }

    private void TryAdd(T item)
    {
        var parent = _parentSelector(item);
        if (parent != null && parent.Equals(Key))
        {
            _itemSource.Add(
                new ObservableTreeNode<T, TKey>(item, _source, _keySelector, _parentSelector)
            );
        }
    }

    public NotifyCollectionChangedSynchronizedViewList<ObservableTreeNode<T, TKey>> Items { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            foreach (var observableTreeNode in _itemSource)
            {
                observableTreeNode.Dispose();
            }

            Items.Dispose();
        }

        base.Dispose(disposing);
    }

    public IEnumerable<ObservableTreeNode<T, TKey>> GetAllMenuFromRoot()
    {
        if (ParentNode != null)
        {
            foreach (var ancestor in ParentNode.GetAllMenuFromRoot())
            {
                yield return ancestor;
            }
        }

        yield return this;
    }

    public ObservableTreeNode<T, TKey>? FindNode(Func<ObservableTreeNode<T, TKey>, bool> filter)
    {
        if (filter(this))
        {
            return this;
        }

        foreach (var observableTreeNode in _itemSource)
        {
            var node = observableTreeNode.FindNode(filter);
            if (node != null)
            {
                return node;
            }
        }

        return null;
    }

    public ObservableTreeNode<T, TKey>? GetNode(TKey nodeId)
    {
        if (Key.Equals(nodeId))
        {
            return this;
        }

        foreach (var observableTreeNode in _itemSource)
        {
            var node = observableTreeNode.GetNode(nodeId);
            if (node != null)
            {
                return node;
            }
        }

        return null;
    }
}
