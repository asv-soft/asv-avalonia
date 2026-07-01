using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.GeoMap;

public static class ViewModelRegistrations
{
    extension(Asv.Avalonia.ViewModelRegistrations.Builder builder)
    {
        public Asv.Avalonia.ViewModelRegistrations.Builder RegisterTileProvider<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TTileProvider
        >()
            where TTileProvider : class, ITileProvider
        {
            builder.AppBuilder.Services.AddSingleton<ITileProvider, TTileProvider>();
            return builder;
        }
    }
}
