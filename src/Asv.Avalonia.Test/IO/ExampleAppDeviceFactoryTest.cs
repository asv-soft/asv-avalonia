using Asv.Avalonia.Example;
using Asv.IO;
using Asv.IO.Device;
using Xunit;

namespace Asv.Avalonia.Test.IO;

public class ExampleAppDeviceFactoryTest
{
    [Fact]
    public void Addresses_AllExampleMessagesMatchIdentityWithoutDiscovery()
    {
        var factory = new ExampleAppDeviceFactory(new ExampleDeviceConfig());
        Assert.True(factory.TryGetDeviceAddress(new ExampleDeviceId("example", 2), out var expected));
        ExampleMessageBase[] packets =
        [new ExampleMessage1 { SenderId = 2 }, new ExampleMessage2 { SenderId = 2 }, new ExampleMessage3 { SenderId = 2 }];
        foreach (var message in packets)
        {
            Assert.True(factory.TryGetSourceAddress(message, out var address));
            Assert.Equal(expected, address);
        }
        Assert.True(factory.TryGetSourceAddress(new ExampleMessage1 { SenderId = 1 }, out var different));
        Assert.NotEqual(expected, different);
    }

}
