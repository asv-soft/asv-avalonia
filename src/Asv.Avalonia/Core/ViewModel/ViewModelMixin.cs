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
    
    extension(IServiceProvider services)
    {
        public TViewModelInterface CreateViewModel<TViewModelInterface>(NavId id)
            where TViewModelInterface : IViewModel
        {
            return services
                .GetRequiredKeyedService<ViewModelFactory<TViewModelInterface>>(id.TypeId)
                .Invoke(id.Args);
        }
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder Register<TViewModelInterface, TViewModelImplementation>(string typeId)
            where TViewModelInterface : class, IViewModel
            where TViewModelImplementation : class, TViewModelInterface
        {
            builder.Services.AddKeyedTransient<ViewModelFactory<TViewModelInterface>>(
                typeId, 
                (services, key) => args =>
                {
                    Debug.Assert(key != null, nameof(key) + " != null");
                    var viewModelTypeId = (string)key;
                    var vm = ActivatorUtilities.CreateInstance<TViewModelImplementation>(services, args);
                    if (vm.Id.TypeId != viewModelTypeId)
                    {
                        throw new InvalidOperationException(
                            $"Registered implementation {typeof(TViewModelImplementation).Name} of {typeof(TViewModelInterface).Name} {nameof(IViewModel.Id)}.{nameof(IViewModel.Id.TypeId)} != {viewModelTypeId}"
                        );
                    }

                    return vm;
                });
            return this;
        }
        
    }
}
