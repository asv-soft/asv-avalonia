using Material.Icons;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class PropertyToggleButtonGroupViewModelTest
{
    [Fact]
    public void Constructor_InheritsComboBoxViewModel()
    {
        // Arrange
        using var model = new BindableReactiveProperty<IHeadlinedViewModel?>();

        // Act
        using var property = new PropertyToggleButtonGroupReactive("group", model);

        // Assert
        Assert.IsAssignableFrom<PropertyComboBoxViewModel>(property);
    }

    [Fact]
    public void Reactive_ModelUpdate_UpdatesSelectedButton()
    {
        // Arrange
        var speed = CreateItem("speed", "Speed");
        var quality = CreateItem("quality", "Quality");
        using var model = new BindableReactiveProperty<IHeadlinedViewModel?>(speed);
        using var property = new PropertyToggleButtonGroupReactive("group", model);
        property.ItemsSource.Add(speed);
        property.ItemsSource.Add(quality);

        // Act
        model.Value = quality;

        // Assert
        var buttonItems = GetButtonItems(property);
        Assert.False(buttonItems[0].IsSelected);
        Assert.True(buttonItems[1].IsSelected);
        Assert.Equal(quality, property.SelectedItem.Value);
    }

    [Fact]
    public async Task SelectItem_Reactive_UpdatesModelAndSelectedButton()
    {
        // Arrange
        var speed = CreateItem("speed", "Speed");
        var quality = CreateItem("quality", "Quality");
        using var model = new BindableReactiveProperty<IHeadlinedViewModel?>(speed);
        using var property = new PropertyToggleButtonGroupReactive("group", model);
        property.ItemsSource.Add(speed);
        property.ItemsSource.Add(quality);

        // Act
        await property.SelectItem(quality, TestContext.Current.CancellationToken);

        // Assert
        var buttonItems = GetButtonItems(property);
        Assert.Equal(quality, model.Value);
        Assert.False(buttonItems[0].IsSelected);
        Assert.True(buttonItems[1].IsSelected);
    }

    [Fact]
    public void Reactive_ModelItemWithSameId_SelectsLocalButton()
    {
        // Arrange
        var modelItem = CreateItem("quality", "Quality");
        var speed = CreateItem("speed", "Speed");
        var quality = CreateItem("quality", "Quality");
        using var model = new BindableReactiveProperty<IHeadlinedViewModel?>(modelItem);
        using var property = new PropertyToggleButtonGroupReactive("group", model);

        // Act
        property.ItemsSource.Add(speed);
        property.ItemsSource.Add(quality);

        // Assert
        var buttonItems = GetButtonItems(property);
        Assert.False(buttonItems[0].IsSelected);
        Assert.True(buttonItems[1].IsSelected);
    }

    [Fact]
    public async Task SelectItem_CallbackFailure_RestoresPreviousSelectedButton()
    {
        // Arrange
        var speed = CreateItem("speed", "Speed");
        var quality = CreateItem("quality", "Quality");
        using var update = new Subject<IHeadlinedViewModel?>();
        using var property = new PropertyToggleButtonGroupCallback(
            "group",
            _ => ValueTask.FromResult<IHeadlinedViewModel?>(speed),
            (_, _) => throw new InvalidOperationException("Write failed"),
            update
        );
        property.ItemsSource.Add(speed);
        property.ItemsSource.Add(quality);
        await property.Refresh(TestContext.Current.CancellationToken);

        // Act
        await property.SelectItem(quality, TestContext.Current.CancellationToken);

        // Assert
        var buttonItems = GetButtonItems(property);
        Assert.Equal(speed, property.SelectedItem.Value);
        Assert.True(buttonItems[0].IsSelected);
        Assert.False(buttonItems[1].IsSelected);
        Assert.Equal("Write failed", property.ErrorMessage);
    }

    private static IHeadlinedViewModel CreateItem(string id, string header)
    {
        return new HeadlinedViewModel(id)
        {
            Header = header,
            Icon = MaterialIconKind.Tune,
            IconColor = AsvColorKind.Success,
        };
    }

    private static List<PropertyToggleButtonGroupItemViewModel> GetButtonItems(
        PropertyToggleButtonGroupViewModel property
    )
    {
        var items = new List<PropertyToggleButtonGroupItemViewModel>();
        foreach (var item in property.ButtonItemsView)
        {
            items.Add(item);
        }

        return items;
    }
}
