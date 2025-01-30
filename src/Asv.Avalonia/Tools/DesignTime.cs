using Avalonia.Controls;

namespace Asv.Avalonia;

public static class DesignTime
{
    public static void ThrowIfNotDesignMode()
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This method is for design mode only");
        }
    }

    public static ILogService Log => NullLogService.Instance;
}
