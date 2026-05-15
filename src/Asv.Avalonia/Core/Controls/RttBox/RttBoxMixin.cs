namespace Asv.Avalonia;

public static class RttBoxMixin
{
    extension(ViewLocatorMixin.Builder builder)
    {
        public ViewLocatorMixin.Builder RegisterRttBox()
        {
            return builder
                .RegisterViewFor<KeyValueRttBoxViewModel, KeyValueRttBoxView>()
                .RegisterViewFor<SingleRttBoxViewModel, SingleRttBoxView>()
                .RegisterViewFor<SplitDigitRttBoxViewModel, SplitDigitRttBoxView>()
                .RegisterViewFor<TwoColumnRttBoxViewModel, TwoColumnRttBoxView>()
                .RegisterViewFor<GeoPointRttBoxViewModel, GeoPointRttBoxView>();
        }
    }
}
