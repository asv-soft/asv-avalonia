using Asv.Common;
using Asv.Modeling;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public sealed class ValidationEventRoutingTest
{
    [Fact]
    public async Task NestedValidationError_UpdatesContainingDialog()
    {
        using var dialog = new TestDialogViewModel();
        using var intermediate = new TestChildViewModel();
        using var leaf = new LeafViewModel();

        intermediate.SetParent(dialog);
        leaf.SetParent(intermediate);

        var seen = new TaskCompletionSource<ValidationEvent>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var eventSubscription = leaf.Events.Catch<ValidationEvent>(e => seen.TrySetResult(e));

        var becameInvalid = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var validitySubscription = dialog.IsValid.Subscribe(v =>
        {
            if (!v)
            {
                becameInvalid.TrySetResult();
            }
        });

        leaf.Value.Value = string.Empty;

        var evt = await seen.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await becameInvalid.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(leaf.Value.HasErrors);
        Assert.False(dialog.IsValid.Value);
        Assert.True(evt.IsHandled);
    }

    [Fact]
    public async Task ValidationOnDialogSource_UpdatesIsValid()
    {
        using var dialog = new SelfValidatingDialogViewModel();

        await WaitForIsValidAsync(dialog, false, () => dialog.Name.Value = string.Empty);

        Assert.True(dialog.Name.HasErrors);
        Assert.False(dialog.IsValid.Value);
    }

    [Fact]
    public async Task ValidationOnChildSource_BubblesToDialog()
    {
        using var dialog = new TestDialogViewModel();
        using var leaf = new LeafViewModel();
        leaf.SetParent(dialog);

        var evt = await AwaitValidationAsync(leaf, () => leaf.Value.Value = string.Empty);
        await WaitForIsValidAsync(dialog, false);

        Assert.True(evt.IsHandled);
        Assert.True(leaf.Value.HasErrors);
        Assert.False(dialog.IsValid.Value);
    }

    [Fact]
    public async Task TwoInvalidProperties_CorrectedOneAtATime_UpdateIsValid()
    {
        using var dialog = new TwoFieldDialogViewModel();

        Assert.True(dialog.Name.HasErrors);
        Assert.True(dialog.Code.HasErrors);
        Assert.False(dialog.IsValid.Value);

        // Correcting only one field must not enable the dialog while the other is still invalid.
        await WaitForErrorsClearedAsync(dialog.Name, () => dialog.Name.Value = "ok");

        Assert.False(dialog.Name.HasErrors);
        Assert.True(dialog.Code.HasErrors);
        Assert.False(dialog.IsValid.Value);

        // Correcting the second field enables the dialog.
        await WaitForIsValidAsync(dialog, true, () => dialog.Code.Value = "ok");

        Assert.False(dialog.Name.HasErrors);
        Assert.False(dialog.Code.HasErrors);
        Assert.True(dialog.IsValid.Value);
    }

    [Fact]
    public async Task HandledValidation_DoesNotReachAncestorAboveDialog()
    {
        using var ancestor = new RecordingViewModel();
        using var dialog = new TestDialogViewModel();
        using var leaf = new LeafViewModel();

        dialog.SetParent(ancestor);
        leaf.SetParent(dialog);

        var evt = await AwaitValidationAsync(leaf, () => leaf.Value.Value = string.Empty);
        await WaitForIsValidAsync(dialog, false);

        Assert.True(evt.IsHandled);
        Assert.Empty(ancestor.Received);
    }

    private static async Task<ValidationEvent> AwaitValidationAsync(
        IViewModel source,
        Action trigger
    )
    {
        var tcs = new TaskCompletionSource<ValidationEvent>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var subscription = source.Events.Catch<ValidationEvent>(e => tcs.TrySetResult(e));
        trigger();
        return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static Task WaitForIsValidAsync(DialogViewModelBase dialog, bool expected) =>
        WaitForIsValidAsync(dialog, expected, null);

    private static async Task WaitForIsValidAsync(
        DialogViewModelBase dialog,
        bool expected,
        Action? trigger
    )
    {
        if (trigger is null && dialog.IsValid.Value == expected)
        {
            return;
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = dialog.IsValid.Subscribe(v =>
        {
            if (v == expected)
            {
                tcs.TrySetResult();
            }
        });
        trigger?.Invoke();
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }

    private static async Task WaitForErrorsClearedAsync(
        IBindableReactiveProperty property,
        Action trigger
    )
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        System.EventHandler<System.ComponentModel.DataErrorsChangedEventArgs> handler = (_, _) =>
        {
            if (!property.HasErrors)
            {
                tcs.TrySetResult();
            }
        };
        property.ErrorsChanged += handler;
        try
        {
            trigger();
            await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            property.ErrorsChanged -= handler;
        }
    }

    private sealed class TestDialogViewModel : DialogViewModelBase
    {
        public TestDialogViewModel()
            : base("test-dialog") { }
    }

    private sealed class TestChildViewModel : ViewModel
    {
        public TestChildViewModel()
            : base("test-child") { }
    }

    private sealed class RecordingViewModel : ViewModel
    {
        private readonly IDisposable _subscription;

        public RecordingViewModel()
            : base("test-ancestor")
        {
            _subscription = Events.Catch<ValidationEvent>(e => Received.Add(e));
        }

        public List<ValidationEvent> Received { get; } = [];

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscription.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    private sealed class LeafViewModel : ViewModel
    {
        private readonly IDisposable _validation;

        public LeafViewModel()
            : base("test-leaf")
        {
            Value = new BindableReactiveProperty<string>("initial");
            _validation = Value.EnableValidationRoutable(Validate, this);
        }

        public BindableReactiveProperty<string> Value { get; }

        private static ValidationResult Validate(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? ValidationResult.FailAsNullOrWhiteSpace
                : ValidationResult.Success;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _validation.Dispose();
                Value.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    private sealed class SelfValidatingDialogViewModel : DialogViewModelBase
    {
        private readonly IDisposable _validation;

        public SelfValidatingDialogViewModel()
            : base("test-self-dialog")
        {
            Name = new BindableReactiveProperty<string>("initial");
            _validation = Name.EnableValidationRoutable(Validate, this);
        }

        public BindableReactiveProperty<string> Name { get; }

        private static ValidationResult Validate(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? ValidationResult.FailAsNullOrWhiteSpace
                : ValidationResult.Success;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _validation.Dispose();
                Name.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    private sealed class TwoFieldDialogViewModel : DialogViewModelBase
    {
        private readonly IDisposable _nameValidation;
        private readonly IDisposable _codeValidation;

        public TwoFieldDialogViewModel()
            : base("test-two-field-dialog")
        {
            Name = new BindableReactiveProperty<string>(string.Empty);
            Code = new BindableReactiveProperty<string>(string.Empty);
            _nameValidation = Name.EnableValidationRoutable(
                Validate,
                this,
                isForceValidation: true
            );
            _codeValidation = Code.EnableValidationRoutable(
                Validate,
                this,
                isForceValidation: true
            );
        }

        public BindableReactiveProperty<string> Name { get; }

        public BindableReactiveProperty<string> Code { get; }

        private static ValidationResult Validate(string value) =>
            string.IsNullOrWhiteSpace(value)
                ? ValidationResult.FailAsNullOrWhiteSpace
                : ValidationResult.Success;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nameValidation.Dispose();
                _codeValidation.Dispose();
                Name.Dispose();
                Code.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
