using Asv.Avalonia.IO;
using Xunit;

namespace Asv.Avalonia.Test.IO;

public class PacketFieldChartTest
{
    [Theory]
    [InlineData("int32", "-505", -505)]
    [InlineData("uint64", "9000100031", 9000100031)]
    [InlineData("double", "28.225", 28.225)]
    [InlineData("float?", "0.640307", 0.640307)]
    [InlineData("half", "1.5", 1.5)]
    public void NumericFields_ParseInvariantValues(string type, string text, double expected)
    {
        var field = new PacketFieldSnapshot("value", type, text, null, null, []);

        var accepted = PacketFieldChart.TryGetNumber(field, out var value);

        Assert.True(accepted);
        Assert.Equal(expected, value);
    }

    [Fact]
    public void NumericFields_UseCapturedNumberInsteadOfFormattedText()
    {
        var field = new PacketFieldSnapshot("value", "uint16", "0x00FF", null, null, [])
        {
            NumericValue = 255,
        };

        var accepted = PacketFieldChart.TryGetNumber(field, out var value);

        Assert.True(accepted);
        Assert.Equal(255, value);
    }

    [Theory]
    [InlineData("string", "123")]
    [InlineData("bool", "1")]
    [InlineData("array", "12")]
    [InlineData("double", "NaN")]
    [InlineData("double", "Infinity")]
    [InlineData("int32?", "∅")]
    public void NonNumericOrMissingValues_DoNotBecomeChartPoints(string type, string text)
    {
        var field = new PacketFieldSnapshot("value", type, text, null, null, []);

        Assert.False(PacketFieldChart.TryGetNumber(field, out _));
    }
}
