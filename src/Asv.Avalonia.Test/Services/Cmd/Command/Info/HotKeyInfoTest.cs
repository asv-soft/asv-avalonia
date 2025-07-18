using Avalonia.Input;
using JetBrains.Annotations;
using Xunit;

namespace Asv.Avalonia.Test;

[TestSubject(typeof(HotKeyInfo))]
public class HotKeyInfoTest
{
    [Fact]
    public void Parse_ValidInputWithSingleKey_ReturnsCorrectHotKeyInfo()
    {
        // Arrange
        string input = "Ctrl+A";

        // Act
        var hotKeyInfo = HotKeyInfo.Parse(input);

        // Assert
        Assert.Equal(Key.A, hotKeyInfo.Gesture.Key);
        Assert.Equal(KeyModifiers.Control, hotKeyInfo.Gesture.KeyModifiers);
        Assert.Null(hotKeyInfo.AdditionalKey);
    }

    [Fact]
    public void Parse_ValidInputWithAdditionalKey_ReturnsCorrectHotKeyInfo()
    {
        // Arrange
        string input = "Ctrl+Shift+B ; C";

        // Act
        var hotKeyInfo = HotKeyInfo.Parse(input);

        // Assert
        Assert.Equal(Key.B, hotKeyInfo.Gesture.Key);
        Assert.Equal(KeyModifiers.Control | KeyModifiers.Shift, hotKeyInfo.Gesture.KeyModifiers);
        Assert.Equal(Key.C, hotKeyInfo.AdditionalKey);
    }

    [Fact]
    public void Parse_EmptyString_ThrowsFormatException()
    {
        // Arrange
        string input = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => HotKeyInfo.Parse(input));
    }

    [Fact]
    public void Parse_InvalidFormatWithTooManyParts_ThrowsFormatException()
    {
        // Arrange
        string input = "Ctrl+A ; B ; C";

        // Act & Assert
        Assert.Throws<FormatException>(() => HotKeyInfo.Parse(input));
    }

    [Fact]
    public void Parse_InvalidKeyGesture_ThrowsException()
    {
        // Arrange
        string input = "InvalidKey";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => HotKeyInfo.Parse(input));
    }

    [Fact]
    public void Parse_InvalidAdditionalKey_ThrowsArgumentException()
    {
        // Arrange
        string input = "Ctrl+A ; InvalidKey";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => HotKeyInfo.Parse(input));
    }

    [Fact]
    public void Constructor_WithKeyAndModifiers_CreatesCorrectHotKeyInfo()
    {
        // Arrange
        var key = Key.Z;
        var modifiers = KeyModifiers.Alt;

        // Act
        var hotKeyInfo = new HotKeyInfo(key, modifiers);

        // Assert
        Assert.Equal(key, hotKeyInfo.Gesture.Key);
        Assert.Equal(modifiers, hotKeyInfo.Gesture.KeyModifiers);
        Assert.Null(hotKeyInfo.AdditionalKey);
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesCorrectHotKeyInfo()
    {
        // Arrange
        string input = "Ctrl+Shift+X ; Y";

        // Act
        HotKeyInfo hotKeyInfo = input;

        // Assert
        Assert.Equal(Key.X, hotKeyInfo.Gesture.Key);
        Assert.Equal(KeyModifiers.Control | KeyModifiers.Shift, hotKeyInfo.Gesture.KeyModifiers);
        Assert.Equal(Key.Y, hotKeyInfo.AdditionalKey);
    }

    [Fact]
    public void ToString_WithAdditionalKey_ReturnsCorrectFormat()
    {
        // Arrange
        var hotKeyInfo = new HotKeyInfo(new KeyGesture(Key.D, KeyModifiers.Control), Key.E);

        // Act
        var result = hotKeyInfo.ToString();

        // Assert
        Assert.Equal("Ctrl+D ; E", result);
    }

    [Fact]
    public void ToString_WithoutAdditionalKey_ReturnsGestureOnly()
    {
        // Arrange
        var hotKeyInfo = new HotKeyInfo(new KeyGesture(Key.F, KeyModifiers.Alt));

        // Act
        var result = hotKeyInfo.ToString();

        // Assert
        Assert.Equal("Alt+F", result);
    }
}
