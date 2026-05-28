using System;
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
    private readonly ReactiveProperty<string?> _textModel;
    private string _savedText = string.Empty;

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
        _textModel = new ReactiveProperty<string?>(string.Empty).DisposeItWith(Disposable);
        Text = new HistoricalStringProperty(nameof(Text), _textModel, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        FilePath = new BindableReactiveProperty<string>(NewFileTitle).DisposeItWith(Disposable);

        Header = "Text file";
        Icon = PageIcon;

        var filePath = context.NavArgs.FirstOrDefault(x => x.Key == FilePathArg).Value;
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            Load(filePath);
        }

        Text.ModelValue.Subscribe(_ => UpdateStateIcon()).DisposeItWith(Disposable);
        UpdateStateIcon();
    }

    public HistoricalStringProperty Text { get; }

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

        await File.WriteAllTextAsync(filePath, GetText(), Encoding.UTF8, cancel);
        SetFilePath(filePath);
        _savedText = GetText();
        UpdateStateIcon();
    }

    protected override void AfterLoadExtensions() { }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Text;
    }

    private void Load(string filePath)
    {
        Text.ModelValue.Value = File.ReadAllText(filePath, Encoding.UTF8);
        _savedText = GetText();
        SetFilePath(filePath);
        UpdateStateIcon();
    }

    private void SetFilePath(string filePath)
    {
        CurrentFilePath = filePath;
        FilePath.Value = filePath;
        Header = Path.GetFileName(filePath);
    }

    private void UpdateStateIcon()
    {
        var isModified = !string.Equals(GetText(), _savedText, StringComparison.Ordinal);
        Status = isModified ? MaterialIconKind.Pencil : null;
        StatusColor = isModified ? AsvColorKind.Warning : AsvColorKind.None;
    }

    private string GetText()
    {
        return Text.ModelValue.Value ?? string.Empty;
    }
}
