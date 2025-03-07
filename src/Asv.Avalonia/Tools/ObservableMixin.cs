using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public static class ObservableMixin
{
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
