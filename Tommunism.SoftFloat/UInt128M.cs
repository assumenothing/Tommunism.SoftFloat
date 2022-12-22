#region Copyright
/*============================================================================

This is a C# port of the SoftFloat library release 3e by Thomas Kaiser (2022).
The copyright from the original source code is listed below.

This C source file is part of the SoftFloat IEEE Floating-Point Arithmetic
Package, Release 3e, by John R. Hauser.

Copyright 2011, 2012, 2013, 2014, 2015, 2016, 2017, 2018 The Regents of the
University of California.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

 1. Redistributions of source code must retain the above copyright notice,
    this list of conditions, and the following disclaimer.

 2. Redistributions in binary form must reproduce the above copyright notice,
    this list of conditions, and the following disclaimer in the documentation
    and/or other materials provided with the distribution.

 3. Neither the name of the University nor the names of its contributors may
    be used to endorse or promote products derived from this software without
    specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS "AS IS", AND ANY
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE, ARE
DISCLAIMED.  IN NO EVENT SHALL THE REGENTS OR CONTRIBUTORS BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

=============================================================================*/
#endregion

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tommunism.SoftFloat;

// NOTE: It looks like .NET 7 added support for 128-bit integers. But we have a few specialiazation functionss that might not be so easy to
// do with the new integer type.

[StructLayout(LayoutKind.Sequential, Pack = sizeof(ulong), Size = sizeof(ulong) * 2)]
internal struct UInt128M : IEquatable<UInt128M>, IComparable<UInt128M>
{
    #region Fields

    public static readonly UInt128M Zero = new();
    public static readonly UInt128M One = new(0x0000000000000000, 0x0000000000000001);
    public static readonly UInt128M MinValue = new(0x0000000000000000, 0x0000000000000000);
    public static readonly UInt128M MaxValue = new(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);

    public ulong V00;
    public ulong V64;

    #endregion

    #region Constructors

    public UInt128M(ulong v64, ulong v0) => (V00, V64) = (v0, v64);

    #endregion

    #region Properties

    public bool IsZero => (V00 | V64) == 0;

    #endregion

    #region Methods

    public int CompareTo(UInt128M other)
    {
        if (other.V64 < V64) return 1;
        if (V64 < other.V64) return -1;
        if (other.V00 < V00) return 1;
        if (V00 < other.V00) return -1;
        return 0;
    }

    public void Deconstruct(out ulong v64, out ulong v0) => (v64, v0) = (V64, V00);

    public bool Equals(UInt128M other) => this == other;

    public override bool Equals(object? obj) => obj is UInt128M int128 && Equals(int128);

    public override int GetHashCode() => HashCode.Combine(V00, V64);

    public static UInt128M Multiply(ulong a, ulong b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a * b;
#else
        SFUInt128 z;

        var a32 = (uint)(a >> 32);
        var a0 = (uint)a;

        var b32 = (uint)(b >> 32);
        var b0 = (uint)b;

        z.V00 = (ulong)a0 * b0;
        var mid1 = (ulong)a32 * b0;
        var mid = mid1 + (ulong)a0 * b32;
        z.V64 = (ulong)a32 * b32;

        z.V64 += (mid < mid1 ? (1UL << 32) : 0UL) | (mid >> 32);
        mid <<= 32;
        z.V00 += mid;
        z.V64 += z.V00 < mid ? 1UL : 0UL;
        return z;
#endif
    }

    public static UInt128M Multiply64ByShifted32(ulong a, uint b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a * ((ulong)b << 32);
#else
        var mid = (ulong)(uint)a * b;
        return new SFUInt128(
            (ulong)(uint)(a >> 32) * b + (mid >> 32),
            mid << 32
        );
#endif
    }

    public UInt128M ShiftRightJam(int dist)
    {
        Debug.Assert(dist > 0, "Shift amount is out of range.");

#if NET7_0_OR_GREATER
        if (dist >= 128)
            return !IsZero ? One : Zero;

        var a = (UInt128)this;
        return (a >> dist) | ((a << -dist) != 0 ? UInt128.One : UInt128.Zero);
#else
        if (dist < 64)
        {
            var negDist = -dist;
            return new SFUInt128(
                V64 >> dist,
                (V64 << negDist) | (V00 >> dist) | ((V00 << negDist) != 0 ? 1UL : 0UL)
            );
        }
        else
        {
            return new SFUInt128(
                0,
                (dist < 127)
                    ? (V64 >> dist) | (((V64 & ((1UL << dist) - 1)) | V00) != 0 ? 1UL : 0UL)
                    : (!IsZero ? 1UL : 0UL)
            );
        }
#endif
    }

