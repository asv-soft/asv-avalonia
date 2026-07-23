using System.Diagnostics.CodeAnalysis;

namespace Asv.Avalonia;

/// <summary>
/// Reads the log entries previously written by the application.
/// </summary>
public interface ILogReaderService
{
    /// <summary>
    /// Loads the stored log entries as an asynchronous stream.
    /// The order of the returned entries is not guaranteed.
    /// </summary>
    /// <param name="cancel">A token that cancels the enumeration.</param>
    /// <returns>An asynchronous sequence of the stored log entries.</returns>
    [RequiresUnreferencedCode(
        "Uses Newtonsoft.Json reflection-based serialization, which is not trim safe."
    )]
    IAsyncEnumerable<LogMessage> LoadItemsFromLogFile(CancellationToken cancel = default);
}
