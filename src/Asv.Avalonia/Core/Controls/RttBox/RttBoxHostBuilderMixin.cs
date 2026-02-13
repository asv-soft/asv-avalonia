namespace Asv.Avalonia;

public static class RttBoxHostBuilderMixin
{
    public static AppHostControls.Builder RegisterRttBox(this AppHostControls.Builder builder)
    {
        return builder
            .RegisterViewFor<KeyValueRttBoxViewModel, KeyValueRttBoxView>()
            .RegisterViewFor<SingleRttBoxViewModel, SingleRttBoxView>()
            .RegisterViewFor<SplitDigitRttBoxViewModel, SplitDigitRttBoxView>()
            .RegisterViewFor<TwoColumnRttBoxViewModel, TwoColumnRttBoxView>()
            .RegisterViewFor<GeoPointRttBoxViewModel, GeoPointRttBoxView>();
    }
}
