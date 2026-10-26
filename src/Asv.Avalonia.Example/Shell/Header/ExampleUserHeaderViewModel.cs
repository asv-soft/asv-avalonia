namespace Asv.Avalonia.Example;

public sealed class ExampleUserHeaderViewModel() : ViewModel("example.header.user")
{
    public string Initials => "DU";
    public string DisplayName => HeaderExampleStrings.DisplayName;
    public string Email => "demo@example.com";
    public string Description => HeaderExampleStrings.Description;
}
