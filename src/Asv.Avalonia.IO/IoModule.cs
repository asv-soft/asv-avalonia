namespace Asv.Avalonia.IO;

public class IoModule : IExportInfo
{
    public const string Name = "Asv.Avalonia.IO";
    public static IExportInfo Instance { get; } = new IoModule();

    private IoModule() { }

    public string ModuleName => Name;
}
