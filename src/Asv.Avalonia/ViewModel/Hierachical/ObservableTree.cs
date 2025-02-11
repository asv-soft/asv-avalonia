﻿using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public delegate ObservableTreeNode<T, TKey> CreateNodeDelegate<T, TKey>(
    T item,
    IReadOnlyObservableList<T> flatList,
    Func<T, TKey> keySelector,
    Func<T, TKey?> parentSelector
)
    where TKey : notnull;

public class ObservableTree<T, TKey> : AsyncDisposableOnce
    where TKey : notnull
{
    private readonly IReadOnlyObservableList<T> _flatList;
    private readonly Func<T, TKey> _keySelector;
    private readonly Func<T, TKey?> _parentSelector;
    private readonly CreateNodeDelegate<T, TKey> _createNodeFactory;
    private readonly ObservableList<ObservableTreeNode<T, TKey>> _itemSource;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;

    public ObservableTree(
        IReadOnlyObservableList<T> flatList,
        Func<T, TKey> keySelector,
        Func<T, TKey?> parentSelector,
        CreateNodeDelegate<T, TKey>? createNodeFactory = null
    )
    {
        _flatList = flatList;
        _keySelector = keySelector;
        _parentSelector = parentSelector;
        _createNodeFactory =
            createNodeFactory
            ?? (
                (item, list, selector, func) =>
                    new ObservableTreeNode<T, TKey>(item, list, selector, func)
            );
        _itemSource = new ObservableList<ObservableTreeNode<T, TKey>>();
        Items = _itemSource.ToNotifyCollectionChangedSlim();
        foreach (var item in flatList)
        {
            TryAdd(item);
        }

        _sub1 = flatList.ObserveAdd().Subscribe(x => TryAdd(x.Value));
        _sub2 = flatList.ObserveRemove().Subscribe(TryRemove);
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
            _itemSource.Add(_createNodeFactory(item, _flatList, _keySelector, _parentSelector));
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
