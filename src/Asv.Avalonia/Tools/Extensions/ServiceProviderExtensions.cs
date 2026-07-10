using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class ServiceProviderExtensions
{
    extension(IServiceProvider services)
    {
        public TViewModelInterface CreateViewModel<TViewModelInterface, TArgs>(TArgs args)
            where TViewModelInterface : IViewModel
        {
            return services
                .GetRequiredService<ViewModelFactoryDelegate<TViewModelInterface, TArgs>>()
                .Invoke(args);
        }

        public TViewModelInterface? TryCreateViewModel<TViewModelInterface, TArgs>(TArgs args)
            where TViewModelInterface : class, IViewModel
        {
            return services
                .GetService<ViewModelFactoryDelegate<TViewModelInterface, TArgs>>()
                ?.Invoke(args);
        }

        public TViewModelInterface CreateViewModel<TViewModelInterface, TArgs>(
            string key,
            TArgs args
        )
            where TViewModelInterface : IViewModel
        {
            return services
                .GetRequiredKeyedService<ViewModelFactoryDelegate<TViewModelInterface, TArgs>>(key)
                .Invoke(args);
        }

        public TViewModelInterface? TryCreateViewModel<TViewModelInterface, TArgs>(
            string key,
            TArgs args
        )
            where TViewModelInterface : class, IViewModel
        {
            return services
                .GetKeyedService<ViewModelFactoryDelegate<TViewModelInterface, TArgs>>(key)
                ?.Invoke(args);
        }

        public TTreeSubpage CreateTreeSubPage<TContext, TTreeSubpage>(
            string subPageId,
            ITreeSubPageContext<TContext> context
        )
            where TContext : class, ITreePageViewModel
            where TTreeSubpage : ITreeSubpage
        {
            return services.CreateViewModel<TTreeSubpage, ITreeSubPageContext<TContext>>(
                subPageId,
                context
            );
        }
    }
}

public delegate TViewModelInterface ViewModelFactoryDelegate<out TViewModelInterface, in TArgs>(
    TArgs args
)
    where TViewModelInterface : IViewModel;
