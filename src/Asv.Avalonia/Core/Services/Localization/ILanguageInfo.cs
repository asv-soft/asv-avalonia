namespace Asv.Avalonia;

/// <summary>
/// Describes an application UI language.
/// </summary>
public interface ILanguageInfo
{
    /// <summary>
    /// Gets the culture identifier of the language, for example <c>en</c> or <c>ru</c>.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the human-readable name shown in the UI.
    /// </summary>
    string DisplayName { get; }
}
