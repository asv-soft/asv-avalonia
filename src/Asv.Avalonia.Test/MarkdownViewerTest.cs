using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Xunit;

namespace Asv.Avalonia.Test;

public class MarkdownViewerTest
{
    [Fact]
    public void Text_WithBoldSpan_RendersBoldRun()
    {
        // Arrange
        const string markdown = "Normal **bold** normal";

        // Act
        var runs = GetParagraphRuns(markdown);

        // Assert
        Assert.Collection(
            runs,
            run =>
            {
                Assert.Equal("Normal ", run.Text);
                Assert.NotEqual(FontWeight.Bold, run.FontWeight);
            },
            run =>
            {
                Assert.Equal("bold", run.Text);
                Assert.Equal(FontWeight.Bold, run.FontWeight);
            },
            run =>
            {
                Assert.Equal(" normal", run.Text);
                Assert.NotEqual(FontWeight.Bold, run.FontWeight);
            }
        );
    }

    [Fact]
    public void Text_WithBoldSpanInsideColorToken_RendersBoldRun()
    {
        // Arrange
        const string markdown = "[color=Warning;]normal **bold**[/color]";

        // Act
        var runs = GetParagraphRuns(markdown);

        // Assert
        Assert.Collection(
            runs,
            run =>
            {
                Assert.Equal("normal ", run.Text);
                Assert.NotEqual(FontWeight.Bold, run.FontWeight);
            },
            run =>
            {
                Assert.Equal("bold", run.Text);
                Assert.Equal(FontWeight.Bold, run.FontWeight);
            }
        );
    }

    [Fact]
    public void Text_WithEscapedBoldMarkers_RendersLiteralMarkers()
    {
        // Arrange
        const string markdown = @"\**not bold\**";

        // Act
        var runs = GetParagraphRuns(markdown);

        // Assert
        var run = Assert.Single(runs);
        Assert.Equal("**not bold**", run.Text);
        Assert.NotEqual(FontWeight.Bold, run.FontWeight);
    }

    private static IReadOnlyList<Run> GetParagraphRuns(string markdown)
    {
        var viewer = new MarkdownViewer { Text = markdown };
        var root = Assert.IsType<StackPanel>(viewer.Content);
        var block = Assert.IsType<SelectableTextBlock>(Assert.Single(root.Children));
        return block.Inlines?.OfType<Run>().ToArray() ?? [];
    }
}
