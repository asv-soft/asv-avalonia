using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Example;

public class DialogItemImageViewModel(ILoggerFactory loggerFactory)
    : DialogViewModelBase(DialogId, loggerFactory)
{
    public const string DialogId = $"{BaseId}.item.image";

    private const string DesignImagePath = "avares://Asv.Avalonia.Example/Assets/logo.png";

    public DialogItemImageViewModel()
        : this(NullLoggerFactory.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();

        Image = new Bitmap(AssetLoader.Open(new Uri(DesignImagePath)));
    }

    public required Bitmap Image { get; set; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}
