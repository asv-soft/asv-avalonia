namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Receives session completion notifications from an interaction session.
/// </summary>
internal interface IMapInteractionSessionOwner
{
    /// <summary>
    /// Ends the specified session.
    /// </summary>
    /// <param name="session">The session being completed.</param>
    void End(IMapInteractionSession session);
}
