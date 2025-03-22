using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Asv.Common;
using Asv.Mavlink;
using Asv.Mavlink.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.Example;

public class ParamItemViewModelConfig
{
    public bool IsStarred { get; set; }
    public string Name { get; set; }
    public bool IsPinned { get; set; }
}

public class ParamItemViewModel : RoutableViewModel // TODO: Сделать все свойства и команды рутбл
{
    private readonly ILogger _log;
    private readonly ParamItem _paramItem;
    private bool _internalUpdate;

    public ParamItemViewModel()
        : base("param_item") // use base class instead of ParamItem, because there is no way to create an empty Param item
    {
        DesignTime.ThrowIfNotDesignMode();

        // var t = new ParamItem();
        Name = "param" + Guid.NewGuid();
        DisplayName = Name;
        Description = "Design description";
        ValueDescription = "Value design description";
        IsRebootRequired = true;
    }

    public ParamItemViewModel(
        NavigationId id,
        ParamItem paramItem,
        ILoggerFactory log,
        ParamsConfig config
    )
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(paramItem);
        ArgumentNullException.ThrowIfNull(log);
        ArgumentNullException.ThrowIfNull(config);

        _log = log.CreateLogger<ParamItemViewModel>();
        _paramItem = paramItem;
        Name = paramItem.Name;
        DisplayName = paramItem.Info.DisplayName ?? string.Empty;
        Units = paramItem.Info.Units ?? string.Empty;
        Description = paramItem.Info.Description ?? string.Empty;
        ValueDescription = paramItem.Info.UnitsDisplayName ?? string.Empty;
        IsRebootRequired = paramItem.Info.IsRebootRequired;
        var fromCfg = config.Params.FirstOrDefault(p => p.Name == Name);

        IsUpdate = new BindableReactiveProperty<bool>();
        IsWriting = new BindableReactiveProperty<bool>();
        IsPinned = new BindableReactiveProperty<bool>();
        IsStarred = new BindableReactiveProperty<bool>(fromCfg?.IsStarred ?? false);
        Value = new BindableReactiveProperty<string>();
        StarKind = new BindableReactiveProperty<MaterialIconKind>();
        IsSynced = new BindableReactiveProperty<bool>();

        PinItem = new ReactiveCommand(_ => IsPinned.Value = !IsPinned.Value);

        _sub = paramItem.IsSynced.AsObservable().Subscribe(_ => IsSynced.Value = _);

        _sub1 = paramItem.Value.Subscribe(param =>
        {
            if (_internalUpdate)
            {
                return;
            }

            Value.Value = param.Type switch
            {
                MavParamType.MavParamTypeUint8 => ((byte)param).ToString(),
                MavParamType.MavParamTypeInt8 => ((sbyte)param).ToString(),
                MavParamType.MavParamTypeUint16 => ((ushort)param).ToString(),
                MavParamType.MavParamTypeInt16 => ((short)param).ToString(),
                MavParamType.MavParamTypeUint32 => ((uint)param).ToString(),
                MavParamType.MavParamTypeInt32 or MavParamType.MavParamTypeInt64 => (
                    (int)param
                ).ToString(),
                MavParamType.MavParamTypeUint64 => ((ulong)param).ToString(),
                MavParamType.MavParamTypeReal32 => ((float)param).ToString(
                    CultureInfo.InvariantCulture
                ),
                MavParamType.MavParamTypeReal64 => ((double)param).ToString(
                    CultureInfo.InvariantCulture
                ),
                _ => Value.Value,
            };
        });

