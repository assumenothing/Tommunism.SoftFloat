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
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tommunism.SoftFloat;

using static Primitives;
using static Specialize;

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

partial class Internals
{
    // signF16UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SignF16UI(uint_fast16_t a) => ((a >> 15) & 1) != 0;

    // expF16UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int_fast8_t ExpF16UI(uint_fast16_t a) => (int_fast8_t)((a >> 10) & 0x1F);

    // fracF16UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint_fast16_t FracF16UI(uint_fast16_t a) => a & 0x03FF;

    // packToF16UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint16_t PackToF16UI(bool sign, int_fast8_t exp, uint_fast16_t sig)
    {
        Debug.Assert(((uint_fast8_t)exp & ~0x1FU) == 0); // TODO: If this is signed, then how are negative values handled?
        Debug.Assert((sig & ~0x03FFU) == 0);
        return (uint16_t)((sign ? (1U << 15) : 0U) | (((uint_fast16_t)exp & 0x1F) << 10) | (sig & 0x03FF));
    }

    // isNaNF16UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaNF16UI(uint_fast16_t a) => (~a & 0x7C00) == 0 && (a & 0x03FF) != 0;

    // softfloat_normSubnormalF16Sig
    public static (int_fast8_t exp, uint_fast16_t sig) NormSubnormalF16Sig(uint_fast16_t sig)
    {
        var shiftDist = CountLeadingZeroes16(sig) - 5;
        return (
            exp: 1 - shiftDist,
            sig: sig << shiftDist
        );
    }

    // softfloat_roundPackToF16
    public static Float16 RoundPackToF16(SoftFloatState state, bool sign, int_fast16_t exp, uint_fast16_t sig)
    {
        var roundingMode = state.RoundingMode;
        var roundNearEven = roundingMode == RoundingMode.NearEven;
        var roundIncrement = (!roundNearEven && roundingMode != RoundingMode.NearMaxMag)
            ? ((roundingMode == (sign ? RoundingMode.Min : RoundingMode.Max)) ? 0xFU : 0)
            : 0x8U;
        var roundBits = sig & 0xF;

        if (0x1D <= (uint_fast16_t)exp)
        {
            if (exp < 0)
            {
                var isTiny = state.DetectTininess == Tininess.BeforeRounding || exp < -1 || sig + roundIncrement < 0x8000;
                sig = ShiftRightJam32(sig, -exp);
                exp = 0;
                roundBits = sig & 0xF;

                if (isTiny && roundBits != 0)
                    state.RaiseFlags(ExceptionFlags.Underflow);
            }
            else if (0x1D < exp || 0x8000 <= sig + roundIncrement)
            {
                state.RaiseFlags(ExceptionFlags.Overflow | ExceptionFlags.Inexact);
                return Float16.FromUI16((ushort)(PackToF16UI(sign, 0x1F, 0) - (roundIncrement == 0 ? 1U : 0U)));
            }
        }

        sig = ((sig + roundIncrement) >> 4);
        if (roundBits != 0)
        {
            state.ExceptionFlags |= ExceptionFlags.Inexact;
            if (roundingMode == RoundingMode.Odd)
            {
                sig |= 1;
                return Float16.FromUI16(PackToF16UI(sign, exp, sig));
            }
        }

        sig &= ~(((roundBits ^ 8) == 0 & roundNearEven) ? 1U : 0U);
        if (sig == 0)
            exp = 0;

        return Float16.FromUI16(PackToF16UI(sign, exp, sig));
    }

    // softfloat_normRoundPackToF16
    public static Float16 NormRoundPackToF16(SoftFloatState state, bool sign, int_fast16_t exp, uint_fast16_t sig)
    {
        var shiftDist = CountLeadingZeroes16(sig) - 1;
        exp -= shiftDist;
        if (4 <= shiftDist && (uint)exp < 0x1D)
        {
            return Float16.FromUI16((ushort)PackToF16UI(sign, sig != 0 ? exp : 0, sig << (shiftDist - 4)));
        }
        else
        {
            return RoundPackToF16(state, sign, exp, sig << shiftDist);
        }
    }

