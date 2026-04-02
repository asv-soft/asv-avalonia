using Asv.Common;

namespace Asv.Avalonia;

public class UnitException : ValidationException
{
    public UnitException()
        : base() { }

    public UnitException(string? message, string? localizedMessage = null)
        : base(message, localizedMessage) { }

    public UnitException(
        string? message,
        Exception? innerException,
        string? localizedMessage = null
    )
        : base(message, innerException, localizedMessage) { }
}
