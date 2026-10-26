using Asv.Avalonia.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class TrafficInspectorDockTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task MessagesRemainSelectableAfterDetachAndReattach() => fixture.Session.Dispatch(() =>
    {
        var theme = new StyleInclude(new Uri("avares://Asv.Avalonia/")) { Source = new Uri("avares://Asv.Avalonia/Theme.axaml") };
        Application.Current!.Styles.Add(theme);
        using var capture = new PacketCaptureService(NullDeviceManager.Instance, TimeProvider.System, [],
            Options.Create(new PacketInspectorOptions()), NullLogger<PacketCaptureService>.Instance);
        using var inspector = new PacketInspectorPageViewModel(NullPageContext.Instance, NullLoggerFactory.Instance,
            NullDialogService.Instance, NullExtensionService.Instance, capture, TimeProvider.System,
            NullDeviceManager.Instance, NullThemeService.Instance);
        inspector.PauseCommand.Execute(R3.Unit.Default);
        for (var i = 1; i <= 2; i++)
        {
            inspector.Messages.Add(new PacketMessageRow(new PacketSnapshot(i, DateTimeOffset.UtcNow, 0,
                new PacketGroupKey("test", "127.0.0.1:1234", PacketDirection.Rx, "test", i.ToString(), null),
                "UDP", "Test", $"Message {i}", [], null), 1, 1));
        }
        var template = new FuncDataTemplate<PacketInspectorPageViewModel>((_, _) => CreateGrid());
        Application.Current.DataTemplates.Add(template);
        var opened = new List<Window>();
        using var subscription = Window.WindowOpenedEvent.AddClassHandler<Window>((window, _) => opened.Add(window));
        var dock = new DockControl();
        var main = new Window { Width = 1400, Height = 800, Content = dock };
        try
        {
            main.Show();
            dock.Items.Add(inspector);
            dock.SelectedItem = inspector;
            main.UpdateLayout();
            Select(main, inspector.Messages[0]);
            for (var cycle = 0; cycle < 2; cycle++)
            {
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
                var header = dock.GetVisualDescendants().OfType<DockTabStripItem>().Single();
                main.MouseDown(header.TranslatePoint(new Point(20, 12), main)!.Value, MouseButton.Left);
                main.MouseMove(new Point(1480, 80), RawInputModifiers.LeftMouseButton);
                main.MouseUp(new Point(1480, 80), MouseButton.Left);
                var detached = opened.OfType<DockWindow>().Single(window => window.IsVisible);
                detached.UpdateLayout();
                Select(detached, inspector.Messages[1]);
                detached.Close();
                Assert.Null(detached.Page);
                Assert.Null(detached.Content);
                Dispatcher.UIThread.RunJobs();
                main.UpdateLayout();
                Select(main, inspector.Messages[0]);
                Select(main, inspector.Messages[1]);
            }
        }
        finally
        {
            main.Close();
            foreach (var window in opened.Where(window => window.IsVisible).ToArray()) window.Close();
            Application.Current.DataTemplates.Remove(template);
            Application.Current.Styles.Remove(theme);
        }

        static DataGrid CreateGrid()
        {
            var grid = new DataGrid { AutoGenerateColumns = false, IsReadOnly = true };
            // Reproduce the inspector selection binding without unrelated workspace widgets.
            grid.Bind(DataGrid.ItemsSourceProperty, new Binding(nameof(PacketInspectorPageViewModel.Messages)));
            grid.Columns.Add(new DataGridTextColumn { Header = "Message", Binding = new Binding(nameof(PacketMessageRow.Name)) });
            grid.Bind(DataGrid.SelectedItemProperty, new Binding(nameof(PacketInspectorPageViewModel.SelectedMessage)) { Mode = BindingMode.TwoWay });
            return grid;
        }

        void Select(Window window, PacketMessageRow message)
        {
            var grid = window.GetVisualDescendants().OfType<DataGrid>().Single(grid => ReferenceEquals(grid.ItemsSource, inspector.Messages));
            grid.ScrollIntoView(message, grid.Columns[0]);
            window.UpdateLayout();
            var row = grid.GetVisualDescendants().OfType<DataGridRow>().Single(row => ReferenceEquals(row.DataContext, message));
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            var cell = row.GetVisualDescendants().OfType<DataGridCell>().First();
            var point = cell.TranslatePoint(new Point(cell.Bounds.Width / 2, cell.Bounds.Height / 2), window)!.Value;
            window.MouseDown(point, MouseButton.Left);
            window.MouseUp(point, MouseButton.Left);
            Assert.Same(message, grid.SelectedItem);
            Assert.Same(message, inspector.SelectedMessage);
            Assert.Same(message.Packet, inspector.DetailsPacket);
        }
    }, TestContext.Current.CancellationToken);
}
