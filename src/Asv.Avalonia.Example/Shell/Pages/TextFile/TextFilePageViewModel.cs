using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Modeling;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

public class TextFilePageViewModel : PageViewModel<TextFilePageViewModel>, ISupportSaveAs
{
    public const string PageId = "text_file";
    public const MaterialIconKind PageIcon = MaterialIconKind.FileOutline;
    public const string FileExtension = "asvmd";
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

        Header = "ASV Markdown file";
        Icon = PageIcon;

        var filePath = context.NavArgs.FirstOrDefault(x => x.Key == FilePathArg).Value;
        if (!string.IsNullOrWhiteSpace(filePath))
        {
            Load(filePath);
        }

        Text.ModelValue.Subscribe(_ => UpdateStateIcon()).DisposeItWith(Disposable);
        UpdateStateIcon();
        Events.Catch<DesktopDragEvent>(OnDesktopDragEvent).DisposeItWith(Disposable);
    }

    private async ValueTask OnDesktopDragEvent(
        IViewModel owner,
        DesktopDragEvent e,
        CancellationToken cancel
    )
    {
        var files = await GetDraggedFilePaths(e.Args, cancel);
        if (files.Length == 0)
        {
            return;
        }

        var builder = new StringBuilder();
        foreach (var path in files)
        {
            if (cancel.IsCancellationRequested)
            {
                return;
            }

            if (!Path.Exists(path))
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append(await File.ReadAllTextAsync(path, Encoding.UTF8, cancel));
        }

        if (builder.Length == 0)
        {
            return;
        }

        InsertText(builder.ToString());
        e.Args.Handled = true;
    }

    public HistoricalStringProperty Text { get; }

    public BindableReactiveProperty<string> FilePath { get; }

    public string? CurrentFilePath
    {
        get;
        private set => SetField(ref field, value);
    }

    public string? DefaultFileName =>
        CurrentFilePath != null ? Path.GetFileName(CurrentFilePath) : $"document.{FileExtension}";

    public string? DefaultExtension => FileExtension;

    public string? TypeFilter => $"{FileExtension},*";

    private static string NewFileTitle => "New ASV Markdown file";

    public static NavArgs CreateOpenArgs(string filePath)
    {
        return new NavArgs(new KeyValuePair<string, string?>(FilePathArg, filePath));
    }

    public async ValueTask Save(CancellationToken cancel = default)
    {
        if (CurrentFilePath == null)
        {
            return;
        }

        await SaveAs(CurrentFilePath, cancel);
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

    private void InsertText(string text)
    {
        var currentText = GetText();
        Text.ViewValue.Value =
            currentText.Length == 0 ? text : $"{currentText}{Environment.NewLine}{text}";
    }

    private static async ValueTask<string[]> GetDraggedFilePaths(
        DragEventArgs args,
        CancellationToken cancel
    )
    {
        var paths = new List<string>();
        if (args.DataTransfer is IAsyncDataTransfer asyncDataTransfer)
        {
            var asyncFiles = await asyncDataTransfer.TryGetFilesAsync().WaitAsync(cancel);
            AddStorageItems(paths, asyncFiles);
        }

        var transferFiles = args.DataTransfer.TryGetFiles();
        AddStorageItems(paths, transferFiles);

        var fileNamesFormat = DataFormat.CreateStringPlatformFormat("FileNames");
        var fileNames = args.DataTransfer.TryGetValues(fileNamesFormat);
        if (fileNames != null)
        {
            foreach (var path in fileNames)
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    paths.Add(path);
                }
            }
        }

        return paths.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static void AddStorageItems(List<string> paths, IEnumerable<IStorageItem>? files)
    {
        if (files != null)
        {
            foreach (var file in files)
            {
                var path = file.TryGetLocalPath();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    paths.Add(path);
                }
            }
        }
    }

    private string GetText()
    {
        return Text.ModelValue.Value ?? string.Empty;
    }
}
