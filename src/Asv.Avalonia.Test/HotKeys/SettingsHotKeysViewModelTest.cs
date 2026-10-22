using Asv.Cfg;
using Asv.Modeling;
using Avalonia.Input;
using Xunit;

namespace Asv.Avalonia.Test;

public class SettingsHotKeysViewModelTest
{
    private static SettingsHotKeysViewModel Create(IHotKeyService service) =>
        new(new TestContext(), service, NullDialogService.Instance);

    private sealed class TestContext : ITreeSubPageContext<ISettingsPage>
    {
        public NavArgs Args => default;
        public ISettingsPage Context => throw new NotSupportedException();
    }

    [Fact]
    public async Task SearchAndFilters_FindCommandsGesturesModifiedAndUnassigned()
    {
        await using var service = HotKeyServiceTest.Create(new InMemoryConfiguration());
        using var editor = Create(service);
        var item = editor.Items.Single(x => x.Action.ActionId == SaveAction.Id);
        editor.SearchText = item.Action.Name;
        Assert.Same(item, Assert.Single(editor.Items));
        editor.SearchText = "Ctrl + Shift + K";
        Assert.Empty(editor.Items);
        Assert.True(editor.TryAssign(item, new(Key.K, KeyModifiers.Control | KeyModifiers.Shift)));
        Assert.Same(item, Assert.Single(editor.Items));
        editor.SearchText = null;
        editor.FilterIndex = 1;
        Assert.Same(item, Assert.Single(editor.Items));
        item.ClearCommand.Execute(default);
        editor.FilterIndex = 2;
        Assert.Same(item, Assert.Single(editor.Items));
        Assert.False(item.IsAssigned);
        editor.FilterIndex = 3;
        Assert.Empty(editor.Items);
    }

    [Fact]
    public async Task TryAssign_ConflictRequiresExplicitReplacementAndApplyPersistsBothChanges()
    {
        await using var service = HotKeyServiceTest.Create(new InMemoryConfiguration());
        using var editor = Create(service);
        var open = editor.Items.Single(x => x.Action.ActionId == SaveAction.Id);
        var refresh = editor.Items.Single(x => x.Action.ActionId == RefreshAction.Id);
        var original = open.Gesture;
        Assert.False(editor.TryAssign(open, refresh.Gesture));
        Assert.Equal(original, open.Gesture);
        Assert.False(editor.HasChanges);
        Assert.True(editor.TryAssign(open, refresh.Gesture, replaceConflicts: true));
        Assert.False(refresh.IsAssigned);
        Assert.Equal(Key.F5, open.Gesture.Key);
        Assert.Equal(original, service[SaveAction.Id]);
        Assert.True(editor.CanApply);
        editor.ApplyCommand.Execute(default);
        Assert.Equal(Key.F5, service[SaveAction.Id]!.Key);
        Assert.Equal(Key.None, service[RefreshAction.Id]!.Key);
        Assert.False(editor.HasChanges);
    }

    [Fact]
    public async Task CancelAndResetAll_KeepDraftSeparateFromSavedAssignments()
    {
        await using var service = HotKeyServiceTest.Create(new InMemoryConfiguration());
        var custom = new KeyGesture(Key.K, KeyModifiers.Alt);
        service[SaveAction.Id] = custom;
        using var editor = Create(service);
        editor.ResetAllCommand.Execute(default);
        Assert.True(editor.HasChanges);
        Assert.Equal(custom, service[SaveAction.Id]);
        editor.CancelCommand.Execute(default);
        Assert.False(editor.HasChanges);
        Assert.Equal(custom, editor.Items.Single(x => x.Action.ActionId == SaveAction.Id).Gesture);
        editor.ResetAllCommand.Execute(default);
        editor.ApplyCommand.Execute(default);
        Assert.Equal(new SaveAction().DefaultHotKey, service[SaveAction.Id]);
    }

    [Fact]
    public async Task ExistingConflicts_AreFilterableAndBlockApplyUntilResolved()
    {
        await using var service = HotKeyServiceTest.Create(new InMemoryConfiguration());
        service[SaveAction.Id] = new(Key.F5);
        using var editor = Create(service);
        Assert.True(editor.HasConflicts);
        editor.FilterIndex = 3;
        Assert.Equal(2, editor.Items.Count);
        Assert.All(editor.Items, item => Assert.True(item.HasConflict));
        editor.Items.Single(x => x.Action.ActionId == SaveAction.Id).ClearCommand.Execute(default);
        Assert.Empty(editor.Items);
        Assert.False(editor.HasConflicts);
        Assert.True(editor.CanApply);
    }

    [Fact]
    public async Task Dispose_DisposesRowsAndRejectsFurtherEdits()
    {
        await using var service = HotKeyServiceTest.Create(new InMemoryConfiguration());
        var editor = Create(service);
        var row = editor.Items[0];
        editor.Dispose();
        Assert.True(row.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => editor.TryAssign(row, new(Key.K)));
    }
}
