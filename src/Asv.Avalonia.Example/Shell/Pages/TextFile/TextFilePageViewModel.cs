using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

public class TextFilePageViewModel : PageViewModel<TextFilePageViewModel>, ISupportSaveAs
{
    public const string PageId = "text_file";
    public const MaterialIconKind PageIcon = MaterialIconKind.FileOutline;
    private const string FilePathArg = "file";

    public TextFilePageViewModel()
        : this(
            DesignTime.PageContext,
            NullLoggerFactory.Instance,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public TextFilePageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService extensionService
    )
        : base(PageId, context, loggerFactory, dialogService, extensionService)
    {
        Text = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        FilePath = new BindableReactiveProperty<string>(NewFileTitle).DisposeItWith(Disposable);

        Header = "Text file";
        Icon = PageIcon;

        var filePath = context.NavArgs.FirstOrDefault(x => x.Key == FilePathArg).Value;
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            Load(filePath);
        }
    }

    public BindableReactiveProperty<string> Text { get; }

    public BindableReactiveProperty<string> FilePath { get; }

    public string? CurrentFilePath
    {
        get;
        private set => SetField(ref field, value);
    }

    public string? DefaultFileName =>
        CurrentFilePath != null ? Path.GetFileName(CurrentFilePath) : "text-file-example.txt";

    public string? DefaultExtension => "txt";

    public string? TypeFilter => "txt,*";

    private static string NewFileTitle => "New text file";

    public static NavArgs CreateOpenArgs(string filePath)
    {
        return new NavArgs(new KeyValuePair<string, string?>(FilePathArg, filePath));
    }

    public async ValueTask Save()
    {
        if (CurrentFilePath == null)
        {
            return;
        }

        await SaveAs(CurrentFilePath);
    }

    public async ValueTask SaveAs(string filePath, CancellationToken cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        await File.WriteAllTextAsync(filePath, Text.Value, Encoding.UTF8, cancel);
        SetFilePath(filePath);
    }

    protected override void AfterLoadExtensions() { }

    private void Load(string filePath)
    {
        Text.Value = File.ReadAllText(filePath, Encoding.UTF8);
        SetFilePath(filePath);
    }

    private void SetFilePath(string filePath)
    {
        CurrentFilePath = filePath;
        FilePath.Value = filePath;
        Header = Path.GetFileName(filePath);
    }
}
