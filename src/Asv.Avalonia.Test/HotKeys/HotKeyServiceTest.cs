using Asv.Cfg;
using Avalonia.Input;
using Microsoft.Extensions.Logging.Abstractions;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class HotKeyServiceTest
{
    internal static HotKeyService Create(InMemoryConfiguration config) =>
        new(
            new ShellHost(),
            config,
            [new SaveAction(), new RefreshAction()],
            NullLoggerFactory.Instance
        );

    [Fact]
    public async Task DisabledAssignment_RemovesMenuHintAndRestoresIt()
    {
        await using var service = Create(new InMemoryConfiguration());
        service[SaveAction.Id] = new(Key.None);
        using var menu = new MenuItem("save-test", "Save");
        menu.BindHotKey(service, SaveAction.Id);
        Assert.Null(menu.HotKey);
        service[SaveAction.Id] = null;
        Assert.Equal(new SaveAction().DefaultHotKey, menu.HotKey);
        service[SaveAction.Id] = new(Key.None);
        Assert.Null(menu.HotKey);
    }

    [Fact]
    public async Task DisabledAssignment_RoundTripsAndNullRestoresDefault()
    {
        var config = new InMemoryConfiguration();
        await using (var service = Create(config))
        {
            service[SaveAction.Id] = new KeyGesture(Key.None);
        }

        await using var reopened = Create(config);
        Assert.Equal(Key.None, reopened[SaveAction.Id]!.Key);
        reopened[SaveAction.Id] = null;
        Assert.Equal(new SaveAction().DefaultHotKey, reopened[SaveAction.Id]);
        Assert.DoesNotContain(SaveAction.Id, config.Get<HotKeyServiceConfig>().HotKeys.Keys);
    }

    [Fact]
    public async Task ApplyHotKeys_InvalidActionDoesNotChangeAnyAssignment()
    {
        var config = new InMemoryConfiguration();
        await using var service = Create(config);
        Assert.Throws<KeyNotFoundException>(() =>
            service.ApplyHotKeys(
                new Dictionary<string, KeyGesture?>
                {
                    [SaveAction.Id] = new(Key.K, KeyModifiers.Control),
                    ["missing"] = new(Key.O),
                }
            )
        );
        Assert.Equal(new SaveAction().DefaultHotKey, service[SaveAction.Id]);
        Assert.Empty(config.Get<HotKeyServiceConfig>().HotKeys);
    }

    [Fact]
    public async Task ApplyHotKeys_EventsObserveCompleteBatchAndCustomValuesRoundTrip()
    {
        var config = new InMemoryConfiguration();
        await using var service = Create(config);
        var replacement = new KeyGesture(Key.K, KeyModifiers.Control);
        var observed = new List<KeyGesture?>();
        using var subscription = service.OnHotKeyGestureChanged.Subscribe(_ =>
            observed.Add(service[RefreshAction.Id])
        );
        service.ApplyHotKeys(
            new Dictionary<string, KeyGesture?>
            {
                [SaveAction.Id] = new(Key.None),
                [RefreshAction.Id] = replacement,
            }
        );
        Assert.Equal(2, observed.Count);
        Assert.All(observed, value => Assert.Equal(replacement, value));
        await using var reopened = Create(config);
        Assert.Equal(replacement, reopened[RefreshAction.Id]);
        Assert.Equal(Key.None, reopened[SaveAction.Id]!.Key);
    }
}
