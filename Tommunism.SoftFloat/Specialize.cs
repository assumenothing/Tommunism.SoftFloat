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
using System.Runtime.CompilerServices;

namespace Tommunism.SoftFloat;

using static Primitives;
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

// NOTE: Using values from the 8086 specialization code.
// TODO: Make this an instance class to allow different specialiazation code to be used (e.g., ARM support)?

internal static class Specialize
{
    // init_detectTininess
    /// <summary>
    /// Default value for 'softfloat_detectTininess'.
    /// </summary>
    public const Tininess InitialDetectTininess = Tininess.AfterRounding;

    // ui32_fromPosOverflow
    public const uint UInt32FromPosOverflow = 0xFFFFFFFF;

    // ui32_fromNegOverflow
    public const uint UInt32FromNegOverflow = 0xFFFFFFFF;

    // ui32_fromNaN
    public const uint UInt32FromNaN = 0xFFFFFFFF;

    // i32_fromPosOverflow
    public const int Int32FromPosOverflow = -0x7FFFFFFF - 1;

    // i32_fromNegOverflow
    public const int Int32FromNegOverflow = -0x7FFFFFFF - 1;

    // i32_fromNaN
    public const int Int32FromNaN = -0x7FFFFFFF - 1;

    // ui64_fromPosOverflow
    public const ulong UInt64FromPosOverflow = 0xFFFFFFFFFFFFFFFF;

    // ui64_fromNegOverflow
    public const ulong UInt64FromNegOverflow = 0xFFFFFFFFFFFFFFFF;

    // ui64_fromNaN
    public const ulong UInt64FromNaN = 0xFFFFFFFFFFFFFFFF;

    // i64_fromPosOverflow
    public const long Int64FromPosOverflow = -0x7FFFFFFFFFFFFFFF - 1;

    // i64_fromNegOverflow
    public const long Int64FromNegOverflow = -0x7FFFFFFFFFFFFFFF - 1;

    // i64_fromNaN
    public const long Int64FromNaN = -0x7FFFFFFFFFFFFFFF - 1;

    #region Float16

    // defaultNaNF16UI
    /// <summary>
    /// The bit pattern for a default generated 16-bit floating-point NaN.
    /// </summary>
    public const uint16_t DefaultNaNFloat16Bits = 0xFE00;

    public static Float16 DefaultNaNFloat16 => Float16.FromBitsUI16(DefaultNaNFloat16Bits);

    // softfloat_isSigNaNF16UI
    /// <summary>
    /// Returns true when 16-bit unsigned integer <paramref name="bits"/> has the bit pattern of a 16-bit floating-point signaling NaN.
    /// </summary>
    public static bool IsSignalNaNFloat16Bits(uint_fast16_t bits) => (bits & 0x7E00) == 0x7C00 && (bits & 0x01FF) != 0;

