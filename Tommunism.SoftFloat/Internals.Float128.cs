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
    // signF128UI64
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SignF128UI64(uint64_t a64) => (a64 >> 63) != 0;

    // expF128UI64
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int_fast32_t ExpF128UI64(uint64_t a64) => (int_fast32_t)((uint)(a64 >> 48) & 0x7FFF);

    // fracF128UI64
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint_fast64_t FracF128UI64(uint64_t a64) => a64 & 0x0000FFFFFFFFFFFF;

    // packToF128UI64
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint64_t PackToF128UI64(bool sign, int_fast32_t exp, uint_fast64_t sig64)
    {
        Debug.Assert(((uint_fast16_t)exp & ~0x7FFFU) == 0); // TODO: If this is signed, then how are negative values handled?
        Debug.Assert((sig64 & ~0x0000FFFFFFFFFFFFU) == 0);
        return (sign ? (1UL << 63) : 0UL) | ((uint64_t)((uint_fast32_t)exp & 0x7FFF) << 48) | (sig64 & 0x0000FFFFFFFFFFFF);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float128 PackToF128(bool sign, int_fast32_t exp, uint_fast64_t sig64, uint_fast64_t sig0) =>
        Float128.FromBitsUI128(v64: PackToF128UI64(sign, exp, sig64), v0: sig0);

    // isNaNF128UI
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaNF128UI(ulong a64, ulong a0) => (~a64 & 0x7FFF000000000000) == 0 && (a0 != 0 || (a64 & 0x0000FFFFFFFFFFFF) != 0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int_fast32_t exp, SFUInt128 sig) NormSubnormalF128Sig(SFUInt128 sig) => NormSubnormalF128Sig(sig.V64, sig.V00);

    // softfloat_normSubnormalF128Sig
    public static (int_fast32_t exp, SFUInt128 sig) NormSubnormalF128Sig(uint_fast64_t sig64, uint_fast64_t sig0)
    {
        int shiftDist;
        if (sig64 == 0)
        {
            shiftDist = CountLeadingZeroes64(sig0) - 15;
            return (
                exp: -64 - shiftDist,
                sig: (shiftDist < 0)
                    ? new SFUInt128(
                        v64: sig0 >> (-shiftDist),
                        v0: sig0 << shiftDist
                    )
                    : new SFUInt128(
                        v64: sig0 << shiftDist,
                        v0: 0
                    )
            );
        }
        else
        {
            shiftDist = CountLeadingZeroes64(sig64) - 15;
            return (
                exp: 1 - shiftDist,
                sig: ShortShiftLeft128(sig64, sig0, shiftDist)
            );
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float128 RoundPackToF128(SoftFloatContext context, bool sign, int_fast32_t exp, SFUInt128 sig, uint_fast64_t sigExtra) =>
        RoundPackToF128(context, sign, exp, sig.V00, sig.V64, sigExtra);

    // softfloat_roundPackToF128
    public static Float128 RoundPackToF128(SoftFloatContext context, bool sign, int_fast32_t exp, uint_fast64_t sig64, uint_fast64_t sig0, uint_fast64_t sigExtra)
    {
        var roundingMode = context.Rounding;
        var roundNearEven = roundingMode == RoundingMode.NearEven;
        var roundIncrement = (!roundNearEven && roundingMode != RoundingMode.NearMaxMag)
            ? (roundingMode == (sign ? RoundingMode.Min : RoundingMode.Max) && sigExtra != 0)
            : (0x8000000000000000 <= sigExtra);

        if (0x7FFD <= (uint32_t)exp)
        {
            if (exp < 0)
            {
                var isTiny = context.DetectTininess == TininessMode.BeforeRounding || exp < -1 || !roundIncrement ||
                    LT128(sig64, sig0, 0x0001FFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);
                (sigExtra, sig64, sig0) = ShiftRightJam128Extra(sig64, sig0, sigExtra, -exp);
                exp = 0;
                if (isTiny && sigExtra != 0)
                    context.RaiseFlags(ExceptionFlags.Underflow);

                roundIncrement = (!roundNearEven && roundingMode != RoundingMode.NearMaxMag)
                    ? (roundingMode == (sign ? RoundingMode.Min : RoundingMode.Max) && sigExtra != 0)
                    : (0x8000000000000000 <= sigExtra);
            }
            else if (0x7FFD < exp || (exp == 0x7FFD && EQ128(sig64, sig0, 0x0001FFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF) && roundIncrement))
            {
                context.RaiseFlags(ExceptionFlags.Overflow | ExceptionFlags.Inexact);
                return (roundNearEven || roundingMode == RoundingMode.NearMaxMag || roundingMode == (sign ? RoundingMode.Min : RoundingMode.Max))
                    ? PackToF128(sign, 0x7FFF, 0, 0)
                    : PackToF128(sign, 0x7FFE, 0x0000FFFFFFFFFFFF, 0xFFFFFFFFFFFFFFFF);
            }
        }

        if (sigExtra != 0)
        {
            context.ExceptionFlags |= ExceptionFlags.Inexact;
            if (roundingMode == RoundingMode.Odd)
                return PackToF128(sign, exp, sig64, sig0 | 1);
        }

        if (roundIncrement)
        {
            (sig64, sig0) = Add128(sig64, sig0, 0, 1);
            sig0 &= ~((sigExtra & 0x7FFFFFFFFFFFFFFF) == 0 && roundNearEven ? 1UL : 0);
        }
        else
        {
            if ((sig64 | sig0) == 0)
                exp = 0;
        }

        return PackToF128(sign, exp, sig64, sig0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Float128 NormRoundPackToF128(SoftFloatContext context, bool sign, int_fast32_t exp, SFUInt128 sig) =>
        NormRoundPackToF128(context, sign, exp, sig.V64, sig.V00);

    // softfloat_normRoundPackToF128
    public static Float128 NormRoundPackToF128(SoftFloatContext context, bool sign, int_fast32_t exp, uint_fast64_t sig64, uint_fast64_t sig0)
    {
        uint_fast64_t sigExtra;

        if (sig64 == 0)
        {
            exp -= 64;
            sig64 = sig0;
            sig0 = 0;
        }

        var shiftDist = CountLeadingZeroes64(sig64) - 15;
        exp -= shiftDist;
        if (0 <= shiftDist)
        {
            if (shiftDist != 0)
                (sig64, sig0) = ShortShiftLeft128(sig64, sig0, shiftDist);

            if ((uint32_t)exp < 0x7FFD)
                return PackToF128(sign, (int_fast32_t)sig64 | (sig0 != 0 ? exp : 0), sig64, sig0);

            sigExtra = 0;
        }
        else
        {
            (sigExtra, sig64, sig0) = ShortShiftRightJam128Extra(sig64, sig0, 0, -shiftDist);
        }

        return RoundPackToF128(context, sign, exp, sig64, sig0, sigExtra);
    }

    // softfloat_addMagsF128
    public static Float128 AddMagsF128(SoftFloatContext context, uint_fast64_t uiA64, uint_fast64_t uiA0, uint_fast64_t uiB64, uint_fast64_t uiB0, bool signZ)
    {
        int_fast32_t expA, expB, expDiff, expZ;
        SFUInt128 sigA, sigB, sigZ;
        uint_fast64_t sigZExtra;

        expA = ExpF128UI64(uiA64);
        sigA = new SFUInt128(v64: FracF128UI64(uiA64), v0: uiA0);
        expB = ExpF128UI64(uiB64);
        sigB = new SFUInt128(v64: FracF128UI64(uiB64), v0: uiB0);

        expDiff = expA - expB;
        if (expDiff == 0)
        {
            if (expA == 0x7FFF)
            {
                return ((sigA.V64 | sigA.V00 | sigB.V64 | sigB.V00) != 0)
                    ? context.PropagateNaNFloat128Bits(uiA64, uiA0, uiB64, uiB0)
                    : Float128.FromBitsUI128(v64: uiA64, v0: uiA0);
            }

            sigZ = sigA + sigB;
            if (expA == 0)
                return PackToF128(signZ, 0, sigZ.V64, sigZ.V00);

            expZ = expA;
            sigZ.V64 |= 0x0002000000000000;
            sigZExtra = 0;
        }
        else
        {
            if (expDiff < 0)
            {
                if (expB == 0x7FFF)
                {
                    return sigB.IsZero
                        ? context.PropagateNaNFloat128Bits(uiA64, uiA0, uiB64, uiB0)
                        : PackToF128(signZ, 0x7FFF, 0, 0);
                }

                expZ = expB;
                if (expA != 0)
                {
                    sigA.V64 |= 0x0001000000000000;
                }
                else
                {
                    ++expDiff;
                    sigZExtra = 0;
                    if (expDiff == 0)
                        goto newlyAligned;
                }

                (sigZExtra, sigA) = ShiftRightJam128Extra(sigA, 0, -expDiff);
            }
            else
            {
                if (expA == 0x7FFF)
                {
                    return sigA.IsZero
                        ? context.PropagateNaNFloat128Bits(uiA64, uiA0, uiB64, uiB0)
                        : Float128.FromBitsUI128(v64: uiA64, v0: uiA0);
                }

                expZ = expA;
                if (expB != 0)
                {
                    sigB.V64 |= 0x0001000000000000;
                }
                else
                {
                    --expDiff;
                    sigZExtra = 0;
                    if (expDiff == 0)
                        goto newlyAligned;
                }

                (sigZExtra, sigB) = ShiftRightJam128Extra(sigB, 0, expDiff);
            }

        newlyAligned:
            sigA.V64 |= 0x0001000000000000;
            sigZ = sigA + sigB;
            --expZ;
            if (sigZ.V64 < 0x0002000000000000)
                return RoundPackToF128(context, signZ, expZ, sigZ, sigZExtra);

            ++expZ;
        }

        (sigZExtra, sigZ) = ShortShiftRightJam128Extra(sigZ, sigZExtra, 1);
        return RoundPackToF128(context, signZ, expZ, sigZ, sigZExtra);
    }

    // softfloat_subMagsF128
    public static Float128 SubMagsF128(SoftFloatContext context, uint_fast64_t uiA64, uint_fast64_t uiA0, uint_fast64_t uiB64, uint_fast64_t uiB0, bool signZ)
    {
        int_fast32_t expA, expB, expDiff, expZ;
        SFUInt128 sigA, sigB, sigZ;

        expA = ExpF128UI64(uiA64);
        sigA = new SFUInt128(v64: FracF128UI64(uiA64), v0: uiA0);
        expB = ExpF128UI64(uiB64);
        sigB = new SFUInt128(v64: FracF128UI64(uiB64), v0: uiB0);

        sigA <<= 4;
        sigB <<= 4;
        expDiff = expA - expB;

        if (expDiff == 0)
        {
            if (expA == 0x7FFF)
            {
                if ((sigA.V64 | sigA.V00 | sigB.V64 | sigB.V00) != 0)
                    return context.PropagateNaNFloat128Bits(uiA64, uiA0, uiB64, uiB0);

                context.RaiseFlags(ExceptionFlags.Invalid);
                return context.DefaultNaNFloat128;
            }

            expZ = expA;
            if (expZ == 0)
                expZ = 1;

            // Use CompareTo() and a switch statement instead of spaghetti code (comparison operators are more computationally expensive).
            switch (sigA.CompareTo(sigB))
            {
                case 1:
                {
                    sigZ = sigA - sigB;
                    break;
                }
                case -1:
                {
                    signZ = !signZ;
                    sigZ = sigB - sigA;
                    break;
                }
                default:
                {
                    return PackToF128(context.Rounding == RoundingMode.Min, 0, 0, 0);
                }
            }
        }
        else if (0 < expDiff)
        {
            if (expA == 0x7FFF)
            {
                return sigA.IsZero
                    ? context.PropagateNaNFloat128Bits(uiA64, uiA0, uiB64, uiB0)
                    : Float128.FromBitsUI128(v64: uiA64, v0: uiA0);
            }

            if (expB != 0)
            {
                sigB.V64 |= 0x0010000000000000;
                sigB = ShiftRightJam128(sigB, expDiff);
            }
            else
            {
                --expDiff;
                if (expDiff != 0)
                    sigB = ShiftRightJam128(sigB, expDiff);
            }

            expZ = expA;
            sigA.V64 |= 0x0010000000000000;

            sigZ = sigA - sigB;
        }
        else //if (expDiff < 0)
        {
            if (expB == 0x7FFF)
            {
                return sigB.IsZero
                    ? context.PropagateNaNFloat128Bits(uiA64, uiA0, uiB64, uiB0)
                    : PackToF128(!signZ, 0x7FFF, 0, 0);
            }

            if (expA != 0)
            {
                sigA.V64 |= 0x0010000000000000;
                sigA = ShiftRightJam128(sigA, -expDiff);
            }
            else
            {
                ++expDiff;
                if (expDiff != 0)
                    sigA = ShiftRightJam128(sigA, -expDiff);
            }

            expZ = expB;
            sigB.V64 |= 0x0010000000000000;

            signZ = !signZ;
            sigZ = sigB - sigA;
        }

        return NormRoundPackToF128(context, signZ, expZ - 5, sigZ);
    }

    // softfloat_mulAddF128
    public static Float128 MulAddF128(SoftFloatContext context, uint_fast64_t uiA64, uint_fast64_t uiA0, uint_fast64_t uiB64, uint_fast64_t uiB0, uint_fast64_t uiC64, uint_fast64_t uiC0, MulAdd op)
    {
        Debug.Assert(op is MulAdd.None or MulAdd.SubC or MulAdd.SubProd, "Invalid MulAdd operation.");

        bool signA, signB, signC, signZ;
        int_fast32_t expA, expB, expC, expZ, shiftDist, expDiff;
        SFUInt128 sigA, sigB, sigC, uiZ, sigZ, x128;
        uint_fast64_t magBits, sigZExtra, sig256Z0;
        SFUInt256 sig256Z, sig256C;

        signA = SignF128UI64(uiA64);
        expA = ExpF128UI64(uiA64);
        sigA = new SFUInt128(v64: FracF128UI64(uiA64), v0: uiA0);
        signB = SignF128UI64(uiB64);
        expB = ExpF128UI64(uiB64);
        sigB = new SFUInt128(v64: FracF128UI64(uiB64), v0: uiB0);
        signC = SignF128UI64(uiC64) ^ (op == MulAdd.SubC);
        expC = ExpF128UI64(uiC64);
        sigC = new SFUInt128(v64: FracF128UI64(uiC64), v0: uiC0);
        signZ = signA ^ signB ^ (op == MulAdd.SubProd);

        if (expA == 0x7FFF)
        {
            if (!sigA.IsZero || (expB == 0x7FFF && !sigB.IsZero))
                return context.PropagateNaNFloat128Bits(uiA64, uiA0, uiB64, uiB0, uiC64, uiC0);

            magBits = (uint_fast32_t)expB | sigB.V64 | sigB.V00;
            goto infProdArg;
        }

        if (expB == 0x7FFF)
        {
            if (!sigB.IsZero)
                return context.PropagateNaNFloat128Bits(uiA64, uiA0, uiB64, uiB0, uiC64, uiC0);

            magBits = (uint_fast32_t)expA | sigA.V64 | sigA.V00;
            goto infProdArg;
        }

        if (expC == 0x7FFF)
        {
            if (!sigC.IsZero)
            {
                uiZ = SFUInt128.Zero;
                return context.PropagateNaNFloat128Bits(uiZ.V64, uiZ.V00, uiC64, uiC0);
            }

            return Float128.FromBitsUI128(v64: uiC64, v0: uiC0);
        }

        if (expA == 0)
        {
            if (sigA.IsZero)
            {
                if (((uint_fast32_t)expC | sigC.V64 | sigC.V00) == 0 && signZ != signC)
                    return PackToF128(context.Rounding == RoundingMode.Min, 0, 0, 0);

                return Float128.FromBitsUI128(v64: uiC64, v0: uiC0);
            }

            (expA, sigA) = NormSubnormalF128Sig(sigA);
        }

        if (expB == 0)
        {
            if (sigB.IsZero)
            {
                if (((uint_fast32_t)expC | sigC.V64 | sigC.V00) == 0 && signZ != signC)
                    return PackToF128(context.Rounding == RoundingMode.Min, 0, 0, 0);

                return Float128.FromBitsUI128(v64: uiC64, v0: uiC0);
            }

            (expB, sigB) = NormSubnormalF128Sig(sigB);
        }

        expZ = expA + expB - 0x3FFE;
        sigA.V64 |= 0x0001000000000000;
        sigB.V64 |= 0x0001000000000000;
        sigA <<= 8;
        sigB <<= 15;

        sig256Z = sigA * sigB;
        sigZ = sig256Z.V128_UI128; // IndexWord(4, 3) & IndexWord(4, 2)

        shiftDist = 0;
        if ((sigZ.V64 & 0x0100000000000000) == 0)
        {
            --expZ;
            shiftDist = -1;
        }

        if (expC == 0)
        {
            if (sigC.IsZero)
            {
                shiftDist += 8;
                sigZExtra = sig256Z.V064 | sig256Z.V000;
                sigZExtra = (sigZ.V00 << (64 - shiftDist)) | (sigZExtra != 0 ? 1UL : 0);
                sigZ >>= shiftDist;
                return RoundPackToF128(context, signZ, expZ - 1, sigZ, sigZExtra);
            }

            (expC, sigC) = NormSubnormalF128Sig(sigC);
        }

        sigC.V64 |= 0x0001000000000000;
        sigC <<= 8;

        expDiff = expZ - expC;
        if (expDiff < 0)
        {
            expZ = expC;
            if (signZ == signC || expDiff < -1)
            {
                shiftDist -= expDiff;
                if (shiftDist != 0)
                    sigZ = ShiftRightJam128(sigZ, shiftDist);
            }
            else
            {
                if (shiftDist == 0)
                {
                    x128 = ShortShiftRightJam128(sig256Z.V064, sig256Z.V000, 1);
                    sig256Z.V064 = (sigZ.V00 << 63) | x128.V64;
                    sig256Z.V000 = x128.V00;
                    sigZ >>= 1;
                    sig256Z.V128_UI128 = sigZ;
                }
            }
        }
        else
        {
            if (shiftDist != 0)
                sig256Z += sig256Z; // <<= 1

            if (expDiff == 0)
            {
                sigZ = sig256Z.V128_UI128; // IndexWord(4, 3) & IndexWord(4, 2)
            }
            else
            {
                // Compiler thinks that it isn't used, because of the SkipInit hacks later in the code.
                sig256C = new SFUInt256(v128: sigC, v0: SFUInt128.Zero);
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                sig256C = ShiftRightJam256M(sig256C, expDiff);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
            }
        }

        shiftDist = 8;
        if (signZ == signC)
        {
            if (expDiff <= 0)
            {
                sigZ = sigC + sigZ;
            }
            else
            {
                Unsafe.SkipInit(out sig256C); // prevent compiler error -- it's always assigned
                sig256Z += sig256C;
                sigZ = sig256Z.V128_UI128;
            }

            if ((sigZ.V64 & 0x0200000000000000) != 0)
            {
                ++expZ;
                shiftDist = 9;
            }
        }
        else
        {
            if (expDiff < 0)
            {
                signZ = signC;
                if (expDiff < -1)
                {
                    sigZ = sigC - sigZ;
                    sigZExtra = sig256Z.V064 | sig256Z.V000; // part of !IsZero check
                    if (sigZExtra != 0)
                        sigZ -= new SFUInt128(v64: 0, v0: 1);

                    if ((sigZ.V64 & 0x0100000000000000) == 0)
                    {
                        --expZ;
                        shiftDist = 7;
                    }

                    sigZExtra = (sigZ.V00 << (64 - shiftDist)) | (sigZExtra != 0 ? 1UL : 0);
                    sigZ >>= shiftDist;
                    return RoundPackToF128(context, signZ, expZ - 1, sigZ, sigZExtra);
                }
                else
                {
                    sig256C = new SFUInt256(v128: sigC, v0: SFUInt128.Zero);
                    sig256Z = sig256C - sig256Z;
                }
            }
            else if (expDiff == 0)
            {
                sigZ -= sigC;
                if (sigZ.IsZero && sig256Z.V000_UI128.IsZero)
                    return PackToF128(context.Rounding == RoundingMode.Min, 0, 0, 0);

                sig256Z.V128_UI128 = sigZ;
                if ((sigZ.V64 & 0x8000000000000000) != 0)
                {
                    signZ = !signZ;
                    sig256Z = -sig256Z;
                }
            }
            else
            {
                Unsafe.SkipInit(out sig256C); // prevent compiler error -- it's always assigned
                sig256Z -= sig256C;

                if (1 < expDiff)
                {
                    sigZ = sig256Z.V128_UI128;
                    if ((sigZ.V64 & 0x0100000000000000) == 0)
                    {
                        --expZ;
                        shiftDist = 7;
                    }

                    sigZExtra = sig256Z.V064 | sig256Z.V000;
                    sigZExtra = (sigZ.V00 << (64 - shiftDist)) | (sigZExtra != 0 ? 1UL : 0);
                    sigZ >>= shiftDist;
                    return RoundPackToF128(context, signZ, expZ - 1, sigZ, sigZExtra);
                }
            }

            sigZ = sig256Z.V128_UI128;
            sigZExtra = sig256Z.V064;
            sig256Z0 = sig256Z.V000;
            if (sigZ.V64 != 0)
            {
                if (sig256Z0 != 0)
                    sigZExtra |= 1;
            }
            else
            {
                expZ -= 64;
                sigZ.V64 = sigZ.V00;
                sigZ.V00 = sigZExtra;
                sigZExtra = sig256Z0;
                if (sigZ.V64 == 0)
                {
                    expZ -= 64;
                    sigZ.V64 = sigZ.V00;
                    sigZ.V00 = sigZExtra;
                    sigZExtra = 0;
                    if (sigZ.V64 == 0)
                    {
                        expZ -= 64;
                        sigZ.V64 = sigZ.V00;
                        sigZ.V00 = 0;
                    }
                }
            }

            shiftDist = CountLeadingZeroes64(sigZ.V64);
            expZ += 7 - shiftDist;
            shiftDist = 15 - shiftDist;
            if (0 < shiftDist)
            {
                sigZExtra = (sigZ.V00 << (64 - shiftDist)) | (sigZExtra != 0 ? 1UL : 0);
                sigZ >>= shiftDist;
                return RoundPackToF128(context, signZ, expZ - 1, sigZ, sigZExtra);
            }
            else if (shiftDist != 0)
            {
                shiftDist = -shiftDist;
                sigZ <<= shiftDist;
                x128 = new SFUInt128(v64: 0, v0: sigZExtra) << shiftDist;
                sigZ.V00 |= x128.V64;
                sigZExtra = x128.V00;
            }

            return RoundPackToF128(context, signZ, expZ - 1, sigZ, sigZExtra);
        }

        sigZExtra = sig256Z.V064 | sig256Z.V000;
        sigZExtra = (sigZ.V00 << (64 - shiftDist)) | (sigZExtra != 0 ? 1UL : 0);
        sigZ >>= shiftDist;
        return RoundPackToF128(context, signZ, expZ - 1, sigZ, sigZExtra);

    infProdArg:
        if (magBits != 0)
        {
            uiZ = new SFUInt128(v64: PackToF128UI64(signZ, 0x7FFF, 0), v0: 0);
            if (expC != 0x7FFF)
                return Float128.FromBitsUI128(uiZ);

            if (!sigC.IsZero)
                return context.PropagateNaNFloat128Bits(uiZ.V64, uiZ.V00, uiC64, uiC0);

            if (signZ == signC)
                return Float128.FromBitsUI128(uiZ);
        }

        context.RaiseFlags(ExceptionFlags.Invalid);

        var defaultNaNBits = context.DefaultNaNFloat128Bits;
        return context.PropagateNaNFloat128Bits(defaultNaNBits.GetUpperUI64(), defaultNaNBits.GetLowerUI64(), uiC64, uiC0);
    }
}