    public UInt128M ShortShiftRightJam(int dist)
    {
        Debug.Assert(dist is > 0 and < 64, "Shift amount is out of range.");

#if NET7_0_OR_GREATER
        var a = (UInt128)this;
        return (a >> dist) | (((ulong)a << (-dist)) != 0 ? UInt128.One : UInt128.Zero);
#else
        var negDist = -dist;
        return new SFUInt128(
            V64 >> dist,
            (V64 << negDist) | (V00 >> dist) | ((V00 << negDist) != 0 ? 1UL : 0UL)
        );
#endif
    }

    public override string ToString() => $"0x{V64:x16}{V00:x16}";

    public static explicit operator UInt128M(ulong value) => new(0, value);

    public static explicit operator ulong(UInt128M value) => value.V00;

#if NET7_0_OR_GREATER
    public static implicit operator UInt128(UInt128M value) => new(value.V64, value.V00);

    public static implicit operator UInt128M(UInt128 value) => new(value.GetUpperUI64(), value.GetLowerUI64());
#endif

    public static bool operator ==(UInt128M a, UInt128M b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a == (UInt128)b;
#else
        return (a.V64 == b.V64) && (a.V00 == b.V00);
#endif
    }

    public static bool operator !=(UInt128M a, UInt128M b) => !(a == b);

    public static bool operator <(UInt128M a, UInt128M b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a < (UInt128)b;
#else
        return (a.V64 < b.V64) || ((a.V64 == b.V64) && (a.V00 < b.V00));
#endif
    }

    public static bool operator >(UInt128M a, UInt128M b) => b < a;

    public static bool operator <=(UInt128M a, UInt128M b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a <= (UInt128)b;
#else
        return (a.V64 < b.V64) || ((a.V64 == b.V64) && (a.V00 <= b.V00));
#endif
    }

    public static bool operator >=(UInt128M a, UInt128M b) => b <= a;

    public static UInt128M operator <<(UInt128M a, int dist)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a << dist;
#else
        // An out of range shift is fine, internally C# requires 32-bit shifts are ANDed by 63 anyways.
        return new SFUInt128(
            (a.V64 << dist) | (a.V00 >> -dist),
            a.V00 << dist
        );
#endif
    }

    public static UInt128M operator >>>(UInt128M a, int dist) => a >> dist;

    public static UInt128M operator >>(UInt128M a, int dist)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a >> dist;
#else
        // An out of range shift is fine, internally C# requires 64-bit shifts are ANDed by 63 anyways.
        return new SFUInt128(
            a.V64 >> dist,
            (a.V64 << -dist) | (a.V00 >> dist)
        );
#endif
    }

    public static UInt128M operator +(UInt128M a, UInt128M b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a + (UInt128)b;
#else
        SFUInt128 z;
        z.V00 = a.V00 + b.V00;
        z.V64 = a.V64 + b.V64 + (z.V00 < a.V00 ? 1UL : 0UL);
        return z;
#endif
    }

    public static UInt128M operator +(UInt128M a, ulong b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a + b;
#else
        SFUInt128 z;
        z.V00 = a.V00 + b;
        z.V64 = a.V64 + (z.V00 < a.V00 ? 1UL : 0UL);
        return z;
#endif
    }

    public static UInt128M operator -(UInt128M a, UInt128M b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a - (UInt128)b;
#else
        SFUInt128 z;
        z.V00 = a.V00 - b.V00;
        z.V64 = a.V64 - b.V64 - (a.V00 < b.V00 ? 1UL : 0UL);
        return z;
#endif
    }

    public static UInt128M operator -(UInt128M a, ulong b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a - b;
#else
        SFUInt128 z;
        z.V00 = a.V00 - b;
        z.V64 = a.V64 - (a.V00 < b ? 1UL : 0UL);
        return z;
#endif
    }

    public static UInt128M operator -(UInt128M a)
    {
#if NET7_0_OR_GREATER
        return -(UInt128)a;
#else
        SFUInt128 z;
        z.V00 = 0 - a.V00;
        z.V64 = 0 - a.V64 - (0 < a.V00 ? 1UL : 0UL);
        return z;
#endif
    }

    public static UInt128M operator *(UInt128M a, uint b)
    {
#if NET7_0_OR_GREATER
        return (UInt128)a * b;
#else
        SFUInt128 z;
        z.V00 = a.V00 * b;
        var mid = (ulong)(uint)(a.V00 >> 32) * b;
        var carry = (uint)(z.V00 >> 32) - (uint)mid;
        z.V64 = a.V64 * b + (uint)((mid + carry) >> 32);
        return z;
#endif
    }

    public static UInt128M operator --(UInt128M a) => a - One;

    #endregion
}
