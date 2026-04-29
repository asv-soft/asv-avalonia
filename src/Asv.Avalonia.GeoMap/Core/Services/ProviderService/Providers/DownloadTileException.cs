namespace Asv.Avalonia.GeoMap;

public class DownloadTileException : LocalizedWithTitleException
{
    public DownloadTileException() { }

    public DownloadTileException(string? message, string? localizedMessage = null)
        : base(message, localizedMessage) { }

    public DownloadTileException(
        string? message,
        Exception? innerException,
        string? localizedMessage = null
    )
        : base(message, innerException, localizedMessage) { }

    public DownloadTileException(
        string? message,
        string? localizedMessage = null,
        string? localizedTitle = null
    )
        : base(message, localizedMessage, localizedTitle) { }

    public DownloadTileException(
        string? message,
        Exception? innerException,
        string? localizedMessage = null,
        string? localizedTitle = null
    )
        : base(message, innerException, localizedMessage, localizedTitle) { }
}
