using System.Collections.Specialized;
using System.Diagnostics;
using Asv.Common;
using Asv.Modeling;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Provides extension methods for working with <see cref="IViewModel"/> components,
/// including navigation, hierarchy traversal, and event propagation.
/// </summary>
public static class RoutableMixin
{
    public static IDisposable SetRoutableParent<TModel, TView>(
        this ISynchronizedView<TModel, TView> src,
        IViewModel parent
    )
        where TView : class, IViewModel
    {
        src.ForEach(item => item.Parent = parent);
        var sub1 = src.ObserveAdd().Subscribe(x => x.Value.View.Parent = parent);
        var sub2 = src.ObserveRemove().Subscribe(x => x.Value.View.Parent = null);
        return Disposable.Combine(sub1, sub2);
    }

    public static ISynchronizedView<TModel, TView> SetRoutableParent<TModel, TView>(
        this ISynchronizedView<TModel, TView> src,
        IViewModel parent,
        CompositeDisposable dispose
    )
        where TView : class, IViewModel
    {
        src.ForEach(item => item.Parent = parent);
        src.ObserveAdd().Subscribe(x => x.Value.View.Parent = parent).DisposeItWith(dispose);
        src.ObserveRemove().Subscribe(x => x.Value.View.Parent = null).DisposeItWith(dispose);
        return src;
    }

    public static INotifyCollectionChangedSynchronizedViewList<TView> SetRoutableParent<TView>(
        this INotifyCollectionChangedSynchronizedViewList<TView> src,
        IViewModel parent,
        CompositeDisposable dispose
    )
        where TView : class, IViewModel
    {
        src.ForEach(item => item.Parent = parent);
        Observable
            .FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                handler => (sender, args) => handler(args),
                h => src.CollectionChanged += h,
                h => src.CollectionChanged -= h
            )
            .Subscribe(parent, HandleCollectionChanged)
            .DisposeItWith(dispose);

        return src;
    }

    public static TCollection SetRoutableParent<TCollection, TItem>(
        this TCollection src,
        IViewModel parent,
        CompositeDisposable dispose
    )
        where TCollection : INotifyCollectionChanged, IEnumerable<TItem>
        where TItem : IViewModel
    {
        src.SetRoutableParent<TCollection, TItem>(parent).DisposeItWith(dispose);
        return src;
    }

    public static TCollection SetRoutableParent<TCollection, TItem>(
        this TCollection src,
        IViewModel parent,
        CancellationToken dispose
    )
        where TCollection : INotifyCollectionChanged, IEnumerable<TItem>
        where TItem : IViewModel
    {
        src.SetRoutableParent<TCollection, TItem>(parent).RegisterTo(dispose);
        return src;
    }

    public static IDisposable SetRoutableParent<TCollection, TItem>(
        this TCollection src,
        IViewModel parent
    )
        where TCollection : INotifyCollectionChanged, IEnumerable<TItem>
        where TItem : IViewModel
    {
        src.ForEach(item => item.Parent = parent);
        return Observable
            .FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                handler => (sender, args) => handler(args),
                h => src.CollectionChanged += h,
                h => src.CollectionChanged -= h
            )
            .Subscribe(parent, HandleCollectionChanged);
    }

    private static void HandleCollectionChanged(
        NotifyCollectionChangedEventArgs arg,
        IViewModel parent
    )
    {
        switch (arg.Action)
        {
            case NotifyCollectionChangedAction.Add:
                Debug.Assert(arg.NewItems != null, "arg.NewItems != null");
                foreach (var item in arg.NewItems)
                {
                    if (item is IViewModel routable)
                    {
                        routable.Parent = parent;
                    }
                }

                break;
            case NotifyCollectionChangedAction.Remove:
                Debug.Assert(arg.OldItems != null, "arg.OldItems != null");
                foreach (var item in arg.OldItems)
                {
                    if (item is IViewModel routable)
                    {
                        routable.Parent = null;
                        routable.Dispose();
                    }
                }

                break;
            case NotifyCollectionChangedAction.Replace:
                Debug.Assert(arg.OldItems != null, "arg.OldItems != null");
                Debug.Assert(arg.NewItems != null, "arg.NewItems != null");
                foreach (var item in arg.OldItems)
                {
                    if (item is IViewModel routable)
                    {
                        routable.Parent = null;
                        routable.Dispose();
                    }
                }

                foreach (var item in arg.NewItems)
                {
                    if (item is IViewModel routable)
                    {
                        routable.Parent = parent;
                    }
                }

                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Reset:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static T SetRoutableParent<T>(this T src, IViewModel parent)
        where T : class, IViewModel
    {
        src.Parent = parent;
        return src;
    }

    public static IDisposable SetRoutableParent<T>(
        this IObservableCollection<T> src,
        IViewModel parent
    )
        where T : class, IViewModel
    {
        var sub1 = src.ObserveAdd().Subscribe(x => x.Value.Parent = parent);
        var sub2 = src.ObserveRemove().Subscribe(x => x.Value.Parent = null);
        foreach (var routable in src)
        {
            routable.Parent = parent;
        }
        return Disposable.Combine(sub1, sub2);
    }

    public static NavPath GetPathFromRoot(this IViewModel src)
    {
        return new NavPath(src.GetPathFromRoot<IViewModel, NavId>());
    }

    public static async ValueTask<IViewModel> NavigateByPath(this IViewModel src, NavPath path)
    {
        var index = 0;
        while (true)
        {
            if (path.Count <= index)
            {
                return src;
            }

            src = await src.Navigate(path[index]);
            index++;
        }
    }

    /// <summary>
    /// Finds the first parent of the current element that matches the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="IViewModel"/> to search for.</typeparam>
    /// <param name="src">The starting <see cref="IViewModel"/> element.</param>
    /// <returns>The first matching parent of type <typeparamref name="T"/>, or <c>null</c> if none is found.</returns>
    public static T? FindParentOfType<T>(this IViewModel? src)
        where T : IViewModel
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
