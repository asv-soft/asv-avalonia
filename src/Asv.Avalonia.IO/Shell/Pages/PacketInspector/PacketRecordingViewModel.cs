using Asv.Common;
using Asv.IO;
using Avalonia.Controls;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia.IO;

public sealed class PacketRecordingViewModel : ViewModel
{
    private readonly PacketCaptureService? _capture;
    private readonly IProtocolFactory _factory;
    private readonly IDialogService _dialogs;
    private readonly DispatcherTimer _timer;
    private PacketRecordingSession? _session;
    private bool _busy;

    public PacketRecordingViewModel(PacketCaptureService? capture, IProtocolFactory factory, IDialogService dialogs)
        : base("packet-recording")
    {
        _capture = capture;
        _factory = factory;
        _dialogs = dialogs;
        ToggleCommand = new ReactiveCommand(ToggleAsync).DisposeItWith(Disposable);
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (_, _) => UpdateStatus();
        if (!Design.IsDesignMode && capture != null)
        {
            _timer.Start();
        }
        UpdateStatus();
    }

    public IReadOnlyList<string> Formats { get; } = [RecordingStrings.Binary, RecordingStrings.JsonLines];
    public int FormatIndex { get; set => SetField(ref field, value); }
    public string FilePath { get; private set => SetField(ref field, value); } = string.Empty;
    public string FileName { get; private set => SetField(ref field, value); } = string.Empty;
    public string Status { get; private set => SetField(ref field, value); } = string.Format(RecordingStrings.Counters, 0, 0);
    public string? Error { get; private set => SetField(ref field, value); }
    public bool IsRecording { get; private set => SetField(ref field, value); }
    public bool CanChooseFormat { get; private set => SetField(ref field, value); } = true;
    public string ActionText { get; private set => SetField(ref field, value); } = RecordingStrings.Start;
    public ReactiveCommand ToggleCommand { get; }

    private void UpdateStatus()
    {
        IsRecording = _session?.IsRecording == true;
        CanChooseFormat = _session == null && !_busy;
        ActionText = _busy ? (_session == null ? RecordingStrings.Preparing : RecordingStrings.Stopping)
            : _session != null ? RecordingStrings.Stop : RecordingStrings.Start;
        ToggleCommand.ChangeCanExecute(!_busy && _capture != null && !IsDisposed);
        if (_session != null)
        {
            Status = string.Format(RecordingStrings.Counters, _session.MessagesWritten, _session.BytesWritten);
            Error = _session.Error?.Message;
        }
    }

    private async ValueTask ToggleAsync(Unit argument, CancellationToken cancellationToken)
    {
        if (_busy || _capture == null)
        {
            return;
        }
        _busy = true;
        UpdateStatus();
        try
        {
            if (_session != null)
            {
                var session = _session;
                _capture.DetachRecording(session);
                await session.StopAsync();
                if (!IsDisposed)
                {
                    UpdateStatus();
                }
                _session = null;
                return;
            }
            Error = null;
            var format = (PacketRecordingFormat)FormatIndex;
            if (!Enum.IsDefined(format))
            {
                throw new InvalidOperationException("Select a recording format.");
            }
            var extension = format == PacketRecordingFormat.Binary ? "bin" : "jsonl";
            var path = await _dialogs.ShowSaveFileDialogAsync(
                RecordingStrings.Save,
                extension,
                $"traffic-{DateTime.Now:yyyyMMdd-HHmmss}",
                extension);
            if (string.IsNullOrWhiteSpace(path) || cancellationToken.IsCancellationRequested || IsDisposed)
            {
                return;
            }
            var stream = new FileStream(path, new FileStreamOptions
            {
                Mode = FileMode.Create,
                Access = FileAccess.Write,
                Share = FileShare.Read,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
            });
            var recording = new PacketRecordingSession(_factory, format, stream);
            try
            {
                _capture.AttachRecording(recording);
            }
            catch
            {
                await recording.DisposeAsync();
                throw;
            }
            _session = recording;
            FilePath = path;
            FileName = Path.GetFileName(path);
            Status = string.Format(RecordingStrings.Counters, 0, 0);
        }
        catch (Exception error)
        {
            Error = error.Message;
        }
        finally
        {
            _busy = false;
            if (!IsDisposed)
            {
                UpdateStatus();
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            if (_session != null)
            {
                _capture?.DetachRecording(_session);

                // The capture service owns the asynchronous drain through application shutdown.
                _session = null;
            }
        }
        base.Dispose(disposing);
    }
}
