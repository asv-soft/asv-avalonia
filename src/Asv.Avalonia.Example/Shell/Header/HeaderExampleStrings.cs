namespace Asv.Avalonia.Example;

public static class HeaderExampleStrings
{
    public static string DisplayName => RS.ResourceManager.GetString("HeaderExample_DisplayName", RS.Culture) ?? "Demo User";
    public static string Description => RS.ResourceManager.GetString("HeaderExample_Description", RS.Culture) ?? "Example profile card.";
}
