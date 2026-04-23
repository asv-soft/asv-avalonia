using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public static class BindableReactivePropertyMixin
{
    public static IDisposable EnableValidationRoutable<T>(
        this BindableReactiveProperty<T> prop,
        Func<T, ValidationResult> validationFunc,
        IViewModel source,
        bool isForceValidation = false
    )
    {
        prop.EnableValidation();

        if (isForceValidation)
        {
            prop.ForceValidate();
        }

        return prop.SubscribeAwait(
            async (v, ct) =>
            {
                var result = validationFunc(v);
                if (!result.IsSuccess)
                {
                    prop.OnErrorResume(
                        result.ValidationException?.GetExceptionWithLocalizationOrSelf()
                            ?? new UnknownValidationException()
                    );
                }

                await source.Rise(new ValidationEvent(source, prop, result), ct);
            }
        );
    }
}
