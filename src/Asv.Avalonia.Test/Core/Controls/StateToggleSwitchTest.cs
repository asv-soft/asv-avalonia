using Material.Icons;
using Xunit;

namespace Asv.Avalonia.Test;

public class StateToggleSwitchTest
{
    [Fact]
    public void Constructor_UsesUncheckedStateByDefault()
    {
        // Arrange & Act
        var control = new StateToggleSwitch();

        // Assert
        Assert.Equal("OFF", control.StateText);
        Assert.Equal(MaterialIconKind.ToggleSwitchOff, control.StateIcon);
        Assert.Equal(AsvColorKind.Error, control.StateStatus);
        Assert.Equal(0, control.ThumbColumn);
        Assert.Equal(1, control.ContentColumn);
    }

    [Fact]
    public void IsChecked_UpdatesState()
    {
        // Arrange
        var control = new StateToggleSwitch
        {
            CheckedText = "READY",
            CheckedIcon = MaterialIconKind.CheckCircle,
            CheckedStatus = AsvColorKind.Success,
            UncheckedText = "WAIT",
            UncheckedIcon = MaterialIconKind.AlertCircle,
            UncheckedStatus = AsvColorKind.Warning,
        };

        // Act
        control.IsChecked = true;

        // Assert
        Assert.Equal("READY", control.StateText);
        Assert.Equal(MaterialIconKind.CheckCircle, control.StateIcon);
        Assert.Equal(AsvColorKind.Success, control.StateStatus);
        Assert.Equal(1, control.ThumbColumn);
        Assert.Equal(0, control.ContentColumn);
    }
}
