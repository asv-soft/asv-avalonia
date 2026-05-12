using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public sealed class DialogItemHotKeyCaptureViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.hotkey.capture";

    public DialogItemHotKeyCaptureViewModel()
        : this(DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        HotKey.Value = global::Avalonia.Input.KeyGesture.Parse("Ctrl+C");
    }

    public DialogItemHotKeyCaptureViewModel(ILoggerFactory loggerFactory)
        : base(DialogId)
    {
        HotKey = new BindableReactiveProperty<global::Avalonia.Input.KeyGesture?>().DisposeItWith(
            Disposable
        );
    }

    public BindableReactiveProperty<global::Avalonia.Input.KeyGesture?> HotKey { get; }

    public override IEnumerable<IViewModel> GetChildren() => [];
}
