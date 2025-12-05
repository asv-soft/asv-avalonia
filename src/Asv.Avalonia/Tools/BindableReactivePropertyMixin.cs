using Asv.Common;
using R3;

namespace Asv.Avalonia;

public static class BindableReactivePropertyMixin
{
    public static IDisposable EnableValidationRoutable<T>(
        this BindableReactiveProperty<T> prop,
        Func<T, ValidationResult> validationFunc,
        IRoutable source,
        bool isForceValidation = false
    )
    {
        prop.EnableValidation();

        if (isForceValidation)
        {
            prop.ForceValidate();
        }

        return prop.SubscribeAwait(
            async (v, _) =>
            {
                var result = validationFunc(v);
                if (!result.IsSuccess)
                {
                    prop.OnErrorResume(
                        result.ValidationException?.GetExceptionWithLocalization()
                            ?? result.ValidationException
                            ?? new Exception(RS.ValidationException_Unknown_Message)
                    );
                }

                await source.Rise(new ValidationEvent(source, prop, result));
            }
        );
    }
}
