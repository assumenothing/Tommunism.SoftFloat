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
using System.Runtime.CompilerServices;

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

partial class Internals
{
    // signF32UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SignF32UI(uint_fast32_t a) => (a >> 31) != 0;

    // expF32UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int_fast16_t ExpF32UI(uint_fast32_t a) => (int_fast16_t)((a >> 23) & 0xFF);

    // fracF32UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint_fast32_t FracF32UI(uint_fast32_t a) => a & 0x007FFFFF;

    // packToF32UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint32_t PackToF32UI(bool sign, int_fast16_t exp, uint_fast32_t sig) =>
        (sign ? (1U << 31) : 0U) + ((uint_fast32_t)exp << 23) + sig;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float32 PackToF32(bool sign, int_fast16_t exp, uint_fast32_t sig) =>
        Float32.FromBitsUI32(PackToF32UI(sign, exp, sig));

    // isNaNF32UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaNF32UI(uint_fast32_t a) => (~a & 0x7F800000) == 0 && (a & 0x007FFFFF) != 0;

    // softfloat_normSubnormalF32Sig
    public static (int_fast16_t exp, uint_fast32_t sig) NormSubnormalF32Sig(uint_fast32_t sig)
    {
        var shiftDist = CountLeadingZeroes32(sig) - 8;
        return (
            exp: 1 - shiftDist,
            sig: sig << shiftDist
        );
    }

    // softfloat_roundPackToF32
    public static Float32 RoundPackToF32(SoftFloatContext context, bool sign, int_fast16_t exp, uint_fast32_t sig)
    {
        var roundingMode = context.Rounding;
        var roundNearEven = roundingMode == RoundingMode.NearEven;
        var roundIncrement = (!roundNearEven && roundingMode != RoundingMode.NearMaxMag)
            ? ((roundingMode == (sign ? RoundingMode.Min : RoundingMode.Max)) ? 0x7FU : 0)
            : 0x40U;
        var roundBits = sig & 0x7F;

        if (0xFD <= (uint_fast16_t)exp)
        {
            if (exp < 0)
            {
                var isTiny = context.DetectTininess == TininessMode.BeforeRounding || exp < -1 || sig + roundIncrement < 0x80000000;
                sig = ShiftRightJam32(sig, -exp);
                exp = 0;
                roundBits = sig & 0x7F;

                if (isTiny && roundBits != 0)
                    context.RaiseFlags(ExceptionFlags.Underflow);
            }
            else if (0xFD < exp || 0x80000000 <= sig + roundIncrement)
            {
                context.RaiseFlags(ExceptionFlags.Overflow | ExceptionFlags.Inexact);
                return Float32.FromBitsUI32(PackToF32UI(sign, 0xFF, 0) - (roundIncrement == 0 ? 1U : 0U));
            }
        }

        sig = (sig + roundIncrement) >> 7;
        if (roundBits != 0)
        {
            context.ExceptionFlags |= ExceptionFlags.Inexact;
            if (roundingMode == RoundingMode.Odd)
            {
                sig |= 1;
                return PackToF32(sign, exp, sig);
            }
        }

        sig &= ~(((roundBits ^ 0x40) == 0 & roundNearEven) ? 1U : 0U);
        if (sig == 0)
            exp = 0;

        return PackToF32(sign, exp, sig);
    }

    // softfloat_normRoundPackToF32
    public static Float32 NormRoundPackToF32(SoftFloatContext context, bool sign, int_fast16_t exp, uint_fast32_t sig)
    {
        var shiftDist = CountLeadingZeroes32(sig) - 1;
        exp -= shiftDist;
        if (7 <= shiftDist && (uint)exp < 0xFD)
        {
            return PackToF32(sign, sig != 0 ? exp : 0, sig << (shiftDist - 7));
        }
        else
        {
            return RoundPackToF32(context, sign, exp, sig << shiftDist);
        }
    }

