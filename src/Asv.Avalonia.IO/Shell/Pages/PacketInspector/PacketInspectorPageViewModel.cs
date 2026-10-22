using System.Collections.ObjectModel;
using System.Text;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Avalonia.Controls;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R3;

namespace Asv.Avalonia.IO;

public class PacketInspectorPageViewModel : PageViewModel<IPage>
{
    public const string PageId = "io.packet-inspector";
    public const MaterialIconKind PageIcon = MaterialIconKind.SwapVertical;
    private readonly PacketCaptureService? _capture;
    private readonly PacketFileReader? _fileReader;
    private CancellationTokenSource? _fileRead;
    private readonly TimeProvider _time;
    private readonly IThemeService _themeService;
    private readonly IDeviceManager _deviceManager;
    private readonly Dictionary<string, PacketDevicePresentation> _devicePresentations = new();
    private readonly PacketDevicePresentation _unidentifiedDevice = new(
        RS.PacketInspector_Unidentified, MaterialIconKind.HelpCircleOutline, AsvColorKind.None);
    private readonly DispatcherTimer _timer;
    private PacketCaptureState? _state;
    private PacketMessageRow? _selectedMessage;
    private PacketJournalRow? _selectedEvent;
    private PacketGroupKey? _historyKey;
    private long _detailsSequence = -1;
    private int _detailsTab = -1;
    private string _search = string.Empty;
    private int _directionIndex;
    private bool _refreshing;

    public PacketInspectorPageViewModel()
        : this(
            DesignTime.PageContext,
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService,
            new PacketCaptureService(
                NullDeviceManager.Instance,
                TimeProvider.System,
                [],
                Options.Create(new PacketInspectorOptions()),
                DesignTime.LoggerFactory.CreateLogger<PacketCaptureService>()
            ),
            TimeProvider.System,
            NullDeviceManager.Instance,
            DesignTime.ThemeService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        _capture!.DisposeItWith(Disposable);
        var now = _time.GetUtcNow();
        var timestamp = _time.GetTimestamp();
        PacketFieldSnapshot[] fields =
        [
            new("Sequence", "UInt16", "42", null, null, []),
            new("Payload", "Object", string.Empty, null, null,
            [
                new("Voltage", "Float", "12.4", "V", null, []),
                new("Temperature", "Float", "24.6", "°C", null, []),
            ]),
        ];
        _state = new PacketCaptureState(
            [
                new(1, now, timestamp,
                    new("preview", "127.0.0.1:14550", PacketDirection.Rx, "example", "1", null),
                    "UDP", "Example", "Telemetry", fields, null),
                new(2, now, timestamp,
                    new("preview", "127.0.0.1:14550", PacketDirection.Tx, "example", "2", null),
                    "UDP", "Example", "Command", [new("Enabled", "Boolean", "true", null, null, [])], null),
            ],
            [new("preview", "UDP", "127.0.0.1:14550")],
            0,
            0
        );
        UpdateFilters(_state);
        Refresh();
        SelectedMessage = Messages.FirstOrDefault();
    }

