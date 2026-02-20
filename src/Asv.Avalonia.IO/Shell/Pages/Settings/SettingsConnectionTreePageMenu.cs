using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R3;

namespace Asv.Avalonia.IO;

public class SettingsConnectionTreePageMenu : TreePage
{
    public SettingsConnectionTreePageMenu(ILoggerFactory loggerFactory)
        : base(
            SettingsConnectionViewModel.SubPageId,
            RS.SettingsPageExtension_TreePage_Title,
            SettingsConnectionViewModel.Icon,
            SettingsConnectionViewModel.SubPageId,
            NavigationId.Empty,
            loggerFactory
        ) { }
}
