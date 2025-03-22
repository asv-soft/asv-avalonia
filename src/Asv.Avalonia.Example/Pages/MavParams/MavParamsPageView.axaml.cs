using Avalonia.Controls;
using Avalonia.Interactivity;
using R3;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(MavParamsPageViewModel))]
public partial class MavParamsPageView : UserControl
{
    public MavParamsPageView()
    {
        InitializeComponent();
    }

    private void ItemDockPanel_DoubleTapped(object? sender, RoutedEventArgs e) // TODO: Think about a way to replace it
    {
        if (Design.IsDesignMode)
        {
            return;
        }

        if (DataContext is MavParamsPageViewModel viewModel)
        {
            viewModel.SelectedItem.Value?.PinItem.Execute(Unit.Default);
        }
    }
}
