using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Tommunism.SoftFloat;

using static Primitives;

// NOTE: Most spans, indexing, or enumeration in this implementation is highly dependent on the host endianness.

[StructLayout(LayoutKind.Sequential, Pack = sizeof(ulong), Size = sizeof(ulong) * 4)]
internal struct UInt256 : IEquatable<UInt256>, IReadOnlyList<ulong>
{
    #region Fields

    public static readonly UInt256 Zero = new();
    public static readonly UInt256 MinValue = new(0x0000000000000000, 0x0000000000000000, 0x0000000000000000, 0x0000000000000000);
    public static readonly UInt256 MaxValue = new(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);

    // The meaning of these fields are dependent on the host's endianness.
    internal ulong _v0, _v1, _v2, _v3;

    #endregion

    #region Constructors

    public UInt256(ulong v192, ulong v128, ulong v64, ulong v0) =>
        (_v0, _v1, _v2, _v3) = BitConverter.IsLittleEndian ? (v0, v64, v128, v192) : (v192, v128, v64, v0);

    public UInt256(UInt128 v128, UInt128 v0)
    {
        if (BitConverter.IsLittleEndian)
        {
            (_v1, _v0) = v0;
            (_v3, _v2) = v128;
        }
        else
        {
            (_v3, _v2) = v0;
            (_v1, _v0) = v128;
        }
    }

    internal UInt256(ReadOnlySpan<ulong> span)
    {
        Debug.Assert(span.Length >= 4, "Span is too small.");
        (_v0, _v1, _v2, _v3) = (span[0], span[1], span[2], span[3]);
    }

    public UInt256(ReadOnlySpan<ulong> span, bool isLittleEndian)
    {
        Debug.Assert(span.Length >= 4, "Span is too small.");
        (_v0, _v1, _v2, _v3) = isLittleEndian
            ? (span[0], span[1], span[2], span[3])
            : (span[3], span[2], span[1], span[0]);
    }

    #endregion

    #region Properties

    // NOTE: All V{000,064,128,192}* properties are host endian-specific.

    public ulong V000
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        get => BitConverter.IsLittleEndian ? _v0 : _v3;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (BitConverter.IsLittleEndian)
                _v0 = value;
            else
                _v3 = value;
        }
    }

    public ulong V064
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => BitConverter.IsLittleEndian ? _v1 : _v2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (BitConverter.IsLittleEndian)
                _v1 = value;
            else
                _v2 = value;
        }
    }

    public ulong V128
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        get => BitConverter.IsLittleEndian ? _v2 : _v1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (BitConverter.IsLittleEndian)
                _v2 = value;
            else
                _v1 = value;
        }
    }

    public ulong V192
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        get => BitConverter.IsLittleEndian ? _v3 : _v0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (BitConverter.IsLittleEndian)
                _v3 = value;
            else
                _v0 = value;
        }
    }

    public UInt128 V000_UI128
    {
        get => BitConverter.IsLittleEndian ? new UInt128(v0: _v0, v64: _v1) : new UInt128(v0: _v3, v64: _v2);
        set
        {
            if (BitConverter.IsLittleEndian)
                (_v1, _v0) = value;
            else
                (_v3, _v2) = value;
        }
    }

    public UInt128 V128_UI128
    {
        get => BitConverter.IsLittleEndian ? new UInt128(v0: _v2, v64: _v3) : new UInt128(v0: _v1, v64: _v0);
        set
        {
            if (BitConverter.IsLittleEndian)
                (_v3, _v2) = value;
            else
                (_v1, _v0) = value;
        }
    }

    public ulong this[int index]
    {
        get
        {
            Debug.Assert(index is >= 0 and < 4, "Index is out of range.");
            return index switch
            {
                0 => _v0,
                1 => _v1,
                2 => _v2,
                3 => _v3,
                // easier to inline without exception
                _ => 0 //throw new ArgumentOutOfRangeException(nameof(value))
            };
        }
        set
        {
            Debug.Assert(index is >= 0 and < 4, "Index is out of range.");
            switch (index)
            {
                case 0: _v0 = value; return;
                case 1: _v0 = value; return;
                case 2: _v0 = value; return;
                case 3: _v0 = value; return;
            }

            // easier to inline without exception
            //throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    int IReadOnlyCollection<ulong>.Count => 4;

    #endregion

    #region Methods

    // NOTE: This operation requires very careful layout of the fields and packing of this structure.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Span<ulong> AsSpan()
    {
        Debug.Assert(Unsafe.SizeOf<UInt256>() == sizeof(ulong) * 4);
        return MemoryMarshal.CreateSpan(ref _v0, 4);
    }

    public void Deconstruct(out ulong v192, out ulong v128, out ulong v64, out ulong v0) =>
        (v192, v128, v64, v0) = BitConverter.IsLittleEndian ? (_v3, _v2, _v1, _v0) : (_v0, _v1, _v2, _v3);

    public override bool Equals(object? obj) => obj is UInt256 int256 && Equals(int256);

    public bool Equals(UInt256 other) =>
        _v0 == other._v0 && _v1 == other._v1 && _v2 == other._v2 && _v3 == other._v3;

    public IEnumerator<ulong> GetEnumerator()
    {
        yield return _v0;
        yield return _v1;
        yield return _v2;
        yield return _v3;
    }

    public override int GetHashCode() => HashCode.Combine(_v0, _v1, _v2, _v3);

    public void ToSpan(Span<ulong> span)
    {
        Debug.Assert(span.Length >= 4, "Span is too small.");
        (span[0], span[1], span[2], span[3]) = (_v0, _v1, _v2, _v3);
    }

    public override string ToString() => BitConverter.IsLittleEndian
        ? $"0x{_v3:x16}{_v2:x16}{_v1:x16}{_v0:x16}"
        : $"0x{_v0:x16}{_v1:x16}{_v2:x16}{_v3:x16}";

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator UInt256(ulong value) => new(v0: value, v64: 0, v128: 0, v192: 0);

    public static implicit operator UInt256(UInt128 value) => new(v0: value, v128: UInt128.Zero);

    public static explicit operator ulong(UInt256 value) => value.V000;

    public static explicit operator UInt128(UInt256 value) => value.V000_UI128;

    public static bool operator ==(UInt256 left, UInt256 right) => left.Equals(right);

    public static bool operator !=(UInt256 left, UInt256 right) => !(left == right);

    public static UInt256 operator +(UInt256 left, UInt256 right)
    {
        Span<ulong> result = stackalloc ulong[4];
        Add256M(left.AsSpan(), right.AsSpan(), result);
        return new UInt256(result);
    }

    public static UInt256 operator -(UInt256 left, UInt256 right)
    {
        Span<ulong> result = stackalloc ulong[4];
        Sub256M(left.AsSpan(), right.AsSpan(), result);
        return new UInt256(result);
    }

    public static UInt256 operator -(UInt256 value)
    {
        Span<ulong> result = stackalloc ulong[4];
        Sub256M(Zero.AsSpan(), value.AsSpan(), result);
        return new UInt256(result);
    }

    #endregion
}
