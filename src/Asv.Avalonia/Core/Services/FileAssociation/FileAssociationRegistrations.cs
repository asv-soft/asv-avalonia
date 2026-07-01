using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class FileAssociationRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder FileAssociation => builder.Core.Services.FileAssociation;
    }

    extension(ServicesRegistrations.Builder builder)
    {
        public Builder FileAssociation => new(builder);

        public ServicesRegistrations.Builder RegisterFileAssociation()
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeFileAssociation();
            }

            builder.AppBuilder.Services.AddSingleton<
                IFileAssociationService,
                FileAssociationService
            >();
            builder.AppBuilder.Services.AddHostedService<ForwardArgsToSelectedControlHandler>();
            return builder;
        }

        private ServicesRegistrations.Builder RegisterDesignTimeFileAssociation()
        {
            builder.AppBuilder.Services.AddSingleton(NullFileAssociationService.Instance);
            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder Register<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TFileHandler
        >()
            where TFileHandler : class, IFileHandler
        {
            builder.AppBuilder.Services.AddSingleton<IFileHandler, TFileHandler>();
            return this;
        }
    }
}
