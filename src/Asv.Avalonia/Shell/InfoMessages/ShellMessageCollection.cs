using Asv.Common;
using Asv.Modeling;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class ShellMessageCollection : ViewModel
{
    public const string TypeId = "messages";
    public const int MaxCount = 3;

    public ShellMessageCollection()
        : base(TypeId)
    {
        ItemsSource = [];
        ItemsView = ItemsSource.ToNotifyCollectionChangedSlim().AddTo(ref DisposableBag);
        ItemsSource.SetRoutableParent(this).AddTo(ref DisposableBag);
        ItemsSource.DisposeRemovedItems().AddTo(ref DisposableBag);
        ItemsSource
            .ObserveCountChanged()
            .Subscribe(_ =>
            {
                var last = ItemsSource.LastOrDefault();
                if (last is null)
                {
                    ErrorState = ShellErrorState.Normal;
                    return;
                }
                if (last.Severity == ShellErrorState.Error)
                {
                    ErrorState = ShellErrorState.Error;
                    return;
                }
                if (last.Severity == ShellErrorState.Warning)
                {
                    ErrorState = ShellErrorState.Warning;
                    return;
                }
                ErrorState = ShellErrorState.Normal;
            })
            .AddTo(ref DisposableBag);
        ItemsSource
            .ObserveAdd()
            .Subscribe(_ =>
            {
                if (ItemsSource.Count > MaxCount)
                {
                    ItemsSource.RemoveAt(0);
                }
            })
            .AddTo(ref DisposableBag);
    }

    public ShellErrorState ErrorState
    {
        get;
        set => SetField(ref field, value);
    }

    public ObservableList<ShellMessageViewModel> ItemsSource { get; }

    public NotifyCollectionChangedSynchronizedViewList<ShellMessageViewModel> ItemsView { get; }
}
