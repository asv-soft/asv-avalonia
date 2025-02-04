using System.Collections.Immutable;
using System.Composition.Hosting;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class ShellViewModel : RoutableViewModel, IShell
{
    private readonly ObservableStack<string[]> _backwardStack = new();
    private readonly ObservableStack<string[]> _forwardStack = new();
    private readonly ReactiveProperty<IRoutable> _selectedControl;
    private readonly ReactiveProperty<string[]> _selectedControlPath;

    private readonly ObservableList<IPage> _pages = new();
    private readonly IContainerHost _container;
    private readonly ICommandService _cmd;

    public const string ShellId = "shell";

    protected ShellViewModel(IContainerHost ioc)
        : base(ShellId)
    {
        ArgumentNullException.ThrowIfNull(ioc);
        _container = ioc;
        _cmd = ioc.GetExport<ICommandService>();
        GoHome = new ReactiveCommand((_, c) => GoHomeAsync(c));
        PagesView = _pages.ToNotifyCollectionChangedSlim();
        Status = new BindableReactiveProperty<ShellStatus>(ShellStatus.Normal);
        Close = new ReactiveCommand((_, c) => CloseAsync(c));

        Backward = new ReactiveCommand((_, c) => BackwardAsync(c));
        Forward = new ReactiveCommand((_, c) => ForwardAsync(c));

        _selectedControl = new ReactiveProperty<IRoutable>(this);
        _selectedControlPath = new ReactiveProperty<string[]>(GetPath(this));
        _selectedControl.Subscribe(x => _selectedControlPath.Value = GetPath(x));
        _selectedControlPath.Subscribe(x =>
        {
            if (x is not { Length: > 0 })
            {
                return;
            }

            _backwardStack.Push(x);
            _forwardStack.Clear();
            CheckBackwardForwardCanExecute();
        });
        InputElement.GotFocusEvent.AddClassHandler<TopLevel>(GotFocus, handledEventsToo: true);
        InputElement.KeyDownEvent.AddClassHandler<TopLevel>(
            OnKeyDownCustom,
            handledEventsToo: true
        );
    }

    private void GotFocus(TopLevel control, GotFocusEventArgs args)
    {
        if (args.Source is Control { DataContext: IRoutable routable })
        {
            routable.RiseGotFocusEvent();
        }
    }

    private void OnKeyDownCustom(TopLevel source, KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.KeyModifiers == KeyModifiers.None)
        {
            // we don't want to handle key events without modifiers
            return;
        }

        var gesture = new KeyGesture(keyEventArgs.Key, keyEventArgs.KeyModifiers);
        if (
            _cmd.CanExecuteCommand(
                gesture,
                SelectedControl.CurrentValue,
                out var command,
                out var target
            )
        )
        {
            if (target != null && command != null)
            {
                target.ExecuteCommand(command.Info.Id, null);
                keyEventArgs.Handled = true;
            }
        }
    }

    public ReadOnlyReactiveProperty<IRoutable> SelectedControl => _selectedControl;
    public ReadOnlyReactiveProperty<string[]> SelectedControlPath => _selectedControlPath;

    private string[] GetPath(IRoutable vm)
    {
        return vm.GetAllFrom(this).Skip(1).Select(x => x.Id).ToArray();
    }

    public ReactiveCommand Backward { get; }

    public async ValueTask BackwardAsync(CancellationToken cancel = default)
    {
        if (_backwardStack.TryPop(out var path))
        {
            _forwardStack.Push(path);
            await this.NavigateTo(path);
            CheckBackwardForwardCanExecute();
        }
    }

    private void CheckBackwardForwardCanExecute()
    {
        Backward.ChangeCanExecute(_backwardStack.Count != 0);
        Forward.ChangeCanExecute(_forwardStack.Count != 0);
    }

    public IObservableCollection<string[]> ForwardStack => _forwardStack;
    public ReactiveCommand Forward { get; }

    public async ValueTask ForwardAsync(CancellationToken cancel = default)
    {
        if (_forwardStack.TryPop(out var path))
        {
            _backwardStack.Push(path);
            await this.NavigateTo(path);
            CheckBackwardForwardCanExecute();
        }
    }

    public IObservableCollection<string[]> BackwardStack => _backwardStack;

    protected virtual ValueTask CloseAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyObservableList<IPage> Pages => _pages;
    public ReactiveCommand GoHome { get; }

    public async ValueTask GoHomeAsync(CancellationToken cancel = default) =>
        await Navigate(HomePageViewModel.PageId);

    public BindableReactiveProperty<ShellStatus> Status { get; }
    public ReactiveCommand Close { get; }
    public BindableReactiveProperty<IPage?> SelectedPage { get; } = new();
    public NotifyCollectionChangedSynchronizedViewList<IPage> PagesView { get; }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        var page = _pages.FirstOrDefault(x => x.Id == id);
        if (page == null)
        {
            if (_container.TryGetExport<IPage>(id, out page))
            {
                _pages.Add(page);
                page.Parent = this;
                _selectedControl.Value = page;
            }
        }

        SelectedPage.Value = page;
        return ValueTask.FromResult<IRoutable>(page);
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is ExecuteCommandEvent cmd)
        {
            // write command to log
        }

        if (e is GotFocusEvent gotFocus)
        {
            _selectedControl.Value = gotFocus.Source;
        }

        return ValueTask.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PagesView.Dispose();
            Status.Dispose();
            Close.Dispose();
            SelectedPage.Dispose();
            foreach (var page in _pages)
            {
                page.Dispose();
            }

            _pages.Clear();
        }

        base.Dispose(disposing);
    }
}

public enum ShellStatus
{
    Normal,
    Warning,
    Error,
}
