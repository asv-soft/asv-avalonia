using System.ComponentModel;
using System.Runtime.CompilerServices;
using Asv.Common;
using Asv.Modeling;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents the base implementation of a view model that provides
/// property change notifications and a proper undo and disposal mechanism.
/// This class is designed to be inherited by other view models.
/// </summary>
public abstract class ViewModel : IViewModel
{
    private static readonly CompositeDisposable DisposedDisposable = CreateDisposedDisposable();

    // ReSharper disable once ReplaceWithFieldKeyword
    private DisposableBag _disposableBag;
    protected ref DisposableBag DisposableBag => ref _disposableBag;

    // ReSharper disable once ReplaceWithFieldKeyword
    private IUndoController? _undo;

    // ReSharper disable once ReplaceWithFieldKeyword
    private ILayoutController? _layout;
    private int _isDisposed;
    private CancellationTokenSource? _cancel;
    private CompositeDisposable? _dispose;
    private readonly Subject<IViewModel?> _parentChanged;

    protected ViewModel(string typeId, NavArgs args = default)
    {
        Id = new NavId(typeId, args);
        _parentChanged = new Subject<IViewModel?>();
        Events = new RoutedEventController<IViewModel>(this).AddTo(ref DisposableBag);
        Events.Catch<TreeVisitorEvent>(e => e.Visit(this)).AddTo(ref DisposableBag);
        RootTracking = new ViewModelRootTrackingController(this).AddTo(ref DisposableBag); // Warning! create it after events, to ensure it receives all events related to root tracking
    }

    public IRootTrackingController<IShell> RootTracking { get; }
    public IRoutedEventController<IViewModel> Events { get; }

    public IUndoController Undo =>
        _undo ??= new UndoController<IViewModel>(this).AddTo(ref DisposableBag);

    public ILayoutController Layout =>
        _layout ??= new LayoutController<IViewModel>(this).AddTo(ref DisposableBag);

    public NavId Id { get; }

    public object? Tag { get; set; }

    public IViewModel? Parent
    {
        get;
        private set => SetField(ref field, value);
    }

    public void SetParent(IViewModel? parent)
    {
        Parent = parent;
        _parentChanged.OnNext(parent);
    }

    public Observable<IViewModel?> ParentChanged => _parentChanged;

    public virtual IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    public virtual ValueTask<IViewModel> Navigate(NavId id)
    {
        return ValueTask.FromResult(GetChildren().FirstOrDefault(x => x.Id == id) ?? this);
    }

    #region Property changes

    /// <summary>
    /// Occurs when a property value is about to change.
    /// Implements <see cref="INotifyPropertyChanging"/> to support pre-change notifications.
    /// </summary>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Occurs when a property value changes.
    /// Implements <see cref="INotifyPropertyChanged"/> to support UI binding updates.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanging"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that is changing. Automatically set by the caller if not provided.
    /// </param>
    private void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that changed. Automatically set by the caller if not provided.
    /// </param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets the field to the specified value and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the field.</typeparam>
    /// <param name="field">The backing field reference.</param>
    /// <param name="value">The new value to set.</param>
    /// <param name="propertyName">
    /// The name of the property that changed. Automatically set by the caller if not provided.
    /// </param>
    /// <returns>
    /// <c>true</c> if the field value was changed; otherwise, <c>false</c>.
    /// </returns>
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        OnPropertyChanging(propertyName);
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    #region Dispose

    public bool IsDisposed => Volatile.Read(ref _isDisposed) != 0;

    protected CancellationToken DisposeCancel
    {
        get
        {
            if (IsDisposed)
            {
                return CancellationToken.None;
            }

            var current = Volatile.Read(ref _cancel);
            if (current != null)
            {
                return current.Token;
            }

            var created = new CancellationTokenSource();
            current = Interlocked.CompareExchange(ref _cancel, created, null);
            if (current != null)
            {
                created.Dispose();
                return current.Token;
            }

            if (IsDisposed)
            {
                if (Interlocked.CompareExchange(ref _cancel, null, created) == created)
                {
                    created.Cancel(false);
                    created.Dispose();
                }

                return CancellationToken.None;
            }

            return created.Token;
        }
    }

