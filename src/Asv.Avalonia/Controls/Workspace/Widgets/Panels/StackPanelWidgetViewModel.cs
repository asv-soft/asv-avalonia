using Asv.Modeling;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class StackPanelWidgetViewModel(string id, ILoggerFactory loggerFactory)
    : PanelWidgetViewModel(id, loggerFactory)
{
    public StackPanelWidgetViewModel()
        : this(DesignTime.Id.TypeId, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        ItemsSource.Add(new UnitPropertyViewModel());
        ItemsSource.Add(new UnitPropertyViewModel());
        ItemsSource.Add(new UnitPropertyViewModel());
    }
}
