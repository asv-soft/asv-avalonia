using Asv.Common;
using R3;

namespace Asv.Avalonia.IO;

public sealed class PacketFileExportViewModel : ViewModel
{
    private readonly PacketFileExporter _exporter;
    private readonly IDialogService _dialogs;
    private CancellationTokenSource? _operation;
    private string? _source;

    public PacketFileExportViewModel(PacketFileExporter exporter, IDialogService dialogs) : base("packet-file-export")
    {
        _exporter = exporter;
        _dialogs = dialogs;
        ExportCommand = new ReactiveCommand(ExportAsync).DisposeItWith(Disposable);
        CancelCommand = new ReactiveCommand(_ => _operation?.Cancel()).DisposeItWith(Disposable);
    }

    public ReactiveCommand ExportCommand { get; }
    public ReactiveCommand CancelCommand { get; }
    public bool CanExport { get; private set => SetField(ref field, value); }
    public bool IsBusy { get; private set => SetField(ref field, value); }
    public double Progress { get; private set => SetField(ref field, value); }
    public string Status { get; private set => SetField(ref field, value); } = string.Empty;
    public string Error { get; private set => SetField(ref field, value); } = string.Empty;

    public void SetSource(string? source)
    {
        if (_source != source)
        {
            _operation?.Cancel();
            _source = source;
            if (!IsBusy)
            {
                Status = string.Empty;
            }
            Error = string.Empty;
        }
        CanExport = !IsBusy && !string.IsNullOrEmpty(_source);
    }

    private async ValueTask ExportAsync(Unit unit, CancellationToken cancellationToken)
    {
        if (!CanExport || _source is not { } source)
        {
            return;
        }
        using var operation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, DisposeCancel);
        _operation = operation;
        IsBusy = true;
        CanExport = false;
        Progress = 0;
        Status = string.Empty;
        Error = string.Empty;
        try
        {
            var destination = await _dialogs.ShowSaveFileDialogAsync(FileExportStrings.Save, "jsonl", Path.GetFileNameWithoutExtension(source), "jsonl");
            if (string.IsNullOrWhiteSpace(destination))
            {
                return;
            }
            operation.Token.ThrowIfCancellationRequested();
            Status = FileExportStrings.Exporting;
            var progress = new Progress<double>(value =>
            {
                if (!IsDisposed && ReferenceEquals(_operation, operation))
                {
                    Progress = value;
                }
            });
            var result = await _exporter.ExportJsonLinesAsync(source, destination, progress, operation.Token);
            if (!IsDisposed)
            {
                Progress = 100;
                Status = string.Format(FileExportStrings.Completed, result.Messages, result.Errors);
            }
        }
        catch (OperationCanceledException) when (operation.IsCancellationRequested)
        {
            if (!IsDisposed)
            {
                Status = FileExportStrings.Canceled;
            }
        }
        catch (Exception error)
        {
            if (!IsDisposed)
            {
                Error = error.Message;
            }
        }
        finally
        {
            if (ReferenceEquals(_operation, operation))
            {
                _operation = null;
            }
            if (!IsDisposed)
            {
                IsBusy = false;
                CanExport = !string.IsNullOrEmpty(_source);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _operation?.Cancel();
        }
        base.Dispose(disposing);
    }
}
