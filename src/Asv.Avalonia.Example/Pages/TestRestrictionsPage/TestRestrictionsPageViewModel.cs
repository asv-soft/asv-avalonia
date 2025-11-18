using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia.Example;

[ExportPage(PageId)]
public class TestRestrictionsPageViewModel : PageViewModel<TestRestrictionsPageViewModel>
{
    public const string PageId = "test_restrictions";
    public const MaterialIconKind PageIcon = MaterialIconKind.Block;
    public const AsvColorKind PageIconColor = AsvColorKind.None;

    private readonly ReactiveProperty<bool> _isChanged;

    public TestRestrictionsPageViewModel()
        : this(DesignTime.CommandService, NullLoggerFactory.Instance, DesignTime.DialogService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public TestRestrictionsPageViewModel(
        ICommandService cmd,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(PageId, cmd, loggerFactory, dialogService)
    {
        Title = "Restrictions page";
        Icon = PageIcon;
        IconColor = PageIconColor;

        _isChanged = new ReactiveProperty<bool>(false).DisposeItWith(Disposable);
        _isChanged
            .Subscribe(hasChanges =>
            {
                if (hasChanges)
                {
                    Status = MaterialIconKind.Pencil;
                    StatusColor = AsvColorKind.Error;
                }
                else
                {
                    Status = null;
                    StatusColor = AsvColorKind.None;
                }
            })
            .DisposeItWith(Disposable);

        Input1 = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        Input2 = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);
        Input3 = new BindableReactiveProperty<string?>().DisposeItWith(Disposable);

        Input1.Subscribe(_ => OnDataChanged()).DisposeItWith(Disposable);
        Input2.Subscribe(_ => OnDataChanged()).DisposeItWith(Disposable);
        Input3.Subscribe(_ => OnDataChanged()).DisposeItWith(Disposable);
    }

    public BindableReactiveProperty<string?> Input1 { get; }

    public BindableReactiveProperty<string?> Input2 { get; }

    public BindableReactiveProperty<string?> Input3 { get; }

    private void OnDataChanged()
    {
        _isChanged.Value =
            !string.IsNullOrWhiteSpace(Input1.Value)
            || !string.IsNullOrWhiteSpace(Input2.Value)
            || !string.IsNullOrWhiteSpace(Input3.Value);
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        if (e is PageCloseAttemptEvent close)
        {
            if (!string.IsNullOrWhiteSpace(Input1.Value))
            {
                close.AddRestriction(new Restriction(this, "Input1 contains unsaved data"));
            }
            if (!string.IsNullOrWhiteSpace(Input2.Value))
            {
                close.AddRestriction(new Restriction(this, "Input2 contains unsaved data"));
            }
            if (!string.IsNullOrWhiteSpace(Input3.Value))
            {
                close.AddRestriction(new Restriction(this, "Input3 contains unsaved data"));
            }
        }
        return base.InternalCatchEvent(e);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}