    // softfloat_addMagsF16
    public static Float16 AddMagsF16(SoftFloatState state, uint_fast16_t uiA, uint_fast16_t uiB)
    {
        int_fast8_t expA, expB, expDiff, expZ, shiftDist;
        uint_fast16_t sigA, sigB, sigZ, uiZ, sigX, sigY;
        uint_fast32_t sig32Z;
        bool signZ;

        expA = ExpF16UI(uiA);
        sigA = FracF16UI(uiA);
        expB = ExpF16UI(uiB);
        sigB = FracF16UI(uiB);

        expDiff = expA - expB;
        if (expDiff == 0)
        {
            if (expA == 0)
            {
                uiZ = uiA + sigB;
                return Float16.FromUI16((ushort)uiZ);
            }

            if (expA == 0x1F)
            {
                if ((sigA | sigB) != 0)
                {
                    uiZ = PropagateNaNF16UI(state, uiA, uiB);
                    return Float16.FromUI16((ushort)uiZ);
                }

                uiZ = uiA;
                return Float16.FromUI16((ushort)uiZ);
            }

            signZ = SignF16UI(uiA);
            expZ = expA;
            sigZ = 0x0800 + sigA + sigB;
            if ((sigZ & 1) == 0 && expZ < 0x1E)
            {
                uiZ = PackToF16UI(signZ, expZ, sigZ >> 1);
                return Float16.FromUI16((ushort)uiZ);
            }

            sigZ <<= 3;
        }
        else
        {
            signZ = SignF16UI(uiA);
            if (expDiff < 0)
            {
                if (expB == 0x1F)
                {
                    if (sigB != 0)
                    {
                        uiZ = PropagateNaNF16UI(state, uiA, uiB);
                        return Float16.FromUI16((ushort)uiZ);
                    }

                    uiZ = PackToF16UI(signZ, 0x1F, 0);
                    return Float16.FromUI16((ushort)uiZ);
                }

                if (expDiff <= -13)
                {
                    uiZ = PackToF16UI(signZ, expB, sigB);
                    if (((uint_fast8_t)expA | sigA) != 0)
                        goto addEpsilon;

                    return Float16.FromUI16((ushort)uiZ);
                }

                expZ = expB;
                sigX = sigB | 0x0400;
                sigY = sigA + (expA != 0 ? 0x0400 : sigA);
                shiftDist = 19 + expDiff;
            }
            else
            {
                uiZ = uiA;
                if (expA == 0x1F)
                {
                    if (sigA != 0)
                    {
                        uiZ = PropagateNaNF16UI(state, uiA, uiB);
                        return Float16.FromUI16((ushort)uiZ);
                    }

                    return Float16.FromUI16((ushort)uiZ);
                }

                if (13 <= expDiff)
                {
                    if (((uint_fast8_t)expB | sigB) != 0)
                        goto addEpsilon;

                    return Float16.FromUI16((ushort)uiZ);
                }

                expZ = expA;
                sigX = sigA | 0x0400;
                sigY = sigB + (expB != 0 ? 0x0400 : sigB);
                shiftDist = 19 - expDiff;
            }

            sig32Z = (sigX << 19) + (sigY << shiftDist);
            if (sig32Z < 0x40000000)
            {
                --expZ;
                sig32Z <<= 1;
            }

            sigZ = sig32Z >> 16;
            if ((sig32Z & 0xFFFF) != 0)
            {
                sigZ |= 1;
            }
            else
            {
                if ((sigZ & 0xF) == 0 && expZ < 0x1E)
                {
                    sigZ >>= 4;
                    uiZ = PackToF16UI(signZ, expZ, sigZ);
                    return Float16.FromUI16((ushort)uiZ);
                }
            }
        }

        return RoundPackToF16(state, signZ, expZ, sigZ);

    addEpsilon:
        var roundingMode = state.RoundingMode;
        if (roundingMode != RoundingMode.NearEven)
        {
            if (roundingMode == (SignF16UI(uiZ) ? RoundingMode.Min : RoundingMode.Max))
            {
                ++uiZ;
                if ((ushort)(uiZ << 1) == 0xF800U)
                    state.RaiseFlags(ExceptionFlags.Overflow | ExceptionFlags.Inexact);
            }
            else
            {
                uiZ |= 1;
            }
        }

        state.ExceptionFlags |= ExceptionFlags.Inexact;
        return Float16.FromUI16((ushort)uiZ);
    }

