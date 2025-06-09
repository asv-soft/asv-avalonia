using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Threading.Tasks;
using Asv.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public class SeparatorViewModel : DialogViewModelBase
{
    public const string ViewModelId = $"{PacketViewerViewModel.PageId}.dialog.separator";
    public const string DefaultSeparator = ";";
    public const string DefaultShieldSymbol = ",";

    [ImportingConstructor]
    public SeparatorViewModel()
        : base(ViewModelId)
    {
        IsSemicolon = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        IsComa = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsTab = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        Separator = new BindableReactiveProperty<string>(DefaultSeparator).DisposeItWith(
            Disposable
        );
        ShieldSymbol = new BindableReactiveProperty<string>(DefaultShieldSymbol).DisposeItWith(
            Disposable
        );

        _sub2 = IsSemicolon
            .Where(value => value)
            .Subscribe(_ =>
            {
                Separator.Value = DefaultSeparator;
                ShieldSymbol.Value = DefaultShieldSymbol;
            });

        _sub3 = IsComa
            .Where(value => value)
            .Subscribe(_ =>
            {
                Separator.Value = ",";
                ShieldSymbol.Value = ";";
            });

        _sub4 = IsTab
            .Where(value => value)
            .Subscribe(_ =>
            {
                Separator.Value = "\t";
                ShieldSymbol.Value = DefaultShieldSymbol;
            });
    }

    public BindableReactiveProperty<bool> IsSemicolon { get; }
    public BindableReactiveProperty<bool> IsComa { get; }
    public BindableReactiveProperty<bool> IsTab { get; }
    public BindableReactiveProperty<string> Separator { get; }
    public BindableReactiveProperty<string> ShieldSymbol { get; }

    public override void ApplyDialog(ContentDialog dialog)
    {
        ArgumentNullException.ThrowIfNull(dialog);

        _sub5 = IsValid.Subscribe(isValid =>
        {
            dialog.IsPrimaryButtonEnabled = isValid;
        });
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
            _sub5.Dispose();
        }

        base.Dispose(isDisposing);
    }

    #endregion
}
