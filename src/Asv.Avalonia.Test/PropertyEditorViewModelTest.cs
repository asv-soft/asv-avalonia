using Xunit;

namespace Asv.Avalonia.Test;

public class PropertyEditorViewModelTest
{
    [Fact]
    public void Items_ContainsAllSourceProperties()
    {
        // Arrange
        using var editor = new PropertyEditorViewModel("editor");
        var enabled = new TestPropertyViewModel("enabled");
        var disabled = new TestPropertyViewModel("disabled") { IsVisible = false };
        editor.ItemsSource.Add(enabled);
        editor.ItemsSource.Add(disabled);

        // Act
        var items = GetItems(editor);

        // Assert
        Assert.Equal([enabled, disabled], items);
    }

    [Fact]
    public void ItemsSource_AddItem_UpdatesItems()
    {
        // Arrange
        using var editor = new PropertyEditorViewModel("editor");
        var first = new TestPropertyViewModel("first");
        var second = new TestPropertyViewModel("second");
        editor.ItemsSource.Add(first);

        // Act
        editor.ItemsSource.Add(second);
        var items = GetItems(editor);

        // Assert
        Assert.Equal([first, second], items);
    }

    private static List<IPropertyViewModel> GetItems(PropertyEditorViewModel editor)
    {
        var result = new List<IPropertyViewModel>();
        foreach (var item in editor.Items)
        {
            result.Add(item);
        }

        return result;
    }

    private sealed class TestPropertyViewModel(string id) : PropertyViewModel(id) { }
}
