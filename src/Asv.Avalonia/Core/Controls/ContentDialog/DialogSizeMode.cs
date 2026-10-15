namespace Asv.Avalonia;

/// <summary>
/// Determines how much of the dialog host is occupied by a dialog.
/// </summary>
public enum DialogSizeMode
{
    /// <summary>Sizes to content using the standard dialog limits.</summary>
    Compact,

    /// <summary>Fills the available width and sizes the height to content.</summary>
    Wide,

    /// <summary>Fills the available width and height.</summary>
    Expanded,
}