        _sub2 = Value.Subscribe(val =>
        {
            _internalUpdate = true;
            switch (paramItem.Type)
            {
                case MavParamType.MavParamTypeUint8:
                {
                    if (byte.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeInt8:
                {
                    if (sbyte.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeUint16:
                {
                    if (ushort.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeInt16:
                {
                    if (short.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeUint32:
                {
                    if (uint.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeInt32:
                {
                    if (int.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeUint64:
                {
                    if (ulong.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeInt64:
                {
                    if (long.TryParse(val, out var result))
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                case MavParamType.MavParamTypeReal32:
                case MavParamType.MavParamTypeReal64:
                {
                    if (
                        float.TryParse(
                            val.Replace(",", "."),
                            CultureInfo.InvariantCulture,
                            out var result
                        )
                    )
                    {
                        paramItem.Value.OnNext(result);
                    }

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            _internalUpdate = false;
        });

        Write = new ReactiveCommand(
            async (_, cancel) =>
            {
                try
                {
                    IsWriting.Value = true;
                    await paramItem.Write(cancel);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Write {Name} error", Name);
                }
                finally
                {
                    IsWriting.Value = false;
                }
            }
        );

        Update = new ReactiveCommand(
            async (_, cancel) =>
            {
                try
                {
                    IsUpdate.Value = true;
                    await paramItem.Read(cancel);
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Read {Name} error", Name);
                }
                finally
                {
                    IsUpdate.Value = false;
                }
            }
        );

        _sub3 = IsStarred.Subscribe(isStarted =>
            StarKind.Value = isStarted ? MaterialIconKind.Star : MaterialIconKind.StarBorder
        );

        _sub4 = IsStarred.Subscribe(_ =>
        {
            var existItem = config.Params.FirstOrDefault(__ => __.Name == Name);

            if (existItem != null)
            {
                config.Params.Remove(existItem);
            }

            config.Params.Add(
                new ParamItemViewModelConfig
                {
                    IsStarred = IsStarred.CurrentValue,
                    IsPinned = IsPinned.CurrentValue,
                    Name = Name,
                }
            );
        });
    }

    public string Name { get; }

    public string DisplayName { get; set; }
    public string Units { get; set; }
    public ReactiveCommand Update { get; set; }
    public ReactiveCommand Write { get; }
    public ReactiveCommand PinItem { get; }
    public string ValueDescription { get; }
    public string Description { get; }
    public bool IsRebootRequired { get; }
    public BindableReactiveProperty<bool> IsPinned { get; }
    public BindableReactiveProperty<bool> IsSynced { get; }
    public BindableReactiveProperty<string> Value { get; }
    public BindableReactiveProperty<bool> IsStarred { get; }
    private BindableReactiveProperty<bool> IsWriting { get; }
    private BindableReactiveProperty<bool> IsUpdate { get; }
    public BindableReactiveProperty<MaterialIconKind> StarKind { get; }

    public bool Filter(string searchText, bool starredOnly)
    {
        if (starredOnly)
        {
            if (!IsStarred.Value)
            {
                return false;
            }
        }

        if (searchText.IsNullOrWhiteSpace())
        {
            return true;
        }

        return Name.Contains(searchText, StringComparison.InvariantCultureIgnoreCase);
    }

    public ParamItemViewModelConfig GetConfig()
    {
        return new ParamItemViewModelConfig
        {
            IsStarred = IsStarred.Value,
            IsPinned = IsPinned.Value,
            Name = Name,
        };
    }

    public void SetConfig(ParamItemViewModelConfig item)
    {
        IsStarred.Value = item.IsStarred;
        IsPinned.Value = item.IsPinned;
    }

    public async Task WriteParamData(CancellationToken cancel)
    {
        await _paramItem.Write(cancel);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    #region Dispose

    private readonly IDisposable _sub;
    private readonly IDisposable _sub1;
    private readonly IDisposable _sub2;
    private readonly IDisposable _sub3;
    private readonly IDisposable _sub4;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub.Dispose();
            _sub1.Dispose();
            _sub2.Dispose();
            _sub3.Dispose();
            _sub4.Dispose();
            _paramItem.Dispose();
            IsSynced.Dispose();
            IsStarred.Dispose();
            IsPinned.Dispose();
            Value.Dispose();
            Update.Dispose();
            Write.Dispose();
            PinItem.Dispose();
            IsWriting.Dispose();
            IsUpdate.Dispose();
            StarKind.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion
}
