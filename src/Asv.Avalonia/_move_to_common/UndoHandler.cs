using System.Diagnostics;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public static class UndoHandlerMixin
{
   extension<T>(ISupportUndo<T> that) 
       where T : ISupportRoutedEvents<T>, ISupportNavigation<T>
   {
       public UndoHandler<TChange> Register<TChange>(
           string id,
           UndoHandler<TChange>.Delegate undo,
           UndoHandler<TChange>.Delegate redo)
           where TChange : IChange, new()
       {
           that.Undo.Register("asd", new ReactiveProperty<string>())
          that.Undo.Register(new UndoHandler<TChange>(id, undo, redo));
       }
   }
}

public class UndoHandler<TChange>(
    string id,
    UndoHandler<TChange>.Delegate undo,
    UndoHandler<TChange>.Delegate redo)
    : IUndoHandler, IDisposable
    where TChange : IChange, new()
{
    private readonly Subject<IChange> _changes = new();

    public delegate ValueTask Delegate(TChange change, CancellationToken cancel);

    public void ApplyChange(TChange change)
    {
        if (MuteChanges)
        {
            return;
        }
        _changes.OnNext(change);
    }
    
    public IChange Create()
    {
        return new TChange();
    }

    public ValueTask Undo(IChange change, CancellationToken cancel)
    {
        Debug.Assert(change != null, nameof(change) + " != null");
        return undo((TChange)change, cancel);
    }

    public ValueTask Redo(IChange change, CancellationToken cancel)
    {
        Debug.Assert(change != null, nameof(change) + " != null");
        return redo((TChange)change, cancel);
    }

    public string ChangeId => id;

    public Observable<IChange> Changes => _changes;

    public bool MuteChanges { get; set; }

    public void Dispose()
    {
        _changes.Dispose();
    }
}