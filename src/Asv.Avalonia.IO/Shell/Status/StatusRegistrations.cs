namespace Asv.Avalonia.IO;

public static class StatusRegistrations
{
    extension(ShellRegistrations.Builder builder)
    {
        public ShellRegistrations.Builder RegisterConnectionStatus()
        {
            builder.AppBuilder.Status.Register<
                ConnectionRateStatusViewModel,
                ConnectionRateStatusView
            >();

            return builder;
        }
    }
}
