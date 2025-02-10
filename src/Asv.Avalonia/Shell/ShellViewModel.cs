using System.Collections.Immutable;
using System.Composition.Hosting;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Rendering;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public abstract class ShellViewModel : RoutableViewModel, IShell
{
    private readonly CompositionHost _container;
    private readonly ObservableList<IPage> _pages = new();
    public const string ShellId = "shell";

    protected ShellViewModel(IContainerHost host)
        : base(ShellId)
    {
        ArgumentNullException.ThrowIfNull(host);

        _container = host.Host;
        Pages = _pages.ToNotifyCollectionChangedSlim();
        Back = new ReactiveCommand((_, c) => BackwardAsync(c));
        Forward = new ReactiveCommand((_, c) => ForwardAsync(c));
        GoHome = new ReactiveCommand((_, c) => BackwardAsync(c));
        Message = new ReactiveCommand(async (_, c) => await MessageAsync(c));
    }

    public ReactiveCommand Back { get; }

    public ValueTask BackwardAsync(CancellationToken cancel = default)
    {
        throw new NotImplementedException();
    }

    public ReactiveCommand Message { get; }

    private async ValueTask MessageAsync(CancellationToken cancel = default)
    {
        var mainWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime!)?.MainWindow;
        var fileDialogServiceImplementation = _container.GetExport<IFileDialogService>();
        var simpleDialogServiceImplementation = _container.GetExport<ISimpleDialogService>();
        await simpleDialogServiceImplementation.ShowUnitInputDialog("Test", "Test");
    }

    public ReactiveCommand Forward { get; }

    public ValueTask ForwardAsync(CancellationToken cancel = default)
    {
        throw new NotImplementedException();
    }

    public ReactiveCommand GoHome { get; }

    public ValueTask GoHomeAsync(CancellationToken cancel = default)
    {
        throw new NotImplementedException();
    }

    public ValueTask<IPage?> OpenPage(string pageId)
    {
        if (_container.TryGetExport<IPage>(pageId, out var page))
        {
            page.Parent = this;
            _pages.Add(page);
            return ValueTask.FromResult<IPage?>(page);
        }

        return ValueTask.FromResult<IPage?>(null);
    }

    protected abstract void InternalAddPageToMainTab(IPage export);

    public NotifyCollectionChangedSynchronizedViewList<IPage> Pages { get; }

    public override IEnumerable<IRoutable> Children
    {
        get
        {
            foreach (var page in _pages)
            {
                yield return page;
            }
        }
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is ExecuteCommandEvent cmd)
        {
            // write command to log
        }

        if (e is FocusedEvent focus)
        {
            // write to navigation history
        }

        return ValueTask.CompletedTask;
    }
}
