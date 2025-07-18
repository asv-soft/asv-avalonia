using System.ComponentModel;
using System.Runtime.CompilerServices;
using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class ObservableTreeNode<T, TKey>
    : AsyncDisposableOnce,
        IComparable<ObservableTreeNode<T, TKey>>,
        IComparable,
        INotifyPropertyChanged
    where TKey : IEquatable<TKey>
{
    private readonly Func<T, TKey> _parentSelector;
    private readonly IComparer<T> _comparer;
    private readonly CreateNodeDelegate<T, TKey> _createNodeFactory;
    private readonly ObservableList<ObservableTreeNode<T, TKey>> _itemSource;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly IReadOnlyObservableList<T> _source;
    private readonly Func<T, TKey> _keySelector;

    public ObservableTreeNode(
        T baseItem,
        IReadOnlyObservableList<T> source,
        Func<T, TKey> keySelector,
        Func<T, TKey> parentSelector,
        IComparer<T> comparer,
        CreateNodeDelegate<T, TKey> createNodeFactory,
        ObservableTreeNode<T, TKey>? parentNode
    )
    {
        _source = source;
        _keySelector = keySelector;
        _parentSelector = parentSelector;
        _comparer = comparer;
        _createNodeFactory = createNodeFactory;
        ParentNode = parentNode;
        _itemSource = [];
        Items = _itemSource.ToNotifyCollectionChangedSlim();
        Key = keySelector(baseItem);
        Base = baseItem;
        foreach (var item in source)
        {
            TryAdd(item);
        }

        _itemSource.Sort();
        _sub1 = source
            .ObserveAdd()
            .Subscribe(x =>
            {
                TryAdd(x.Value);
                _itemSource.Sort();
            });
        _sub2 = source.ObserveRemove().Subscribe(TryRemove);
    }

    public TKey Key { get; }
    public T Base { get; }
    public ObservableTreeNode<T, TKey>? ParentNode { get; }

    private void TryRemove(CollectionRemoveEvent<T> e)
    {
        var parent = _parentSelector(e.Value);
        if (parent.Equals(Key))
        {
            var node = _itemSource.FirstOrDefault(x => x.Key.Equals(_keySelector(e.Value)));
            if (node != null)
            {
                _itemSource.Remove(node);
            }
        }
    }

    private void TryAdd(T item)
    {
        var parent = _parentSelector(item);
        if (parent.Equals(Key))
        {
            _itemSource.Add(
                _createNodeFactory(
                    item,
                    _source,
                    _keySelector,
                    _parentSelector,
                    _comparer,
                    _createNodeFactory,
                    this
                )
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

    public int CompareTo(ObservableTreeNode<T, TKey>? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        return _comparer.Compare(Base, other.Base);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        if (ReferenceEquals(this, obj))
        {
            return 0;
        }

        return obj is ObservableTreeNode<T, TKey> other
            ? CompareTo(other)
            : throw new ArgumentException(
                $"Object must be of type {nameof(ObservableTreeNode<T, TKey>)}"
            );
    }

    public static bool operator <(
        ObservableTreeNode<T, TKey>? left,
        ObservableTreeNode<T, TKey>? right
    )
    {
        return Comparer<ObservableTreeNode<T, TKey>>.Default.Compare(left, right) < 0;
    }

    public static bool operator >(
        ObservableTreeNode<T, TKey>? left,
        ObservableTreeNode<T, TKey>? right
    )
    {
        return Comparer<ObservableTreeNode<T, TKey>>.Default.Compare(left, right) > 0;
    }

    public static bool operator <=(
        ObservableTreeNode<T, TKey>? left,
        ObservableTreeNode<T, TKey>? right
    )
    {
        return Comparer<ObservableTreeNode<T, TKey>>.Default.Compare(left, right) <= 0;
    }

    public static bool operator >=(
        ObservableTreeNode<T, TKey>? left,
        ObservableTreeNode<T, TKey>? right
    )
    {
        return Comparer<ObservableTreeNode<T, TKey>>.Default.Compare(left, right) >= 0;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
