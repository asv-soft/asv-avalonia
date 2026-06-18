using Asv.Modeling;

namespace Asv.Avalonia;

public class DashboardWidget : WorkspaceWidget
{
    public DashboardWidget()
     : base(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
        Dashboard = new DashboardViewModel();
    }
    public DashboardWidget(string typeId)
     : base(typeId)
    {
        Dashboard = new DashboardViewModel("dashboard");
        Dashboard.SetParent(this);
    }

    public IDashboard Dashboard { get; }
}