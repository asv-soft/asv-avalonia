using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Threading;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class PropertyUnitViewTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task Focus_OnAnotherControl_ShowsFormattedDisplay() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();

            // Act
            scene.After.Focus();

            // Assert
            Assert.False(scene.Property.IsInEditMode);
            Assert.Equal("1234.57", scene.Display.Text);
            Assert.True(scene.Display.IsVisible);
            Assert.Equal(0, scene.Editor.Opacity);
        });

    [Fact]
    public Task GotFocus_ViaTab_ShowsUnroundedEditableText() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();

            // Act
            scene.Press(Key.Tab, PhysicalKey.Tab);

            // Assert
            Assert.True(scene.Editor.IsFocused);
            Assert.True(scene.Property.IsInEditMode);
            Assert.Equal("1234.567", scene.Editor.Text);
            Assert.False(scene.Display.IsVisible);
            Assert.Equal(1, scene.Editor.Opacity);
            Assert.True(scene.Property.IsSync);
        });

    [Fact]
    public Task GotFocus_ViaPointer_ShowsUnroundedEditableText() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();
            var point = Assert.IsType<Point>(
                scene.Editor.TranslatePoint(new Point(20, 10), scene.Window)
            );

            // Act
            scene.Window.MouseDown(point, MouseButton.Left);
            scene.Window.MouseUp(point, MouseButton.Left);

            // Assert
            Assert.True(scene.Property.IsInEditMode);
            Assert.Equal("1234.567", scene.Editor.Text);
            Assert.False(scene.Display.IsVisible);
        });

    [Fact]
    public Task Enter_WithValidDraft_AppliesFullPrecisionAndFormatsDisplay() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();
            scene.Editor.Focus();
            scene.Editor.Text = "9876.54321";

            // Act
            scene.Press(Key.Enter, PhysicalKey.Enter);

            // Assert
            Assert.Equal(9876.54321, scene.Model.Value);
            Assert.False(scene.Property.IsInEditMode);
            Assert.Equal("9876.54", scene.Display.Text);
            Assert.True(scene.Display.IsVisible);
        });

    [Fact]
    public Task LostFocus_WithValidDraft_AppliesFullPrecisionAndFormatsDisplay() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();
            scene.Editor.Focus();
            scene.Editor.Text = "9876.54321";

            // Act
            scene.After.Focus();

            // Assert
            Assert.Equal(9876.54321, scene.Model.Value);
            Assert.False(scene.Property.IsInEditMode);
            Assert.Equal("9876.54", scene.Display.Text);
            Assert.True(scene.Display.IsVisible);
        });

    [Fact]
    public Task Escape_WithPendingChanges_RestoresOriginalValueAndDisplay() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();
            scene.Editor.Focus();
            scene.Editor.Text = "9876.54321";

            // Act
            scene.Press(Key.Escape, PhysicalKey.Escape);

            // Assert
            Assert.Equal(1234.567, scene.Model.Value);
            Assert.Equal("1234.567", scene.Editor.Text);
            Assert.Equal("1234.57", scene.Display.Text);
            Assert.True(scene.Display.IsVisible);
        });

    [Fact]
    public Task LostFocus_WithInvalidDraft_KeepsDraftVisibleForCorrection() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();
            scene.Editor.Focus();
            scene.Editor.Text = "invalid";

            // Act
            scene.After.Focus();

            // Assert
            Assert.Equal(1234.567, scene.Model.Value);
            Assert.True(scene.Property.HasValidationErrors);
            Assert.True(scene.Property.IsInEditMode);
            Assert.Equal("invalid", scene.Editor.Text);
            Assert.Equal(1, scene.Editor.Opacity);
            Assert.False(scene.Display.IsVisible);
        });

    [Fact]
    public Task TextInput_AfterEnter_ReentersEditingWithoutLosingFocus() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();
            scene.Editor.Focus();
            scene.Press(Key.Enter, PhysicalKey.Enter);
            scene.Editor.SelectAll();

            // Act
            scene.Window.KeyTextInput("2.345678");

            // Assert
            Assert.True(scene.Editor.IsFocused);
            Assert.True(scene.Property.IsInEditMode);
            Assert.Equal("2.345678", scene.Editor.Text);
            Assert.False(scene.Display.IsVisible);
        });

    [Fact]
    public Task Enter_AfterResumingEditing_AppliesNewValueAndFormatsDisplay() =>
        Run(() =>
        {
            // Arrange
            using var scene = new Scene();
            scene.Editor.Focus();
            scene.Press(Key.Enter, PhysicalKey.Enter);
            scene.Editor.SelectAll();
            scene.Window.KeyTextInput("2.345678");

            // Act
            scene.Press(Key.Enter, PhysicalKey.Enter);

            // Assert
            Assert.Equal(2.345678, scene.Model.Value);
            Assert.Equal("2.35", scene.Display.Text);
        });

    private Task Run(Action action) =>
        fixture.Session.Dispatch(action, TestContext.Current.CancellationToken);

    private sealed class Scene : IDisposable
    {
        private readonly Application _application = Assert.IsAssignableFrom<Application>(
            Application.Current
        );
        private readonly DistanceUnit _unit = DistanceUnitTestSetup.CreateUnit();
        private readonly StyleInclude _theme;
        public ReactiveProperty<double> Model { get; } = new(1234.567);
        public PropertyUnitReactive Property { get; }
        public Window Window { get; }
        public TextBox Editor { get; }
        public TextBlock Display =>
            Assert.IsType<TextBlock>(View.FindControl<TextBlock>("PART_DisplayText"));
        public Button After { get; } = new() { Content = "After" };
        private PropertyUnitView View { get; }

        public Scene()
        {
            _theme = new StyleInclude(new Uri("avares://Asv.Avalonia/"))
            {
                Source = new Uri("avares://Asv.Avalonia/Theme.axaml"),
            };
            _application.Styles.Add(_theme);
            Property = new PropertyUnitReactive("distance", _unit, Model, format: "F2");
            View = new PropertyUnitView { DataContext = Property };
            var before = new Button { Content = "Before" };
            Window = new Window
            {
                Width = 500,
                Height = 200,
                Content = new StackPanel { Children = { before, View, After } },
            };
            Window.Show();
            before.Focus();
            Dispatcher.UIThread.RunJobs();
            Editor = Assert.IsType<TextBox>(View.FindControl<TextBox>("PART_TextBox"));
        }

        public void Press(Key key, PhysicalKey physicalKey)
        {
            Window.KeyPress(key, RawInputModifiers.None, physicalKey, null);
            Window.KeyRelease(key, RawInputModifiers.None, physicalKey, null);
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose()
        {
            Window.Close();
            Property.Dispose();
            Model.Dispose();
            _unit.Dispose();
            _application.Styles.Remove(_theme);
        }
    }
}
