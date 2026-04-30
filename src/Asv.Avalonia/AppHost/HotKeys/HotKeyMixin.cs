using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class HotKeyMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseHotKeys(Action<Builder>? configure = null)
        {
            builder.Services.AddSingleton<IHotKeyService, HotKeyService>();
            if (configure == null)
            {
                new Builder(builder).RegisterDefault();
            }
            else
            {
                configure(new Builder(builder));
            }

            return builder;
        }
        public Builder HotKeys => new(builder);
    }
    
    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder RegisterDefault()
        {
            return Register<UndoAction>()
                .Register<OpenHomePageAction>();
        }

        public Builder Register<THotKeyAction>() 
            where THotKeyAction : class, IHotKeyAction
        {
            builder.Services.AddSingleton<IHotKeyAction, THotKeyAction>();
            return this;
        }
    }
}
