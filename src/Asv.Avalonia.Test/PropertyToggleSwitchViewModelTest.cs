using Material.Icons;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class PropertyToggleSwitchViewModelTest
{
    [Fact]
    public void Constructor_SetsDefaultIconColor()
    {
        // Arrange
        using var model = new BindableReactiveProperty<bool>(true);

        // Act
        using var property = new PropertyToggleSwitchReactive("toggle", model);

        // Assert
        Assert.Equal(AsvColorKind.Info3, property.IconColor);
    }

    [Fact]
    public void Constructor_SetsDefaultStateVisuals()
    {
        // Arrange
        using var model = new BindableReactiveProperty<bool>(true);

        // Act
        using var property = new PropertyToggleSwitchReactive("toggle", model);

        // Assert
        Assert.Equal("ON", property.CheckedText);
        Assert.Equal(MaterialIconKind.ToggleSwitch, property.CheckedIcon);
        Assert.Equal(AsvColorKind.Success, property.CheckedStatus);
        Assert.Equal("OFF", property.UncheckedText);
        Assert.Equal(MaterialIconKind.ToggleSwitchOff, property.UncheckedIcon);
        Assert.Equal(AsvColorKind.Error, property.UncheckedStatus);
    }

    [Fact]
    public void Reactive_ModelUpdate_UpdatesValue()
    {
        // Arrange
        using var model = new BindableReactiveProperty<bool>(false);
        using var property = new PropertyToggleSwitchReactive("toggle", model);

        // Act
        model.Value = true;

        // Assert
        Assert.True(property.Value.Value);
    }

    [Fact]
    public async Task SetValueFromUser_Reactive_UpdatesModel()
    {
        // Arrange
        using var model = new BindableReactiveProperty<bool>(false);
        using var property = new PropertyToggleSwitchReactive("toggle", model);

        // Act
        await property.SetValueFromUser(true, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(model.Value);
        Assert.True(property.Value.Value);
    }

    [Fact]
    public async Task Refresh_Callback_UpdatesValue()
    {
        // Arrange
        using var update = new Subject<bool>();
        using var property = new PropertyToggleSwitchCallback(
            "toggle",
            _ => ValueTask.FromResult(true),
            (_, _) => ValueTask.CompletedTask,
            update
        );

        // Act
        await property.Refresh(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(property.Value.Value);
    }

    [Fact]
    public async Task SetValueFromUser_CallbackFailure_RestoresPreviousValue()
    {
        // Arrange
        using var update = new Subject<bool>();
        using var property = new PropertyToggleSwitchCallback(
            "toggle",
            _ => ValueTask.FromResult(false),
            (_, _) => throw new InvalidOperationException("Write failed"),
            update
        );

        await property.Refresh(TestContext.Current.CancellationToken);

        // Act
        await property.SetValueFromUser(true, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(property.Value.Value);
        Assert.Equal("Write failed", property.ErrorMessage);
    }
}
