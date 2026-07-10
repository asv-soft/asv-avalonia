using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ViewModelRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ViewModel => builder.Core.ViewModel;
    }

    extension(CoreRegistrations.Builder builder)
    {
        public Builder ViewModel => new(builder);
    }

    public class Builder(CoreRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterKeyed<
            TViewModelInterface,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModelImplementation
        >(string key)
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.AppBuilder.Services.AddKeyedTransient<
                TViewModelInterface,
                TViewModelImplementation
            >(key);
            return this;
        }

        public Builder Register<
            TViewModelInterface,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModelImplementation
        >()
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.AppBuilder.Services.AddTransient<
                TViewModelInterface,
                TViewModelImplementation
            >();
            return this;
        }

        public Builder RegisterWithArgs<
            TViewModelInterface,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModelImplementation,
            TArgs
        >()
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.AppBuilder.Services.AddTransient<
                ViewModelFactoryDelegate<TViewModelInterface, TArgs>
            >(services =>
                args =>
                {
                    Debug.Assert(args != null, nameof(args) + " != null");
                    return ActivatorUtilities.CreateInstance<TViewModelImplementation>(
                        services,
                        args
                    );
                }
            );
            return this;
        }

        public Builder RegisterKeyedWithArgs<
            TViewModelInterface,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TViewModelImplementation,
            TArgs
        >(string key)
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.AppBuilder.Services.AddKeyedTransient<
                ViewModelFactoryDelegate<TViewModelInterface, TArgs>
            >(
                key,
                (services, _) =>
                    args =>
                    {
                        Debug.Assert(args != null, nameof(args) + " != null");
                        return ActivatorUtilities.CreateInstance<TViewModelImplementation>(
                            services,
                            args
                        );
                    }
            );
            return this;
        }
    }
}
