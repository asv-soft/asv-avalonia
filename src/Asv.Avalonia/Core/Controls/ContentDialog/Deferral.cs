namespace Asv.Avalonia;

public delegate ValueTask DeferralCompletedHandler();

/// <summary>
/// Stores a <see cref="DeferralCompletedHandler"/> to be invoked upon completion of the
/// deferral and manipulates the state of the deferral.
/// </summary>
public class Deferral
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Deferral"/> class.
    /// Specifies a <see cref="DeferralCompletedHandler"/> to be called
    /// upon completion of the deferral.
    /// </summary>
    /// <param name="completedHandler">A DeferralCompletedHandler to be called upon completion of the deferral.</param>
    public Deferral(DeferralCompletedHandler completedHandler)
    {
        _handler =
            completedHandler
            ?? throw new ArgumentNullException(
                nameof(completedHandler),
                @"Completion delegate cannot be null"
            );
    }

    /// <summary>
    /// If the DeferralCompletedHandler has not yet been invoked, this will call it and drop the reference to the delegate.
    /// </summary>
    public void Complete()
    {
        _handler.Invoke();
    }

    private readonly DeferralCompletedHandler _handler;
}
