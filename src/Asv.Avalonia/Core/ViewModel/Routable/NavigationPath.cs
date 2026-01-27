using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Asv.IO;
using Newtonsoft.Json;

namespace Asv.Avalonia;

/// <summary>
/// Represents a list of <see cref="NavigationId"/> instances optimized for performance and memory usage.
/// Uses an inline array for small collections and switches to a dynamic array for larger ones.
/// </summary>
public struct NavigationPath : IEquatable<NavigationPath>, ISizedSpanSerializable, IJsonSerializable
{
    public const char Separator = '/';

    private const int InlineCapacity = 6;

    [InlineArray(InlineCapacity)]
    private struct InlineNavigationIds
    {
        public const int Length = InlineCapacity;
        private NavigationId _first;
    }

    private InlineNavigationIds _inlineIds;
    private int _count;
    private NavigationId[]? _overflowIds;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationPath"/> struct.
    /// </summary>
    /// <param name="ids">The initial set of <see cref="NavigationPath"/> items to add to the list.</param>
    public NavigationPath(ReadOnlySpan<NavigationId> ids = default)
    {
        _count = ids.Length;
        if (_count <= InlineCapacity)
        {
            ids.CopyTo(_inlineIds);
        }
        else
        {
            _overflowIds = new NavigationId[_count + InlineCapacity]; // Additional capacity for growth
            ids.CopyTo(_overflowIds);
        }
    }

    public NavigationPath(ref ReadOnlySpan<byte> buffer)
    {
        Deserialize(ref buffer);
    }