    // softfloat_subMagsF16
    public static Float16 SubMagsF16(SoftFloatState state, uint_fast16_t uiA, uint_fast16_t uiB)
    {
        int_fast8_t expA, expB, expDiff, expZ, shiftDist;
        uint_fast16_t sigA, sigB, uiZ, sigZ, sigX, sigY;
        int_fast16_t sigDiff;
        bool signZ;

        expA = ExpF16UI(uiA);
        sigA = FracF16UI(uiA);
        expB = ExpF16UI(uiB);
        sigB = FracF16UI(uiB);

        expDiff = expA - expB;
        if (expDiff == 0)
        {
            if (expA == 0x1F)
            {
                if ((sigA | sigB) != 0)
                {
                    uiZ = PropagateNaNF16UI(state, uiA, uiB);
                    return Float16.FromUI16((ushort)uiZ);
                }

                state.RaiseFlags(ExceptionFlags.Invalid);
                uiZ = DefaultNaNF16UI;
                return Float16.FromUI16((ushort)uiZ);
            }

            sigDiff = (int_fast16_t)sigA - (int_fast16_t)sigB;
            if (sigDiff == 0)
            {
                uiZ = PackToF16UI(state.RoundingMode == RoundingMode.Min, 0, 0);
                return Float16.FromUI16((ushort)uiZ);
            }

            if (expA != 0)
                --expA;

            signZ = SignF16UI(uiA);
            if (sigDiff < 0)
            {
                signZ = !signZ;
                sigDiff = -sigDiff;
            }

            Debug.Assert(sigDiff >= 0);
            shiftDist = CountLeadingZeroes16((uint_fast16_t)sigDiff) - 5;
            expZ = expA - shiftDist;
            if (expZ < 0)
            {
                shiftDist = expA;
                expZ = 0;
            }

            uiZ = PackToF16UI(signZ, expZ, (uint_fast16_t)sigDiff << shiftDist);
            return Float16.FromUI16((ushort)uiZ);
        }
        else
        {
            signZ = SignF16UI(uiA);
            if (expDiff < 0)
            {
                signZ = !signZ;
                if (expB == 0x1F)
                {
                    if (sigB != 0)
                    {
                        uiZ = PropagateNaNF16UI(state, uiA, uiB);
                        return Float16.FromUI16((ushort)uiZ);
                    }

                    uiZ = PackToF16UI(signZ, 0x1F, 0);
                    return Float16.FromUI16((ushort)uiZ);
                }

                if (expDiff <= -13)
                {
                    uiZ = PackToF16UI(signZ, expB, sigB);
                    if (((uint_fast16_t)expA | sigA) != 0)
                        goto subEpsilon;

                    return Float16.FromUI16((ushort)uiZ);
                }

                expZ = expA + 19;
                sigX = sigB | 0x0400;
                sigY = sigA + (expA != 0 ? 0x0400 : sigA);
                expDiff = -expDiff;
            }
            else
            {
                uiZ = uiA;
                if (expA == 0x1F)
                {
                    if (sigA != 0)
                    {
                        uiZ = PropagateNaNF16UI(state, uiA, uiB);
                        return Float16.FromUI16((ushort)uiZ);
                    }

                    return Float16.FromUI16((ushort)uiZ);
                }

                if (13 <= expDiff)
                {
                    if (((uint_fast16_t)expB | sigB) != 0)
                        goto subEpsilon;

                    return Float16.FromUI16((ushort)uiZ);
                }

                expZ = expB + 19;
                sigX = sigA | 0x0400;
                sigY = sigB + (expB != 0 ? 0x0400 : sigB);
            }

            uint_fast32_t sig32Z = (sigX << expDiff) - sigY;
            shiftDist = CountLeadingZeroes16(sig32Z) - 1;
            sig32Z <<= shiftDist;
            expZ -= shiftDist;
            sigZ = sig32Z >> 16;
            if ((sig32Z & 0xFFFF) != 0)
            {
                sigZ |= 1;
            }
            else
            {
                if ((sigZ & 0xF) == 0 && (uint)expZ < 0x1E)
                {
                    sigZ >>= 4;
                    uiZ = PackToF16UI(signZ, expZ, sigZ);
                    return Float16.FromUI16((ushort)uiZ);
                }
            }

            return RoundPackToF16(state, signZ, expZ, sigZ);
        }

    subEpsilon:
        var roundingMode = state.RoundingMode;
        if (roundingMode != RoundingMode.NearEven)
        {
            if (roundingMode == RoundingMode.MinMag || (roundingMode == (SignF16UI(uiZ) ? RoundingMode.Max : RoundingMode.Min)))
            {
                --uiZ;
            }
            else if (roundingMode == RoundingMode.Odd)
            {
                uiZ = (uiZ - 1) | 1;
            }
        }

        state.ExceptionFlags |= ExceptionFlags.Inexact;
        return Float16.FromUI16((ushort)uiZ);
    }

