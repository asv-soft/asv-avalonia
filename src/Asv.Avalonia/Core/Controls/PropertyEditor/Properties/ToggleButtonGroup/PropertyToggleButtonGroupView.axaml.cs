using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Avalonia;

public partial class PropertyToggleButtonGroupView : UserControl
{
    public PropertyToggleButtonGroupView()
    {
        InitializeComponent();
    }

    private async void ItemButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (
            DataContext is not PropertyToggleButtonGroupViewModel viewModel
            || sender
                is not Control
                {
                    DataContext: PropertyToggleButtonGroupItemViewModel { Item: { } item },
                }
        )
        {
            return;
        }

        await viewModel.SelectItem(item);
    }
}
