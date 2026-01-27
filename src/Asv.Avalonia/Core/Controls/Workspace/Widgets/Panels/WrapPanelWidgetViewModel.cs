using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public class WrapPanelWidgetViewModel(NavigationId id, ILoggerFactory loggerFactory)
    : PanelWidgetViewModel(id, loggerFactory)
{
    public WrapPanelWidgetViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        ItemsSource.Add(new SingleRttBoxViewModel());
        ItemsSource.Add(new SingleRttBoxViewModel());
        ItemsSource.Add(new SingleRttBoxViewModel());
        ItemsSource.Add(new KeyValueRttBoxViewModel());
        ItemsSource.Add(new KeyValueRttBoxViewModel());
        ItemsSource.Add(new SplitDigitRttBoxViewModel());
        ItemsSource.Add(new SplitDigitRttBoxViewModel());
        ItemsSource.Add(new TwoColumnRttBoxViewModel());
        ItemsSource.Add(new TwoColumnRttBoxViewModel());
        ItemsSource.Add(new GeoPointRttBoxViewModel());
        ItemsSource.Add(new GeoPointRttBoxViewModel());
    }
}
