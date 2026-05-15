using Asv.Common;

namespace Asv.Avalonia;

public class LocalizedWithTitleException : LocalizedException
{
    public LocalizedWithTitleException()
    {
        LocalizedTitle = string.Empty;
    }

    public LocalizedWithTitleException(string? message, string? localizedMessage = null)
        : this(message, localizedMessage, string.Empty) { }

    public LocalizedWithTitleException(
        string? message,
        Exception? innerException,
        string? localizedMessage = null
    )
        : this(message, innerException, localizedMessage, string.Empty) { }

    public LocalizedWithTitleException(
        string? message,
        string? localizedMessage = null,
        string? localizedTitle = null
    )
        : base(message, localizedMessage)
    {
        LocalizedTitle = localizedTitle ?? string.Empty;
    }

    public LocalizedWithTitleException(
        string? message,
        Exception? innerException,
        string? localizedMessage = null,
        string? localizedTitle = null
    )
        : base(message, innerException, localizedMessage)
    {
        LocalizedTitle = localizedTitle ?? string.Empty;
    }

    public string LocalizedTitle { get; }
    public new string LocalizedMessage => base.LocalizedMessage ?? string.Empty;
}
