namespace Asv.Avalonia.IO;

public static class FileExportStrings
{
    private static string Get(string name) => RS.ResourceManager.GetString($"FileExport_{name}", RS.Culture) ?? name;
    public static string Title => Get(nameof(Title));
    public static string Export => Get(nameof(Export));
    public static string Save => Get(nameof(Save));
    public static string Hint => Get(nameof(Hint));
    public static string Exporting => Get(nameof(Exporting));
    public static string Completed => Get(nameof(Completed));
    public static string Canceled => Get(nameof(Canceled));
    public static string SameFile => Get(nameof(SameFile));
}
