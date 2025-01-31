using System.Collections.ObjectModel;
using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class ObservableTree<T, TKey> : AsyncDisposableOnce
    where TKey : notnull
{
    private readonly IReadOnlyObservableList<T> _source;
    private readonly Func<T, TKey> _keySelector;
    private readonly Func<T, TKey?> _parentSelector;
    private readonly ObservableList<ObservableTreeNode<T, TKey>> _itemSource;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    public ObservableTree(IReadOnlyObservableList<T> source, Func<T, TKey> keySelector, Func<T, TKey?> parentSelector)
    {
        _source = source;
        _keySelector = keySelector;
        _parentSelector = parentSelector;
        _itemSource = new ObservableList<ObservableTreeNode<T, TKey>>();
        Items = _itemSource.ToNotifyCollectionChangedSlim();
        foreach (var item in source)
        {
            TryAdd(item);
        }

        _sub1 = source.ObserveAdd().Subscribe(x => TryAdd(x.Value));
        _sub2 = source.ObserveRemove().Subscribe(TryRemove);
    }

    public NotifyCollectionChangedSynchronizedViewList<ObservableTreeNode<T, TKey>> Items { get; }

    private void TryRemove(CollectionRemoveEvent<T> e)
    {
        var parent = _parentSelector(e.Value);
        var key = _keySelector(e.Value);
        if (parent == null)
        {
            var node = _itemSource.FirstOrDefault(x => x.Key.Equals(key));
            if (node != null)
            {
                _itemSource.Remove(node);
                node.Dispose();
            }
        }
    }

    private void TryAdd(T item)
    {
        if (_parentSelector(item) == null)
        {
            _itemSource.Add(new ObservableTreeNode<T, TKey>(item, _source, _keySelector, _parentSelector));
        }
    }

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

    public ObservableTreeNode<T, TKey>? FindNode(Func<ObservableTreeNode<T, TKey>, bool> filter)
    {
        foreach (var node in _itemSource)
        {
            var child = node.FindNode(filter);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }

    public ObservableTreeNode<T, TKey>? GetNode(TKey nodeId)
    {
        foreach (var node in _itemSource)
        {
            if (node.Key.Equals(nodeId))
            {
                return node;
            }

            var child = node.GetNode(nodeId);
            if (child != null)
            {
                return child;
            }
        }

        return null;
    }
}