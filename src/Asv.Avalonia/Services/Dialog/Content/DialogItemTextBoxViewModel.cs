using Asv.Common;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia;

public class DialogItemTextBoxViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}.item.textbox";

    public DialogItemTextBoxViewModel()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        if (Design.IsDesignMode)
        {
            Message = "Example";
        }
    }

    public DialogItemTextBoxViewModel(ILoggerFactory loggerFactory)
        : base(DialogId, loggerFactory)
    {
        Input = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string?> Input { get; }
    public string? Message { get; set; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
