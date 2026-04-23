using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class EditApiKeyDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.edit-api-key";

    public EditApiKeyDialogViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        ApiKey.Value = "test-api-key-12345";
    }

    public EditApiKeyDialogViewModel(ILoggerFactory loggerFactory, string? currentKey = null)
        : base(DialogId, loggerFactory)
    {
        ApiKey = new BindableReactiveProperty<string>(currentKey ?? string.Empty).DisposeItWith(
            Disposable
        );
    }

    public BindableReactiveProperty<string> ApiKey { get; }

    public override IEnumerable<IViewModel> GetChildren() => [];
}
