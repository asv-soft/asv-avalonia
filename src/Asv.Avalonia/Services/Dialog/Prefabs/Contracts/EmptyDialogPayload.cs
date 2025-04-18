namespace Asv.Avalonia;

public sealed class EmptyDialogPayload
{
    private EmptyDialogPayload() { }

    public static EmptyDialogPayload EmptyDialog { get; } = new();
}
