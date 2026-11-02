using Asv.Avalonia.Example;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Avalonia.Controls;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class MapControlsPageLifecycleTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task RulerChangesAfterDetachAndReattachDoNotUseDisposedSubject() => fixture.Session.Dispatch(() =>
    {
        var view = new MapControlsPageView();
        var ruler = view.FindControl<MapRuler>("PART_Ruler")!;
        var window = new Window { Width = 800, Height = 450 };
        try
        {
            window.Show();
            for (var cycle = 0; cycle < 2; cycle++)
            {
                window.Content = view;
                ruler.StartPoint = new GeoPoint(55, 60 + cycle, 0);
                ruler.StopPoint = new GeoPoint(56, 61 + cycle, 0);
                window.Content = null;
                // Binding teardown can raise changes after visual detachment.
                ruler.StartPoint = null;
                ruler.StopPoint = null;
            }
            window.Content = view;
            ruler.StartPoint = new GeoPoint(55, 60, 0);
            ruler.StopPoint = new GeoPoint(56, 61, 0);
            window.Close();
            ruler.StartPoint = null;
            ruler.StopPoint = null;
        }
        finally
        {
            window.Close();
        }
    }, TestContext.Current.CancellationToken);
}
