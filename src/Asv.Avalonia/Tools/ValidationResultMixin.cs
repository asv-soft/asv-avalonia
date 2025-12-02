using Asv.Common;

namespace Asv.Avalonia;

public static class ValidationResultMixin
{
    /// <summary>
    /// Gets a localized error message from a validation exception
    /// </summary>
    /// <remarks>
    /// Works only with base ValidationException's inheritors
    /// </remarks>
    /// <param name="value">Validation exception.</param>
    /// <returns>String with the localized exception message or null</returns>
    public static string? GetLocalizedExceptionMessage(ValidationException? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value.GetType() == typeof(IsNullOrWhiteSpaceValidationException))
        {
            return RS.ValidationResult_ErrorText_IsNullOrWhiteSpace;
        }

        if (value.GetType() == typeof(InvalidCharactersValidationException))
        {
            return RS.ValidationResult_ErrorText_InvalidCharacters;
        }

        if (value.GetType() == typeof(NotNumberValidationException))
        {
            return RS.ValidationResult_ErrorText_NotANumber;
        }

        return RS.ValidationResult_ErrorText_Unknown;
    }
}

public readonly struct ValidationResultWrapper
{
    public required ValidationResult Validation { get; init; }
    public string? LocalizedErrorText { get; init; }

    public ValidationException? GetLocalizedException() =>
        LocalizedErrorText is null ? null : new ValidationException(LocalizedErrorText);
}
