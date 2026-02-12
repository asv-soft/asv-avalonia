namespace Asv.Avalonia;

public static class RttBoxHostBuilderMixin
{
    public static ControlsHostBuilder.Builder RttBox(this ControlsHostBuilder.Builder builder)
    {
        return builder
            .RegisterViewFor<KeyValueRttBoxViewModel, KeyValueRttBoxView>()
            .RegisterViewFor<SingleRttBoxViewModel, SingleRttBoxView>()
            .RegisterViewFor<SplitDigitRttBoxViewModel, SplitDigitRttBoxView>()
            .RegisterViewFor<TwoColumnRttBoxViewModel, TwoColumnRttBoxView>()
            .RegisterViewFor<GeoPointRttBoxViewModel, GeoPointRttBoxView>();
    }
}