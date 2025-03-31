using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example.PacketViewer.Dialogs;

public class SeparatorViewModel : DialogViewModelBase
{
    public const string ViewModelId = "packet_viewer.separator.dialog";

    private readonly ILogger _logger;
    private readonly ObservableList<PacketMessageViewModel> _packetsList;

    public SeparatorViewModel()
        : base(ViewModelId)
    {
        FilePath = new BindableReactiveProperty<string>(string.Empty);
        IsSemicolon = new BindableReactiveProperty<bool>(true);
        IsComa = new BindableReactiveProperty<bool>(false);
        IsTab = new BindableReactiveProperty<bool>(false);
    }

    public SeparatorViewModel(
        ILoggerFactory loggerFactory,
        ObservableList<PacketMessageViewModel> packetsList
    )
        : base(ViewModelId)
    {
        _logger = loggerFactory.CreateLogger<SeparatorViewModel>();
        _packetsList = packetsList ?? throw new ArgumentNullException(nameof(packetsList));

        FilePath = new BindableReactiveProperty<string>(string.Empty);
        IsSemicolon = new BindableReactiveProperty<bool>(true);
        IsComa = new BindableReactiveProperty<bool>(false);
        IsTab = new BindableReactiveProperty<bool>(false);

        _sub1 = FilePath.EnableValidation(
            value =>
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception("Путь к файлу обязателен")
                    );
                }

                if (!Directory.Exists(Path.GetDirectoryName(value)))
                {
                    return ValueTask.FromResult<ValidationResult>(
                        new Exception("Указанная папка не существует")
                    );
                }

                return ValidationResult.Success;
            },
            this,
            true
        );

        _sub2 = IsSemicolon.Subscribe(value =>
        {
            if (value)
            {
                IsComa.Value = false;
                IsTab.Value = false;
            }
        });

        _sub3 = IsComa.Subscribe(value =>
        {
            if (value)
            {
                IsSemicolon.Value = false;
                IsTab.Value = false;
            }
        });

        _sub4 = IsTab.Subscribe(value =>
        {
            if (value)
            {
                IsSemicolon.Value = false;
                IsComa.Value = false;
            }
        });
    }

    public BindableReactiveProperty<string> FilePath { get; }
    public BindableReactiveProperty<bool> IsSemicolon { get; }
    public BindableReactiveProperty<bool> IsComa { get; }
    public BindableReactiveProperty<bool> IsTab { get; }

    private async Task ExportToCsvAsync(IProgress<double> progress, CancellationToken cancel)
    {
        try
        {
            var separator = ";";
            var shieldSymbol = ",";

            if (IsComa.Value)
            {
                separator = ",";
                shieldSymbol = ";";
            }
            else if (IsTab.Value)
            {
                separator = "\t";
                shieldSymbol = ",";
            }

            var fullPath = FilePath.Value;

            CsvHelper.SaveToCsv(
                _packetsList,
                fullPath,
                separator,
                shieldSymbol,
                new CsvColumn<PacketMessageViewModel>("Date", x => x.DateTime.ToString("G")),
                new CsvColumn<PacketMessageViewModel>("Type", x => x.Type),
                new CsvColumn<PacketMessageViewModel>("Source", x => x.Source),
                new CsvColumn<PacketMessageViewModel>("Message", x => x.Message)
            );

            _logger.LogInformation("Файл сохранен по пути: {0}", fullPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении файла по пути: {0}", FilePath.Value);
            throw;
        }
    }

    public void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        _sub5 = IsValid.Subscribe(isValid =>
        {
            dialog.IsPrimaryButtonEnabled = isValid;
        });

        dialog.PrimaryButtonCommand = new ReactiveCommand<IProgress<double>>(
            async (p, ct) => await ExportToCsvAsync(p, ct)
        );
    }

    public override IEnumerable<IRoutable> GetRoutableChildren() => [];

    #region Dispose

    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;
    private readonly IDisposable _sub4;
    private IDisposable _sub5;

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
            _sub4.Dispose();
            _sub5?.Dispose();
        }

        base.Dispose(isDisposing);
    }

    #endregion
}
