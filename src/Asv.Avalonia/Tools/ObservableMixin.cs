using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public static class ObservableMixin
{
    public static IDisposable PopulateTo<TOrigin, TColl, TDest>(
        this IObservableCollection<TOrigin> src,
        ICollection<TColl> dest,
        Func<TOrigin, TDest?> addAction,
        Func<TOrigin, TDest, bool> filterToRemove,
        bool disposeDestRemoved = true
    )
        where TDest : TColl
    {
        var sub1 = src.ObserveAdd().Select(x => x.Value).Subscribe(addAction, OnItemAdd);

        var sub2 = src.ObserveRemove().Subscribe(dest, OnItemRemove);

        foreach (var item in src)
        {
            OnItemAdd(item, addAction);
        }

        return Disposable.Combine(sub1, sub2, Disposable.Create(OnDisposed));

        // Local function to remove all populated items from dest
        void OnDisposed()
        {
            var itemsToDelete = new List<TDest>(src.Count);
            foreach (var origin in src)
            {
                foreach (var item in dest)
                {
                    if (item is TDest destItem && filterToRemove(origin, destItem))
                    {
                        itemsToDelete.Add(destItem);
                    }
                }
            }

            foreach (var dest1 in itemsToDelete)
            {
                dest.Remove(dest1);
                if (disposeDestRemoved)
                {
                    (dest1 as IDisposable)?.Dispose();
                }
            }
        }

        // Local function to remove populated item, when it was removed from src
        void OnItemRemove(CollectionRemoveEvent<TOrigin> x, ICollection<TColl> d)
        {
            var itemsToDelete = new List<TDest>(1);
            foreach (var item in d)
            {
                if (item is TDest destItem && filterToRemove(x.Value, destItem))
                {
                    itemsToDelete.Add(destItem);
                }
            }

            foreach (var dest1 in itemsToDelete)
            {
                d.Remove(dest1);
                if (disposeDestRemoved)
                {
                    (dest1 as IDisposable)?.Dispose();
                }
            }
        }

        // Local function to add populated item, when it was added to src
        void OnItemAdd(TOrigin x, Func<TOrigin, TDest?> cb)
        {
            var item = cb(x);
            if (item != null)
            {
                dest.Add(item);
            }
        }
    }

    public static IDisposable OnAddOrRemove<TOrigin, TFilter>(
        this IObservableCollection<TOrigin> src,
        Action<TFilter> addAction,
        Action<TFilter> removeAction
    )
        where TFilter : TOrigin
    {
        var sub1 = src.ObserveAdd()
            .Select(x => x.Value)
            .Where(x => x is TFilter)
            .Cast<TOrigin, TFilter>()
            .Subscribe(addAction);
        var sub2 = src.ObserveRemove()
            .Select(x => x.Value)
            .Where(x => x is TFilter)
            .Cast<TOrigin, TFilter>()
            .Subscribe(removeAction);
        return Disposable.Combine(sub1, sub2);
    }

    public static IDisposable DisposeMany<TModel, TView>(this ISynchronizedView<TModel, TView> src)
        where TView : IDisposable
    {
        return src.ObserveRemove().Subscribe(x => x.Value.View.Dispose());
    }

    public static ISynchronizedView<TModel, TView> DisposeMany<TModel, TView>(
        this ISynchronizedView<TModel, TView> src,
        DisposableBag subscriptionDispose
    )
        where TView : IDisposable
    {
        src.ObserveRemove()
            .Subscribe(x => x.Value.View.Dispose())
            .DisposeItWith(subscriptionDispose);
        return src;
    }

    public static IDisposable DisposeRemovedItems<T>(this IObservableCollection<T> src)
        where T : IDisposable
    {
        return src.ObserveRemove().Subscribe(x => x.Value.Dispose());
    }

    #region ClearWithItemsDispose

    public static void ClearWithItemsDispose<T>(this ObservableList<T> src)
        where T : IDisposable
    {
        src.CollectionItemsDispose();
        src.Clear();
    }

    public static void ClearWithItemsDispose<TKey, TValue>(
        this ObservableDictionary<TKey, TValue> src
    )
        where TKey : notnull
        where TValue : IDisposable
    {
        src.CollectionItemsDispose();
        src.Clear();
    }

    public static void ClearWithItemsDispose<T>(this ObservableQueue<T> src)
        where T : IDisposable
    {
        src.CollectionItemsDispose();
        src.Clear();
    }

    public static void ClearWithItemsDispose<T>(this ObservableHashSet<T> src)
        where T : IDisposable
    {
        src.CollectionItemsDispose();
        src.Clear();
    }

    public static void ClearWithItemsDispose<T>(this ObservableStack<T> src)
        where T : IDisposable
    {
        src.CollectionItemsDispose();
        src.Clear();
    }

    private static void CollectionItemsDispose<T>(this IObservableCollection<T> src)
        where T : IDisposable
    {
        foreach (var item in src)
        {
            item.Dispose();
        }
    }

    private static void CollectionItemsDispose<TKey, TValue>(
        this IObservableCollection<KeyValuePair<TKey, TValue>> src
    )
        where TKey : notnull
        where TValue : IDisposable
    {
        foreach (var item in src)
        {
            item.Value.Dispose();
        }
    }

    #endregion

    #region RemoveAll

    public static void RemoveAll<T>(this ObservableList<T> src)
    {
        src.RemoveRange(0, src.Count);
    }

    public static void RemoveAll<TKey, TValue>(this ObservableList<KeyValuePair<TKey, TValue>> src)
    {
        src.RemoveRange(0, src.Count);
    }

    public static void RemoveAll<T>(this ObservableRingBuffer<T> src)
    {
        while (src.Count > 0)
        {
            src.RemoveLast();
        }
    }

    public static void RemoveAll<T>(this ObservableFixedSizeRingBuffer<T> src)
    {
        while (src.Count > 0)
        {
            src.RemoveLast();
        }
    }

    public static void PopAll<T>(this ObservableStack<T> src)
    {
        src.PopRange(src.Count);
    }

    public static void DequeueAll<T>(this ObservableQueue<T> src)
    {
        src.DequeueRange(src.Count);
    }

    #endregion
}
