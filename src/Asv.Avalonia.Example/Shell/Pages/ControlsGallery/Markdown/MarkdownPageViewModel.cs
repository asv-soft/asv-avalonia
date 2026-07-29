using Avalonia.Platform;
using Material.Icons;

namespace Asv.Avalonia.Example;

public class MarkdownPageViewModel : ControlsGallerySubPage
{
    public const string PageId = "markdown-example";
    public const MaterialIconKind PageIcon = MaterialIconKind.CodeBraces;

    private const string DocumentationAsset =
        "avares://Asv.Avalonia.Example/Assets/Markdown/Documentation";

    public MarkdownPageViewModel()
        : this(
            NullTreeSubPageContext<ControlsGalleryPageViewModel>.Instance,
            DesignTime.LocalizationService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        SetParent(DesignTime.Shell);
    }

    public MarkdownPageViewModel(
        ITreeSubPageContext<IControlsGalleryPage> context,
        ILocalizationService localization
    )
        : base(PageId, context)
    {
        Documentation = LoadDocumentation(localization);
    }

    public string Documentation { get; }

    private static string LoadDocumentation(ILocalizationService localization)
    {
        var languageId = localization.CurrentLanguage.Value.Id;
        var uri = new Uri($"{DocumentationAsset}.{languageId}.md");
        if (!AssetLoader.Exists(uri))
        {
            uri = new Uri($"{DocumentationAsset}.md");
        }

        using var stream = AssetLoader.Open(uri);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
