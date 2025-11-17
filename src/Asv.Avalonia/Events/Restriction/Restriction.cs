namespace Asv.Avalonia;

/// <summary>
/// Represents a reason why the action should be cancelled.
/// This can be used to provide detailed feedback on why the action should be cancelled.
/// </summary>
public class Restriction
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Restriction"/> class.
    /// </summary>
    /// <param name="source">The source of the restriction, typically a UI component or module.</param>
    /// <param name="message">(Optional) The message explaining why the action should be canceled.</param>
    public Restriction(IRoutable source, string? message = null)
    {
        Source = source;
        Message = message;
    }

    /// <summary>
    /// Gets the source that asked for the cancellation.
    /// </summary>
    public IRoutable Source { get; }

    /// <summary>
    /// Gets the message explaining why the action should be canceled.
    /// </summary>
    public string? Message { get; }
}
