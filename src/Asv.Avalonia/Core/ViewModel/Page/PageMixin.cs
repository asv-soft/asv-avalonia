using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class PageMixin
{
    extension(IServiceProvider services)
    {
        public IPage CreatePage(string pageId, IPageContext context)
        {
            return services.CreateViewModel<IPage, IPageContext>(pageId, context);
        }
    }
}
