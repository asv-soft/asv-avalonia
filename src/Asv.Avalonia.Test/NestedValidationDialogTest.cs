using Asv.Avalonia.Example;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public sealed class NestedValidationDialogTest
{
    [Fact]
    public async Task Fields_StartInvalidAndFollowCorrections()
    {
        using var viewModel = new NestedValidationDialogViewModel();

        await WaitForValidAsync(viewModel, false);

        Assert.True(viewModel.Name.HasErrors);
        Assert.True(viewModel.Details.Code.HasErrors);
        Assert.True(viewModel.Details.Field.Value.HasErrors);
        Assert.False(viewModel.IsValid.Value);

        // Filling Name and Code keeps the dialog invalid while Value stays empty.
        viewModel.Name.Value = "name";
        viewModel.Details.Code.Value = "code";
        Assert.True(viewModel.Details.Field.Value.HasErrors);
        Assert.False(viewModel.IsValid.Value);

        // Filling Value makes the dialog valid.
        viewModel.Details.Field.Value.Value = "value";
        await WaitForValidAsync(viewModel, true);
        Assert.True(viewModel.IsValid.Value);

        // Clearing Code makes it invalid again.
        viewModel.Details.Code.Value = string.Empty;
        await WaitForValidAsync(viewModel, false);
        Assert.False(viewModel.IsValid.Value);
    }

    private static async Task WaitForValidAsync(DialogViewModelBase viewModel, bool expected)
    {
        if (viewModel.IsValid.Value == expected)
        {
            return;
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = viewModel.IsValid.Subscribe(v =>
        {
            if (v == expected)
            {
                tcs.TrySetResult();
            }
        });
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }
}

[Collection(WorkspaceUiTestCollection.Name)]
public sealed class NestedValidationDialogUiTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public Task PrimaryButton_DisabledUntilValid_AndApplyDialogReplacesSubscription() =>
        Run(async () =>
        {
            using var viewModel = new NestedValidationDialogViewModel();

            var first = CreateDialog(viewModel);
            viewModel.ApplyDialog(first);
            Assert.False(first.IsPrimaryButtonEnabled);

            viewModel.Name.Value = "name";
            viewModel.Details.Code.Value = "code";
            viewModel.Details.Field.Value.Value = "value";
            await WaitForValidAsync(viewModel, true);
            Assert.True(first.IsPrimaryButtonEnabled);

            // Applying a second dialog replaces the previous subscription.
            var second = CreateDialog(viewModel);
            viewModel.ApplyDialog(second);
            Assert.True(second.IsPrimaryButtonEnabled);

            viewModel.Details.Code.Value = string.Empty;
            await WaitForValidAsync(viewModel, false);
            Assert.False(second.IsPrimaryButtonEnabled);
            Assert.True(first.IsPrimaryButtonEnabled);
        });

    private static ContentDialog CreateDialog(NestedValidationDialogViewModel viewModel) =>
        new(viewModel) { PrimaryButtonText = "Confirm", SecondaryButtonText = "Cancel" };

    private Task Run(Func<Task> action) =>
        fixture.Session.Dispatch(
            async () =>
            {
                await action();
                return true;
            },
            TestContext.Current.CancellationToken
        );

    private static async Task WaitForValidAsync(DialogViewModelBase viewModel, bool expected)
    {
        if (viewModel.IsValid.Value == expected)
        {
            return;
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var subscription = viewModel.IsValid.Subscribe(v =>
        {
            if (v == expected)
            {
                tcs.TrySetResult();
            }
        });
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
    }
}
