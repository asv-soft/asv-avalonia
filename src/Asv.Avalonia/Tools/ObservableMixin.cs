﻿using ObservableCollections;
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
        return new CompositeDisposable(sub1, sub2);
    }

    public static IDisposable DisposeRemovedViewItems<TModel, TView>(
        this ISynchronizedView<TModel, TView> src
    )
        where TView : IDisposable
    {
        return src.ObserveRemove().Subscribe(x => x.Value.View.Dispose());
    }

    public static IDisposable DisposeRemovedItems<T>(this IObservableCollection<T> src)
        where T : IDisposable
    {
        return src.ObserveRemove().Subscribe(x => x.Value.Dispose());
    }
}
