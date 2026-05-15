namespace Asv.Avalonia;

public class NullAppInfo : IAppInfo
{
    public static IAppInfo Instance { get; } = new NullAppInfo();

    private NullAppInfo() { }

    public string Name => "Asv.Avalonia";
    public string Description => "Avalonia library SDK";
    public string Version => "0.1.0";
    public string CompanyName => "Asv soft";
    public string AvaloniaVersion => "0.11.3";
}
