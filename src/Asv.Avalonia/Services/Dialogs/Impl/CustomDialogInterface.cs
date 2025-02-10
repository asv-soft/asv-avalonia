using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Layout;

namespace Asv.Avalonia
{
    public static class CustomDialogInterface
    {
            public static async Task<object?> ShowCustomDialog(TopLevel? parent, string title, string message, bool isInputDialog = false, string defaultButton = "Close", params string[] buttonNames)
        {
            parent ??= Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
            if (parent == null)
            {
                return ShowConsoleDialog(title, message, isInputDialog, defaultButton, buttonNames);
            }

            var tcs = new TaskCompletionSource<object?>();

            var dialog = new Window()
            {
                Title = title,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            var stackPanel = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = message, Margin = new Thickness(10) },
                    isInputDialog ? new TextBox { Margin = new Thickness(5) } : new Control(),
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                },
            };

            ((StackPanel)stackPanel.Children[2]).Children.AddRange(CreateButtons(tcs, dialog, isInputDialog, defaultButton, buttonNames));
            dialog.Content = stackPanel;

            dialog.Closed += (sender, e) => tcs.TrySetResult(null);

            var parentWindow = parent as Window;
            if (parentWindow == null)
            {
                throw new InvalidOperationException("Parent is not a Window");
            }

            var parentWidth = parentWindow.Bounds.Width;
            var parentHeight = parentWindow.Bounds.Height;
            dialog.Width = parentWidth * 0.3;
            dialog.Height = parentHeight * 0.3;
            dialog.MaxWidth = parentWidth * 0.5;
            dialog.MaxHeight = parentHeight * 0.5;

            await dialog.ShowDialog<object?>(parentWindow);
            var result = await tcs.Task;
            dialog.Close();
            return result;
        }

        private static List<Control> CreateButtons(TaskCompletionSource<object?> tcs, Window dialog, bool isInputDialog, string defaultButton, params string[] buttonNames)
        {
            var buttons = new List<Control>();
            Button? defaultBtn = null;

            foreach (var name in buttonNames)
            {
                var button = new Button
                {
                    Content = name,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    IsDefault = name == defaultButton,
                };

                if (name == defaultButton)
                {
                    defaultBtn = button;
                }

                button.Click += (sender, e) =>
                {
                    var stackPanel = dialog.Content as StackPanel;
                    if (stackPanel != null)
                    {
                        tcs.TrySetResult(isInputDialog ? ((TextBox)stackPanel.Children[1]).Text : (object?)name);
                    }
                    else
                    {
                        tcs.TrySetResult(null);
                    }

                    dialog.Close();
                };

                button.GotFocus += (sender, e) => HighlightButton(button);
                button.LostFocus += (sender, e) => RemoveHighlightButton(button);

                buttons.Add(button);
            }

            var closeButton = new Button
            {
                Content = "Close",
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center,
                IsCancel = true,
            };

            closeButton.Click += (sender, e) =>
            {
                tcs.TrySetResult(null);
                dialog.Close();
            };

            closeButton.GotFocus += (sender, e) => HighlightButton(closeButton);
            closeButton.LostFocus += (sender, e) => RemoveHighlightButton(closeButton);

            buttons.Add(closeButton);
            
            for (int i = 0; i < buttons.Count; i++)
            {
                var nextIndex = (i + 1) % buttons.Count;
                var prevIndex = (i - 1 + buttons.Count) % buttons.Count;

                buttons[i].KeyDown += (sender, e) =>
                {
                    if (e.Key == Key.Right || e.Key == Key.Down)
                    {
                        buttons[nextIndex].Focus();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.Left || e.Key == Key.Up)
                    {
                        buttons[prevIndex].Focus();
                        e.Handled = true;
                    }
                };
            }
            
            defaultBtn?.Focus();
            if (defaultBtn != null)
            {
                HighlightButton(defaultBtn);
            }

            return buttons;
        }

        private static void HighlightButton(Button button)
        {
            button.Classes.Add("highlighted");
        }

        private static void RemoveHighlightButton(Button button)
        {
            button.Classes.Remove("highlighted");
        }

        private static object? ShowConsoleDialog(string title, string message, bool isInputDialog, string defaultButton, params string[] buttonNames)
        {
            Console.WriteLine(title);
            Console.WriteLine(message);

            if (isInputDialog)
            {
                Console.Write("Input: ");
                var input = Console.ReadLine();
                return input;
            }
            else
            {
                for (int i = 0; i < buttonNames.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {buttonNames[i]}");
                }

                Console.WriteLine($"{buttonNames.Length + 1}. Close");
                Console.Write("Choose an option: ");
                var choice = Console.ReadLine();
                if (int.TryParse(choice, out int index) && index > 0 && index <= buttonNames.Length)
                {
                    return buttonNames[index - 1];
                }

                return null;
            }
        }
    }
}
