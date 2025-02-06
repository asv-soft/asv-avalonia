namespace Asv.Avalonia;

public readonly struct ValidationResult
{
    public required bool IsSuccess { get; init; }
    public bool IsFailed => !this.IsSuccess;
    public Exception ValidationException { get; init; }

    public static ValidationResult Create(Exception exc)
    {
        return new ValidationResult { IsSuccess = false, ValidationException = exc };
    }

    public static implicit operator ValidationResult(Exception exc)
    {
        return new ValidationResult { IsSuccess = false, ValidationException = exc };
    }

    public static implicit operator ValueTask<ValidationResult>(ValidationResult result)
    {
        return ValueTask.FromResult(result);
    }

    public static ValidationResult Success { get; } = new ValidationResult { IsSuccess = true };
}
