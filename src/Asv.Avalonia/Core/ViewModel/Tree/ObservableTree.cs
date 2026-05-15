using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public delegate ObservableTreeNode<T, TKey> CreateNodeDelegate<T, TKey>(
    T baseItem,
    IReadOnlyObservableList<T> source,
    Func<T, TKey> keySelector,
    Func<T, TKey> parentSelector,
    IComparer<T> comparer,
    CreateNodeDelegate<T, TKey> createNodeFactory,
    ObservableTreeNode<T, TKey>? parentNode
)
    where TKey : IEquatable<TKey>;

public class ObservableTree<T, TKey> : AsyncDisposableOnce
    where TKey : IEquatable<TKey>
{
    private readonly IReadOnlyObservableList<T> _flatList;
    private readonly TKey _rootKey;
    private readonly Func<T, TKey> _keySelector;
    private readonly Func<T, TKey> _parentSelector;
    private readonly IComparer<T> _comparer;
    private readonly CreateNodeDelegate<T, TKey> _createNodeFactory;
    private readonly ObservableList<ObservableTreeNode<T, TKey>> _itemSource;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;

    public ObservableTree(
        IReadOnlyObservableList<T> flatList,
        TKey rootKey,
        Func<T, TKey> keySelector,
        Func<T, TKey> parentSelector,
        IComparer<T> comparer,
        CreateNodeDelegate<T, TKey>? createNodeFactory = null
    )
    {
        _flatList = flatList;
        _rootKey = rootKey;
        _keySelector = keySelector;
        _parentSelector = parentSelector;
        _comparer = comparer;
        _createNodeFactory = createNodeFactory ?? DefaultNodeFactory;
        _itemSource = [];
        Items = _itemSource.ToNotifyCollectionChangedSlim();

        foreach (var item in flatList)
        {
            TryAdd(item);
        }

        _itemSource.Sort();
        _sub1 = flatList.ObserveAdd().Subscribe(x => TryAdd(x.Value));
        _sub2 = flatList.ObserveRemove().Subscribe(TryRemove);
        _sub3 = flatList.ObserveClear().Subscribe(_ => _itemSource.Clear());
    }

    private static ObservableTreeNode<T, TKey> DefaultNodeFactory(
        T item,
        IReadOnlyObservableList<T> list,
        Func<T, TKey> key,
        Func<T, TKey> parent,
        IComparer<T> comp,
        CreateNodeDelegate<T, TKey> factory,
        ObservableTreeNode<T, TKey>? parentNode
    )
    {
        return new ObservableTreeNode<T, TKey>(item, list, key, parent, comp, factory, parentNode);
    }

    public NotifyCollectionChangedSynchronizedViewList<ObservableTreeNode<T, TKey>> Items { get; }

    private void TryRemove(CollectionRemoveEvent<T> e)
    {
        var parent = _parentSelector(e.Value);
        var key = _keySelector(e.Value);
        if (parent.Equals(_rootKey))
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
        if (_parentSelector(item).Equals(_rootKey))
        {
            _itemSource.Add(
                _createNodeFactory(
                    item,
                    _flatList,
                    _keySelector,
                    _parentSelector,
                    _comparer,
                    _createNodeFactory,
                    null
                )
            );
            _itemSource.Sort();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
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
        return _itemSource
            .Select(node => node.FindNode(filter))
            .OfType<ObservableTreeNode<T, TKey>>()
            .FirstOrDefault();
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
