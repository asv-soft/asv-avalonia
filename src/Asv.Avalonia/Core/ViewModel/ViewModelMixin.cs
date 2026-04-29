using System.Diagnostics;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ViewModelMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ViewModel => new(builder);
    }
    
    public delegate TViewModelInterface FactoryDelegate<out TViewModelInterface, in TArgs>(TArgs args)
        where TViewModelInterface : IViewModel;
    
    extension(IServiceProvider services)
    {
        public TViewModelInterface CreateViewModel<TViewModelInterface, TArgs>(TArgs args)
            where TViewModelInterface : IViewModel
        {
            return 
                services
                .GetRequiredService<FactoryDelegate<TViewModelInterface, TArgs>>()
                .Invoke(args);
        }
        
        public TViewModelInterface CreateViewModel<TViewModelInterface, TArgs>(string key, TArgs args)
            where TViewModelInterface : IViewModel
        {
            return 
                services
                    .GetRequiredKeyedService<FactoryDelegate<TViewModelInterface, TArgs>>(key)
                    .Invoke(args);
        }
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder RegisterKeyed<TViewModelInterface, TViewModelImplementation>(string key)
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.Services.AddKeyedTransient<TViewModelInterface, TViewModelImplementation>(key);
            return this;
        }
        public Builder Register<TViewModelInterface, TViewModelImplementation>()
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.Services.AddTransient<TViewModelInterface, TViewModelImplementation>();
            return this;
        }

        public Builder RegisterWithArgs<TViewModelInterface, TViewModelImplementation, TArgs>()
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.Services.AddTransient<FactoryDelegate<TViewModelInterface, TArgs>>(
                services => 
                    args =>
                    {
                        Debug.Assert(args != null, nameof(args) + " != null");
                        return ActivatorUtilities.CreateInstance<TViewModelImplementation>(services, args);
                    });
            return this;
        }
        public Builder RegisterKeyedWithArgs<TViewModelInterface, TViewModelImplementation, TArgs>(string key)
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.Services.AddKeyedTransient<FactoryDelegate<TViewModelInterface, TArgs>>(
                key,
                (services, _) => 
                    args =>
                    {
                        Debug.Assert(args != null, nameof(args) + " != null");
                        return ActivatorUtilities.CreateInstance<TViewModelImplementation>(services, args);
                    });
            return this;
        }
        
    }
}
