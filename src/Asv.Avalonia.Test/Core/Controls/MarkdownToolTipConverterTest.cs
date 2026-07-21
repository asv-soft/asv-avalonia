using System.Globalization;
using Avalonia.Media;
using Xunit;

namespace Asv.Avalonia.Test;

public class MarkdownToolTipConverterTest
{
    [Fact]
    public void Convert_WithoutDescription_ReturnsNull()
    {
        // Arrange
        object?[] values = ["Header", " "];

        // Act
        var result = MarkdownToolTipConverter.Instance.Convert(
            values,
            typeof(object),
            null,
            CultureInfo.InvariantCulture
        );

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Convert_WithDescription_ReturnsMarkdownViewer()
    {
        // Arrange
        object?[] values = ["Header", "- Ready"];

        // Act
        var result = MarkdownToolTipConverter.Instance.Convert(
            values,
            typeof(object),
            null,
            CultureInfo.InvariantCulture
        );

        // Assert
        var viewer = Assert.IsType<MarkdownViewer>(result);
        Assert.Equal(TextWrapping.Wrap, viewer.TextWrapping);
        Assert.Equal($"### Header{Environment.NewLine}{Environment.NewLine}- Ready", viewer.Text);
    }

    [Fact]
    public void Convert_WithSingleValue_ReturnsMarkdownViewerWithoutHeader()
    {
        // Arrange
        const string description = "- Ready";

        // Act
        var result = MarkdownToolTipConverter.Instance.Convert(
            description,
            typeof(object),
            null,
            CultureInfo.InvariantCulture
        );

        // Assert
        var viewer = Assert.IsType<MarkdownViewer>(result);
        Assert.Equal(description, viewer.Text);
    }

    [Fact]
    public void Convert_WithSingleValueAndParameter_UsesParameterAsHeader()
    {
        // Arrange
        const string description = "- Ready";

        // Act
        var result = MarkdownToolTipConverter.Instance.Convert(
            description,
            typeof(object),
            "Header",
            CultureInfo.InvariantCulture
        );

        // Assert
        var viewer = Assert.IsType<MarkdownViewer>(result);
        Assert.Equal($"### Header{Environment.NewLine}{Environment.NewLine}- Ready", viewer.Text);
    }

    [Fact]
    public void Convert_WithMarkdownCharactersInHeader_EscapesHeader()
    {
        // Arrange
        object?[] values = ["Header [1]", "Description"];

        // Act
        var result = MarkdownToolTipConverter.Instance.Convert(
            values,
            typeof(object),
            null,
            CultureInfo.InvariantCulture
        );

        // Assert
        var viewer = Assert.IsType<MarkdownViewer>(result);
        Assert.Equal(
            $"### Header \\[1\\]{Environment.NewLine}{Environment.NewLine}Description",
            viewer.Text
        );
    }

    [Fact]
    public void RttBoxToolTipConverter_Instance_UsesMarkdownToolTipConverter()
    {
        // Arrange
        object?[] values = ["Header", "Description"];

        // Act
        var result = RttBoxToolTipConverter.Instance.Convert(
            values,
            typeof(object),
            null,
            CultureInfo.InvariantCulture
        );

        // Assert
        Assert.IsType<MarkdownViewer>(result);
    }
}
