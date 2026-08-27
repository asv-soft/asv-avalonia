using Asv.Avalonia.GeoMap;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Xunit;

namespace Asv.Avalonia.Test;

public class MapZoomControlTest
{
    [Fact]
    public void Constructor_UsesMapZoomDefaults()
    {
        // Arrange & Act
        var control = new MapZoomControl();

        // Assert
        Assert.Equal(10, control.Zoom);
        Assert.Equal(IZoomService.MinZoomLevel, control.MinZoom);
        Assert.Equal(IZoomService.MaxZoomLevel, control.MaxZoom);
        Assert.True(control.CanZoomIn);
        Assert.True(control.CanZoomOut);
    }

    [Fact]
    public void ZoomButtons_WhenEnabled_ChangeZoomAndDisplayedValue()
    {
        // Arrange
        var control = new MapZoomControl
        {
            MinZoom = 1,
            MaxZoom = 19,
            Zoom = 10,
        };
        var zoomInButton = control.FindControl<Button>("PART_ZoomInButton");
        var zoomOutButton = control.FindControl<Button>("PART_ZoomOutButton");
        var zoomText = control.FindControl<TextBlock>("PART_ZoomText");

        Assert.NotNull(zoomInButton);
        Assert.NotNull(zoomOutButton);
        Assert.NotNull(zoomText);

        // Act
        zoomInButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Assert
        Assert.Equal(11, control.Zoom);
        Assert.Equal("11", zoomText.Text);

        // Act
        zoomOutButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        // Assert
        Assert.Equal(10, control.Zoom);
        Assert.Equal("10", zoomText.Text);
    }

    [Theory]
    [InlineData(3, true, false)]
    [InlineData(4, true, true)]
    [InlineData(5, false, true)]
    public void Zoom_AtRangePosition_UpdatesButtonAvailability(
        int zoom,
        bool expectedCanZoomIn,
        bool expectedCanZoomOut
    )
    {
        // Arrange & Act
        var control = new MapZoomControl
        {
            MinZoom = 3,
            MaxZoom = 5,
            Zoom = zoom,
        };
        var zoomInButton = control.FindControl<Button>("PART_ZoomInButton");
        var zoomOutButton = control.FindControl<Button>("PART_ZoomOutButton");

        // Assert
        Assert.NotNull(zoomInButton);
        Assert.NotNull(zoomOutButton);
        Assert.Equal(expectedCanZoomIn, control.CanZoomIn);
        Assert.Equal(expectedCanZoomOut, control.CanZoomOut);
        Assert.Equal(expectedCanZoomIn, zoomInButton.IsEnabled);
        Assert.Equal(expectedCanZoomOut, zoomOutButton.IsEnabled);
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(6, 5)]
    public void Zoom_OutsideRange_IsClamped(int zoom, int expectedZoom)
    {
        // Arrange & Act
        var control = new MapZoomControl
        {
            MinZoom = 3,
            MaxZoom = 5,
            Zoom = zoom,
        };

        // Assert
        Assert.Equal(expectedZoom, control.Zoom);
    }

    [Fact]
    public void BoundsChange_CurrentZoomOutsideRange_ClampsAndUpdatesAvailability()
    {
        // Arrange
        var lowerBoundControl = new MapZoomControl { Zoom = 10 };

        // Act
        lowerBoundControl.MinZoom = 12;

        // Assert
        Assert.Equal(12, lowerBoundControl.Zoom);
        Assert.True(lowerBoundControl.CanZoomIn);
        Assert.False(lowerBoundControl.CanZoomOut);

        // Arrange
        var upperBoundControl = new MapZoomControl
        {
            MinZoom = 3,
            MaxZoom = 19,
            Zoom = 10,
        };

        // Act
        upperBoundControl.MaxZoom = 5;

        // Assert
        Assert.Equal(5, upperBoundControl.Zoom);
        Assert.False(upperBoundControl.CanZoomIn);
        Assert.True(upperBoundControl.CanZoomOut);
    }
}
