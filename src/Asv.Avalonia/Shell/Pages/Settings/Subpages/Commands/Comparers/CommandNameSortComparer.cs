using System.Collections;
using System.Globalization;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

public sealed class CommandNameSortComparer : MarkupExtension, IComparer
{
    public static CommandNameSortComparer Instance { get; } = new();

    private CommandNameSortComparer() { }

    public override object ProvideValue(IServiceProvider provider) => Instance;

    public int Compare(object? x, object? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null)
        {
            return -1;
        }

        if (y is null)
        {
            return 1;
        }

        var left = GetName(x);
        var right = GetName(y);

        if (left is null && right is null)
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        return CultureInfo.CurrentCulture.CompareInfo.Compare(
            left,
            right,
            CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType
        );
    }

    private static string? GetName(object row) =>
        row switch
        {
            string s => s,
            ICommandInfo cmd => cmd.Name,
            CommandViewModel vm => vm.Name,
            _ => null,
        };
}
