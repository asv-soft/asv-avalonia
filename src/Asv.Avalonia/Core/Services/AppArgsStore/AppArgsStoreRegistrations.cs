using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class AppArgsStoreRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterAppArgsStore()
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeAppArgsStore();
            }

            builder.AppBuilder.Services.AddSingleton<IAppArgsStore, AppArgsStore>();
            return builder;
        }

        private ServicesRegistrations.Builder RegisterDesignTimeAppArgsStore()
        {
            builder.AppBuilder.Services.AddSingleton(NullAppArgsStore.Instance);
            return builder;
        }
    }
}
