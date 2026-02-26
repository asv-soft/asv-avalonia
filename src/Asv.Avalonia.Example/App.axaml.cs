using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

public class App : AsvApplication
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
