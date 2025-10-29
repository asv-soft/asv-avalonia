﻿using System.Composition;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

[ExportExtensionFor<IShell>]
public class ShellLeftMenuExtenderExample : IExtensionFor<IShell>
{
    private readonly ILoggerFactory _loggerFactory;

    [ImportingConstructor]
    public ShellLeftMenuExtenderExample(ILayoutService layoutService, ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public void Extend(IShell context, CompositeDisposable contextDispose)
    {
        context.LeftMenu.Add(
            new MenuItem("home", "Home page", _loggerFactory)
            {
                Icon = OpenHomePageCommand.StaticInfo.Icon,
                Command = new BindableAsyncCommand(OpenHomePageCommand.Id, context),
            }
        );
    }
}
