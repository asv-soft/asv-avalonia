using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public class DialogItemTextViewModel(ILoggerFactory loggerFactory)
    : DialogViewModelBase(DialogId, loggerFactory)
{
    public const string DialogId = $"{BaseId}.item.text";

    public DialogItemTextViewModel()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        if (Design.IsDesignMode)
        {
            Message = "Example";
        }
    }

    public required string Message { get; set; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
