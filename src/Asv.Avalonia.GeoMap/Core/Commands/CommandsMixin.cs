namespace Asv.Avalonia.GeoMap;

public static class CommandsMixin
{
    public static GeoMapMixin.Builder AddCommands(this GeoMapMixin.Builder builder)
    {
        builder.Parent.Commands.Register<ChangeMapModeCommand>();
        builder.Parent.Commands.Register<ChangeTileProviderCommand>();

        return builder;
    }
}
