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
    public const uint ui32_fromPosOverflow = 0xFFFFFFFF;

    // ui32_fromNegOverflow
    public const uint ui32_fromNegOverflow = 0xFFFFFFFF;

    // ui32_fromNaN
    public const uint ui32_fromNaN = 0xFFFFFFFF;

    // i32_fromPosOverflow
    public const int i32_fromPosOverflow = -0x7FFFFFFF - 1;

    // i32_fromNegOverflow
    public const int i32_fromNegOverflow = -0x7FFFFFFF - 1;

    // i32_fromNaN
    public const int i32_fromNaN = -0x7FFFFFFF - 1;

    // ui64_fromPosOverflow
    public const ulong ui64_fromPosOverflow = 0xFFFFFFFFFFFFFFFF;

    // ui64_fromNegOverflow
    public const ulong ui64_fromNegOverflow = 0xFFFFFFFFFFFFFFFF;

    // ui64_fromNaN
    public const ulong ui64_fromNaN = 0xFFFFFFFFFFFFFFFF;

    // i64_fromPosOverflow
    public const long i64_fromPosOverflow = -0x7FFFFFFFFFFFFFFF - 1;

    // i64_fromNegOverflow
    public const long i64_fromNegOverflow = -0x7FFFFFFFFFFFFFFF - 1;

    // i64_fromNaN
    public const long i64_fromNaN = -0x7FFFFFFFFFFFFFFF - 1;

    #region Float16

    // defaultNaNF16UI
    /// <summary>
    /// The bit pattern for a default generated 16-bit floating-point NaN.
    /// </summary>
    public const uint_fast16_t DefaultNaNF16UI = 0xFE00;

    // softfloat_isSigNaNF16UI
    /// <summary>
    /// Returns true when 16-bit unsigned integer <paramref name="uiA"/> has the bit pattern of a 16-bit floating-point signaling NaN.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSigNaNF16UI(uint_fast16_t uiA) => (uiA & 0x7E00) == 0x7C00 && (uiA & 0x01FF) != 0;

    // softfloat_f16UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="uiA"/> has the bit pattern of a 16-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="zPtr"/>. If the NaN is a signaling NaN, the invalid
    /// exception is raised.
    /// </summary>
    public static void F16UIToCommonNaN(SoftFloatState state, uint_fast16_t uiA, out SoftFloatCommonNaN zPtr)
    {
        if (IsSigNaNF16UI(uiA))
            state.RaiseFlags(ExceptionFlags.Invalid);

        zPtr = new SoftFloatCommonNaN()
        {
            Sign = (uiA >> 15) != 0,
            Value = new UInt128(upper: (ulong)uiA << 54, lower: 0)
        };
    }

    // softfloat_commonNaNToF16UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="aPtr"/> into a 16-bit floating-point NaN, and returns the bit pattern of this
    /// value as an unsigned integer.
    /// </summary>
    public static uint_fast16_t CommonNaNToF16UI(in SoftFloatCommonNaN aPtr) =>
        (aPtr.Sign ? (1U << 15) : 0) | 0x7E00 | (uint_fast16_t)(aPtr.Value >> 118);

    // softfloat_propagateNaNF16UI
    /// <summary>
    /// Interpreting <paramref name="uiA"/> and <paramref name="uiB"/> as the bit patterns of two 16-bit floating-point values, at least
    /// one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="uiA"/> or
    /// <paramref name="uiB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static uint_fast16_t PropagateNaNF16UI(SoftFloatState state, uint_fast16_t uiA, uint_fast16_t uiB)
    {
        var isSigNaNA = IsSigNaNF16UI(uiA);
        var isSigNaNB = IsSigNaNF16UI(uiB);

        // Make NaNs non-signaling.
        var uiNonsigA = uiA | 0x0200;
        var uiNonsigB = uiB | 0x0200;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNF16UI(uiB) ? uiNonsigB : uiNonsigA;
            }
            else
            {
                return IsNaNF16UI(uiA) ? uiNonsigA : uiNonsigB;
            }
        }

    returnLargerMag:
        var uiMagA = uiA & 0x7FFF;
        var uiMagB = uiB & 0x7FFF;
        if (uiMagA < uiMagB) return uiNonsigB;
        if (uiMagB < uiMagA) return uiNonsigA;
        return (uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB;
    }

    #endregion

    #region Float32

    // defaultNaNF32UI
    /// <summary>
    /// The bit pattern for a default generated 32-bit floating-point NaN.
    /// </summary>
    public const uint_fast32_t DefaultNaNF32UI = 0xFFC00000;

    // softfloat_isSigNaNF32UI
    /// <summary>
    /// Returns true when 32-bit unsigned integer <paramref name="uiA"/> has the bit pattern of a 32-bit floating-point signaling NaN.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSigNaNF32UI(uint_fast32_t uiA) => (uiA & 0x7FC00000) == 0x7FC00000 && (uiA & 0x003FFFFF) != 0;

    // softfloat_f32UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="uiA"/> has the bit pattern of a 32-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="zPtr"/>. If the NaN is a signaling NaN, the invalid
    /// exception is raised.
    /// </summary>
    public static void F32UIToCommonNaN(SoftFloatState state, uint_fast32_t uiA, out SoftFloatCommonNaN zPtr)
    {
        if (IsSigNaNF32UI(uiA))
            state.RaiseFlags(ExceptionFlags.Invalid);

        zPtr = new SoftFloatCommonNaN()
        {
            Sign = (uiA >> 31) != 0,
            Value = new UInt128(upper: (ulong)uiA << 41, lower: 0)
        };
    }

    // softfloat_commonNaNToF32UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="aPtr"/> into a 32-bit floating-point NaN, and returns the bit pattern of this
    /// value as an unsigned integer.
    /// </summary>
    public static uint_fast32_t CommonNaNToF32UI(in SoftFloatCommonNaN aPtr) =>
        (aPtr.Sign ? (1U << 31) : 0U) | 0x7FC00000 | (uint_fast32_t)(aPtr.Value >> 105);

    // softfloat_propagateNaNF32UI
    /// <summary>
    /// Interpreting <paramref name="uiA"/> and <paramref name="uiB"/> as the bit patterns of two 32-bit floating-point values, at least
    /// one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="uiA"/> or
    /// <paramref name="uiB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static uint_fast32_t PropagateNaNF32UI(SoftFloatState state, uint_fast32_t uiA, uint_fast32_t uiB)
    {
        var isSigNaNA = IsSigNaNF32UI(uiA);
        var isSigNaNB = IsSigNaNF32UI(uiB);

        // Make NaNs non-signaling.
        var uiNonsigA = uiA | 0x00400000;
        var uiNonsigB = uiB | 0x00400000;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNF32UI(uiB) ? uiNonsigB : uiNonsigA;
            }
            else
            {
                return IsNaNF32UI(uiA) ? uiNonsigA : uiNonsigB;
            }
        }

    returnLargerMag:
        var uiMagA = uiA & 0x7FFFFFFF;
        var uiMagB = uiB & 0x7FFFFFFF;
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
    public const uint_fast64_t DefaultNaNF64UI = 0xFFF8000000000000;

    // softfloat_isSigNaNF64UI
    /// <summary>
    /// Returns true when 64-bit unsigned integer <paramref name="uiA"/> has the bit pattern of a 64-bit floating-point signaling NaN.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSigNaNF64UI(uint_fast64_t uiA) =>
        (uiA & 0x7FF8000000000000) == 0x7FF8000000000000 && (uiA & 0x0007FFFFFFFFFFFF) != 0;

    // softfloat_f64UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="uiA"/> has the bit pattern of a 64-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="zPtr"/>. If the NaN is a signaling NaN, the invalid
    /// exception is raised.
    /// </summary>
    public static void F64UIToCommonNaN(SoftFloatState state, uint_fast64_t uiA, out SoftFloatCommonNaN zPtr)
    {
        if (IsSigNaNF64UI(uiA))
            state.RaiseFlags(ExceptionFlags.Invalid);

        zPtr = new SoftFloatCommonNaN()
        {
            Sign = (uiA >> 63) != 0,
            Value = new UInt128(upper: uiA << 12, lower: 0)
        };
    }

    // softfloat_commonNaNToF64UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="aPtr"/> into a 64-bit floating-point NaN, and returns the bit pattern of this
    /// value as an unsigned integer.
    /// </summary>
    public static uint_fast64_t CommonNaNToF64UI(in SoftFloatCommonNaN aPtr) =>
        (aPtr.Sign ? (1UL << 63) : 0) | 0x7FF8000000000000 | (uint_fast64_t)(aPtr.Value >> 76);

    // softfloat_propagateNaNF64UI
    /// <summary>
    /// Interpreting <paramref name="uiA"/> and <paramref name="uiB"/> as the bit patterns of two 64-bit floating-point values, at least
    /// one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="uiA"/> or
    /// <paramref name="uiB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static uint_fast64_t PropagateNaNF64UI(SoftFloatState state, uint_fast64_t uiA, uint_fast64_t uiB)
    {
        var isSigNaNA = IsSigNaNF64UI(uiA);
        var isSigNaNB = IsSigNaNF64UI(uiB);

        // Make NaNs non-signaling.
        var uiNonsigA = uiA | 0x0008000000000000;
        var uiNonsigB = uiB | 0x0008000000000000;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNF64UI(uiB) ? uiNonsigB : uiNonsigA;
            }
            else
            {
                return IsNaNF64UI(uiA) ? uiNonsigA : uiNonsigB;
            }
        }

    returnLargerMag:
        var uiMagA = uiA & 0x7FFFFFFFFFFFFFFF;
        var uiMagB = uiB & 0x7FFFFFFFFFFFFFFF;
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
    public const uint16_t DefaultNaNExtF80UI64 = 0xFFFF;

    // defaultNaNExtF80UI0
    /// <summary>
    /// The bit pattern for the lower 64 bits of a default generated 80-bit extended floating-point NaN.
    /// </summary>
    public const uint64_t DefaultNaNExtF80UI0 = 0xC000000000000000;

    // softfloat_isSigNaNExtF80UI
    /// <summary>
    /// Returns true when the 80-bit unsigned integer formed from concatenating 16-bit <paramref name="uiA64"/> and 64-bit
    /// <paramref name="uiA0"/> has the bit pattern of an 80-bit extended floating-point signaling NaN.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSigNaNExtF80UI(uint_fast16_t uiA64, uint_fast64_t uiA0) =>
        (uiA64 & 0x7FFF) == 0x7FFF && (uiA0 & 0x4000000000000000) == 0 && (uiA0 & 0x3FFFFFFFFFFFFFFF) != 0;

    // softfloat_extF80UIToCommonNaN
    /// <summary>
    /// Assuming the unsigned integer formed from concatenating <paramref name="uiA64"/> and <paramref name="uiA0"/> has the bit pattern of
    /// an 80-bit extended floating-point NaN, converts this NaN to the common NaN form, and stores the resulting common NaN at the
    /// location pointed to by <paramref name="zPtr"/>. If the NaN is a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static void ExtF80UIToCommonNaN(SoftFloatState state, uint_fast16_t uiA64, uint_fast64_t uiA0, out SoftFloatCommonNaN zPtr)
    {
        if (IsSigNaNExtF80UI(uiA64, uiA0))
            state.RaiseFlags(ExceptionFlags.Invalid);

        zPtr = new SoftFloatCommonNaN()
        {
            Sign = (uiA64 >> 15) != 0,
            Value = new UInt128(upper: uiA0 << 1, lower: 0)
        };
    }

    // softfloat_commonNaNToExtF80UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="aPtr"/> into an 80-bit extended floating-point NaN, and returns the bit
    /// pattern of this value as an unsigned integer.
    /// </summary>
    public static UInt128 CommonNaNToExtF80UI(in SoftFloatCommonNaN aPtr) => new(
        upper: (aPtr.Sign ? (1UL << 15) : 0) | 0x7FFF,
        lower: 0xC000000000000000 | (uint64_t)(aPtr.Value >> 65)
    );

    // softfloat_propagateNaNExtF80UI
    /// <summary>
    /// Interpreting the unsigned integer formed from concatenating <paramref name="uiA64"/> and <paramref name="uiA0"/> as an 80-bit
    /// extended floating-point value, and likewise interpreting the unsigned integer formed from concatenating <paramref name="uiB64"/>
    /// and <paramref name="uiB0"/> as another 80-bit extended floating-point value, and assuming at least on of these floating-point
    /// values is a NaN, returns the bit pattern of the combined NaN result. If either original floating-point value is a signaling NaN,
    /// the invalid exception is raised.
    /// </summary>
    public static UInt128 PropagateNaNExtF80UI(SoftFloatState state, uint_fast16_t uiA64, uint_fast64_t uiA0, uint_fast16_t uiB64, uint_fast64_t uiB0)
    {
        var isSigNaNA = IsSigNaNExtF80UI(uiA64, uiA0);
        var isSigNaNB = IsSigNaNExtF80UI(uiB64, uiB0);

        // Make NaNs non-signaling.
        var uiNonsigA0 = uiA0 | 0xC000000000000000;
        var uiNonsigB0 = uiB0 | 0xC000000000000000;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNExtF80UI((int_fast16_t)uiB64, uiB0)
                    ? new UInt128(upper: uiB64, lower: uiNonsigB0)
                    : new UInt128(upper: uiA64, lower: uiNonsigA0);
            }
            else
            {
                return IsNaNExtF80UI((int_fast16_t)uiA64, uiA0)
                    ? new UInt128(upper: uiA64, lower: uiNonsigA0)
                    : new UInt128(upper: uiB64, lower: uiNonsigB0);
            }
        }

    returnLargerMag:
        var uiMagA64 = uiA64 & 0x7FFF;
        var uiMagB64 = uiB64 & 0x7FFF;

        int cmp = uiMagA64.CompareTo(uiMagB64);
        if (cmp == 0) cmp = uiA0.CompareTo(uiB0);
        if (cmp == 0) cmp = uiB64.CompareTo(uiA64);
        return cmp <= 0
            ? new UInt128(upper: uiB64, lower: uiNonsigB0)
            : new UInt128(upper: uiA64, lower: uiNonsigA0);
    }

    #endregion

    #region Float128

    // defaultNaNF128UI64
    /// <summary>
    /// The bit pattern for the upper 64 bits of a default generated 128-bit floating-point NaN.
    /// </summary>
    public const uint_fast64_t DefaultNaNF128UI64 = 0xFFFF800000000000;

    // defaultNaNF128UI0
    /// <summary>
    /// The bit pattern for the lowper 64 bits of a default generated 128-bit floating-point NaN.
    /// </summary>
    public const uint_fast64_t DefaultNaNF128UI0 = 0x0000000000000000;

    // softfloat_isSigNaNF128UI
    /// <summary>
    /// Returns true when the 128-bit unsigned integer formed from concatenating 64-bit <paramref name="uiA64"/> and 64-bit
    /// <paramref name="uiA0"/> has the bit pattern of a 128-bit floating-point signaling NaN.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSigNaNF128UI(uint_fast64_t uiA64, uint_fast64_t uiA0) =>
        (uiA64 & 0x7FFF800000000000) == 0x7FFF000000000000 && (uiA0 != 0 || (uiA64 & 0x00007FFFFFFFFFFF) != 0);

    // softfloat_f128UIToCommonNaN
    /// <summary>
    /// Assuming the unsigned integer formed from concatenating <paramref name="uiA64"/> and <paramref name="uiA0"/> has the bit pattern of
    /// an 128-bit floating-point NaN, converts this NaN to the common NaN form, and stores the resulting common NaN at the location
    /// pointed to by <paramref name="zPtr"/>. If the NaN is a signaling NaN, the invalid exception is raised.
    /// </summary>
    public static void F128UIToCommonNaN(SoftFloatState state, uint_fast64_t uiA64, uint_fast64_t uiA0, out SoftFloatCommonNaN zPtr)
    {
        if (IsSigNaNF128UI(uiA64, uiA0))
            state.RaiseFlags(ExceptionFlags.Invalid);

        var NaNSig = ShortShiftLeft128(uiA64, uiA0, 16);
        zPtr = new SoftFloatCommonNaN()
        {
            Sign = (uiA64 >> 63) != 0,
            Value = new UInt128(upper: NaNSig.V64, lower: NaNSig.V00)
        };
    }

    // softfloat_commonNaNToF128UI
    /// <summary>
    /// Converts the common NaN pointed to by 'aPtr' into a 128-bit floating-point NaN, and returns the bit pattern of this value as an
    /// unsigned integer.
    /// </summary>
    public static UInt128 CommonNaNToF128UI(in SoftFloatCommonNaN aPtr)
    {
        var uiZ = aPtr.Value >> 16;
        uiZ |= new UInt128(upper: (aPtr.Sign ? (1UL << 63) : 0) | 0x7FFF800000000000, lower: 0);
        return uiZ;
    }

    // softfloat_propagateNaNF128UI
    /// <summary>
    /// Interpreting the unsigned integer formed from concatenating <paramref name="uiA64"/> and <paramref name="uiA0"/> as a 128-bit
    /// floating-point value, and likewise interpreting the unsigned integer formed from concatenating <paramref name="uiB64"/> and
    /// <paramref name="uiB0"/> as another 128-bit floating-point value, and assuming at least on of these floating-point values is a NaN,
    /// returns the bit pattern of the combined NaN result. If either original floating-point value is a signaling NaN, the invalid
    /// exception is raised.
    /// </summary>
    public static UInt128 PropagateNaNF128UI(SoftFloatState state, uint_fast64_t uiA64, uint_fast64_t uiA0, uint_fast64_t uiB64, uint_fast64_t uiB0)
    {
        var isSigNaNA = IsSigNaNF128UI(uiA64, uiA0);
        var isSigNaNB = IsSigNaNF128UI(uiB64, uiB0);

        // Make NaNs non-signaling.
        var uiNonsigA0 = uiA0 | 0x0000800000000000;
        var uiNonsigB0 = uiB0 | 0x0000800000000000;

        if (isSigNaNA | isSigNaNB)
        {
            state.RaiseFlags(ExceptionFlags.Invalid);
            if (isSigNaNA)
            {
                if (isSigNaNB)
                    goto returnLargerMag;

                return IsNaNF128UI(uiB64, uiB0)
                    ? new UInt128(upper: uiB64, lower: uiNonsigB0)
                    : new UInt128(upper: uiA64, lower: uiNonsigA0);
            }
            else
            {
                return IsNaNF128UI(uiA64, uiA0)
                    ? new UInt128(upper: uiA64, lower: uiNonsigA0)
                    : new UInt128(upper: uiB64, lower: uiNonsigB0);
            }
        }

    returnLargerMag:
        var uiMagA64 = uiA64 & 0x7FFFFFFFFFFFFFFF;
        var uiMagB64 = uiB64 & 0x7FFFFFFFFFFFFFFF;

        int cmp = uiMagA64.CompareTo(uiMagB64);
        if (cmp == 0) cmp = uiA0.CompareTo(uiB0);
        if (cmp == 0) cmp = uiB64.CompareTo(uiA64);
        return cmp <= 0
            ? new UInt128(upper: uiB64, lower: uiNonsigB0)
            : new UInt128(upper: uiA64, lower: uiNonsigA0);
    }

    #endregion
}