    // softfloat_addMagsF32
    public static Float32 AddMagsF32(SoftFloatContext context, uint_fast32_t uiA, uint_fast32_t uiB)
    {
        int_fast16_t expA, expB, expDiff, expZ;
        uint_fast32_t sigA, sigB, sigZ;
        bool signZ;

        expA = ExpF32UI(uiA);
        sigA = FracF32UI(uiA);
        expB = ExpF32UI(uiB);
        sigB = FracF32UI(uiB);

        expDiff = expA - expB;
        if (expDiff == 0)
        {
            if (expA == 0)
                return Float32.FromBitsUI32(uiA + sigB);

            if (expA == 0xFF)
            {
                if ((sigA | sigB) != 0)
                    return context.PropagateNaNFloat32Bits(uiA, uiB);

                return Float32.FromBitsUI32(uiA);
            }

            signZ = SignF32UI(uiA);
            expZ = expA;
            sigZ = 0x01000000 + sigA + sigB;
            if ((sigZ & 1) == 0 && expZ < 0xFE)
                return PackToF32(signZ, expZ, sigZ >> 1);

            sigZ <<= 6;
        }
        else
        {
            signZ = SignF32UI(uiA);
            sigA <<= 6;
            sigB <<= 6;
            if (expDiff < 0)
            {
                if (expB == 0xFF)
                {
                    if (sigB != 0)
                        return context.PropagateNaNFloat32Bits(uiA, uiB);

                    return PackToF32(signZ, 0xFF, 0);
                }

                expZ = expB;
                sigA += expA != 0 ? 0x20000000 : sigA;
                sigA = ShiftRightJam32(sigA, -expDiff);
            }
            else
            {
                if (expA == 0xFF)
                {
                    if (sigA != 0)
                        return context.PropagateNaNFloat32Bits(uiA, uiB);

                    return Float32.FromBitsUI32(uiA);
                }

                expZ = expA;
                sigB += expB != 0 ? 0x20000000 : sigB;
                sigB = ShiftRightJam32(sigB, expDiff);
            }

            sigZ = 0x20000000 + sigA + sigB;
            if (sigZ < 0x40000000)
            {
                --expZ;
                sigZ <<= 1;
            }
        }

        return RoundPackToF32(context, signZ, expZ, sigZ);
    }

    // softfloat_subMagsF32
    public static Float32 SubMagsF32(SoftFloatContext context, uint_fast32_t uiA, uint_fast32_t uiB)
    {
        int_fast16_t expA, expB, expDiff, expZ;
        uint_fast32_t sigA, sigB, sigX, sigY;
        int_fast32_t sigDiff;
        int_fast8_t shiftDist;
        bool signZ;

        expA = ExpF32UI(uiA);
        sigA = FracF32UI(uiA);
        expB = ExpF32UI(uiB);
        sigB = FracF32UI(uiB);

        expDiff = expA - expB;
        if (expDiff == 0)
        {
            if (expA == 0xFF)
            {
                if ((sigA | sigB) != 0)
                    return context.PropagateNaNFloat32Bits(uiA, uiB);

                context.RaiseFlags(ExceptionFlags.Invalid);
                return context.DefaultNaNFloat32;
            }

            sigDiff = (int_fast32_t)sigA - (int_fast32_t)sigB;
            if (sigDiff == 0)
                return PackToF32(context.Rounding == RoundingMode.Min, 0, 0);

            if (expA != 0)
                --expA;

            signZ = SignF32UI(uiA);
            if (sigDiff < 0)
            {
                signZ = !signZ;
                sigDiff = -sigDiff;
            }

            Debug.Assert(sigDiff >= 0);
            shiftDist = CountLeadingZeroes32((uint_fast32_t)sigDiff) - 8;
            expZ = expA - shiftDist;
            if (expZ < 0)
            {
                shiftDist = expA;
                expZ = 0;
            }

            return PackToF32(signZ, expZ, (uint_fast32_t)sigDiff << shiftDist);
        }
        else
        {
            signZ = SignF32UI(uiA);
            sigA <<= 7;
            sigB <<= 7;

            if (expDiff < 0)
            {
                signZ = !signZ;
                if (expB == 0xFF)
                {
                    if (sigB != 0)
                        return context.PropagateNaNFloat32Bits(uiA, uiB);

                    return PackToF32(signZ, 0xFF, 0);
                }

                expZ = expB - 1;
                sigX = sigB | 0x40000000;
                sigY = sigA + (expA != 0 ? 0x40000000U : sigA);
                expDiff = -expDiff;
            }
            else
            {
                if (expA == 0xFF)
                {
                    if (sigA != 0)
                        return context.PropagateNaNFloat32Bits(uiA, uiB);

                    return Float32.FromBitsUI32(uiA);
                }

                expZ = expA - 1;
                sigX = sigA | 0x40000000;
                sigY = sigB + (expB != 0 ? 0x40000000 : sigB);
            }

            return NormRoundPackToF32(context, signZ, expZ, sigX - ShiftRightJam32(sigY, expDiff));
        }
    }

