namespace Asv.Avalonia;

/// <summary>
/// Localized labels for the hot key editor, backed by the application's resources.
/// </summary>
public static class HotKeyEditorText
{
    private static string Get(string key) =>
        RS.ResourceManager.GetString($"HotKeyEditor_{key}", RS.Culture)!;

    public static string Title => Get(nameof(Title));
    public static string Search => Get(nameof(Search));
    public static string All => Get(nameof(All));
    public static string Modified => Get(nameof(Modified));
    public static string Unassigned => Get(nameof(Unassigned));
    public static string Conflicts => Get(nameof(Conflicts));
    public static string Command => Get(nameof(Command));
    public static string Gesture => Get(nameof(Gesture));
    public static string Default => Get(nameof(Default));
    public static string Reset => Get(nameof(Reset));
    public static string ResetAll => Get(nameof(ResetAll));
    public static string Clear => Get(nameof(Clear));
    public static string Apply => Get(nameof(Apply));
    public static string Cancel => Get(nameof(Cancel));
    public static string Saved => Get(nameof(Saved));
    public static string Pending => Get(nameof(Pending));
    public static string Count => Get(nameof(Count));
    public static string Empty => Get(nameof(Empty));
    public static string Capture => Get(nameof(Capture));
    public static string CaptureHint => Get(nameof(CaptureHint));
    public static string Replace => Get(nameof(Replace));
    public static string ConflictMessage => Get(nameof(ConflictMessage));
    public static string ExternalChanges => Get(nameof(ExternalChanges));
}
