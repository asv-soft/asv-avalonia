namespace Asv.Avalonia;

public class AppInfo : IAppInfo
{
    public required string Title { get; set; }
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string CompanyName { get; set; }
    public required string AvaloniaVersion { get; set; }
}
