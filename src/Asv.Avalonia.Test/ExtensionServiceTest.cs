using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R3;
using Xunit;

namespace Asv.Avalonia.Test;

public class ExtensionServiceTest
{
    [Fact]
    public void Extend_PolicyFiltersExtensions_AppliesFilteredExtensions()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransient<IExtensionFor<TestContext>, FirstExtension>();
        services.AddTransient<IExtensionFor<TestContext>, SecondExtension>();
        services.AddTransient<IExtensionPolicyFor<TestContext>, RemoveSecondPolicyFor>();
        using var provider = services.BuildServiceProvider();
        var service = new ExtensionService(provider, provider.GetRequiredService<ILoggerFactory>());
        using var disposable = new CompositeDisposable();
        var context = new TestContext();

        service.Extend(context, "test", disposable);

        Assert.Equal(["first"], context.Items);
    }

    [Fact]
    public void Extend_PolicyFiltersDisposableExtension_DisposesFilteredExtensionImmediately()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransient<IExtensionFor<TestContext>, FirstExtension>();
        services.AddTransient<IExtensionFor<TestContext>, DisposableSecondExtension>();
        services.AddTransient<IExtensionPolicyFor<TestContext>, RemoveSecondPolicyFor>();
        using var provider = services.BuildServiceProvider();
        var service = new ExtensionService(provider, provider.GetRequiredService<ILoggerFactory>());
        using var disposable = new CompositeDisposable();
        var context = new TestContext();

        service.Extend(context, "test", disposable);

        Assert.Equal(1, DisposableSecondExtension.DisposeCount);
    }

    [Fact]
    public void Extend_PoliciesAreAppliedByOrder_AppliesOrderedPolicyResult()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransient<IExtensionFor<TestContext>, FirstExtension>();
        services.AddTransient<IExtensionFor<TestContext>, SecondExtension>();
        services.AddTransient<IExtensionPolicyFor<TestContext>, TakeFirstPolicyFor>();
        services.AddTransient<IExtensionPolicyFor<TestContext>, ReversePolicyFor>();
        using var provider = services.BuildServiceProvider();
        var service = new ExtensionService(provider, provider.GetRequiredService<ILoggerFactory>());
        using var disposable = new CompositeDisposable();
        var context = new TestContext();

        service.Extend(context, "test", disposable);

        Assert.Equal(["second"], context.Items);
    }

    private sealed class TestContext
    {
        public List<string> Items { get; } = [];
    }

    private sealed class FirstExtension : IExtensionFor<TestContext>
    {
        public const string StaticId = "ext.test.first";

        string ISupportId<string>.Id => StaticId;

        public void Extend(TestContext context, CompositeDisposable contextDispose)
        {
            context.Items.Add("first");
        }
    }

    private interface ISecondExtension;

    private sealed class SecondExtension : IExtensionFor<TestContext>, ISecondExtension
    {
        public const string StaticId = "ext.test.second";

        string ISupportId<string>.Id => StaticId;

        public void Extend(TestContext context, CompositeDisposable contextDispose)
        {
            context.Items.Add("second");
        }
    }

    private sealed class DisposableSecondExtension
        : IExtensionFor<TestContext>,
            ISecondExtension,
            IDisposable
    {
        public const string StaticId = "ext.test.second-disposable";

        public static int DisposeCount { get; private set; }

        string ISupportId<string>.Id => StaticId;

        public DisposableSecondExtension()
        {
            DisposeCount = 0;
        }

        public void Extend(TestContext context, CompositeDisposable contextDispose)
        {
            context.Items.Add("second");
        }

        public void Dispose()
        {
            DisposeCount++;
        }
    }

    private sealed class RemoveSecondPolicyFor : IExtensionPolicyFor<TestContext>
    {
        public const string StaticId = "policy.test.remove-second";

        public int Order => 0;

        string ISupportId<string>.Id => StaticId;

        public IEnumerable<IExtensionFor<TestContext>> Filter(
            TestContext context,
            IEnumerable<IExtensionFor<TestContext>> extensions
        )
        {
            return extensions.Where(extension => extension is not ISecondExtension);
        }
    }

    private sealed class ReversePolicyFor : IExtensionPolicyFor<TestContext>
    {
        public const string StaticId = "policy.test.reverse";

        public int Order => 1;

        string ISupportId<string>.Id => StaticId;

        public IEnumerable<IExtensionFor<TestContext>> Filter(
            TestContext context,
            IEnumerable<IExtensionFor<TestContext>> extensions
        )
        {
            return extensions.Reverse();
        }
    }

    private sealed class TakeFirstPolicyFor : IExtensionPolicyFor<TestContext>
    {
        public const string StaticId = "policy.test.take-first";

        public int Order => 2;

        string ISupportId<string>.Id => StaticId;

        public IEnumerable<IExtensionFor<TestContext>> Filter(
            TestContext context,
            IEnumerable<IExtensionFor<TestContext>> extensions
        )
        {
            return extensions.Take(1);
        }
    }
}
