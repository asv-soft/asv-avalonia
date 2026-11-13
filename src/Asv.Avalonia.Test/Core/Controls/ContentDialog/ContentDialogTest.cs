using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class ContentDialogTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task Enter_WithDisabledDefaultButton_KeepsDialogOpen() =>
        Run(async () =>
        {
            // Arrange
            using var scene = new Scene(isPrimaryButtonEnabled: false);
            var showing = await scene.ShowAsync();
            var primaryClicks = 0;
            scene.Dialog.PrimaryButtonClick += (_, _) => primaryClicks++;

            // Act
            scene.PressEnter();

            // Assert
            Assert.Equal(0, primaryClicks);
            Assert.False(showing.IsCompleted);
            scene.Dialog.Hide();
            Assert.Equal(ContentDialogResult.None, await showing);
        });

    [Fact]
    public Task Enter_WithEnabledDefaultButton_ClosesWithPrimaryResult() =>
        Run(async () =>
        {
            // Arrange
            using var scene = new Scene(isPrimaryButtonEnabled: true);
            var showing = await scene.ShowAsync();

            // Act
            scene.PressEnter();

            // Assert
            Assert.Equal(ContentDialogResult.Primary, await showing);
        });

    private Task Run(Func<Task> action) =>
        fixture.Session.Dispatch(
            async () =>
            {
                await action();
                return true;
            },
            TestContext.Current.CancellationToken
        );

    private sealed class Scene : IDisposable
    {
        private readonly Application _application = Assert.IsAssignableFrom<Application>(
            Application.Current
        );
        private readonly StyleInclude _theme;
        private readonly FuncDataTemplate<TestDialogViewModel> _template;
        private readonly TestDialogViewModel _viewModel = new();
        private readonly Border _content = new() { Focusable = true };

        public Scene(bool isPrimaryButtonEnabled)
        {
            _theme = new StyleInclude(new Uri("avares://Asv.Avalonia/"))
            {
                Source = new Uri("avares://Asv.Avalonia/Theme.axaml"),
            };
            _application.Styles.Add(_theme);
            _template = new FuncDataTemplate<TestDialogViewModel>((_, _) => _content);
            _application.DataTemplates.Insert(0, _template);
            Window = new Window { Width = 800, Height = 600 };
            Window.Show();
            Dispatcher.UIThread.RunJobs();
            Dialog = new ContentDialog(_viewModel)
            {
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                IsPrimaryButtonEnabled = isPrimaryButtonEnabled,
            };
        }

        public Window Window { get; }
        public ContentDialog Dialog { get; }

        public async Task<Task<ContentDialogResult>> ShowAsync()
        {
            var showing = Dialog.ShowAsync(Window);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (!_content.IsLoaded)
            {
                await Task.Delay(10, timeout.Token);
            }

            Dispatcher.UIThread.RunJobs();
            _content.Focus();
            return showing;
        }

        public void PressEnter()
        {
            Window.KeyPress(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Window.KeyRelease(Key.Enter, RawInputModifiers.None, PhysicalKey.Enter, null);
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            Window.Close();
            _application.DataTemplates.Remove(_template);
            _application.Styles.Remove(_theme);
            _viewModel.Dispose();
        }
    }

    private sealed class TestDialogViewModel() : DialogViewModelBase("dialog-test");
}
