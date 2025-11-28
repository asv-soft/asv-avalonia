using System.Collections.Specialized;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Xunit;

namespace Asv.Avalonia.Test;

[TestSubject(typeof(NavigationId))]
public class NavigationIdTest
{
    [Fact]
    public void Constructor_ValidTypeId_NoArgs()
    {
        var id = new NavigationId("Test.Type");
        Assert.Equal("Test.Type", id.Id);
        Assert.Null(id.Args);
    }

    [Fact]
    public void Constructor_ValidTypeId_WithArgs()
    {
        var id = new NavigationId("Test.Type", "param1");
        Assert.Equal("Test.Type", id.Id);
        Assert.Equal("param1", id.Args);
    }

    [Fact]
    public void ExplicitConstructor_ValidTypeId_WithArgs()
    {
        NavigationId nav = "Test.Type?param1=sdsd";
        Assert.Equal("Test.Type", nav.Id);
        Assert.Equal("param1=sdsd", nav.Args);
    }

    [Fact]
    public void Constructor_Throws_OnInvalidTypeId()
    {
        Assert.Throws<ArgumentException>(() => new NavigationId("ТестРуc", "x"));
        Assert.Throws<ArgumentException>(() => new NavigationId("a!b", "x"));
    }

    [Theory]
    [InlineData("type1", "type1", null)]
    [InlineData("type2?hello", "type2", "hello")]
    [InlineData("some.type-3?arg", "some.type-3", "arg")]
    [InlineData("foo-bar", "foo-bar", null)]
    [InlineData("id?", "id", null)]
    public void Parse_String_Works(string str, string expId, string? expArgs)
    {
        NavigationId.Parse(str, out var id, out var args);
        Assert.Equal(expId, id);
        Assert.Equal(expArgs, args);

        var navi = (NavigationId)str;
        Assert.Equal(expId, navi.Id);
        Assert.Equal(expArgs, navi.Args);
    }

    [Fact]
    public void ImplicitOperator_StringToNavigationId_Works()
    {
        NavigationId id = "typeX?args";
        Assert.Equal("typeX", id.Id);
        Assert.Equal("args", id.Args);
    }

    [Fact]
    public void ToString_And_AppendTo_AreConsistent()
    {
        var id = new NavigationId("abc.def", "zzz");
        Assert.Equal("abc.def?zzz", id.ToString());

        var sb = new StringBuilder();
        id.AppendTo(sb);
        Assert.Equal("abc.def?zzz", sb.ToString());
    }

    [Fact]
    public void ChangeArgs_CreatesNewInstanceWithArgs()
    {
        var id = new NavigationId("id");
        var changed = id.ChangeArgs("x");
        Assert.Equal("id", changed.Id);
        Assert.Equal("x", changed.Args);
    }

    [Fact]
    public void Equals_And_Comparison_AreCaseInsensitive()
    {
        var id1 = new NavigationId("abc", "xYz");
        var id2 = new NavigationId("ABC", "XyZ");
        Assert.True(id1 == id2);
        Assert.False(id1 != id2);
        Assert.True(id1.Equals(id2));
        Assert.Equal(0, id1.CompareTo(id2));
    }

    [Fact]
    public void GetHashCode_CaseInsensitive()
    {
        var id1 = new NavigationId("aBc", "Arg");
        var id2 = new NavigationId("AbC", "aRG");
        Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void CompareTo_SortsCorrectly()
    {
        var a = new NavigationId("a");
        var b = new NavigationId("b");
        var a1 = new NavigationId("a", "1");
        Assert.True(a.CompareTo(b) < 0);
        Assert.True(a.CompareTo(a1) < 0);
        Assert.True(a1.CompareTo(a) > 0);
    }

    [Fact]
    public void NormalizeTypeId_ReplacesNonWord()
    {
        var norm = NavigationId.NormalizeTypeId("abc!@#$%def-_.");
        Assert.Equal("abc_____def___", norm);
    }

    [Fact]
    public void Serialization_Binary_Roundtrip()
    {
        var id = new NavigationId("TestType", "SomeArgs");
        var size = id.GetByteSize();
        var buffer = new byte[size];
        var wSpan = new Span<byte>(buffer);
        id.Serialize(ref wSpan);

        var readBuffer = buffer.ToArray();
        var span = new ReadOnlySpan<byte>(readBuffer);
        var id2 = new NavigationId(ref span);
        Assert.Equal(id, id2);
    }

    [Fact]
    public void Serialization_Json_Roundtrip()
    {
        var id = new NavigationId("jsonType", "arg1");
        var sw = new StringWriter();
        using (var writer = new JsonTextWriter(sw))
        {
            id.Serialize(writer);
        }
        var json = sw.ToString();
        using var reader = new JsonTextReader(new StringReader(json));
        var id2 = new NavigationId(reader);
        Assert.Equal(id, id2);
    }

    [Fact]
    public void JsonReader_ThrowsIfNotString()
    {
        var sr = new StringReader("123");
        using var reader = new JsonTextReader(sr);
        reader.Read();
        Assert.Throws<ArgumentNullException>(() => new NavigationId(reader));
    }

    [Fact]
    public void Constructor_WithNameValueCollection_CreatesCorrectArgs()
    {
        var args = new NameValueCollection
        {
            { "key1", "value1" },
            { "key2", "value with space" },
            { "empty", "" },
        };

        var id = new NavigationId("nav.test", args);
        var parsedArgs = NavigationId.ParseArgs(id.Args);

        Assert.Equal("nav.test", id.Id);
        Assert.Equal("value1", parsedArgs["key1"]);
        Assert.Equal("value with space", parsedArgs["key2"]);
        Assert.Equal("", parsedArgs["empty"]);
    }

    [Fact]
    public void ParseArgs_ParsesQueryStringCorrectly()
    {
        var query = "a=1&b=hello%20world&c=";
        var args = NavigationId.ParseArgs(query);

        Assert.Equal("1", args["a"]);
        Assert.Equal("hello world", args["b"]);
        Assert.Equal("", args["c"]);
    }

    [Fact]
    public void CreateArgs_EncodesNameValueCollectionProperly()
    {
        var nvc = new NameValueCollection
        {
            { "k1", "v1" },
            { "key with space", "value with space" },
            { "empty", "" },
        };

        var query = NavigationId.CreateArgs(nvc);
        Assert.Contains("k1=v1", query);
        Assert.Contains("key+with+space=value+with+space", query);
        Assert.Contains("empty=", query);
    }

    [Fact]
    public void CreateArgs_EmptyOrNullCollection_ReturnsEmptyString()
    {
        var emptyArgs = new NameValueCollection();
        Assert.Equal(string.Empty, NavigationId.CreateArgs(emptyArgs));

        NameValueCollection? nullArgs = null;
        Assert.Throws<NullReferenceException>(() => NavigationId.CreateArgs(nullArgs!));
    }

    [Fact]
    public void ArgsSerialization_RoundTrip_Works()
    {
        var input = new NameValueCollection { { "alpha", "beta" }, { "gamma", "delta" } };

        var argsStr = NavigationId.CreateArgs(input);
        var output = NavigationId.ParseArgs(argsStr);

        Assert.Equal("beta", output["alpha"]);
        Assert.Equal("delta", output["gamma"]);
    }
}