    protected CompositeDisposable Disposable
    {
        get
        {
            var current = Volatile.Read(ref _dispose);
            if (current != null)
            {
                return current;
            }

            if (IsDisposed)
            {
                return DisposedDisposable;
            }

            var created = new CompositeDisposable();
            current = Interlocked.CompareExchange(ref _dispose, created, null);
            if (current != null)
            {
                created.Dispose();
                return current;
            }

            if (IsDisposed)
            {
                created.Dispose();
                Interlocked.CompareExchange(ref _dispose, null, created);
                return DisposedDisposable;
            }

            return created;
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Parent = null;
        PropertyChanging = null;
        PropertyChanged = null;

        var cancel = Interlocked.Exchange(ref _cancel, null);
        if (cancel?.Token.CanBeCanceled == true)
        {
            cancel.Cancel(false);
        }

        cancel?.Dispose();
        Interlocked.Exchange(ref _dispose, null)?.Dispose();
        DisposableBag.Dispose();
    }

    private static CompositeDisposable CreateDisposedDisposable()
    {
        var disposable = new CompositeDisposable();
        disposable.Dispose();
        return disposable;
    }

    #endregion

    public override string ToString()
    {
        return $"{GetType().Name}[{Id}]";
    }
}

public abstract class ViewModel<TExtensionIfc> : ViewModel
    where TExtensionIfc : class
{
    private readonly TExtensionIfc _self;

    protected ViewModel(string typeId, NavArgs args, IExtensionService extensionService)
        : base(typeId, args)
    {
        _self =
            this as TExtensionIfc
            ?? throw new Exception(
                $"The class {GetType().FullName} does not implement {typeof(TExtensionIfc).FullName}"
            );

        // we load extensions on the UI thread to avoid deadlocks
        Dispatcher.UIThread.Post(
            () =>
            {
                if (IsDisposed)
                {
                    return;
                }

                extensionService.Extend(_self, Id.TypeId, Disposable);

                AfterLoadExtensions();
            },
            DispatcherPriority.Background
        );
    }

    protected TExtensionIfc Context => _self;

    /// <summary>
    /// Called after all extensions have been loaded and applied.
    /// Derived classes must implement this method to provide additional logic after extension loading.
    /// </summary>
    protected abstract void AfterLoadExtensions();
}

internal sealed class ViewModelRootTrackingController : IRootTrackingController<IShell>, IDisposable
{
    private readonly IViewModel _owner;
    private readonly ReactiveProperty<IShell?> _root;
    private readonly Subject<IShell> _attached;
    private readonly Subject<Unit> _detached;
    private readonly SerialDisposable _parentRootSubscription;
    private readonly CompositeDisposable _dispose;
    private IShell? _currentRoot;
    private bool _isDisposed;

    public ViewModelRootTrackingController(IViewModel owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        _owner = owner;
        _root = new ReactiveProperty<IShell?>();
        _attached = new Subject<IShell>();
        _detached = new Subject<Unit>();
        _parentRootSubscription = new SerialDisposable();
        _dispose = new CompositeDisposable { _root, _attached, _detached, _parentRootSubscription };

        _owner.ParentChanged.Subscribe(_ => UpdateRoot()).DisposeItWith(_dispose);
        UpdateRoot();
    }

    public ReadOnlyReactiveProperty<IShell?> Root => _root;

    public Observable<IShell> Attached => _attached;

    public Observable<Unit> Detached => _detached;

    public IDisposable ExecuteWhenRootAttached(Action<IShell> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_currentRoot is { } root)
        {
            action(root);
        }

        return _attached.Subscribe(action);
    }

    public IDisposable ExecuteWhenRootAttached(Func<IShell, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_currentRoot is { } root)
        {
            action(root).AsTask().SafeFireAndForget();
        }

        return _attached.SubscribeAwait((root, _) => action(root), AwaitOperation.Drop);
    }

    public IDisposable ExecuteWhenRootAttached(Func<IShell, CancellationToken, ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (_currentRoot is { } root)
        {
            action(root, CancellationToken.None).AsTask().SafeFireAndForget();
        }

        return _attached.SubscribeAwait(action, AwaitOperation.Drop);
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _dispose.Dispose();
    }

    private void UpdateRoot()
    {
        _parentRootSubscription.Disposable = Disposable.Empty;

        if (_owner is IShell shell)
        {
            SetRoot(shell);
            return;
        }

        var parent = _owner.Parent;
        if (parent is null)
        {
            ClearRoot();
            return;
        }

        SetRoot(parent.RootTracking.Root.CurrentValue);

        var parentRootSubscription = new CompositeDisposable
        {
            parent.RootTracking.Attached.Subscribe(SetRoot),
            parent.RootTracking.Detached.Subscribe(_ => ClearRoot()),
        };
        _parentRootSubscription.Disposable = parentRootSubscription;
    }

    private void SetRoot(IShell? root)
    {
        if (root is null)
        {
            ClearRoot();
            return;
        }

        if (ReferenceEquals(_currentRoot, root))
        {
            return;
        }

        _currentRoot = root;
        _root.Value = root;
        _attached.OnNext(root);
    }

    private void ClearRoot()
    {
        if (_currentRoot is null)
        {
            return;
        }

        _currentRoot = null;
        _root.Value = null;
        _detached.OnNext(Unit.Default);
    }
}
