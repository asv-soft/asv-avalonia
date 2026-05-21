using System.Text;
using Asv.Modeling;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public interface IAppPath
{
    string GetAppPathFolder(string additionalPrefix);
    string GetPageFolder(NavId pageId, string additionalPrefix);
}

public class AppPath : IAppPath
{
    private static readonly HashSet<char> InvalidFileNameChars =
    [
        .. Path.GetInvalidFileNameChars(),
    ];
    private readonly string _rootDirectory;

    public AppPath(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentException.ThrowIfNullOrWhiteSpace(environment.ContentRootPath);

        _rootDirectory = Path.Combine(environment.ContentRootPath, "data");
        Directory.CreateDirectory(_rootDirectory);
    }

    public string GetAppPathFolder(string additionalPrefix)
    {
        return _rootDirectory;
    }

    public string GetPageFolder(NavId pageId, string additionalPrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(additionalPrefix);

        var directory = Path.Combine(
            _rootDirectory,
            EscapeFileNameSegment(additionalPrefix),
            EscapeFileNameSegment(pageId.ToString())
        );
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string EscapeFileNameSegment(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (InvalidFileNameChars.Contains(ch) || ch == '%')
            {
                builder.Append('%');
                builder.Append(((int)ch).ToString("X4"));
                continue;
            }

            builder.Append(ch);
        }

        var escaped = builder.ToString().TrimEnd(' ', '.');
        return escaped.Length == 0 || escaped is "." or ".." ? "_" : escaped;
    }
}
