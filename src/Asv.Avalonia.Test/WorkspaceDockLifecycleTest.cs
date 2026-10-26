using Asv.Avalonia.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class WorkspaceDockLifecycleTest(WorkspaceUiTestFixture fixture)
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public Task SwitchingTrafficInspectorToHome_PreservesWorkspace(bool leftOpen, bool rightOpen) => Run(() =>
    {
        var theme = new StyleInclude(new Uri("avares://Asv.Avalonia/"))
        {
            Source = new Uri("avares://Asv.Avalonia/Theme.axaml"),
        };
        Application.Current!.Styles.Add(theme);
        using var capture = new PacketCaptureService(
            NullDeviceManager.Instance,
            TimeProvider.System,
            [],
            Options.Create(new PacketInspectorOptions()),
            NullLogger<PacketCaptureService>.Instance
        );
        using var inspector = new PacketInspectorPageViewModel(
            NullPageContext.Instance,
            NullLoggerFactory.Instance,
            NullDialogService.Instance,
            NullExtensionService.Instance,
            capture,
            TimeProvider.System,
            NullDeviceManager.Instance,
            NullThemeService.Instance
        );
        using var home = new HomePageViewModel(
            NullPageContext.Instance,
            NullAppInfo.Instance,
            NullLoggerFactory.Instance,
            NullDialogService.Instance,
            NullExtensionService.Instance
        );
        var inspectorView = new PacketInspectorPageView { DataContext = inspector };
        var homeView = new HomePageView { DataContext = home };
        var dock = new DockControl { ItemsSource = new IPage[] { inspector, home } };
        dock.DataTemplates.Add(new FuncDataTemplate<PacketInspectorPageViewModel>((_, _) => inspectorView));
        dock.DataTemplates.Add(new FuncDataTemplate<HomePageViewModel>((_, _) => homeView));
        var window = new Window { Width = 1400, Height = 800, Content = dock };
        try
        {
            window.Show();
            dock.SelectedItem = inspector;
            window.UpdateLayout();
            var panel = Assert.IsType<WorkspacePanel>(inspectorView.ItemsPanelRoot);
            var count = panel.Children.Count;
            Assert.True(count > 0);
            Assert.Contains(inspectorView, dock.GetVisualDescendants());
            inspectorView.SetCurrentValue(Workspace.IsLeftPaneOpenProperty, leftOpen);
            inspectorView.SetCurrentValue(Workspace.IsRightPaneOpenProperty, rightOpen);
            window.UpdateLayout();

            dock.SelectedItem = home;
            window.UpdateLayout();
            Assert.Contains(homeView, dock.GetVisualDescendants());
            Assert.DoesNotContain(inspectorView, dock.GetVisualDescendants());

            dock.SelectedItem = inspector;
            window.UpdateLayout();
            Assert.Contains(inspectorView, dock.GetVisualDescendants());
            Assert.Same(panel, inspectorView.ItemsPanelRoot);
            Assert.Equal(count, panel.Children.Count);
            Assert.True(panel.HasLeftPaneContent);
            Assert.True(panel.HasRightPaneContent);
            Assert.Equal(leftOpen, inspectorView.IsLeftPaneOpen);
            Assert.Equal(rightOpen, inspectorView.IsRightPaneOpen);
            Assert.Equal(leftOpen, panel.IsLeftPaneOpen);
            Assert.Equal(rightOpen, panel.IsRightPaneOpen);
        }
        finally
        {
            try
            {
                window.Close();
            }
            finally
            {
                Application.Current.Styles.Remove(theme);
            }
        }
    });

    private Task Run(Action action) =>
        fixture.Session.Dispatch(action, TestContext.Current.CancellationToken);
}
