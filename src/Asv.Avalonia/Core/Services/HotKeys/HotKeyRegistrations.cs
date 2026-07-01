using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class HotKeyRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder HotKeys => builder.Core.Services.HotKeys;
    }

    extension(ServicesRegistrations.Builder builder)
    {
        public Builder HotKeys => new(builder);

        public ServicesRegistrations.Builder RegisterHotKeys(Action<Builder>? configure = null)
        {
            builder.AppBuilder.Services.AddSingleton<IHotKeyService, HotKeyService>();

            if (configure is null)
            {
                new Builder(builder).RegisterDefault();
            }
            else
            {
                configure(new Builder(builder));
            }

            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            return Register<CancelAction>()
                .Register<ClearAction>()
                .Register<ClosePageAction>()
                .Register<OpenFileAction>()
                .Register<OpenHomePageAction>()
                .Register<RedoAction>()
                .Register<RefreshAction>()
                .Register<SaveAction>()
                .Register<SaveAsAction>()
                .Register<SearchAction>()
                .Register<UndoAction>();
        }

        public Builder Register<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                THotKeyAction
        >()
            where THotKeyAction : class, IHotKeyAction
        {
            builder.AppBuilder.Services.AddSingleton<IHotKeyAction, THotKeyAction>();
            return this;
        }
    }
}
