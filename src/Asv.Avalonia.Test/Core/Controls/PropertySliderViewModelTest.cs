using System.Globalization;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class PropertySliderViewModelTest
{
    [Fact]
    public void Constructor_SetsDefaultIconColor()
    {
        // Arrange
        using var model = new BindableReactiveProperty<double>(25);

        // Act
        using var property = new PropertySliderReactive("slider", model);

        // Assert
        Assert.Equal(AsvColorKind.Info3, property.IconColor);
    }

    [Fact]
    public void Reactive_ModelUpdate_UpdatesValueAndDisplayValue()
    {
        // Arrange
        using var model = new BindableReactiveProperty<double>(25);
        using var property = new PropertySliderReactive("slider", model) { ValueFormat = "0.0" };

        // Act
        model.Value = 40.25;

        // Assert
        Assert.Equal(40.25, property.Value.Value);
        Assert.Equal(40.25.ToString("0.0", CultureInfo.CurrentCulture), property.DisplayValue);
    }

    [Fact]
    public async Task SetValueFromUser_Reactive_UpdatesModel()
    {
        // Arrange
        using var model = new BindableReactiveProperty<double>(10);
        using var property = new PropertySliderReactive("slider", model);

        // Act
        await property.SetValueFromUser(42, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(42, model.Value);
        Assert.Equal(42, property.Value.Value);
    }

    [Fact]
    public void Reactive_CustomRange_KeepsInitialModelValue()
    {
        // Arrange
        using var model = new BindableReactiveProperty<double>(150);

        // Act
        using var property = new PropertySliderReactive("slider", model, 0, 200);

        // Assert
        Assert.Equal(150, property.Value.Value);
    }

    [Fact]
    public async Task SetValueFromUser_CallbackFailure_RestoresPreviousValue()
    {
        // Arrange
        using var update = new Subject<double>();
        using var property = new PropertySliderCallback(
            "slider",
            _ => ValueTask.FromResult(20d),
            (_, _) => throw new InvalidOperationException("Write failed"),
            update
        );

        await property.Refresh(TestContext.Current.CancellationToken);

        // Act
        await property.SetValueFromUser(40, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(20, property.Value.Value);
        Assert.Equal("Write failed", property.ErrorMessage);
    }
}
