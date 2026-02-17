using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class LogReaderMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseLogReaderService()
        {
            builder.Services.AddSingleton<ILogReaderService, LogReaderService>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeLogReaderService()
        {
            builder.Services.AddSingleton<ILogReaderService>(NullLogReaderService.Instance);
            return builder;
        }
    }
}
