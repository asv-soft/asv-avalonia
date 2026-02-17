using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class FileAssociationMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseFileAssociation()
        {
            builder.Services.AddSingleton<IFileAssociationService, FileAssociationService>();
            builder.Services.AddHostedService<ForwardArgsToSelectedControlTask>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeFileAssociation()
        {
            builder.Services.AddSingleton(NullFileAssociationService.Instance);
            return builder;
        }
    }
}
