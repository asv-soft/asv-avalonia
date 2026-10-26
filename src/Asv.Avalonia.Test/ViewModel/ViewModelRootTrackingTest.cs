using System.Collections.Concurrent;
using System.Reflection;
using Asv.Modeling;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

// R3's unhandled-exception handler is shared by the process.
[CollectionDefinition(nameof(RootTrackingLifecycleCollection), DisableParallelization = true)]
public sealed class RootTrackingLifecycleCollection;

[Collection(nameof(RootTrackingLifecycleCollection))]
public class ViewModelRootTrackingTest : IDisposable
{
    private readonly Action<Exception> _previous = ObservableSystem.GetUnhandledExceptionHandler();
    private readonly ConcurrentQueue<Exception> _errors = new();

    public ViewModelRootTrackingTest() => ObservableSystem.RegisterUnhandledExceptionHandler(_errors.Enqueue);
    public void Dispose() => ObservableSystem.RegisterUnhandledExceptionHandler(_previous);

    [Fact]
    public void Dispose_ParentDetachesDuringRootCompletion_DoesNotWriteToDisposedProperty()
    {
        using var root = new RootFixture();
        using var parent = new Node("parent");
        using var child = new Node("child");
        parent.SetParent(root.Shell);
        child.SetParent(parent);
        using var subscription = child.RootTracking.Root.Subscribe(
            _ => { }, onCompleted: _ => parent.SetParent(null));

        child.Dispose();

        Assert.Empty(_errors);
        Assert.True(child.IsDisposed);
        Assert.Null(parent.RootTracking.Root.CurrentValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ExecuteWhenAttached_CallbackDisposesOwner_DoesNotSubscribeToDisposedSubject(int kind)
    {
        using var root = new RootFixture();
        using var child = new Node("child");
        child.SetParent(root.Shell);
        var calls = 0;
        void DisposeOwner(IShell shell) { calls++; child.Dispose(); }

        using var subscription = kind switch
        {
            0 => child.RootTracking.ExecuteWhenRootAttached((Action<IShell>)DisposeOwner),
            1 => child.RootTracking.ExecuteWhenRootAttached((IShell shell) =>
            {
                DisposeOwner(shell);
                return ValueTask.CompletedTask;
            }),
            _ => child.RootTracking.ExecuteWhenRootAttached((IShell shell, CancellationToken token) =>
            {
                DisposeOwner(shell);
                return ValueTask.CompletedTask;
            }),
        };

        Assert.Equal(1, calls);
        Assert.Empty(_errors);
    }

    [Fact]
    public void ExecuteWhenAttached_AfterDisposal_DoesNotInvokeStaleRoot()
    {
        using var root = new RootFixture();
        using var child = new Node("child");
        child.SetParent(root.Shell);
        child.Dispose();
        var calls = 0;

        using var subscription = child.RootTracking.ExecuteWhenRootAttached((Action<IShell>)(_ => calls++));

        Assert.Equal(0, calls);
        Assert.Empty(_errors);
    }

    [Fact]
    public void RootChanges_AttachDetachAndReattachRemainObservable()
    {
        using var root = new RootFixture();
        using var parent = new Node("parent");
        using var child = new Node("child");
        child.SetParent(parent);
        var attached = 0;
        var detached = 0;
        using var attachSubscription = child.RootTracking.Attached.Subscribe(_ => attached++);
        using var detachSubscription = child.RootTracking.Detached.Subscribe(_ => detached++);

        parent.SetParent(root.Shell);
        Assert.Same(root.Shell, child.RootTracking.Root.CurrentValue);
        parent.SetParent(null);
        Assert.Null(child.RootTracking.Root.CurrentValue);
        parent.SetParent(root.Shell);
        Assert.Same(root.Shell, child.RootTracking.Root.CurrentValue);
        Assert.Equal(2, attached);
        Assert.Equal(1, detached);
        child.Dispose();
        parent.SetParent(null);
        Assert.Equal(1, detached);
        Assert.Empty(_errors);
    }

    [Fact]
    public void RootObserver_DisposesOwnerDuringAttach_DoesNotPublishLateAttachedEvent()
    {
        using var root = new RootFixture();
        using var child = new Node("child");
        var attached = 0;
        using var attachSubscription = child.RootTracking.Attached.Subscribe(_ => attached++);
        using var rootSubscription = child.RootTracking.Root.Subscribe(value =>
        {
            if (value != null) child.Dispose();
        });

        child.SetParent(root.Shell);

        Assert.True(child.IsDisposed);
        Assert.Equal(0, attached);
        Assert.Empty(_errors);
    }

    private sealed class Node(string id) : ViewModel(id);

    public class ShellProxy : DispatchProxy
    {
        public IRootTrackingController<IShell> Tracking { get; set; } = null!;
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args) => targetMethod!.Name switch
        {
            "get_RootTracking" => Tracking,
            _ => throw new NotSupportedException(targetMethod.Name),
        };
    }

    private sealed class RootFixture : IRootTrackingController<IShell>, IDisposable
    {
        private readonly ReactiveProperty<IShell?> _root;
        public IShell Shell { get; }
        public ReadOnlyReactiveProperty<IShell?> Root => _root;
        public RootFixture()
        {
            Shell = DispatchProxy.Create<IShell, ShellProxy>();
            _root = new ReactiveProperty<IShell?>(Shell);
            ((ShellProxy)(object)Shell).Tracking = this;
        }
        public void Dispose() => _root.Dispose();
    }
}
