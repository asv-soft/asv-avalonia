using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Avalonia;

public partial class PropertyComboBoxView : UserControl
{
    public PropertyComboBoxView()
    {
        InitializeComponent();
    }

    private void ItemButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (
            DataContext is not PropertyComboBoxViewModel viewModel
            || sender is not Control { DataContext: IHeadlinedViewModel item }
        )
        {
            return;
        }

        viewModel.SelectedItem.Value = item;
        PART_FlyoutButton.Flyout?.Hide();
    }
}
