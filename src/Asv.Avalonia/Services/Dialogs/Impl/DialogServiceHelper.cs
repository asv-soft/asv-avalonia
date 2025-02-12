using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Asv.Avalonia
{
    public static class DialogServiceHelper
    {
        /// <summary>
        /// Represents the content of a dialog, including title, message, input field option, default button, and button texts.
        /// </summary>
        public class DialogContent //TODO: make as UserControl, implment ShowAsync() method and make properties as AvaloniaProperties
        {
            public TopLevel? Parent { get; set; }
            public string Title { get; set; }
            public string Message { get; set; } //TODO: make as Control or object (last not recomended) to provide different views as content of dialog
            public bool IsInputDialog { get; set; }
            public bool DefaultButton { get; set; }
            public string PositiveButtonText { get; set; }
            public string NegativeButtonText { get; set; }
            public bool? Result { get; set; }

            public DialogContent(
                string title,
                string message,
                bool isInputDialog,
                bool defaultButton,
                TopLevel? parent,
                string positiveButtonText,
                string negativeButtonText
            )
            {
                Title = title;
                Message = message;
                IsInputDialog = isInputDialog;
                DefaultButton = defaultButton;
                Parent = parent;
                Result = null;
                PositiveButtonText = positiveButtonText;
                NegativeButtonText = negativeButtonText;
            }
        }

        // A simple static field to store a single dialog content
        public static DialogContent CurrentDialogContent { get; private set; }

        /// <summary>
        /// Method to set the dialog content, which can later be accessed from other parts of the code.
        /// </summary>
        /// <param name="content">The dialog content to set.</param>
        public static void SetDialogContent(DialogContent content)
        {
            CurrentDialogContent = content;
        }

        /// <summary>
        /// Method to retrieve the current dialog content.
        /// </summary>
        /// <returns>The current dialog content.</returns>
        public static DialogContent GetDialogContent()
        {
            return CurrentDialogContent;
        }

        public static string GetDefaultButtonText()
        {
            if (CurrentDialogContent?.DefaultButton == true)
            {
                return CurrentDialogContent.PositiveButtonText;
            }
            else if (CurrentDialogContent?.DefaultButton == false)
            {
                return CurrentDialogContent.NegativeButtonText;
            }

            return string.Empty;
        }
    }
}
