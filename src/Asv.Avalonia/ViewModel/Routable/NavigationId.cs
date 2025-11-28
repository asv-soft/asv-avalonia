using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Asv.IO;
using DotNext;
using Newtonsoft.Json;

namespace Asv.Avalonia;

/// <summary>
/// Represents a unique identifier for navigation, consisting of a type identifier and optional arguments.
/// </summary>
public readonly partial struct NavigationId
    : IEquatable<NavigationId>,
        IComparable<NavigationId>,
        ISizedSpanSerializable,
        IJsonSerializable
{
    #region Generation

    private const string AllowedCharacters =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-.";

    public static string GenerateRandomAsString(int length = 16) =>
        Random.Shared.NextString(AllowedCharacters, length);

    public static NavigationId GenerateRandom(int length = 16) => GenerateRandomAsString(length);

    public static string GenerateByHashAsString<T1>(T1 value1) =>
        HashCode.Combine(value1).ToString();

    public static string GenerateByHashAsString<T1, T2>(T1 value1, T2 value2) =>
        HashCode.Combine(value1, value2).ToString();

    public static string GenerateByHashAsString<T1, T2, T3>(T1 value1, T2 value2, T3 value3) =>
        HashCode.Combine(value1, value2, value3).ToString();

    public static string GenerateByHashAsString<T1, T2, T3, T4>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4
    ) => HashCode.Combine(value1, value2, value3, value4).ToString();

    public static string GenerateByHashAsString<T1, T2, T3, T4, T5>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5
    ) => HashCode.Combine(value1, value2, value3, value4, value5).ToString();

    public static NavigationId GenerateByHash<T1>(T1 value1) =>
        new(GenerateByHashAsString(value1), (string?)null);

    public static NavigationId GenerateByHash<T1, T2>(T1 value1, T2 value2) =>
        new(GenerateByHashAsString(value1, value2), (string?)null);

    public static NavigationId GenerateByHash<T1, T2, T3>(T1 value1, T2 value2, T3 value3) =>
        new(GenerateByHashAsString(value1, value2, value3), (string?)null);

    public static NavigationId GenerateByHash<T1, T2, T3, T4>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4
    ) => new(GenerateByHashAsString(value1, value2, value3, value4), (string?)null);

    public static NavigationId GenerateByHash<T1, T2, T3, T4, T5>(
        T1 value1,
        T2 value2,
        T3 value3,
        T4 value4,
        T5 value5
    ) => new(GenerateByHashAsString(value1, value2, value3, value4, value5), (string?)null);

    #endregion

    private const string TypeIdRegexString = "^[a-zA-Z0-9\\._\\-]+$";

    [GeneratedRegex(TypeIdRegexString, RegexOptions.Compiled)]
    private static partial Regex CreateRegex();

    private static readonly Regex TypeIdRegex = CreateRegex();

    public const char Separator = '?';
    public static readonly NavigationId Empty = new();

    [GeneratedRegex(@"[^\w]")]
    private static partial Regex MyRegex();

    private static readonly Regex IdNormalizeRegex = MyRegex();

    public static string NormalizeTypeId(string id) => IdNormalizeRegex.Replace(id, "_");

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationId"/> struct with a type identifier and optional arguments.
    /// </summary>
    /// <param name="typeId">The type identifier, which must contain only Latin letters, dots, and hyphens, and cannot be null.</param>
    /// <param name="args">The optional arguments associated with the navigation identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeId"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="typeId"/> contains invalid characters.</exception>
    public NavigationId(string typeId, string? args)
    {
        if (typeId == null)
        {
            throw new ArgumentNullException(nameof(typeId), "TypeId cannot be null.");
        }

        if (!TypeIdRegex.IsMatch(typeId))
        {
            throw new ArgumentException(
                $"{nameof(typeId)} must contain only Latin letters, dots, and hyphens.",
                nameof(typeId)
            );
        }

        Id = typeId;
        Args = args;
    }

    public NavigationId(string typeId, NameValueCollection args)
        : this(typeId, CreateArgs(args)) { }

    public NavigationId(string value)
    {
        Parse(value, out var typeId, out var args);
        Id = typeId;
        Args = args;
    }

    public NavigationId(ref ReadOnlySpan<byte> buffer)
    {
        Id = BinSerialize.ReadString(ref buffer);
        Args = BinSerialize.ReadBool(ref buffer) ? BinSerialize.ReadString(ref buffer) : null;

        if (!TypeIdRegex.IsMatch(Id))
        {
            throw new ArgumentException(
                $"{nameof(Id)} must contain only Latin letters, dots, and hyphens.",
                nameof(Id)
            );
        }
    }

    public NavigationId(JsonReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        var str = reader.ReadAsString();
        if (str == null)
        {
            throw new ArgumentNullException(
                nameof(reader),
                "String value expected in JSON reader."
            );
        }

        Parse(str, out var typeId, out var args);
        Id = typeId;
        Args = args;
    }

    /// <summary>
    /// Implicitly converts a string to a <see cref="NavigationId"/> instance by parsing it into a type identifier and optional arguments.
    /// The string should be in the format "typeId?args", where "typeId" must contain only Latin letters, dots, and hyphens.
    /// </summary>
    /// <param name="value">The string to convert, which cannot be null.</param>
    /// <returns>A new <see cref="NavigationId"/> instance parsed from the string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the type identifier part of <paramref name="value"/> contains invalid characters.</exception>
    public static implicit operator NavigationId(string value)
    {
        return new NavigationId(value);
    }

    public static void Parse(string? value, out string id, out string? args)
    {
        if (value == null)
        {
            id = string.Empty;
            args = null;
            return;
        }

        var separatorIndex = value.IndexOf(Separator);
        if (separatorIndex == -1)
        {
            // If there is no separator, the whole string is typeId
            id = value;
            args = null;
            return;
        }

        id = value[..separatorIndex];
        args = separatorIndex < value.Length - 1 ? value[(separatorIndex + 1)..] : null;
    }

    public static NameValueCollection ParseArgs(string? args)
    {
        return string.IsNullOrWhiteSpace(args)
            ? new NameValueCollection()
            : HttpUtility.ParseQueryString(args);
    }

    public static string CreateArgs(params IEnumerable<KeyValuePair<string, string>> args)
    {
        var sb = new StringBuilder();
        foreach (var kvp in args)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }

            sb.Append(HttpUtility.UrlEncode(kvp.Key));
            sb.Append('=');
            sb.Append(HttpUtility.UrlEncode(kvp.Value));
        }

        return sb.ToString();
    }

    public static string CreateArgs(IReadOnlyDictionary<string, string> args)
    {
        var sb = new StringBuilder();
        foreach (var kvp in args)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }

            sb.Append(HttpUtility.UrlEncode(kvp.Key));
            sb.Append('=');
            sb.Append(HttpUtility.UrlEncode(kvp.Value));
        }

        return sb.ToString();
    }

    public static string CreateArgs(NameValueCollection args)
    {
        if (args.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();
        foreach (var key in args.AllKeys)
        {
            if (sb.Length > 0)
            {
                sb.Append('&');
            }

            sb.Append(HttpUtility.UrlEncode(key));
            sb.Append('=');
            sb.Append(HttpUtility.UrlEncode(args[key] ?? string.Empty));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets the type identifier of the navigation.
    /// </summary>
    public string Id { get; } = string.Empty;

    /// <summary>
    /// Gets the optional arguments associated with the navigation identifier.
    /// </summary>
    public string? Args { get; } = null;

    #region IEquatable

    /// <summary>
    /// Compares two <see cref="NavigationId"/> instances for equality using case-insensitive comparison.
    /// </summary>
    /// <param name="left">The first <see cref="NavigationId"/> to compare.</param>
    /// <param name="right">The second <see cref="NavigationId"/> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(NavigationId left, NavigationId right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="NavigationId"/> instances for inequality using case-insensitive comparison.
    /// </summary>
    /// <param name="left">The first <see cref="NavigationId"/> to compare.</param>
    /// <param name="right">The second <see cref="NavigationId"/> to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(NavigationId left, NavigationId right) => !left.Equals(right);

    /// <summary>
    /// Determines whether the current <see cref="NavigationId"/> is equal to another <see cref="NavigationId"/> instance,
    /// ignoring case differences in both <see cref="Id"/> and <see cref="Args"/>.
    /// </summary>
    /// <param name="other">The <see cref="NavigationId"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(NavigationId other)
    {
        return string.Equals(Id, other.Id, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(Args, other.Args, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="NavigationId"/> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="NavigationId"/> and equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is NavigationId other && Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current <see cref="NavigationId"/> instance, computed using case-insensitive comparison.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode()
    {
        var hashCode = default(HashCode);
        hashCode.Add(Id, StringComparer.InvariantCultureIgnoreCase);
        hashCode.Add(Args, StringComparer.InvariantCultureIgnoreCase);
        return hashCode.ToHashCode();
    }

    #endregion

    #region IComparable

    /// <summary>
    /// Compares the current instance with another <see cref="NavigationId"/> instance and returns an integer that indicates
    /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the other instance.
    /// Comparison is performed first by <see cref="Id"/> and then by <see cref="Args"/>, ignoring case differences.
    /// </summary>
    /// <param name="other">The <see cref="NavigationId"/> to compare with the current instance.</param>
    /// <returns>
    /// A value less than zero if this instance precedes <paramref name="other"/>,
    /// zero if this instance equals <paramref name="other"/>,
    /// or a value greater than zero if this instance follows <paramref name="other"/> in the sort order.
    /// </returns>
    public int CompareTo(NavigationId other)
    {
        // First, compare the StaticId
        int idComparison = StringComparer.InvariantCultureIgnoreCase.Compare(Id, other.Id);
        if (idComparison != 0)
        {
            return idComparison;
        }

        // If the Ids are equal, compare Args
        if (Args == null && other.Args == null)
        {
            return 0; // Both null — equal
        }

        if (Args == null)
        {
            return -1; // null is less than non-null
        }

        if (other.Args == null)
        {
            return 1; // non-null is greater than null
        }

        return StringComparer.InvariantCultureIgnoreCase.Compare(Args, other.Args);
    }

    #endregion

    /// <summary>
    /// Creates a new <see cref="NavigationId"/> instance with the same <see cref="Id"/> but updated arguments.
    /// </summary>
    /// <param name="args">The new arguments to associate with the navigation identifier.</param>
    /// <returns>A new <see cref="NavigationId"/> instance with updated arguments.</returns>
    public NavigationId ChangeArgs(string? args) => new(Id, args);

    public StringBuilder AppendTo(StringBuilder sb)
    {
        sb.Append(Id);
        if (Args == null)
        {
            return sb;
        }

        sb.Append(Separator);
        sb.Append(Args);
        return sb;
    }

    /// <summary>
    /// Returns a string representation of the current <see cref="NavigationId"/> instance.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        if (Args == null)
        {
            return Id;
        }

        return $"{Id}{Separator}{Args}";
    }

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        throw new NotImplementedException(
            "This is readonly struct. Use constructor with ReadOnlySpan<byte> parameter instead"
        );
    }

    public void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WriteString(ref buffer, Id);
        if (Args != null)
        {
            BinSerialize.WriteBool(ref buffer, true);
            BinSerialize.WriteString(ref buffer, Args);
        }
        else
        {
            BinSerialize.WriteBool(ref buffer, false);
        }
    }

    public int GetByteSize()
    {
        return BinSerialize.GetSizeForString(Id)
            + sizeof(bool) // for the bool indicating Args presence
            + (Args != null ? BinSerialize.GetSizeForString(Args) : 0);
    }

    public void Serialize(JsonWriter writer)
    {
        writer.WriteValue(ToString());
    }

    public void Deserialize(JsonReader reader)
    {
        throw new NotImplementedException(
            "This is readonly struct. Use constructor with ReadOnlySpan<byte> parameter instead"
        );
    }
}
