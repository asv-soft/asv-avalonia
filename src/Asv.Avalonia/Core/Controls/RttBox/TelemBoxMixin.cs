namespace Asv.Avalonia;

public static class RttBoxMixin
{
    public static ControlsHostBuilder RttBox(this ControlsHostBuilder builder)
    {
        return builder
            .Register<KeyValueRttBoxViewModel, KeyValueRttBoxView>()
            .Register<SingleRttBoxViewModel, SingleRttBoxView>()
            .Register<SplitDigitRttBoxViewModel, SplitDigitRttBoxView>()
            .Register<TwoColumnRttBoxViewModel, TwoColumnRttBoxView>();
    }
}