using Avalonia.Input;

namespace Asv.Avalonia;

public class HotKeyInfo(KeyGesture gesture, Key? additional = null) : IEquatable<HotKeyInfo>
{
    private readonly KeyGesture _gesture = gesture;
    private readonly Key? _additional = additional;
    public const char Separator = ';';

    public static HotKeyInfo Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
        }

        var items = value.Split(Separator);
        switch (items.Length)
        {
            case <= 0:
                throw new FormatException($"Invalid hotkey format: {value}");
            case > 2:
                throw new FormatException(
                    $"Invalid hotkey format: {value}. Expected format is 'KeyGesture, AdditionalKey'."
                );
        }

        var keyGesture = KeyGesture.Parse(items[0].Trim());
        var additionalKey = items.Length == 2 ? (Key?)Enum.Parse<Key>(items[1].Trim(), true) : null;

        return new HotKeyInfo(keyGesture, additionalKey);
    }

    public HotKeyInfo(Key key, KeyModifiers modifiers = KeyModifiers.None)
        : this(new KeyGesture(key, modifiers), null) { }

    public static implicit operator HotKeyInfo(string value) => Parse(value);

    public static implicit operator string?(HotKeyInfo? value) => value?.ToString();

    public KeyGesture Gesture => _gesture;
    public Key? AdditionalKey => _additional;

    public bool Equals(HotKeyInfo? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _gesture.Equals(other._gesture) && _additional == other._additional;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((HotKeyInfo)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_gesture, _additional);
    }

    public static bool operator ==(HotKeyInfo? left, HotKeyInfo? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(HotKeyInfo? left, HotKeyInfo? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return AdditionalKey.HasValue
            ? $"{Gesture} {Separator} {AdditionalKey.Value}"
            : Gesture.ToString();
    }
}
