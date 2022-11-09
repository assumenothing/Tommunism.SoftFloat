﻿#region Copyright
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
using System.Runtime.InteropServices;

namespace Tommunism.SoftFloat;

using static Primitives;

// NOTE: It looks like .NET 7 will add builtin support for 128-bit integers. But we have a few specialiazation functionss that might not be
// so easy to do with the new integer type.

[StructLayout(LayoutKind.Sequential, Pack = sizeof(ulong), Size = sizeof(ulong) * 2)]
internal struct UInt128 : IEquatable<UInt128>, IComparable<UInt128>
{
    #region Fields

    public static readonly UInt128 Zero = new();
    public static readonly UInt128 MinValue = new(0x0000000000000000, 0x0000000000000000);
    public static readonly UInt128 MaxValue = new(0xFFFFFFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);

    public ulong V00;
    public ulong V64;

    #endregion

    #region Constructors

    public UInt128(ulong v0, ulong v64) => (V00, V64) = (v0, v64);

    #endregion

    #region Properties

    public bool IsZero => (V00 | V64) == 0;

    #endregion

    #region Methods

    public int CompareTo(UInt128 other)
    {
        if (other.V64 < V64) return 1;
        if (V64 < other.V64) return -1;
        if (other.V00 < V00) return 1;
        if (V00 < other.V00) return -1;
        return 0;
    }

    public void Deconstruct(out ulong v64, out ulong v0) => (v64, v0) = (V64, V00);

    public bool Equals(UInt128 other) => this == other;

    public override bool Equals(object? obj) => obj is UInt128 int128 && Equals(int128);

    public override int GetHashCode() => HashCode.Combine(V00, V64);

    public override string ToString() => $"0x{V64:x16}{V00:x16}";

    public static implicit operator UInt128(ulong value) => new(v0: value, v64: 0);

    public static explicit operator ulong(UInt128 value) => value.V00;

    public static bool operator ==(UInt128 left, UInt128 right) => EQ128(left.V64, left.V00, right.V64, right.V00);

    public static bool operator !=(UInt128 left, UInt128 right) => !EQ128(left.V64, left.V00, right.V64, right.V00);

    public static bool operator <(UInt128 left, UInt128 right) => LT128(left.V64, left.V00, right.V64, right.V00);

    public static bool operator >(UInt128 left, UInt128 right) => LT128(right.V64, right.V00, left.V64, left.V00);

    public static bool operator <=(UInt128 left, UInt128 right) => LE128(left.V64, left.V00, right.V64, right.V00);

    public static bool operator >=(UInt128 left, UInt128 right) => LE128(right.V64, right.V00, left.V64, left.V00);

    public static UInt128 operator <<(UInt128 left, int right) => ShortShiftLeft128(left.V64, left.V00, right);

    public static UInt128 operator >>(UInt128 left, int right) => ShortShiftRight128(left.V64, left.V00, right);

    public static UInt128 operator +(UInt128 left, UInt128 right) => Add128(left.V64, left.V00, right.V64, right.V00);

    public static UInt128 operator +(UInt128 left, ulong right) => Add128(left.V64, left.V00, 0, right);

    public static UInt128 operator -(UInt128 left, UInt128 right) => Sub128(left.V64, left.V00, right.V64, right.V00);

    public static UInt128 operator -(UInt128 left, ulong right) => Sub128(left.V64, left.V00, 0, right);

    public static UInt128 operator -(UInt128 value) => Sub128(0, 0, value.V64, value.V00);

    public static UInt128 operator *(UInt128 left, uint right) => Mul128By32(left.V64, left.V00, right);

    public static UInt256 operator *(UInt128 left, UInt128 right)
    {
        Span<ulong> result = stackalloc ulong[4];
        Mul128To256M(left.V64, left.V00, right.V64, right.V00, result);
        return new UInt256(result);
    }

    #endregion
}
