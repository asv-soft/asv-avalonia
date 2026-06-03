using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Avalonia;

public partial class PropertyComboBoxView : UserControl
{
    public PropertyComboBoxView()
    {
        InitializeComponent();
    }

    private async void ItemButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (
            DataContext is not PropertyComboBoxViewModel viewModel
            || sender is not Control { DataContext: IHeadlinedViewModel item }
        )
        {
            return;
        }

        PART_FlyoutButton.Flyout?.Hide();
        await viewModel.SelectItem(item);
    }
}
