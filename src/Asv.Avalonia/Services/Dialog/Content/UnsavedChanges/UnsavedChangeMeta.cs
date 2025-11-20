namespace Asv.Avalonia;

public class UnsavedChangeMeta
{
    public required IPage Page { get; init; }

    public required List<string> Restrictions { get; init; }
}
