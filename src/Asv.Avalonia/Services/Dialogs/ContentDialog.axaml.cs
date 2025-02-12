using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using R3;

namespace Asv.Avalonia
{
    public partial class ContentDialog : UserControl
    {
        private ContentControl _host;

        private TaskCompletionSource<DialogResult> _tcs;
        private DialogResult _result;

        public ContentDialog(TopLevel parentWindow)
        {
            InitializeComponent();

            PrimaryButtonCommand = new ReactiveCommand(
                (_, __) =>
                {
                    _result = DialogResult.True;
                    CloseDialog(parentWindow);
                    return ValueTask.CompletedTask;
                }
            );
            SecondaryButtonCommand = new ReactiveCommand(
                (_, __) =>
                {
                    _result = DialogResult.False;
                    CloseDialog(parentWindow);
                    return ValueTask.CompletedTask;
                }
            );
            CloseCommand = new ReactiveCommand(
                (_, __) =>
                {
                    _result = DialogResult.Null;
                    CloseDialog(parentWindow);
                    return ValueTask.CompletedTask;
                }
            );
        }

        public static StyledProperty<Control> DialogContentProperty = AvaloniaProperty.Register<
            ContentDialog,
            Control
        >(nameof(DialogContent));
        public static StyledProperty<string> TitleProperty = AvaloniaProperty.Register<
            ContentDialog,
            string
        >(nameof(Title));
        public static StyledProperty<string> MessageProperty = AvaloniaProperty.Register<
            ContentDialog,
            string
        >(nameof(Message));
        public static StyledProperty<bool> IsInputDialogProperty = AvaloniaProperty.Register<
            ContentDialog,
            bool
        >(nameof(IsInputDialog));
        public static StyledProperty<bool> DefaultButtonProperty = AvaloniaProperty.Register<
            ContentDialog,
            bool
        >(nameof(DefaultButton));
        public static StyledProperty<string> PrimaryButtonTextProperty = AvaloniaProperty.Register<
            ContentDialog,
            string
        >(nameof(PrimaryButtonText));
        public static StyledProperty<string> SecondaryButtonTextProperty =
            AvaloniaProperty.Register<ContentDialog, string>(nameof(SecondaryButtonText));
        public static StyledProperty<ReactiveCommand> PrimaryButtonCommandProperty =
            AvaloniaProperty.Register<ContentDialog, ReactiveCommand>(nameof(PrimaryButtonCommand));
        public static StyledProperty<ReactiveCommand> SecondaryButtonCommandProperty =
            AvaloniaProperty.Register<ContentDialog, ReactiveCommand>(
                nameof(SecondaryButtonCommand)
            );
        public static readonly StyledProperty<ReactiveCommand> CloseCommandProperty =
            AvaloniaProperty.Register<ContentDialog, ReactiveCommand>(nameof(CloseCommand));

        public Control DialogContent
        {
            get => GetValue(DialogContentProperty);
            set => SetValue(DialogContentProperty, value);
        }

        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public bool IsInputDialog
        {
            get => GetValue(IsInputDialogProperty);
            set => SetValue(IsInputDialogProperty, value);
        }

        public bool DefaultButton
        {
            get => GetValue(DefaultButtonProperty);
            set => SetValue(DefaultButtonProperty, value);
        }

        public string PrimaryButtonText
        {
            get => GetValue(PrimaryButtonTextProperty);
            set => SetValue(PrimaryButtonTextProperty, value);
        }

        public string SecondaryButtonText
        {
            get => GetValue(SecondaryButtonTextProperty);
            set => SetValue(SecondaryButtonTextProperty, value);
        }

        public ReactiveCommand PrimaryButtonCommand
        {
            get => GetValue(PrimaryButtonCommandProperty);
            set => SetValue(PrimaryButtonCommandProperty, value);
        }

        public ReactiveCommand SecondaryButtonCommand
        {
            get => GetValue(SecondaryButtonCommandProperty);
            set => SetValue(SecondaryButtonCommandProperty, value);
        }

        public ReactiveCommand CloseCommand
        {
            get => GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        public ContentDialog WithTitle(string title)
        {
            Title = title;
            return this;
        }

        public ContentDialog WithMessage(string message)
        {
            Message = message;
            return this;
        }

        public ContentDialog WithIsInputDialog(bool isInputDialog)
        {
            IsInputDialog = isInputDialog;
            return this;
        }

        public ContentDialog WithPositiveButtonText(string positiveButtonText)
        {
            PrimaryButtonText = positiveButtonText;
            return this;
        }

        public ContentDialog WithNegativeButtonText(string negativeButtonText)
        {
            SecondaryButtonText = negativeButtonText;
            return this;
        }

        public ContentDialog WithDefaultButton(bool defaultButton)
        {
            DefaultButton = defaultButton;
            return this;
        }

        public async Task<DialogResult> ShowAsync(TopLevel parentWindow)
        {
            return await ShowDialog(parentWindow);
        }

        public async Task<DialogResult> ShowDialog(TopLevel parentWindow)
        {
            _tcs = new TaskCompletionSource<DialogResult>();

            var ol = OverlayLayer.GetOverlayLayer(parentWindow);

            if (ol == null)
            {
                throw new InvalidOperationException("Error: OverlayLayer is null");
            }

            _host = new ContentControl { Content = this };

            ol.Children.Add(_host);
            IsVisible = true;
            DataContext = this;

            IsVisible = true;
            PseudoClasses.Set(":hidden", false);
            PseudoClasses.Set(":open", true);

            var screen = parentWindow.Screens?.ScreenFromVisual(parentWindow);
            if (screen != null)
            {
                var screenBounds = screen.Bounds;
                var dialogWidth = double.IsNaN(Width) ? 300 : Width;
                var dialogHeight = double.IsNaN(Height) ? 200 : Height;
                var dialogBounds = new Rect(
                    (screenBounds.Width / 2.0) - (dialogWidth / 2),
                    (screenBounds.Height / 2.0) - (dialogHeight / 2),
                    dialogWidth,
                    dialogHeight
                );
                Margin = new Thickness(dialogBounds.X, dialogBounds.Y, 0, 0);
            }

            MinWidth = 300;
            MaxWidth = double.PositiveInfinity;
            MinHeight = 200;
            MaxHeight = double.PositiveInfinity;

            return await _tcs.Task;
        }

        private void CloseDialog(TopLevel parentWindow)
        {
            var overlayLayer = OverlayLayer.GetOverlayLayer(parentWindow);
            if (overlayLayer == null)
            {
                throw new InvalidOperationException("Error: OverlayLayer is null");
            }

            if (overlayLayer.Children.Contains(_host))
            {
                overlayLayer.Children.Remove(_host);
            }
            else
            {
                throw new InvalidOperationException("Error: host is not a child of overlayLayer");
            }

            IsVisible = false;
            PseudoClasses.Set(":hidden", true);
            PseudoClasses.Set(":open", false);

            _tcs.TrySetResult(_result);
        }

        public enum DialogResult
        {
            Null,
            True,
            False,
        }
    }
}
