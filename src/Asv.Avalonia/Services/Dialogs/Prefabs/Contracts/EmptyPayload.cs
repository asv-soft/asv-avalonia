namespace Asv.Avalonia;

public sealed class EmptyPayload
{
    private EmptyPayload() { }

    public static EmptyPayload Empty { get; } = new();
}
