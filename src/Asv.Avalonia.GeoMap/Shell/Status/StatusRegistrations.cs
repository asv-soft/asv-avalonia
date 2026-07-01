namespace Asv.Avalonia.GeoMap;

public static class StatusRegistrations
{
    extension(ShellRegistrations.Builder builder)
    {
        public ShellRegistrations.Builder RegisterMapStatus()
        {
            builder.AppBuilder.Status.Register<MapStatusViewModel, MapStatusView>();
            return builder;
        }
    }
}
