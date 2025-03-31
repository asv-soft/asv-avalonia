using System.Linq;
using Asv.Avalonia.Example.PacketViewer;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(PacketViewerViewModel))]
public partial class PacketViewerView : UserControl
{
    public PacketViewerView()
    {
        InitializeComponent();
    }

    private void OnSourcesChecked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is PacketViewerViewModel vm)
        {
            foreach (var filter in vm.FiltersBySource)
            {
                filter.IsChecked.Value = true;
            }
        }
    }

    private void OnSourcesUnchecked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is PacketViewerViewModel vm)
        {
            foreach (var filter in vm.FiltersBySource)
            {
                filter.IsChecked.Value = false;
            }
        }
    }

    private void OnTypesChecked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is PacketViewerViewModel vm)
        {
            foreach (var filter in vm.FiltersByType)
            {
                filter.IsChecked.Value = true;
            }
        }
    }

    private void OnTypesUnchecked(object? sender, RoutedEventArgs e)
    {
        if (DataContext is PacketViewerViewModel vm)
        {
            foreach (var filter in vm.FiltersByType)
            {
                filter.IsChecked.Value = false;
            }
        }
    }
}
