#region Copyright
// This is a C# port of the SoftFloat library release 3e by Thomas Kaiser.

/*============================================================================

This C source file is part of the SoftFloat IEEE Floating-Point Arithmetic
Package, Release 3e, by John R. Hauser.

Copyright 2011, 2012, 2013, 2014, 2015 The Regents of the University of
California.  All rights reserved.

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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Tommunism.SoftFloat;

using static Primitives;
using static Specialize;
using static Internals;

// Improve Visual Studio's readability a little bit by "redefining" the standard integer types to C99 stdint types.

using int8_t = SByte;
using int16_t = Int16;
using int32_t = Int32;
using int64_t = Int64;

using uint8_t = Byte;
using uint16_t = UInt16;
using uint32_t = UInt32;
using uint64_t = UInt64;

// C# only has 32-bit & 64-bit integer operators by default, so just make these "fast" types 32 or 64 bits.
using int_fast8_t = Int32;
using int_fast16_t = Int32;
using int_fast32_t = Int32;
using int_fast64_t = Int64;
using uint_fast8_t = UInt32;
using uint_fast16_t = UInt32;
using uint_fast32_t = UInt32;
using uint_fast64_t = UInt64;

[StructLayout(LayoutKind.Sequential, Pack = sizeof(ulong), Size = sizeof(ulong))]
public readonly struct Float64
{
    #region Fields

    // WARNING: DO NOT ADD OR CHANGE ANY OF THESE FIELDS!!!
    private readonly ulong _v;

    #endregion

    #region Constructors

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Used to avoid accidentally calling other overloads.")]
    private Float64(ulong v, bool dummy)
    {
        _v = v;
    }

    public Float64(double value)
    {
        _v = BitConverter.DoubleToUInt64Bits(value);
    }

    #endregion

    #region Properties

    // f64_isSignalingNaN
    public bool IsSignalingNaN => IsSigNaNF64UI(_v);

    #endregion

    #region Methods

    public static explicit operator Float64(double value) => new(value);
    public static implicit operator double(Float64 value) => BitConverter.UInt64BitsToDouble(value._v);

    public static Float64 FromUInt64Bits(ulong value) => FromUI64(value);

    public ulong ToUInt64Bits() => _v;

    // THIS IS THE INTERNAL CONSTRUCTOR FOR RAW BITS.
    internal static Float64 FromUI64(ulong v) => new(v, dummy: false);

    #region Integer-to-floating-point Conversions

    // NOTE: These operators use the default software floating-point state.
    public static explicit operator Float64(uint32_t a) => FromUInt32(a);
    public static explicit operator Float64(uint64_t a) => FromUInt64(a);
    public static explicit operator Float64(int32_t a) => FromInt32(a);
    public static explicit operator Float64(int64_t a) => FromInt64(a);

    // ui32_to_f64
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static Float64 FromUInt32(uint32_t a, SoftFloatState? state = null)
    {
        if (a == 0)
            return FromUI64(0);

        var shiftDist = CountLeadingZeroes32(a) + 21;
        return FromUI64(PackToF64UI(false, 0x432 - shiftDist, (uint_fast64_t)a << shiftDist));
    }

    // ui64_to_f64
    public static Float64 FromUInt64(uint64_t a, SoftFloatState? state = null)
    {
        if (a == 0)
            return FromUI64(0);

        state ??= SoftFloatState.Default;
        return (a & 0x8000000000000000) != 0
            ? RoundPackToF64(state, false, 0x43D, ShortShiftRightJam64(a, 1))
            : NormRoundPackToF64(state, false, 0x43C, a);
    }

    // i32_to_f64
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "API consistency and possible future use.")]
    public static Float64 FromInt32(int32_t a, SoftFloatState? state = null)
    {
        if (a == 0)
            return FromUI64(0);

        var sign = a < 0;
        var absA = (uint_fast32_t)(sign ? -a : a);
        var shiftDist = CountLeadingZeroes32(absA) + 21;
        return FromUI64(PackToF64UI(sign, 0x432 - shiftDist, (uint_fast64_t)absA << shiftDist));
    }

    // i64_to_f64
    public static Float64 FromInt64(int64_t a, SoftFloatState? state = null)
    {
        var sign = a < 0;
        if ((a & 0x7FFFFFFFFFFFFFFF) == 0)
            return FromUI64(sign ? PackToF64UI(true, 0x43E, 0UL) : 0UL);

        var absA = (uint_fast64_t)(sign ? -a : a);
        return NormRoundPackToF64(state ?? SoftFloatState.Default, sign, 0x43C, absA);
    }

    #endregion

    #region Floating-point-to-integer Conversions

    public uint32_t ToUInt32(bool exact, SoftFloatState state) => ToUInt32(state.RoundingMode, exact, state);

    public uint64_t ToUInt64(bool exact, SoftFloatState state) => ToUInt64(state.RoundingMode, exact, state);

    public int32_t ToInt32(bool exact, SoftFloatState state) => ToInt32(state.RoundingMode, exact, state);

    public int64_t ToInt64(bool exact, SoftFloatState state) => ToInt64(state.RoundingMode, exact, state);

    // f64_to_ui32
    public uint32_t ToUInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_ui64
    public uint64_t ToUInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_i32
    public int32_t ToInt32(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_i64
    public int64_t ToInt64(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_ui32_r_minMag
    public uint32_t ToUInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_ui64_r_minMag
    public uint64_t ToUInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_i32_r_minMag
    public int32_t ToInt32RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_i64_r_minMag
    public int64_t ToInt64RoundMinMag(bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Floating-point-to-floating-point Conversions

    // f64_to_f16
    public static Float16 ToFloat16(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_f32
    public static Float32 ToFloat32(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_extF80
    public static ExtFloat80 ToExtFloat80(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_to_f128
    public static Float128 ToFloat128(SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Arithmetic Operations

    // f64_roundToInt
    public Float64 RoundToInt(RoundingMode roundingMode, bool exact, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_add
    public static Float64 Add(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_sub
    public static Float64 Subtract(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_mul
    public static Float64 Multiply(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_mulAdd
    public static Float64 MultiplyAndAdd(Float64 a, Float64 b, Float64 c, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_div
    public static Float64 Divide(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_rem
    public static Float64 Modulus(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_sqrt
    public static Float64 SquareRoot(Float64 a, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Comparison Operations

    // f64_eq
    public static bool CompareEqual(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_le
    public static bool CompareLessThanOrEqual(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_lt
    public static bool CompareLessThan(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_eq_signaling
    public static bool CompareEqualSignaling(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_le_quiet
    public static bool CompareLessThanOrEqualQuiet(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    // f64_lt_quiet
    public static bool CompareLessThanQuiet(Float64 a, Float64 b, SoftFloatState? state = null)
    {
        throw new NotImplementedException();
    }

    #endregion

    #endregion
}
