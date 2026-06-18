using System.Globalization;
using Xunit;

namespace Asv.Avalonia.Test;

public class TelemetryProgressBarConverterTest
{
    [Fact]
    public void Convert_FiniteProgress_ShowsDeterminateValue()
    {
        // Arrange
        const double progress = 0.42;

        // Act
        var isVisible = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(bool),
            TelemetryProgressBarConverterMode.IsVisible,
            CultureInfo.InvariantCulture
        );
        var isIndeterminate = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(bool),
            TelemetryProgressBarConverterMode.IsIndeterminate,
            CultureInfo.InvariantCulture
        );
        var value = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(double),
            TelemetryProgressBarConverterMode.Value,
            CultureInfo.InvariantCulture
        );

        // Assert
        Assert.True((bool)isVisible!);
        Assert.False((bool)isIndeterminate!);
        Assert.Equal(progress, (double)value!);
    }

    [Fact]
    public void Convert_InfiniteProgress_ShowsIndeterminateProgress()
    {
        // Arrange
        const double progress = double.PositiveInfinity;

        // Act
        var isVisible = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(bool),
            "IsVisible",
            CultureInfo.InvariantCulture
        );
        var isIndeterminate = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(bool),
            "IsIndeterminate",
            CultureInfo.InvariantCulture
        );
        var value = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(double),
            "Value",
            CultureInfo.InvariantCulture
        );

        // Assert
        Assert.True((bool)isVisible!);
        Assert.True((bool)isIndeterminate!);
        Assert.Equal(0.0, (double)value!);
    }

    [Fact]
    public void Convert_NanProgress_HidesProgress()
    {
        // Arrange
        const double progress = double.NaN;

        // Act
        var isVisible = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(bool),
            TelemetryProgressBarConverterMode.IsVisible,
            CultureInfo.InvariantCulture
        );
        var isIndeterminate = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(bool),
            TelemetryProgressBarConverterMode.IsIndeterminate,
            CultureInfo.InvariantCulture
        );
        var value = TelemetryProgressBarConverter.Instance.Convert(
            progress,
            typeof(double),
            TelemetryProgressBarConverterMode.Value,
            CultureInfo.InvariantCulture
        );

        // Assert
        Assert.False((bool)isVisible!);
        Assert.False((bool)isIndeterminate!);
        Assert.Equal(0.0, (double)value!);
    }
}
