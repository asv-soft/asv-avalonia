using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class StackPanelWidgetViewModel(NavigationId id, ILoggerFactory loggerFactory)
    : PanelWidgetViewModel(id, loggerFactory)
{
    public StackPanelWidgetViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        ItemsSource.Add(new UnitPropertyViewModel());
        ItemsSource.Add(new UnitPropertyViewModel());
        ItemsSource.Add(new UnitPropertyViewModel());
    }
}