    // softfloat_f16UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="bits"/> has the bit pattern of a 16-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the
    /// invalid exception is raised.
    /// </summary>
    public static void Float16BitsToCommonNaN(SoftFloatState state, uint_fast16_t bits, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSignalNaNFloat16Bits(bits))
            state.RaiseFlags(ExceptionFlags.Invalid);

        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits >> 15) != 0,
            Value = new UInt128(upper: (ulong)bits << 54, lower: 0)
        };
    }

    // softfloat_commonNaNToF16UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="commonNaN"/> into a 16-bit floating-point NaN, and returns the bit pattern of
    /// this value as an unsigned integer.
    /// </summary>
    public static uint16_t CommonNaNToFloat16Bits(in SoftFloatCommonNaN commonNaN) =>
        (uint16_t)((commonNaN.Sign ? (1U << 15) : 0) | 0x7E00 | (uint_fast16_t)(commonNaN.Value >> 118));

    // softfloat_propagateNaNF16UI
    /// <summary>
    /// Interpreting <paramref name="bitsA"/> and <paramref name="bitsB"/> as the bit patterns of two 16-bit floating-point values, at
    /// least one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="bitsA"/> or
    /// <paramref name="bitsB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static uint16_t PropagateNaNFloat16Bits(SoftFloatState state, uint_fast16_t bitsA, uint_fast16_t bitsB)
    {
        var isSigNaNA = IsSignalNaNFloat16Bits(bitsA);
        var isSigNaNB = IsSignalNaNFloat16Bits(bitsB);

        // Make NaNs non-signaling.
        var uiNonsigA = bitsA | 0x0200;
        var uiNonsigB = bitsB | 0x0200;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return (uint16_t)(IsNaNF16UI(bitsB) ? uiNonsigB : uiNonsigA);
            }
            else
            {
                return (uint16_t)(IsNaNF16UI(bitsA) ? uiNonsigA : uiNonsigB);
            }
        }

    returnLargerMag:
        var uiMagA = bitsA & 0x7FFF;
        var uiMagB = bitsB & 0x7FFF;
        if (uiMagA < uiMagB) return (uint16_t)uiNonsigB;
        if (uiMagB < uiMagA) return (uint16_t)uiNonsigA;
        return (uint16_t)((uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB);
    }

    #endregion

    #region Float32

    // defaultNaNF32UI
    /// <summary>
    /// The bit pattern for a default generated 32-bit floating-point NaN.
    /// </summary>
    public const uint32_t DefaultNaNFloat32Bits = 0xFFC00000;

    public static Float32 DefaultNaNFloat32 => Float32.FromBitsUI32(DefaultNaNFloat32Bits);

    // softfloat_isSigNaNF32UI
    /// <summary>
    /// Returns true when 32-bit unsigned integer <paramref name="bits"/> has the bit pattern of a 32-bit floating-point signaling NaN.
    /// </summary>
    public static bool IsSigNaNFloat32Bits(uint_fast32_t bits) => (bits & 0x7FC00000) == 0x7FC00000 && (bits & 0x003FFFFF) != 0;

    // softfloat_f32UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="bits"/> has the bit pattern of a 32-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the
    /// invalid exception is raised.
    /// </summary>
    public static void Float32BitsToCommonNaN(SoftFloatState state, uint_fast32_t bits, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSigNaNFloat32Bits(bits))
            state.RaiseFlags(ExceptionFlags.Invalid);

        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits >> 31) != 0,
            Value = new UInt128(upper: (ulong)bits << 41, lower: 0)
        };
    }

    // softfloat_commonNaNToF32UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="commonNaN"/> into a 32-bit floating-point NaN, and returns the bit pattern of
    /// this value as an unsigned integer.
    /// </summary>
    public static uint32_t CommonNaNToFloat32Bits(in SoftFloatCommonNaN commonNaN) =>
        (commonNaN.Sign ? (1U << 31) : 0U) | 0x7FC00000 | (uint_fast32_t)(commonNaN.Value >> 105);

    // softfloat_propagateNaNF32UI
    /// <summary>
    /// Interpreting <paramref name="bitsA"/> and <paramref name="bitsB"/> as the bit patterns of two 32-bit floating-point values, at
    /// least one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="bitsA"/> or
    /// <paramref name="bitsB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static uint32_t PropagateNaNFloat32Bits(SoftFloatState state, uint_fast32_t bitsA, uint_fast32_t bitsB)
    {
        var isSigNaNA = IsSigNaNFloat32Bits(bitsA);
        var isSigNaNB = IsSigNaNFloat32Bits(bitsB);

        // Make NaNs non-signaling.
        var uiNonsigA = bitsA | 0x00400000;
        var uiNonsigB = bitsB | 0x00400000;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNF32UI(bitsB) ? uiNonsigB : uiNonsigA;
            }
            else
            {
                return IsNaNF32UI(bitsA) ? uiNonsigA : uiNonsigB;
            }
        }

    returnLargerMag:
        var uiMagA = bitsA & 0x7FFFFFFF;
        var uiMagB = bitsB & 0x7FFFFFFF;
        if (uiMagA < uiMagB) return uiNonsigB;
        if (uiMagB < uiMagA) return uiNonsigA;
        return (uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB;
    }

    #endregion

    #region Float64

    // defaultNaNF64UI
    /// <summary>
    /// The bit pattern for a default generated 64-bit floating-point NaN.
    /// </summary>
    public const uint64_t DefaultNaNFloat64Bits = 0xFFF8000000000000;

    public static Float64 DefaultNaNFloat64 => Float64.FromBitsUI64(DefaultNaNFloat64Bits);

    // softfloat_isSigNaNF64UI
    /// <summary>
    /// Returns true when 64-bit unsigned integer <paramref name="bits"/> has the bit pattern of a 64-bit floating-point signaling NaN.
    /// </summary>
    public static bool IsSigNaNFloat64Bits(uint_fast64_t bits) =>
        (bits & 0x7FF8000000000000) == 0x7FF8000000000000 && (bits & 0x0007FFFFFFFFFFFF) != 0;

    // softfloat_f64UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="bits"/> has the bit pattern of a 64-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the
    /// invalid exception is raised.
    /// </summary>
    public static void Float64BitsToCommonNaN(SoftFloatState state, uint_fast64_t bits, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSigNaNFloat64Bits(bits))
            state.RaiseFlags(ExceptionFlags.Invalid);

        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits >> 63) != 0,
            Value = new UInt128(upper: bits << 12, lower: 0)
        };
    }

    // softfloat_commonNaNToF64UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="commonNaN"/> into a 64-bit floating-point NaN, and returns the bit pattern of
    /// this value as an unsigned integer.
    /// </summary>
    public static uint64_t CommonNaNToFloat64Bits(in SoftFloatCommonNaN commonNaN) =>
        (commonNaN.Sign ? (1UL << 63) : 0) | 0x7FF8000000000000 | (uint_fast64_t)(commonNaN.Value >> 76);

    // softfloat_propagateNaNF64UI
    /// <summary>
    /// Interpreting <paramref name="bitsA"/> and <paramref name="bitsB"/> as the bit patterns of two 64-bit floating-point values, at
    /// least one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="bitsA"/> or
    /// <paramref name="bitsB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static uint64_t PropagateNaNFloat64Bits(SoftFloatState state, uint_fast64_t bitsA, uint_fast64_t bitsB)
    {
        var isSigNaNA = IsSigNaNFloat64Bits(bitsA);
        var isSigNaNB = IsSigNaNFloat64Bits(bitsB);

        // Make NaNs non-signaling.
        var uiNonsigA = bitsA | 0x0008000000000000;
        var uiNonsigB = bitsB | 0x0008000000000000;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNF64UI(bitsB) ? uiNonsigB : uiNonsigA;
            }
            else
            {
                return IsNaNF64UI(bitsA) ? uiNonsigA : uiNonsigB;
            }
        }

    returnLargerMag:
        var uiMagA = bitsA & 0x7FFFFFFFFFFFFFFF;
        var uiMagB = bitsB & 0x7FFFFFFFFFFFFFFF;
        if (uiMagA < uiMagB) return uiNonsigB;
        if (uiMagB < uiMagA) return uiNonsigA;
        return (uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB;
    }

    #endregion

    #region ExtFloat80

    // defaultNaNExtF80UI64
    /// <summary>
    /// The bit pattern for the upper 16 bits of a default generated 80-bit extended floating-point NaN.
    /// </summary>
    public const uint16_t DefaultNaNExtFloat80BitsUpper = 0xFFFF;

    // defaultNaNExtF80UI0
    /// <summary>
    /// The bit pattern for the lower 64 bits of a default generated 80-bit extended floating-point NaN.
    /// </summary>
    public const uint64_t DefaultNaNExtFloat80BitsLower = 0xC000000000000000;

    public static ExtFloat80 DefaultNaNExtFloat80 => ExtFloat80.FromBitsUI80(DefaultNaNExtFloat80BitsUpper, DefaultNaNExtFloat80BitsLower);

    // softfloat_isSigNaNExtF80UI
    /// <summary>
    /// Returns true when the 80-bit unsigned integer formed from concatenating 16-bit <paramref name="bits64"/> and 64-bit
    /// <paramref name="bits0"/> has the bit pattern of an 80-bit extended floating-point signaling NaN.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSigNaNExtFloat80Bits(uint_fast16_t bits64, uint_fast64_t bits0) =>
        (bits64 & 0x7FFF) == 0x7FFF && (bits0 & 0x4000000000000000) == 0 && (bits0 & 0x3FFFFFFFFFFFFFFF) != 0;

    // softfloat_extF80UIToCommonNaN
    /// <summary>
    /// Assuming the unsigned integer formed from concatenating <paramref name="bits64"/> and <paramref name="bits0"/> has the bit pattern
    /// of an 80-bit extended floating-point NaN, converts this NaN to the common NaN form, and stores the resulting common NaN at the
    /// location pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static void ExtFloat80BitsToCommonNaN(SoftFloatState state, uint_fast16_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSigNaNExtFloat80Bits(bits64, bits0))
            state.RaiseFlags(ExceptionFlags.Invalid);

        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits64 >> 15) != 0,
            Value = new UInt128(upper: bits0 << 1, lower: 0)
        };
    }

    // softfloat_commonNaNToExtF80UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="commonNaN"/> into an 80-bit extended floating-point NaN, and returns the bit
    /// pattern of this value as an unsigned integer.
    /// </summary>
    public static UInt128 CommonNaNToExtFloat80Bits(in SoftFloatCommonNaN commonNaN) => new(
        upper: (commonNaN.Sign ? (1UL << 15) : 0) | 0x7FFF,
        lower: 0xC000000000000000 | (uint64_t)(commonNaN.Value >> 65)
    );

    // softfloat_propagateNaNExtF80UI
    /// <summary>
    /// Interpreting the unsigned integer formed from concatenating <paramref name="bitsA64"/> and <paramref name="bitsA0"/> as an 80-bit
    /// extended floating-point value, and likewise interpreting the unsigned integer formed from concatenating <paramref name="bitsB64"/>
    /// and <paramref name="bitsB0"/> as another 80-bit extended floating-point value, and assuming at least on of these floating-point
    /// values is a NaN, returns the bit pattern of the combined NaN result. If either original floating-point value is a signaling NaN,
    /// the invalid exception is raised.
    /// </summary>
    public static UInt128 PropagateNaNExtFloat80Bits(SoftFloatState state, uint_fast16_t bitsA64, uint_fast64_t bitsA0, uint_fast16_t bitsB64, uint_fast64_t bitsB0)
    {
        var isSigNaNA = IsSigNaNExtFloat80Bits(bitsA64, bitsA0);
        var isSigNaNB = IsSigNaNExtFloat80Bits(bitsB64, bitsB0);

        // Make NaNs non-signaling.
        var uiNonsigA0 = bitsA0 | 0xC000000000000000;
        var uiNonsigB0 = bitsB0 | 0xC000000000000000;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNExtF80UI((int_fast16_t)bitsB64, bitsB0)
                    ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
                    : new UInt128(upper: bitsA64, lower: uiNonsigA0);
            }
            else
            {
                return IsNaNExtF80UI((int_fast16_t)bitsA64, bitsA0)
                    ? new UInt128(upper: bitsA64, lower: uiNonsigA0)
                    : new UInt128(upper: bitsB64, lower: uiNonsigB0);
            }
        }

    returnLargerMag:
        var uiMagA64 = bitsA64 & 0x7FFF;
        var uiMagB64 = bitsB64 & 0x7FFF;

        int cmp = uiMagA64.CompareTo(uiMagB64);
        if (cmp == 0) cmp = bitsA0.CompareTo(bitsB0);
        if (cmp == 0) cmp = bitsB64.CompareTo(bitsA64);
        return cmp <= 0
            ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
            : new UInt128(upper: bitsA64, lower: uiNonsigA0);
    }

    #endregion

    #region Float128

    // defaultNaNF128UI64
    /// <summary>
    /// The bit pattern for the upper 64 bits of a default generated 128-bit floating-point NaN.
    /// </summary>
    public const uint_fast64_t DefaultNaNFloat128BitsUpper = 0xFFFF800000000000;

    // defaultNaNF128UI0
    /// <summary>
    /// The bit pattern for the lowper 64 bits of a default generated 128-bit floating-point NaN.
    /// </summary>
    public const uint_fast64_t DefaultNaNFloat128BitsLower = 0x0000000000000000;

    public static Float128 DefaultNaNFloat128 => Float128.FromBitsUI128(v64: DefaultNaNFloat128BitsUpper, v0: DefaultNaNFloat128BitsLower);

    // softfloat_isSigNaNF128UI
    /// <summary>
    /// Returns true when the 128-bit unsigned integer formed from concatenating 64-bit <paramref name="bits64"/> and 64-bit
    /// <paramref name="bits0"/> has the bit pattern of a 128-bit floating-point signaling NaN.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSigNaNFloat128Bits(uint_fast64_t bits64, uint_fast64_t bits0) =>
        (bits64 & 0x7FFF800000000000) == 0x7FFF000000000000 && (bits0 != 0 || (bits64 & 0x00007FFFFFFFFFFF) != 0);

    // softfloat_f128UIToCommonNaN
    /// <summary>
    /// Assuming the unsigned integer formed from concatenating <paramref name="bits64"/> and <paramref name="bits0"/> has the bit pattern
    /// of an 128-bit floating-point NaN, converts this NaN to the common NaN form, and stores the resulting common NaN at the location
    /// pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static void Float128BitsToCommonNaN(SoftFloatState state, uint_fast64_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSigNaNFloat128Bits(bits64, bits0))
            state.RaiseFlags(ExceptionFlags.Invalid);

        var NaNSig = ShortShiftLeft128(bits64, bits0, 16);
        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits64 >> 63) != 0,
            Value = new UInt128(upper: NaNSig.V64, lower: NaNSig.V00)
        };
    }

    // softfloat_commonNaNToF128UI
    /// <summary>
    /// Converts the common NaN pointed to by 'aPtr' into a 128-bit floating-point NaN, and returns the bit pattern of this value as an
    /// unsigned integer.
    /// </summary>
    public static UInt128 CommonNaNToFloat128Bits(in SoftFloatCommonNaN commonNaN)
    {
        var uiZ = commonNaN.Value >> 16;
        uiZ |= new UInt128(upper: (commonNaN.Sign ? (1UL << 63) : 0) | 0x7FFF800000000000, lower: 0);
        return uiZ;
    }

    // softfloat_propagateNaNF128UI
    /// <summary>
    /// Interpreting the unsigned integer formed from concatenating <paramref name="bitsA64"/> and <paramref name="bitsA0"/> as a 128-bit
    /// floating-point value, and likewise interpreting the unsigned integer formed from concatenating <paramref name="bitsB64"/> and
    /// <paramref name="bitsB0"/> as another 128-bit floating-point value, and assuming at least on of these floating-point values is a NaN,
    /// returns the bit pattern of the combined NaN result. If either original floating-point value is a signaling NaN, the invalid
    /// exception is raised.
    /// </summary>
    public static UInt128 PropagateNaNFloat128Bits(SoftFloatState state, uint_fast64_t bitsA64, uint_fast64_t bitsA0, uint_fast64_t bitsB64, uint_fast64_t bitsB0)
    {
        var isSigNaNA = IsSigNaNFloat128Bits(bitsA64, bitsA0);
        var isSigNaNB = IsSigNaNFloat128Bits(bitsB64, bitsB0);

        // Make NaNs non-signaling.
        var uiNonsigA0 = bitsA0 | 0x0000800000000000;
        var uiNonsigB0 = bitsB0 | 0x0000800000000000;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNF128UI(bitsB64, bitsB0)
                    ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
                    : new UInt128(upper: bitsA64, lower: uiNonsigA0);
            }
            else
            {
                return IsNaNF128UI(bitsA64, bitsA0)
                    ? new UInt128(upper: bitsA64, lower: uiNonsigA0)
                    : new UInt128(upper: bitsB64, lower: uiNonsigB0);
            }
        }

    returnLargerMag:
        var uiMagA64 = bitsA64 & 0x7FFFFFFFFFFFFFFF;
        var uiMagB64 = bitsB64 & 0x7FFFFFFFFFFFFFFF;

        int cmp = uiMagA64.CompareTo(uiMagB64);
        if (cmp == 0) cmp = bitsA0.CompareTo(bitsB0);
        if (cmp == 0) cmp = bitsB64.CompareTo(bitsA64);
        return cmp <= 0
            ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
            : new UInt128(upper: bitsA64, lower: uiNonsigA0);
    }

    #endregion
}
