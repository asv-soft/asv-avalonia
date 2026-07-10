namespace Asv.Avalonia;

public static class RttBoxRegistrations
{
    extension(ControlsRegistrations.Builder builder)
    {
        public ControlsRegistrations.Builder RegisterRttBox()
        {
            builder
                .AppBuilder.ViewLocator.RegisterViewFor<
                    KeyValueRttBoxViewModel,
                    KeyValueRttBoxView
                >()
                .RegisterViewFor<SingleRttBoxViewModel, SingleRttBoxView>()
                .RegisterViewFor<SplitDigitRttBoxViewModel, SplitDigitRttBoxView>()
                .RegisterViewFor<TwoColumnRttBoxViewModel, TwoColumnRttBoxView>()
                .RegisterViewFor<GeoPointRttBoxViewModel, GeoPointRttBoxView>();

            return builder;
        }
    }
}
