using System.Composition;
using Avalonia.Controls;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public interface IShellHost
{
    IShell Shell { get; }
    TopLevel TopLevel { get; }
}

public interface IShell : IRoutable
{
    ReactiveCommand Back { get; }
    ValueTask BackwardAsync(CancellationToken cancel = default);
    ReactiveCommand Forward { get; }
    ValueTask ForwardAsync(CancellationToken cancel = default);
    ReactiveCommand GoHome { get; }
    ValueTask GoHomeAsync(CancellationToken cancel = default);
    ValueTask<IPage?> OpenPage(string pageId);
    NotifyCollectionChangedSynchronizedViewList<IPage> Pages { get; }
}