    public NavigationPath(JsonReader reader)
    {
        Deserialize(reader);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationPath"/> struct.
    /// </summary>
    /// <param name="ids">The enumerable collection of <see cref="NavigationId"/> items to add to the list.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ids"/> is null.</exception>
    public NavigationPath(params IEnumerable<NavigationId> ids)
    {
        if (ids == null)
        {
            throw new ArgumentNullException(
                nameof(ids),
                "The enumerable collection cannot be null."
            );
        }

        _count = 0;
        _overflowIds = null;

        // Check if we can get the length directly for the optimization
        if (ids.TryGetNonEnumeratedCount(out int count))
        {
            _count = count;
            if (_count <= InlineCapacity)
            {
                int index = 0;
                foreach (var id in ids)
                {
                    _inlineIds[index++] = id;
                }
            }
            else
            {
                _overflowIds = new NavigationId[_count + InlineCapacity];
                int index = 0;
                foreach (var id in ids)
                {
                    _overflowIds[index++] = id;
                }
            }
        }
        else
        {
            // If the length is unknown, put in the temporary list
            var tempList = new List<NavigationId>();
            foreach (var id in ids)
            {
                tempList.Add(id);
            }

            _count = tempList.Count;
            if (_count <= InlineCapacity)
            {
                tempList.CopyTo(_inlineIds);
            }
            else
            {
                _overflowIds = new NavigationId[_count + InlineCapacity];
                tempList.CopyTo(_overflowIds);
            }
        }
    }

    /// <summary>
    /// Gets the number of <see cref="NavigationId"/> items in the list.
    /// </summary>
    public readonly int Count => _count;

    /// <summary>
    /// Gets or sets the <see cref="NavigationId"/> at the specified index.
    /// </summary>
    /// <param name="index">The index of the item to get or set.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="index"/> is out of range.</exception>
    public NavigationId this[int index]
    {
        readonly get
        {
            if ((uint)index >= (uint)_count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range.");
            }

            return _overflowIds is null ? _inlineIds[index] : _overflowIds[index];
        }
        set
        {
            if ((uint)index >= (uint)_count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range.");
            }

            if (_overflowIds is null)
            {
                _inlineIds[index] = value;
            }
            else
            {
                _overflowIds[index] = value;
            }
        }
    }

    /// <summary>
    /// Adds a <see cref="NavigationId"/> to the list.
    /// </summary>
    /// <param name="id">The <see cref="NavigationId"/> to add.</param>
    public void Add(NavigationId id)
    {
        if (_count < InlineCapacity && _overflowIds is null)
        {
            _inlineIds[_count] = id;
            _count++;
        }
        else
        {
            AddToOverflow(id);
        }
    }

    private void AddToOverflow(NavigationId id)
    {
        if (_overflowIds is null)
        {
            _overflowIds = new NavigationId[InlineCapacity * 2]; // Double the initial capacity
            ((ReadOnlySpan<NavigationId>)_inlineIds).CopyTo(_overflowIds);
        }
        else if (_count == _overflowIds.Length)
        {
            Array.Resize(ref _overflowIds, _count + InlineCapacity);
        }

        _overflowIds[_count] = id;
        _count++;
    }

    /// <summary>
    /// Gets the span of <see cref="NavigationId"/> items in the list.
    /// </summary>
    /// <returns> A <see cref="ReadOnlySpan{T}"/> of <see cref="NavigationId"/> items.</returns>
    [UnscopedRef]
    public readonly ReadOnlySpan<NavigationId> AsSpan() =>
        _overflowIds is null
            ? ((ReadOnlySpan<NavigationId>)_inlineIds)[.._count]
            : _overflowIds.AsSpan(0, _count);

    #region IEquatable

    /// <summary>
    /// Compares two <see cref="left"/> instances for equality using case-insensitive comparison of their contents.
    /// </summary>
    /// <param name="left">The first <see cref="NavigationPath"/> to compare.</param>
    /// <param name="right">The second <see cref="NavigationPath"/> to compare.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(NavigationPath left, NavigationPath right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="left"/> instances for inequality using case-insensitive comparison of their contents.
    /// </summary>
    /// <param name="left">The first <see cref="NavigationPath"/> to compare.</param>
    /// <param name="right">The second <see cref="NavigationPath"/> to compare.</param>
    /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(NavigationPath left, NavigationPath right) =>
        !left.Equals(right);

    /// <summary>
    /// Determines whether the current <see cref="other"/> is equal to another <see cref="NavigationPath"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="NavigationPath"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the instances are equal; otherwise, <c>false</c>.</returns>
    public bool Equals(NavigationPath other)
    {
        if (_count != other._count)
        {
            return false;
        }

        var thisSpan = AsSpan();
        var otherSpan = other.AsSpan();

        for (int i = 0; i < _count; i++)
        {
            if (!thisSpan[i].Equals(otherSpan[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="obj"/> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the object is a <see cref="NavigationPath"/> and equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is NavigationPath other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current <see cref="NavigationPath"/> instance.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode()
    {
        var hash = default(HashCode);
        var span = AsSpan();
        for (int i = 0; i < _count; i++)
        {
            hash.Add(span[i]);
        }

        return hash.ToHashCode();
    }

    #endregion

    /// <summary>
    /// Creates a new <see cref="NavigationPath"/> containing a subset of the current path.
    /// </summary>
    /// <param name="start">The starting index of the subset (inclusive).</param>
    /// <param name="length">The number of items to include in the subset.</param>
    /// <returns>A new <see cref="NavigationPath"/> containing the specified subset.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="start"/> or <paramref name="length"/> is out of range.</exception>
    public NavigationPath Slice(int start, int length)
    {
        if (start < 0 || start > Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(start),
                $"Start index must be between 0 and {Count}."
            );
        }

        if (length < 0 || start + length > Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                $"Length must be non-negative and start + length must not exceed {Count}."
            );
        }

        return length == 0 ? default : new NavigationPath(AsSpan().Slice(start, length));
    }

    /// <summary>
    /// Gets a subrange of the current <see cref="NavigationPath"/> using a range.
    /// </summary>
    /// <param name="range">The range specifying the subset of the path.</param>
    /// <returns>A new <see cref="NavigationPath"/> containing the specified subset.</returns>
    public NavigationPath this[Range range]
    {
        get
        {
            var (offset, length) = range.GetOffsetAndLength(Count);
            return new NavigationPath(AsSpan().Slice(offset, length));
        }
    }

    /// <summary>
    /// Parses a string into a <see cref="NavigationPath"/>.
    /// The string should be in the format "id1?args1/id2?args2/id3?args3", where each segment is a valid <see cref="NavigationId"/>.
    /// </summary>
    /// <param name="path">The string to parse.</param>
    /// <returns>A new <see cref="NavigationPath"/> instance containing the parsed <see cref="NavigationId"/> items.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when the path format is invalid or contains invalid <see cref="NavigationId"/> segments.</exception>
    public static NavigationPath Parse(string path)
    {
        return new NavigationPath(ParseItems(path));
    }

    public static IEnumerable<NavigationId> ParseItems(string path)
    {
        if (path == null)
        {
            throw new ArgumentNullException(nameof(path), "The navigation path cannot be null.");
        }

        if (string.IsNullOrEmpty(path))
        {
            yield break;
        }

        var segments = path.Split(Separator, StringSplitOptions.RemoveEmptyEntries);
        foreach (var t in segments)
        {
            yield return t;
        }
    }

    /// <summary>
    /// Returns a string representation of the current <see cref="NavigationPath"/> instance using a <see cref="StringBuilder"/> for optimization.
    /// </summary>
    /// <returns>A string that represents the current path in the format "id1?args1/id2?args2/...".</returns>
    public override string ToString()
    {
        if (_count == 0)
        {
            return string.Empty;
        }

        var span = AsSpan();
        var sb = new StringBuilder();
        span[0].AppendTo(sb);

        for (var i = 1; i < _count; i++)
        {
            sb.Append(Separator);
            span[i].AppendTo(sb);
        }

        return sb.ToString();
    }

    public void Deserialize(ref ReadOnlySpan<byte> buffer)
    {
        var size = BinSerialize.ReadPackedUnsignedInteger(ref buffer);
        for (var i = 0; i < size; i++)
        {
            Add(new NavigationId(ref buffer));
        }
    }

    public void Serialize(ref Span<byte> buffer)
    {
        BinSerialize.WritePackedUnsignedInteger(ref buffer, (uint)Count);
        for (var i = 0; i < Count; i++)
        {
            this[i].Serialize(ref buffer);
        }
    }

    public int GetByteSize()
    {
        var size = BinSerialize.GetSizeForPackedUnsignedInteger((uint)Count);
        for (var i = 0; i < Count; i++)
        {
            size += this[i].GetByteSize();
        }

        return size;
    }

    public bool StartWith(NavigationPath other)
    {
        if (other.Count > Count)
        {
            return false;
        }

        var thisSpan = AsSpan();
        var otherSpan = other.AsSpan();

        for (int i = 0; i < other.Count; i++)
        {
            if (!thisSpan[i].Equals(otherSpan[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void Serialize(JsonWriter writer)
    {
        writer.WriteValue(ToString());
    }

    public void Deserialize(JsonReader reader)
    {
        foreach (var item in ParseItems(reader.ReadAsString() ?? string.Empty))
        {
            Add(item);
        }
    }
}
