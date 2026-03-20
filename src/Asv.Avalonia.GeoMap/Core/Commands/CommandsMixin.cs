namespace Asv.Avalonia.GeoMap;

public static class CommandsMixin
{
    public static GeoMapMixin.Builder AddCommands(this GeoMapMixin.Builder builder)
    {
        builder.Parent.Commands.Register<ChangeMapModeCommand>();
        builder.Parent.Commands.Register<ChangeTileProviderCommand>();
        builder.Parent.Commands.Register<ChangeMinZoomCommand>();
        builder.Parent.Commands.Register<ChangeMaxZoomCommand>();

        return builder;
    }
}
