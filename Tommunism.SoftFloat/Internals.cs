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

namespace Tommunism.SoftFloat;

using static Primitives;

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

internal static partial class Internals
{
    #region Rounding

    // softfloat_roundToUI32
    public static uint_fast32_t RoundToUI32(SoftFloatState state, bool sign, uint_fast64_t sig, RoundingMode roundingMode, bool exact)
    {
        uint_fast16_t roundIncrement, roundBits;
        uint_fast32_t z;

        roundIncrement = 0x800U;
        if (roundingMode is not RoundingMode.NearMaxMag and not RoundingMode.NearEven)
        {
            roundIncrement = 0;
            if (sign)
            {
                if (sig == 0)
                    return 0;

                if (roundingMode is RoundingMode.Min or RoundingMode.Odd)
                    goto invalid;
            }
            else
            {
                if (roundingMode == RoundingMode.Max)
                    roundIncrement = 0xFFF;
            }
        }

        roundBits = (uint_fast32_t)sig & 0xFFF;
        sig += roundIncrement;
        if ((sig & 0xFFFFF00000000000) != 0)
            goto invalid;

        z = (uint_fast32_t)(sig >> 12);
        if (roundBits == 0x800 && roundingMode == RoundingMode.NearEven)
            z &= ~(uint_fast32_t)1;

        if (sign && z != 0)
            goto invalid;

        if (roundBits != 0)
        {
            if (roundingMode == RoundingMode.Odd)
                z |= 1U;

            if (exact)
                state.ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return z;

    invalid:
        state.RaiseFlags(ExceptionFlags.Invalid);
        return state.UInt32FromOverflow(sign);
    }

    // softfloat_roundToUI64
    public static ulong RoundToUI64(SoftFloatState state, bool sign, uint_fast64_t sig, uint_fast64_t sigExtra, RoundingMode roundingMode, bool exact)
    {
        if (roundingMode is RoundingMode.NearMaxMag or RoundingMode.NearEven)
        {
            if (0x8000000000000000 <= sigExtra)
            {
                ++sig;
                if (sig == 0)
                    goto invalid;

                if (roundingMode == RoundingMode.NearEven && sigExtra == 0x8000000000000000)
                    sig &= ~(uint_fast64_t)1;
            }
        }
        else
        {
            if (sign)
            {
                if ((sig | sigExtra) == 0)
                    return 0;

                if (roundingMode is RoundingMode.Min or RoundingMode.Odd)
                    goto invalid;
            }
            else
            {
                if (roundingMode == RoundingMode.Max && sigExtra != 0)
                {
                    ++sig;
                    if (sig == 0)
                        goto invalid;

                    if (roundingMode == RoundingMode.NearEven && sigExtra == 0x8000000000000000)
                        sig &= ~(uint_fast64_t)1;
                }
            }
        }

        if (sign && sig != 0)
            goto invalid;

        if (sigExtra != 0)
        {
            if (roundingMode == RoundingMode.Odd)
                sig |= 1;

            if (exact)
                state.ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return sig;

    invalid:
        state.RaiseFlags(ExceptionFlags.Invalid);
        return state.UInt64FromOverflow(sign);
    }

    // softfloat_roundToI32
    public static int_fast32_t RoundToI32(SoftFloatState state, bool sign, uint_fast64_t sig, RoundingMode roundingMode, bool exact)
    {
        uint_fast16_t roundIncrement, roundBits;
        uint_fast32_t sig32;
        int_fast32_t z;

        roundIncrement = 0x800;
        if (roundingMode is not RoundingMode.NearMaxMag and not RoundingMode.NearEven)
        {
            roundIncrement = 0;
            if (sign ? roundingMode is RoundingMode.Min or RoundingMode.Odd : roundingMode == RoundingMode.Max)
                roundIncrement = 0xFFF;
        }

        roundBits = (uint_fast16_t)sig & 0xFFF;
        sig += roundIncrement;
        if ((sig & 0xFFFFF00000000000) != 0)
            goto invalid;

        sig32 = (uint_fast32_t)(sig >> 12);
        if (roundBits == 0x800 && roundingMode == RoundingMode.NearEven)
            sig32 &= ~1U;

        z = sign ? -(int_fast32_t)sig32 : (int_fast32_t)sig32;
        if (z != 0 && ((z < 0) ^ sign))
            goto invalid;

        if (roundBits != 0)
        {
            if (roundingMode == RoundingMode.Odd)
                z |= 1;

            if (exact)
                state.ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return z;

    invalid:
        state.RaiseFlags(ExceptionFlags.Invalid);
        return state.Int32FromOverflow(sign);
    }

    // softfloat_roundToI64
    public static int_fast64_t RoundToI64(SoftFloatState state, bool sign, uint_fast64_t sig, uint_fast64_t sigExtra, RoundingMode roundingMode, bool exact)
    {
        int_fast64_t z;

        if (roundingMode is RoundingMode.NearMaxMag or RoundingMode.NearEven)
        {
            if (0x8000000000000000 <= sigExtra)
            {
                ++sig;
                if (sig == 0)
                    goto invalid;

                if (roundingMode == RoundingMode.NearEven && sigExtra == 0x8000000000000000)
                    sig &= ~1UL;
            }
        }
        else
        {
            if (sigExtra != 0 && (sign ? roundingMode is RoundingMode.Min or RoundingMode.Odd : roundingMode == RoundingMode.Max))
            {
                ++sig;
                if (sig == 0)
                    goto invalid;

                if (roundingMode == RoundingMode.NearEven && sigExtra == 0x8000000000000000)
                    sig &= ~1UL;
            }
        }

        z = sign ? -(int_fast64_t)sig : (int_fast64_t)sig;
        if (z != 0 && ((z < 0) ^ sign))
            goto invalid;

        if (sigExtra != 0)
        {
            if (roundingMode == RoundingMode.Odd)
                z |= 1L;

            if (exact)
                state.ExceptionFlags |= ExceptionFlags.Inexact;
        }

        return z;

    invalid:
        state.RaiseFlags(ExceptionFlags.Invalid);
        return state.Int64FromOverflow(sign);
    }

    #endregion
}
