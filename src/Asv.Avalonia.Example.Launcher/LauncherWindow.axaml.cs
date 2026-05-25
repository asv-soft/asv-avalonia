using System.Threading;
using Avalonia.Controls;

namespace Asv.Avalonia.Example.Launcher;

public partial class LauncherWindow : Window
{
    private readonly CancellationTokenSource _lifecycleCts = new();
    private LauncherWindowViewModel? _viewModel;

    public LauncherWindow()
    {
        InitializeComponent();
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is not LauncherWindowViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
        _viewModel.CloseRequested -= OnCloseRequested;
        _viewModel.CloseRequested += OnCloseRequested;

        await _viewModel.StartAsync(_lifecycleCts.Token);
    }

    protected override void OnClosed(EventArgs e)
    {
        _lifecycleCts.Cancel();

        if (_viewModel is not null)
        {
            _viewModel.CloseRequested -= OnCloseRequested;
            _viewModel.Dispose();
            _viewModel = null;
        }

        _lifecycleCts.Dispose();
        base.OnClosed(e);
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        if (!IsVisible)
        {
            return;
        }

        Close();
    }
}
