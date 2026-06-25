using System.Security.Cryptography;
using System.Text;
using Asv.Modeling;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public interface IAppPath
{
    string GetAppPathFolder(string additionalPrefix);
    string GetAppPathFile(string fileName);
    string GetPageFolder(NavId pageId, string additionalPrefix);
}

public class AppPath : IAppPath
{
    private const int MaxFileNameSegmentLength = 120;
    private static readonly HashSet<char> InvalidFileNameChars =
    [
        .. Path.GetInvalidFileNameChars(),
    ];
    private readonly string _rootDirectory;

    public AppPath(IHostEnvironment environment)
    {
        _rootDirectory = GetRootDirectory(environment);
        Directory.CreateDirectory(_rootDirectory);
    }

    public string GetAppPathFolder(string additionalPrefix)
    {
        return GetAppPathFolder(_rootDirectory, additionalPrefix);
    }

    public string GetAppPathFile(string fileName)
    {
        return GetAppPathFile(_rootDirectory, fileName);
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

    public static string GetAppPathFolder(IHostEnvironment environment, string additionalPrefix)
    {
        return GetAppPathFolder(GetRootDirectory(environment), additionalPrefix);
    }

    public static string GetAppPathFile(IHostEnvironment environment, string fileName)
    {
        return GetAppPathFile(GetRootDirectory(environment), fileName);
    }

    private static string GetRootDirectory(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentException.ThrowIfNullOrWhiteSpace(environment.ContentRootPath);

        return Path.Combine(environment.ContentRootPath, "data");
    }

    private static string GetAppPathFolder(string rootDirectory, string additionalPrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(additionalPrefix);

        var directory = Path.Combine(rootDirectory, EscapeFileNameSegment(additionalPrefix));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static string GetAppPathFile(string rootDirectory, string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        Directory.CreateDirectory(rootDirectory);
        return Path.Combine(rootDirectory, EscapeFileNameSegment(fileName));
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
        if (escaped.Length == 0 || escaped is "." or "..")
        {
            return "_";
        }

        if (escaped.Length <= MaxFileNameSegmentLength)
        {
            return escaped;
        }

        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(escaped)))[..16];
        return $"{escaped[..(MaxFileNameSegmentLength - hash.Length - 1)]}-{hash}";
    }
}
