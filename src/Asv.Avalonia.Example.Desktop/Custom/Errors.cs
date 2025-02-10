using System;
using System.Collections.Generic;

namespace Asv.Avalonia.Example.Desktop.Custom;

public class Errors
{
    public static readonly Dictionary<string, Func<Exception, ErrorResponse>> Errs = new()
    {
        {
            nameof(CustomEx),
            _ => new ErrorResponse // TODO: сделать просто строкой
            {
                Detail = "Чет кастомное", // TODO: локализовать
            }
        },
    };
}
