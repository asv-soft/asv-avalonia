namespace Asv.Avalonia;

public class SystemModule : IExportInfo
{
    public const string Name = "System";
    public static IExportInfo Instance { get; } = new SystemModule();

    private SystemModule() { }

    public string ModuleName => Name;
}
