using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents an event triggered when validation happens.
/// </summary>
/// <param name="source">.</param>
/// <param name="validatedObject">Object that was validated.</param>
/// <param name="validationResult">Result of the validation.</param>
public class ValidationEvent(
    IViewModel source,
    object validatedObject,
    ValidationResult validationResult
) : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Direct)
{
    public object ValidatedObject => validatedObject;
    public ValidationResult ValidationResult => validationResult;
}