    public PacketInspectorPageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService extensions,
        PacketCaptureService capture,
        TimeProvider time,
        IDeviceManager deviceManager,
        IThemeService? themeService = null
    )
        : this(context, loggerFactory, dialogService, extensions, capture, time, deviceManager, themeService, PageId, null) { }

    protected PacketInspectorPageViewModel(
        IPageContext context,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService extensions,
        PacketCaptureService? capture,
        TimeProvider time,
        IDeviceManager deviceManager,
        IThemeService? themeService,
        string pageId,
        PacketFileReader? fileReader)
        : base(pageId, context, loggerFactory, dialogService, extensions)
    {
        _capture = capture;
        _fileReader = fileReader;
        _time = time;
        _deviceManager = deviceManager;
        _themeService = themeService ?? NullThemeService.Instance;
        Header = RS.PacketInspector_Title;
        Icon = PageIcon;
        SourceWidget = new PacketInspectorWidget("packet-sources", RS.PacketInspector_Sources, WorkspaceDock.Left, MaterialIconKind.Network, this).DisposeItWith(Disposable);
        ProtocolWidget = new PacketInspectorWidget("packet-protocols", RS.PacketInspector_Protocols, WorkspaceDock.Left, MaterialIconKind.Layers, this).DisposeItWith(Disposable);
        DeviceWidget = new PacketInspectorWidget("packet-devices", RS.PacketInspector_Devices, WorkspaceDock.Left, MaterialIconKind.Devices, this).DisposeItWith(Disposable);
        TrafficWidget = new PacketInspectorWidget("packet-traffic", RS.PacketInspector_Title, WorkspaceDock.Center, PageIcon, this).DisposeItWith(Disposable);
        DetailsWidget = new PacketInspectorWidget("packet-details", RS.PacketInspector_Details, WorkspaceDock.Right, MaterialIconKind.CodeBraces, this).DisposeItWith(Disposable);
        ChartsWidget = new PacketInspectorWidget("packet-charts", RS.PacketInspector_Charts, WorkspaceDock.Bottom, MaterialIconKind.ChartLine, this).DisposeItWith(Disposable);
        AddToChartCommand = new ReactiveCommand<PacketChartRequest>(AddToChart).DisposeItWith(Disposable);
        ShowFieldInfoCommand = new ReactiveCommand<PacketFieldInfoRequest>().DisposeItWith(Disposable);
        ShowFieldInfoCommand.SubscribeAwait(
            async (request, _) =>
            {
                var markdown = request.Field.CreateDetailsMarkdown(request.Path);
                await dialogService.ShowMarkdownDetailsDialogAsync(
                    RS.PacketInspector_FieldInformation,
                    markdown,
                    markdown,
                    MaterialIconKind.InformationOutline);
            },
            AwaitOperation.Drop).DisposeItWith(Disposable);
        AllSources = new("all", RS.PacketInspector_AllSources, FiltersChanged);
        Sources.Add(AllSources);
        PauseCommand = new ReactiveCommand(_ =>
        {
            IsPaused = !IsPaused;
            if (!IsPaused)
            {
                Refresh();
            }
        }).DisposeItWith(Disposable);
        ClearCommand = new ReactiveCommand(_ =>
        {
            _capture?.Clear();
            foreach (var chart in Charts)
            {
                chart.Clear();
            }
            _historyKey = null;
            _state = null;
            SelectedEvent = null;
            SelectedMessage = null;
            _state = _capture?.Read();
            if (_state != null)
            {
                UpdateFilters(_state);
            }
            Refresh();
        }).DisposeItWith(Disposable);
        ResetCommand = new ReactiveCommand(_ =>
        {
            AllSources.SetChecked(true);
            foreach (var item in Protocols.Concat(Devices))
            {
                item.SetChecked(true);
            }
            _historyKey = null;
            Search = string.Empty;
            DirectionIndex = 0;
            Refresh();
        }).DisposeItWith(Disposable);
        HistoryCommand = new ReactiveCommand(_ =>
        {
            _historyKey = _fileReader != null && _historyKey != null ? null
                : ActiveTab == 0 ? SelectedMessage?.Packet.Key : SelectedEvent?.Packet.Key;
            ActiveTab = 1;
            Refresh();
        }).DisposeItWith(Disposable);
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(250),
        };
        _timer.Tick += (_, _) => ReadCapture();
        if (!Design.IsDesignMode && _fileReader == null)
        {
            _timer.Start();
        }
        OpenFileCommand = new ReactiveCommand().DisposeItWith(Disposable);
        OpenFileCommand.SubscribeAwait(
            async (_, token) =>
            {
                if (IsLoading || _fileReader == null)
                {
                    return;
                }
                var path = await dialogService.ShowOpenFileDialogAsync(RS.FileInspector_Open, "*");
                if (path != null && !IsDisposed)
                {
                    await LoadFileAsync(path, token);
                }
            },
            AwaitOperation.Drop).DisposeItWith(Disposable);
        CloseFileCommand = new ReactiveCommand(_ => CloseFile()).DisposeItWith(Disposable);
        CancelFileCommand = new ReactiveCommand(_ => _fileRead?.Cancel()).DisposeItWith(Disposable);
        if (_fileReader != null)
        {
            Header = RS.FileInspector_Title;
            Icon = MaterialIconKind.FileSearchOutline;
            SourceWidget.Header = RS.FileInspector_File;
            SourceWidget.Icon = MaterialIconKind.FileOutline;
            TrafficWidget.Header = RS.FileInspector_Title;
            TrafficWidget.Icon = MaterialIconKind.FileSearchOutline;
            ActiveTab = 1;
        }
        ReadCapture();
    }

    public ReactiveCommand OpenFileCommand { get; }
    public ReactiveCommand CloseFileCommand { get; }
    public ReactiveCommand CancelFileCommand { get; }
    public string FilePath { get; private set => SetField(ref field, value); } = string.Empty;
    public string FileName { get; private set => SetField(ref field, value); } = RS.FileInspector_Empty;
    public string FileStatus { get; private set => SetField(ref field, value); } = RS.FileInspector_Empty;
    public string FileError { get; private set => SetField(ref field, value); } = string.Empty;
    public bool IsLoading { get; private set => SetField(ref field, value); }
    public bool HasFile { get; private set => SetField(ref field, value); }
    public double FileProgress { get; private set => SetField(ref field, value); }
    public string HistoryActionText { get; private set => SetField(ref field, value); } = RS.PacketInspector_History;
    public string DetailsPosition { get; private set => SetField(ref field, value); } = string.Empty;

    public async Task LoadFileAsync(string path, CancellationToken cancellationToken = default)
    {
        if (_fileReader == null || IsLoading || IsDisposed)
        {
            return;
        }
        using var read = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, DisposeCancel);
        _fileRead = read;
        IsLoading = true;
        FileProgress = 0;
        FileError = string.Empty;
        try
        {
            var progress = new Progress<double>(value =>
            {
                if (!IsDisposed && ReferenceEquals(_fileRead, read))
                {
                    FileProgress = value;
                }
            });
            var result = await Task.Run(() => _fileReader.ReadAsync(path, progress, read.Token), read.Token);
            read.Token.ThrowIfCancellationRequested();
            if (IsDisposed)
            {
                return;
            }
            CloseFile();
            FilePath = result.Path;
            FileName = Path.GetFileName(result.Path);
            HasFile = true;
            FileStatus = string.Format(RS.FileInspector_Status, result.Bytes, result.State.Packets.Count, result.State.Errors);
            FileProgress = 100;
            _state = result.State;
            UpdateFilters(_state);
            Refresh();
            SelectedEvent = Journal.FirstOrDefault();
            SelectedMessage = Messages.FirstOrDefault();
        }
        catch (OperationCanceledException) when (read.IsCancellationRequested) { }
        catch (Exception error)
        {
            if (!IsDisposed)
            {
                FileError = error.Message;
            }
        }
        finally
        {
            if (ReferenceEquals(_fileRead, read))
            {
                _fileRead = null;
            }
            if (!IsDisposed)
            {
                IsLoading = false;
            }
        }
    }

    private void CloseFile()
    {
        foreach (var chart in Charts)
        {
            chart.Dispose();
        }
        Charts.Clear();
        HasCharts = false;
        foreach (var row in Messages)
        {
            row.Dispose();
        }
        Messages.Clear();
        Journal.Clear();
        foreach (var filter in Protocols.Concat(Devices))
        {
            DisposeFilter(filter);
        }
        Protocols.Clear();
        Devices.Clear();
        foreach (var filter in AllSources.Children)
        {
            DisposeFilter(filter);
        }
        AllSources.Children.Clear();
        _devicePresentations.Clear();
        _state = null;
        _historyKey = null;
        HistoryActionText = RS.PacketInspector_History;
        SelectedMessage = null;
        SelectedEvent = null;
        HasFile = false;
        IsEmpty = true;
        FilePath = string.Empty;
        FileName = RS.FileInspector_Empty;
        FileStatus = RS.FileInspector_Empty;
        FileError = string.Empty;
    }

    protected override void AfterLoadExtensions() { }

    public PacketInspectorWidget SourceWidget { get; }
    public PacketInspectorWidget ProtocolWidget { get; }
    public PacketInspectorWidget DeviceWidget { get; }
    public PacketInspectorWidget TrafficWidget { get; }
    public PacketInspectorWidget DetailsWidget { get; }
    public PacketInspectorWidget ChartsWidget { get; }
    public ObservableCollection<PacketFieldChart> Charts { get; } = [];
    public bool HasCharts { get; private set => SetField(ref field, value); }
    public ReactiveCommand<PacketChartRequest> AddToChartCommand { get; }
    public ReactiveCommand<PacketFieldInfoRequest> ShowFieldInfoCommand { get; }

    public override IEnumerable<IViewModel> GetChildren() => base.GetChildren().Concat(
        new IViewModel[] { SourceWidget, ProtocolWidget, DeviceWidget, TrafficWidget, DetailsWidget, ChartsWidget }).Concat(Charts);

    public ObservableCollection<PacketFilterItem> Sources { get; } = [];
    public PacketFilterItem AllSources { get; }
    public ObservableCollection<PacketFilterItem> Protocols { get; } = [];
    public ObservableCollection<PacketFilterItem> Devices { get; } = [];
    public ObservableCollection<PacketMessageRow> Messages { get; } = [];
    public ObservableCollection<PacketJournalRow> Journal { get; } = [];
    public ObservableCollection<PacketFieldSnapshot> Fields { get; } = [];
    public ReactiveCommand PauseCommand { get; }
    public ReactiveCommand ClearCommand { get; }
    public ReactiveCommand ResetCommand { get; }
    public ReactiveCommand HistoryCommand { get; }
    public string[] Directions { get; } = ["RX + TX", "RX", "TX"];
    public string Search
    {
        get => _search;
        set
        {
            SetField(ref _search, value);
            _historyKey = null;
            Refresh();
        }
    }

    public int DirectionIndex
    {
        get => _directionIndex;
        set
        {
            SetField(ref _directionIndex, value);
            Refresh();
        }
    }

    public string PauseActionText { get; private set => SetField(ref field, value); } = RS.PacketInspector_Pause;

    public bool IsPaused
    {
        get;
        set
        {
            SetField(ref field, value);
            PauseActionText = value ? RS.PacketInspector_Resume : RS.PacketInspector_Pause;
            if (value)
            {
                Refresh();
            }
            if (!value && _timer != null)
            {
                ReadCapture();
            }
        }
    }
    public int ActiveTab
    {
        get;
        set
        {
            SetField(ref field, value);
            IsEmpty = value == 0 ? Messages.Count == 0 : Journal.Count == 0;
            UpdateDetails();
        }
    }
    public PacketMessageRow? SelectedMessage
    {
        get => _selectedMessage;
        set
        {
            SetField(ref _selectedMessage, value);
            if (!_refreshing)
            {
                UpdateDetails();
            }
        }
    }
    public PacketJournalRow? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            SetField(ref _selectedEvent, value);
            if (!_refreshing)
            {
                UpdateDetails();
            }
        }
    }
    public string DetailsTitle { get; private set => SetField(ref field, value); } = RS.PacketInspector_SelectMessage;
    public bool HasDetails { get; private set => SetField(ref field, value); }
    public string DetailsProtocol { get; private set => SetField(ref field, value); } = string.Empty;
    public string DetailsDirection { get; private set => SetField(ref field, value); } = string.Empty;
    public string DetailsTime { get; private set => SetField(ref field, value); } = string.Empty;
    public string DetailsEndpoint { get; private set => SetField(ref field, value); } = string.Empty;
    public string DetailsEndpointDescription { get; private set => SetField(ref field, value); } = string.Empty;
    public string DetailsMetadata { get; private set => SetField(ref field, value); } = string.Empty;
    public PacketSnapshot? DetailsPacket { get; private set => SetField(ref field, value); }
    public IReadOnlyList<PacketFieldSnapshot> DetailsFields { get; private set => SetField(ref field, value); } = [];
    public string DetailsText { get; private set => SetField(ref field, value); } = string.Empty;
    public string DetailsError { get; private set => SetField(ref field, value); } = string.Empty;
    public PacketDevicePresentation? DetailsDevice { get; private set => SetField(ref field, value); }
    public string CaptureStatus { get; private set => SetField(ref field, value); } = string.Empty;
    public bool IsEmpty { get; private set => SetField(ref field, value); } = true;

    private void ReadCapture()
    {
        if (IsDisposed || IsPaused || _capture == null)
        {
            return;
        }
        _state = _capture?.Read();
        if (_state != null)
        {
            UpdateFilters(_state);
        }
        Refresh();
    }

    private static string SourceKey(string port, string endpoint) => $"{port.Length}:{port}{endpoint}";
    private static string DeviceKey(DeviceId? device) => device == null ? "unknown" : $"device:{device.DeviceClass.Length}:{device.DeviceClass}{device.AsString()}";

    private PacketDevicePresentation GetDevicePresentation(PacketSnapshot packet)
    {
        var deviceId = packet.Key.DeviceId;
        if (deviceId == null)
        {
            return _unidentifiedDevice;
        }
        var key = DeviceKey(deviceId);
        if (!_devicePresentations.TryGetValue(key, out var presentation))
        {
            presentation = new(packet.Device, _deviceManager.GetIcon(deviceId), _deviceManager.GetDeviceColor(deviceId));
            _devicePresentations.Add(key, presentation);
        }
        return presentation;
    }

    private void UpdateFilters(PacketCaptureState state)
    {
        foreach (var entry in state.Ports)
        {
            var item = AllSources.Children.FirstOrDefault(x => x.Key == entry.Id);
            if (item == null)
            {
                item = new PacketFilterItem(entry.Id, entry.DisplayName, FiltersChanged);
                item.SetChecked(AllSources.IsChecked != false);
                AllSources.Children.Add(item);
            }
            item.UpdatePresentation(entry.DisplayName, entry.Description);
        }
        foreach (var protocol in state.Protocols)
        {
            if (Protocols.All(x => x.Key != protocol.Id))
            {
                Protocols.Add(new(protocol.Id, protocol.Name, FiltersChanged));
            }
        }
        var sources = state.Sources.Concat(state.Packets.Reverse().Select(PacketSource.FromPacket))
            .DistinctBy(x => SourceKey(x.PortId, x.EndpointId)).ToArray();
        var presentedPorts = state.Ports.Select(x => x.Id).ToHashSet();
        foreach (var source in sources)
        {
            var port = AllSources.Children.FirstOrDefault(x => x.Key == source.PortId);
            if (port == null)
            {
                port = new(source.PortId, source.PortName, FiltersChanged);
                port.SetChecked(AllSources.IsChecked != false);
                AllSources.Children.Add(port);
            }
            if (presentedPorts.Add(source.PortId))
            {
                port.UpdatePresentation(source.PortName, source.PortDescription);
            }
            var key = SourceKey(source.PortId, source.EndpointId);
            var child = port.Children.FirstOrDefault(x => x.Key == key);
            if (child == null)
            {
                child = new PacketFilterItem(key, source.EndpointName, FiltersChanged);
                child.SetChecked(port.IsChecked != false);
                port.Children.Add(child);
            }
            child.UpdatePresentation(source.EndpointName, source.EndpointDescription);
        }
        foreach (var packet in state.Packets)
        {
            if (Protocols.All(x => x.Key != packet.Key.ProtocolId))
            {
                Protocols.Add(new(packet.Key.ProtocolId, packet.Protocol, FiltersChanged));
            }
            var key = DeviceKey(packet.Key.DeviceId);
            if (Devices.All(x => x.Key != key))
            {
                Devices.Add(new(key, packet.Device, FiltersChanged)
                {
                    DevicePresentation = GetDevicePresentation(packet),
                });
            }
        }
        var retainedSources = sources;
        foreach (var port in AllSources.Children.ToArray())
        {
            var endpoints = retainedSources.Where(x => x.PortId == port.Key)
                .Select(x => SourceKey(x.PortId, x.EndpointId)).ToHashSet();
            foreach (var child in port.Children.Where(x => !endpoints.Contains(x.Key)).ToArray())
            {
                port.Children.Remove(child);
                child.Dispose();
            }
            if (!retainedSources.Any(x => x.PortId == port.Key) && !state.Ports.Any(x => x.Id == port.Key))
            {
                AllSources.Children.Remove(port);
                DisposeFilter(port);
            }
            else
            {
                port.UpdateParent();
            }
        }
        AllSources.UpdateParent();
    }

    private void FiltersChanged()
    {
        foreach (var port in AllSources.Children)
        {
            port.UpdateParent();
        }
        AllSources.UpdateParent();
        Refresh();
    }

    private void Refresh()
    {
        if (_state == null || IsDisposed)
        {
            return;
        }
        foreach (var chart in Charts)
        {
            chart.Append(_state.Packets);
        }
        var sourceKeys = AllSources.Children.SelectMany(x => x.Children).Where(x => x.IsChecked == true).Select(x => x.Key).ToHashSet();
        var protocols = Protocols.Where(x => x.IsChecked == true).Select(x => x.Key).ToHashSet();
        var devices = Devices.Where(x => x.IsChecked == true).Select(x => x.Key).ToHashSet();
        bool MatchesFilters(PacketSnapshot packet) => sourceKeys.Contains(SourceKey(packet.Key.PortId, packet.Endpoint))
            && protocols.Contains(packet.Key.ProtocolId) && devices.Contains(DeviceKey(packet.Key.DeviceId))
            && (DirectionIndex == 0 || (DirectionIndex == 1 ? packet.Key.Direction == PacketDirection.Rx : packet.Key.Direction == PacketDirection.Tx))
            && (packet.Name.Contains(Search, StringComparison.OrdinalIgnoreCase) || packet.MessageId.Contains(Search, StringComparison.OrdinalIgnoreCase));
        var filtered = _state.Packets.Where(MatchesFilters).ToArray();
        var selectedMessage = SelectedMessage;
        var selectedKey = selectedMessage?.Packet.Key;
        var selectedEvent = SelectedEvent;
        _refreshing = true;
        try
        {
            var existing = Messages.ToDictionary(x => x.Packet.Key);
            var keep = new HashSet<PacketGroupKey>();

            // Buffer eviction must not remove the row the user is inspecting.
            if (selectedMessage != null && _state.Evicted > 0 && MatchesFilters(selectedMessage.Packet))
            {
                keep.Add(selectedMessage.Packet.Key);
            }
            foreach (var group in filtered.GroupBy(x => x.Key))
            {
                var latest = group.Last();
                keep.Add(group.Key);
                var elapsed = _time.GetElapsedTime(group.First().MonotonicTimestamp, _time.GetTimestamp()).TotalSeconds;
                var count = group.Count();
                var frequency = _fileReader == null && count > 1 && elapsed > 0 ? (count - 1) / elapsed : 0;
                if (existing.TryGetValue(group.Key, out var row))
                {
                    row.Update(latest, count, frequency);
                }
                else
                {
                    Messages.Add(new(latest, count, frequency)
                    {
                        DevicePresentation = GetDevicePresentation(latest),
                    });
                }
            }
            for (var i = Messages.Count - 1; i >= 0; i--)
            {
                if (!keep.Contains(Messages[i].Packet.Key))
                {
                    Messages[i].Dispose();
                    Messages.RemoveAt(i);
                }
            }
            var events = filtered.Where(x => _historyKey == null || x.Key == _historyKey);
            var journal = (_fileReader == null ? events.Reverse() : events).ToArray();
            var sequences = journal.Select(x => x.Sequence).ToHashSet();
            for (var i = Journal.Count - 1; i >= 0; i--)
            {
                if (!sequences.Contains(Journal[i].Sequence))
                {
                    Journal.RemoveAt(i);
                }
            }
            for (var i = 0; i < journal.Length; i++)
            {
                if (i >= Journal.Count || Journal[i].Sequence != journal[i].Sequence)
                {
                    Journal.Insert(i, new(journal[i], GetDevicePresentation(journal[i])));
                }
            }
            SelectedMessage = Messages.FirstOrDefault(x => x.Packet.Key == selectedKey);

            // Keep an explicitly selected immutable snapshot even after buffer eviction.
            SelectedEvent = selectedEvent;
        }
        finally
        {
            _refreshing = false;
        }
        IsEmpty = ActiveTab == 0 ? Messages.Count == 0 : Journal.Count == 0;
        CaptureStatus = (IsPaused ? RS.PacketInspector_Paused + " · " : string.Empty)
            + string.Format(RS.PacketInspector_Status, _state.Packets.Count, _state.Evicted, _state.Errors);
        HistoryActionText = _fileReader != null && _historyKey != null ? RS.FileInspector_ShowAll : RS.PacketInspector_History;
        UpdateDetails();
    }

    private void UpdateDetails()
    {
        var packet = ActiveTab == 0 ? SelectedMessage?.Packet : SelectedEvent?.Packet;
        if (_detailsSequence == (packet?.Sequence ?? -1) && _detailsTab == ActiveTab)
        {
            return;
        }
        _detailsSequence = packet?.Sequence ?? -1;
        _detailsTab = ActiveTab;
        DetailsPacket = packet;
        DetailsPosition = packet?.FileEndOffset is { } offset ? $"#{packet.Sequence} · 0x{offset:X8}" : string.Empty;
        Fields.Clear();
        DetailsTitle = packet?.Name ?? RS.PacketInspector_SelectMessage;
        DetailsMetadata = packet == null ? string.Empty
            : $"{packet.Protocol} · {packet.Direction} · {packet.Time}\n{packet.PortName} / {packet.Endpoint}";
        HasDetails = packet != null;
        DetailsProtocol = packet?.Protocol ?? string.Empty;
        DetailsDirection = packet?.Direction ?? string.Empty;
        DetailsTime = packet?.Time ?? string.Empty;
        DetailsEndpoint = packet == null ? string.Empty
            : string.IsNullOrWhiteSpace(packet.EndpointName) ? packet.Endpoint : packet.EndpointName;
        DetailsEndpointDescription = packet?.EndpointDescription ?? packet?.Endpoint ?? string.Empty;
        DetailsDevice = packet == null ? null : GetDevicePresentation(packet);
        DetailsError = packet?.InspectionError ?? string.Empty;
        if (packet == null)
        {
            DetailsFields = [];
            DetailsText = string.Empty;
            return;
        }
        foreach (var field in packet.Fields)
        {
            Fields.Add(field);
        }
        if (Fields.Count == 0)
        {
            DetailsError = string.Join(Environment.NewLine, new[] { DetailsError, RS.PacketInspector_NoFields }.Where(x => x.Length > 0));
        }
        var builder = new StringBuilder();
        var textFields = packet.Fields;
        if (textFields.Count == 1 && textFields[0].Name == packet.Name
            && string.IsNullOrEmpty(textFields[0].Value) && textFields[0].Children.Count > 0)
        {
            textFields = textFields[0].Children;
        }
        DetailsFields = textFields;
        AppendFields(builder, textFields, 0);
        DetailsText = builder.ToString();
    }

    private void AddToChart(PacketChartRequest request)
    {
        var packet = request.Packet;
        var field = request.Field;
        if (!PacketFieldChart.TryGetNumber(field, out _))
        {
            return;
        }
        var indices = new List<int>();
        var names = new List<string>();
        if (!FindField(packet.Fields, field, indices, names)
            || Charts.Any(chart => chart.Matches(packet.Key, indices, names)))
        {
            return;
        }
        var chart = new PacketFieldChart(
            packet.Key,
            indices,
            names,
            field.Units ?? string.Empty,
            _themeService,
            RemoveChart);
        chart.SetParent(this);
        Charts.Add(chart);
        HasCharts = true;
        chart.Append((_state?.Packets ?? []).Append(packet).OrderBy(sample => sample.Sequence));
    }

    private void RemoveChart(PacketFieldChart chart)
    {
        if (Charts.Remove(chart))
        {
            chart.Dispose();
            HasCharts = Charts.Count > 0;
        }
    }

    private static bool FindField(
        IReadOnlyList<PacketFieldSnapshot> fields,
        PacketFieldSnapshot target,
        List<int> indices,
        List<string> names)
    {
        for (var index = 0; index < fields.Count; index++)
        {
            indices.Add(index);
            names.Add(fields[index].Name);
            if (ReferenceEquals(fields[index], target) || FindField(fields[index].Children, target, indices, names))
            {
                return true;
            }
            indices.RemoveAt(indices.Count - 1);
            names.RemoveAt(names.Count - 1);
        }
        return false;
    }

    private static void AppendFields(StringBuilder builder, IReadOnlyList<PacketFieldSnapshot> fields, int depth)
    {
        foreach (var field in fields)
        {
            builder.Append(' ', depth * 2).Append(field.Name).Append(": ").Append(field.Value)
                .Append(' ').Append(field.Units).AppendLine();
            AppendFields(builder, field.Children, depth + 1);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _fileRead?.Cancel();
            _timer.Stop();
            foreach (var chart in Charts)
            {
                chart.Dispose();
            }
            Charts.Clear();
            foreach (var row in Messages)
            {
                row.Dispose();
            }
            foreach (var item in Sources.Concat(Protocols).Concat(Devices))
            {
                DisposeFilter(item);
            }
        }
        base.Dispose(disposing);
    }

    private static void DisposeFilter(PacketFilterItem item)
    {
        foreach (var child in item.Children)
        {
            DisposeFilter(child);
        }
        item.Dispose();
    }
}
