using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.GeoMap;

public static class CommandsMixin
{
    public static GeoMapMixin.Builder AddCommands(this GeoMapMixin.Builder builder)
    {
        builder.Parent.Commands.Register<ChangeMapModeCommand>();

        return builder;
    }
}