    // softfloat_mulAddF16
    public static Float16 MulAddF16(SoftFloatState state, uint_fast16_t uiA, uint_fast16_t uiB, uint_fast16_t uiC, MulAdd op)
    {
        Debug.Assert(op is MulAdd.SubC or MulAdd.SubProd, "Invalid MulAdd operation.");

        bool signA, signB, signC, signProd, signZ;
        int_fast8_t expA, expB, expC, expProd, expZ, expDiff, shiftDist;
        uint_fast16_t sigA, sigB, sigC, magBits, uiZ, sigZ;
        uint_fast32_t sigProd, sig32Z, sig32C;

        signA = SignF16UI(uiA);
        expA = ExpF16UI(uiA);
        sigA = FracF16UI(uiA);
        signB = SignF16UI(uiB);
        expB = ExpF16UI(uiB);
        sigB = FracF16UI(uiB);
        signC = SignF16UI(uiC) ^ (op == MulAdd.SubC);
        expC = ExpF16UI(uiC);
        sigC = FracF16UI(uiC);
        signProd = signA ^ signB ^ (op == MulAdd.SubProd);

        if (expA == 0x1F)
        {
            if (sigA != 0 || (expB == 0x1F && sigB != 0))
            {
                uiZ = PropagateNaNF16UI(state, uiA, uiB);
                uiZ = PropagateNaNF16UI(state, uiZ, uiC);
                return Float16.FromUI16((ushort)uiZ);
            }

            magBits = (uint_fast16_t)expB | sigB;
            goto infProdArg;
        }

        if (expB == 0x1F)
        {
            if (sigB != 0)
            {
                uiZ = PropagateNaNF16UI(state, uiA, uiB);
                uiZ = PropagateNaNF16UI(state, uiZ, uiC);
                return Float16.FromUI16((ushort)uiZ);
            }

            magBits = (uint_fast16_t)expA | sigA;
            goto infProdArg;
        }

        if (expC == 0x1F)
        {
            if (sigC != 0)
            {
                uiZ = 0;
                uiZ = PropagateNaNF16UI(state, uiZ, uiC);
                return Float16.FromUI16((ushort)uiZ);
            }

            uiZ = uiC;
            return Float16.FromUI16((ushort)uiZ);
        }

        if (expA == 0)
        {
            if (sigA == 0)
            {
                uiZ = uiC;
                if (((uint_fast16_t)expC | sigC) == 0 && signProd != signC)
                    uiZ = PackToF16UI(state.RoundingMode == RoundingMode.Min, 0, 0);

                return Float16.FromUI16((ushort)uiZ);
            }

            (expA, sigA) = NormSubnormalF16Sig(sigA);
        }

        if (expB == 0)
        {
            if (sigB == 0)
            {
                uiZ = uiC;
                if (((uint_fast16_t)expC | sigC) == 0 && signProd != signC)
                    uiZ = PackToF16UI(state.RoundingMode == RoundingMode.Min, 0, 0);

                return Float16.FromUI16((ushort)uiZ);
            }

            (expB, sigB) = NormSubnormalF16Sig(sigB);
        }

        expProd = expA + expB - 0xE;
        sigA = (sigA | 0x0400) << 4;
        sigB = (sigB | 0x0400) << 4;
        sigProd = sigA * sigB;

        if (sigProd < 0x20000000)
        {
            --expProd;
            sigProd <<= 1;
        }

        signZ = signProd;
        if (expC == 0)
        {
            if (sigC == 0)
            {
                expZ = expProd - 1;
                sigZ = (sigProd >> 15) | ((sigProd & 0x7FFF) != 0 ? 1U : 0U);
                return RoundPackToF16(state, signZ, expZ, sigZ);
            }

            (expC, sigC) = NormSubnormalF16Sig(sigC);
        }

        sigC = (sigC | 0x0400) << 3;
        expDiff = expProd - expC;

        if (signProd == signC)
        {
            if (expDiff <= 0)
            {
                expZ = expC;
                sigZ = sigC + ShiftRightJam32(sigProd, 16 - expDiff);
            }
            else
            {
                expZ = expProd;
                sig32Z = sigProd + ShiftRightJam32(sigC << 16, expDiff);
                sigZ = (sig32Z >> 16) | ((sig32Z & 0xFFFF) != 0 ? 1U : 0U);
            }

            if (sigZ < 0x4000)
            {
                --expZ;
                sigZ <<= 1;
            }
        }
        else
        {
            sig32C = sigC << 16;
            if (expDiff < 0)
            {
                signZ = signC;
                expZ = expC;
                sig32Z = sig32C - ShiftRightJam32(sigProd, -expDiff);
            }
            else if (expDiff == 0)
            {
                expZ = expProd;
                sig32Z = sigProd - sig32C;
                if (sig32Z == 0)
                {
                    uiZ = PackToF16UI(state.RoundingMode == RoundingMode.Min, 0, 0);
                    return Float16.FromUI16((ushort)uiZ);
                }

                if ((sig32Z & 0x80000000) != 0)
                {
                    signZ = !signZ;
                    sig32Z = (uint_fast32_t)(-(int_fast32_t)sig32Z);
                }
            }
            else
            {
                expZ = expProd;
                sig32Z = sigProd - ShiftRightJam32(sig32C, expDiff);
            }

            shiftDist = CountLeadingZeroes32(sig32Z) - 1;
            expZ -= shiftDist;
            shiftDist -= 16;
            sigZ = (shiftDist < 0)
                ? (sig32Z >> (-shiftDist)) | ((sig32Z << (shiftDist & 31)) != 0 ? 1U : 0U)
                : sig32Z << shiftDist;
        }

        return RoundPackToF16(state, signZ, expZ, sigZ);

    infProdArg:
        if (magBits != 0)
        {
            uiZ = PackToF16UI(signProd, 0x1F, 0);
            if (expC != 0x1F)
                return Float16.FromUI16((ushort)uiZ);

            if (sigC != 0)
            {
                uiZ = PropagateNaNF16UI(state, uiZ, uiC);
                return Float16.FromUI16((ushort)uiZ);
            }

            if (signProd == signC)
                return Float16.FromUI16((ushort)uiZ);
        }

        state.RaiseFlags(ExceptionFlags.Invalid);
        uiZ = DefaultNaNF16UI;
        uiZ = PropagateNaNF16UI(state, uiZ, uiC);
        return Float16.FromUI16((ushort)uiZ);
    }
}
