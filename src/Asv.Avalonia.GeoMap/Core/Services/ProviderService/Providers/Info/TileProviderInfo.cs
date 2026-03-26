namespace Asv.Avalonia.GeoMap;

#pragma warning disable SA1313
public readonly record struct TileProviderGroup(string Id, Func<string> NameCallback)
#pragma warning restore SA1313
{
    public static readonly TileProviderGroup Other = new(
        "Other",
        () => RS.TileProviderGroup_Other_Name
    );
    public static readonly TileProviderGroup Yandex = new(
        "Yandex",
        () => RS.TileProviderGroup_Yandex_Name
    );
    public static readonly TileProviderGroup Bing = new(
        "Bing",
        () => RS.TileProviderGroup_Bing_Name
    );

    public bool Equals(TileProviderGroup other) => Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();

    public override string ToString() => NameCallback();
}

public readonly record struct TileProviderInfo
{
    public required string Id { get; init; }
    public required Func<string> NameCallback { get; init; }
    public string Name => NameCallback();
    public required TileProviderGroup Group { get; init; }
}
