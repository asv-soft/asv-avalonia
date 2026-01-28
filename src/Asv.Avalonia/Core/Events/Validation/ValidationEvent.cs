using Asv.Common;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents an event triggered when validation happens.
/// </summary>
/// <param name="source">.</param>
/// <param name="validatedObject">Object that was validated.</param>
/// <param name="validationResult">Result of the validation.</param>
public class ValidationEvent(
    IRoutable source,
    object validatedObject,
    ValidationResult validationResult
) : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Direct)
{
    public object ValidatedObject => validatedObject;
    public ValidationResult ValidationResult => validationResult;
}