    // softfloat_mulAddF32
    public static Float32 MulAddF32(SoftFloatContext context, uint_fast32_t uiA, uint_fast32_t uiB, uint_fast32_t uiC, MulAdd op)
    {
        Debug.Assert(op is MulAdd.None or MulAdd.SubC or MulAdd.SubProd, "Invalid MulAdd operation.");

        bool signA, signB, signC, signProd, signZ;
        int_fast16_t expA, expB, expC, expProd, expZ, expDiff;
        uint_fast32_t sigA, sigB, sigC, magBits, uiZ, sigZ;
        uint_fast64_t sigProd, sig64Z, sig64C;
        int_fast8_t shiftDist;

        signA = SignF32UI(uiA);
        expA = ExpF32UI(uiA);
        sigA = FracF32UI(uiA);
        signB = SignF32UI(uiB);
        expB = ExpF32UI(uiB);
        sigB = FracF32UI(uiB);
        signC = SignF32UI(uiC) ^ (op == MulAdd.SubC);
        expC = ExpF32UI(uiC);
        sigC = FracF32UI(uiC);
        signProd = signA ^ signB ^ (op == MulAdd.SubProd);

        if (expA == 0xFF)
        {
            if (sigA != 0 || (expB == 0xFF && sigB != 0))
                return context.PropagateNaNFloat32Bits(uiA, uiB, uiC);

            magBits = (uint_fast32_t)expB | sigB;
            goto infProdArg;
        }

        if (expB == 0xFF)
        {
            if (sigB != 0)
                return context.PropagateNaNFloat32Bits(uiA, uiB, uiC);

            magBits = (uint_fast32_t)expA | sigA;
            goto infProdArg;
        }

        if (expC == 0xFF)
        {
            if (sigC != 0)
                return context.PropagateNaNFloat32Bits(0, uiC);

            return Float32.FromBitsUI32(uiC);
        }

        if (expA == 0)
        {
            if (sigA == 0)
            {
                if (((uint_fast32_t)expC | sigC) == 0 && signProd != signC)
                    return PackToF32(context.Rounding == RoundingMode.Min, 0, 0);

                return Float32.FromBitsUI32(uiC);
            }

            (expA, sigA) = NormSubnormalF32Sig(sigA);
        }

        if (expB == 0)
        {
            if (sigB == 0)
            {
                if (((uint_fast32_t)expC | sigC) == 0 && signProd != signC)
                    return PackToF32(context.Rounding == RoundingMode.Min, 0, 0);

                return Float32.FromBitsUI32(uiC);
            }

            (expB, sigB) = NormSubnormalF32Sig(sigB);
        }

        expProd = expA + expB - 0x7E;
        sigA = (sigA | 0x00800000) << 7;
        sigB = (sigB | 0x00800000) << 7;
        sigProd = (uint_fast64_t)sigA * sigB;

        if (sigProd < 0x2000000000000000)
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
                sigZ = (uint_fast32_t)ShortShiftRightJam64(sigProd, 31);
                return RoundPackToF32(context, signZ, expZ, sigZ);
            }

            (expC, sigC) = NormSubnormalF32Sig(sigC);
        }

        sigC = (sigC | 0x00800000) << 6;
        expDiff = expProd - expC;

        if (signProd == signC)
        {
            if (expDiff <= 0)
            {
                expZ = expC;
                sigZ = (uint_fast32_t)(sigC + ShiftRightJam64(sigProd, 32 - expDiff));
            }
            else
            {
                expZ = expProd;
                sig64Z = sigProd + ShiftRightJam64((uint_fast64_t)sigC << 32, expDiff);
                sigZ = (uint_fast32_t)ShortShiftRightJam64(sig64Z, 32);
            }

            if (sigZ < 0x40000000)
            {
                --expZ;
                sigZ <<= 1;
            }
        }
        else
        {
            sig64C = (uint_fast64_t)sigC << 32;
            if (expDiff < 0)
            {
                signZ = signC;
                expZ = expC;
                sig64Z = sig64C - ShiftRightJam64(sigProd, -expDiff);
            }
            else if (expDiff == 0)
            {
                expZ = expProd;
                sig64Z = sigProd - sig64C;
                if (sig64Z == 0)
                    return PackToF32(context.Rounding == RoundingMode.Min, 0, 0);

                if ((sig64Z & 0x8000000000000000) != 0)
                {
                    signZ = !signZ;
                    sig64Z = (uint_fast64_t)(-(int_fast64_t)sig64Z);
                }
            }
            else
            {
                expZ = expProd;
                sig64Z = sigProd - ShiftRightJam64(sig64C, expDiff);
            }

            shiftDist = CountLeadingZeroes64(sig64Z) - 1;
            expZ -= shiftDist;
            shiftDist -= 32;
            sigZ = (shiftDist < 0)
                ? (uint_fast32_t)ShortShiftRightJam64(sig64Z, -shiftDist)
                : (uint_fast32_t)sig64Z << shiftDist;
        }

        return RoundPackToF32(context, signZ, expZ, sigZ);

    infProdArg:
        if (magBits != 0)
        {
            uiZ = PackToF32UI(signProd, 0xFF, 0);
            if (expC != 0xFF)
                return Float32.FromBitsUI32(uiZ);

            if (sigC != 0)
                return context.PropagateNaNFloat32Bits(uiZ, uiC);

            if (signProd == signC)
                return Float32.FromBitsUI32(uiZ);
        }

        context.RaiseFlags(ExceptionFlags.Invalid);
        return context.PropagateNaNFloat32Bits(context.DefaultNaNFloat32Bits, uiC);
    }
}
