using System.Reflection;
using Asv.Cfg;

namespace Asv.Avalonia;

internal class AppHostBuilder : IAppHostBuilder
{
    private const string ZeroVersion = "0.0.0";
    private const string DefaultAppName = "AsvAvaloniaApp";

    private readonly Func<IConfiguration, IAppInfo, string> _defaultUserFolder = (_, info) =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            info.Name
        );
    private readonly string _defaultAppFolder =
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
        ?? throw new ArgumentException("Unable to get app folder");

    public IAppCore Core { get; }

    internal AppHostBuilder(bool isSlim)
    {
        if (!isSlim)
        {
            Core = new AppCore
            {
                AppFolder = _defaultAppFolder,
                UserDataFolder = _defaultUserFolder,
                AppInfo = new AppInfo
                {
                    Title = string.Empty,
                    Name = DefaultAppName,
                    Version = ZeroVersion,
                    CompanyName = string.Empty,
                    AvaloniaVersion = ZeroVersion,
                },
            };

            return;
        }

        Core = new AppCore
        {
            UserDataFolder = (_, info) =>
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    info.Name
                ),
            AppFolder = _defaultAppFolder,
            Configuration = new InMemoryConfiguration(),
            AppInfo = new AppInfo
            {
                Title = string.Empty,
                Name = DefaultAppName,
                Version = ZeroVersion,
                CompanyName = string.Empty,
                AvaloniaVersion = ZeroVersion,
            },
            Args = new AppArgs([]),
            MutexName = _ => null,
            NamedPipe = _ => null,
        };
    }

    public IAppHost Build()
    {
        return new AppHost(Core);
    }
}
