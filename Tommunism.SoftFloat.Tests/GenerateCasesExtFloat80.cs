#region Copyright
/*============================================================================

This is a C# port of the TestFloat library release 3e by Thomas Kaiser (2022).
The copyright from the original source code is listed below.

This C source file is part of TestFloat, Release 3e, a package of programs for
testing the correctness of floating-point arithmetic complying with the IEEE
Standard for Floating-Point, by John R. Hauser.

Copyright 2011, 2012, 2013, 2014, 2015, 2017 The Regents of the University of
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

using Tommunism.RandomNumbers;

namespace Tommunism.SoftFloat.Tests;

internal static class GenerateCasesExtFloat80
{
    #region Fields

    private const int NumQIn = 22;
    private const int NumQOut = 76;
    private const int NumP1 = 4;
    private const int NumP2 = 248;

    private const int NumQInP1 = NumQIn * NumP1;
    private const int NumQOutP1 = NumQOut * NumP1;

    private const int NumQInP2 = NumQIn * NumP2;
    private const int NumQOutP2 = NumQOut * NumP2;

    private static readonly ushort[] QIn = new ushort[NumQIn]
    {
        0x0000,    // positive, subnormal
        0x0001,    // positive, -16382
        0x3FBF,    // positive,    -64
        0x3FFD,    // positive,     -2
        0x3FFE,    // positive,     -1
        0x3FFF,    // positive,      0
        0x4000,    // positive,      1
        0x4001,    // positive,      2
        0x403F,    // positive,     64
        0x7FFE,    // positive,  16383
        0x7FFF,    // positive, infinity or NaN
        0x8000,    // negative, subnormal
        0x8001,    // negative, -16382
        0xBFBF,    // negative,    -64
        0xBFFD,    // negative,     -2
        0xBFFE,    // negative,     -1
        0xBFFF,    // negative,      0
        0xC000,    // negative,      1
        0xC001,    // negative,      2
        0xC03F,    // negative,     64
        0xFFFE,    // negative,  16383
        0xFFFF     // negative, infinity or NaN
    };

    private static readonly ushort[] QOut = new ushort[NumQOut]
    {
        0x0000,    // positive, subnormal
        0x0001,    // positive, -16382
        0x0002,    // positive, -16381
        0x3BFE,    // positive,  -1025
        0x3BFF,    // positive,  -1024
        0x3C00,    // positive,  -1023
        0x3C01,    // positive,  -1022
        0x3F7E,    // positive,   -129
        0x3F7F,    // positive,   -128
        0x3F80,    // positive,   -127
        0x3F81,    // positive,   -126
        0x3FBF,    // positive,    -64
        0x3FFB,    // positive,     -4
        0x3FFC,    // positive,     -3
        0x3FFD,    // positive,     -2
        0x3FFE,    // positive,     -1
        0x3FFF,    // positive,      0
        0x4000,    // positive,      1
        0x4001,    // positive,      2
        0x4002,    // positive,      3
        0x4003,    // positive,      4
        0x401C,    // positive,     29
        0x401D,    // positive,     30
        0x401E,    // positive,     31
        0x401F,    // positive,     32
        0x403C,    // positive,     61
        0x403D,    // positive,     62
        0x403E,    // positive,     63
        0x403F,    // positive,     64
        0x407E,    // positive,    127
        0x407F,    // positive,    128
        0x4080,    // positive,    129
        0x43FE,    // positive,   1023
        0x43FF,    // positive,   1024
        0x4400,    // positive,   1025
        0x7FFD,    // positive,  16382
        0x7FFE,    // positive,  16383
        0x7FFF,    // positive, infinity or NaN
        0x8000,    // negative, subnormal
        0x8001,    // negative, -16382
        0x8002,    // negative, -16381
        0xBBFE,    // negative,  -1025
        0xBBFF,    // negative,  -1024
        0xBC00,    // negative,  -1023
        0xBC01,    // negative,  -1022
        0xBF7E,    // negative,   -129
        0xBF7F,    // negative,   -128
        0xBF80,    // negative,   -127
        0xBF81,    // negative,   -126
        0xBFBF,    // negative,    -64
        0xBFFB,    // negative,     -4
        0xBFFC,    // negative,     -3
        0xBFFD,    // negative,     -2
        0xBFFE,    // negative,     -1
        0xBFFF,    // negative,      0
        0xC000,    // negative,      1
        0xC001,    // negative,      2
        0xC002,    // negative,      3
        0xC003,    // negative,      4
        0xC01C,    // negative,     29
        0xC01D,    // negative,     30
        0xC01E,    // negative,     31
        0xC01F,    // negative,     32
        0xC03C,    // negative,     61
        0xC03D,    // negative,     62
        0xC03E,    // negative,     63
        0xC03F,    // negative,     64
        0xC07E,    // negative,    127
        0xC07F,    // negative,    128
        0xC080,    // negative,    129
        0xC3FE,    // negative,   1023
        0xC3FF,    // negative,   1024
        0xC400,    // negative,   1025
        0xFFFD,    // negative,  16382
        0xFFFE,    // negative,  16383
        0xFFFF     // negative, infinity or NaN
    };

    private static readonly ulong[] P1 = new ulong[NumP1]
    {
        0x0000000000000000,
        0x0000000000000001,
        0x7FFFFFFFFFFFFFFF,
        0x7FFFFFFFFFFFFFFE
    };

    private static readonly ulong[] P2 = new ulong[NumP2]
    {
        0x0000000000000000,
        0x0000000000000001,
        0x0000000000000002,
        0x0000000000000004,
        0x0000000000000008,
        0x0000000000000010,
        0x0000000000000020,
        0x0000000000000040,
        0x0000000000000080,
        0x0000000000000100,
        0x0000000000000200,
        0x0000000000000400,
        0x0000000000000800,
        0x0000000000001000,
        0x0000000000002000,
        0x0000000000004000,
        0x0000000000008000,
        0x0000000000010000,
        0x0000000000020000,
        0x0000000000040000,
        0x0000000000080000,
        0x0000000000100000,
        0x0000000000200000,
        0x0000000000400000,
        0x0000000000800000,
        0x0000000001000000,
        0x0000000002000000,
        0x0000000004000000,
        0x0000000008000000,
        0x0000000010000000,
        0x0000000020000000,
        0x0000000040000000,
        0x0000000080000000,
        0x0000000100000000,
        0x0000000200000000,
        0x0000000400000000,
        0x0000000800000000,
        0x0000001000000000,
        0x0000002000000000,
        0x0000004000000000,
        0x0000008000000000,
        0x0000010000000000,
        0x0000020000000000,
        0x0000040000000000,
        0x0000080000000000,
        0x0000100000000000,
        0x0000200000000000,
        0x0000400000000000,
        0x0000800000000000,
        0x0001000000000000,
        0x0002000000000000,
        0x0004000000000000,
        0x0008000000000000,
        0x0010000000000000,
        0x0020000000000000,
        0x0040000000000000,
        0x0080000000000000,
        0x0100000000000000,
        0x0200000000000000,
        0x0400000000000000,
        0x0800000000000000,
        0x1000000000000000,
        0x2000000000000000,
        0x4000000000000000,
        0x6000000000000000,
        0x7000000000000000,
        0x7800000000000000,
        0x7C00000000000000,
        0x7E00000000000000,
        0x7F00000000000000,
        0x7F80000000000000,
        0x7FC0000000000000,
        0x7FE0000000000000,
        0x7FF0000000000000,
        0x7FF8000000000000,
        0x7FFC000000000000,
        0x7FFE000000000000,
        0x7FFF000000000000,
        0x7FFF800000000000,
        0x7FFFC00000000000,
        0x7FFFE00000000000,
        0x7FFFF00000000000,
        0x7FFFF80000000000,
        0x7FFFFC0000000000,
        0x7FFFFE0000000000,
        0x7FFFFF0000000000,
        0x7FFFFF8000000000,
        0x7FFFFFC000000000,
        0x7FFFFFE000000000,
        0x7FFFFFF000000000,
        0x7FFFFFF800000000,
        0x7FFFFFFC00000000,
        0x7FFFFFFE00000000,
        0x7FFFFFFF00000000,
        0x7FFFFFFF80000000,
        0x7FFFFFFFC0000000,
        0x7FFFFFFFE0000000,
        0x7FFFFFFFF0000000,
        0x7FFFFFFFF8000000,
        0x7FFFFFFFFC000000,
        0x7FFFFFFFFE000000,
        0x7FFFFFFFFF000000,
        0x7FFFFFFFFF800000,
        0x7FFFFFFFFFC00000,
        0x7FFFFFFFFFE00000,
        0x7FFFFFFFFFF00000,
        0x7FFFFFFFFFF80000,
        0x7FFFFFFFFFFC0000,
        0x7FFFFFFFFFFE0000,
        0x7FFFFFFFFFFF0000,
        0x7FFFFFFFFFFF8000,
        0x7FFFFFFFFFFFC000,
        0x7FFFFFFFFFFFE000,
        0x7FFFFFFFFFFFF000,
        0x7FFFFFFFFFFFF800,
        0x7FFFFFFFFFFFFC00,
        0x7FFFFFFFFFFFFE00,
        0x7FFFFFFFFFFFFF00,
        0x7FFFFFFFFFFFFF80,
        0x7FFFFFFFFFFFFFC0,
        0x7FFFFFFFFFFFFFE0,
        0x7FFFFFFFFFFFFFF0,
        0x7FFFFFFFFFFFFFF8,
        0x7FFFFFFFFFFFFFFC,
        0x7FFFFFFFFFFFFFFE,
        0x7FFFFFFFFFFFFFFF,
        0x7FFFFFFFFFFFFFFD,
        0x7FFFFFFFFFFFFFFB,
        0x7FFFFFFFFFFFFFF7,
        0x7FFFFFFFFFFFFFEF,
        0x7FFFFFFFFFFFFFDF,
        0x7FFFFFFFFFFFFFBF,
        0x7FFFFFFFFFFFFF7F,
        0x7FFFFFFFFFFFFEFF,
        0x7FFFFFFFFFFFFDFF,
        0x7FFFFFFFFFFFFBFF,
        0x7FFFFFFFFFFFF7FF,
        0x7FFFFFFFFFFFEFFF,
        0x7FFFFFFFFFFFDFFF,
        0x7FFFFFFFFFFFBFFF,
        0x7FFFFFFFFFFF7FFF,
        0x7FFFFFFFFFFEFFFF,
        0x7FFFFFFFFFFDFFFF,
        0x7FFFFFFFFFFBFFFF,
        0x7FFFFFFFFFF7FFFF,
        0x7FFFFFFFFFEFFFFF,
        0x7FFFFFFFFFDFFFFF,
        0x7FFFFFFFFFBFFFFF,
        0x7FFFFFFFFF7FFFFF,
        0x7FFFFFFFFEFFFFFF,
        0x7FFFFFFFFDFFFFFF,
        0x7FFFFFFFFBFFFFFF,
        0x7FFFFFFFF7FFFFFF,
        0x7FFFFFFFEFFFFFFF,
        0x7FFFFFFFDFFFFFFF,
        0x7FFFFFFFBFFFFFFF,
        0x7FFFFFFF7FFFFFFF,
        0x7FFFFFFEFFFFFFFF,
        0x7FFFFFFDFFFFFFFF,
        0x7FFFFFFBFFFFFFFF,
        0x7FFFFFF7FFFFFFFF,
        0x7FFFFFEFFFFFFFFF,
        0x7FFFFFDFFFFFFFFF,
        0x7FFFFFBFFFFFFFFF,
        0x7FFFFF7FFFFFFFFF,
        0x7FFFFEFFFFFFFFFF,
        0x7FFFFDFFFFFFFFFF,
        0x7FFFFBFFFFFFFFFF,
        0x7FFFF7FFFFFFFFFF,
        0x7FFFEFFFFFFFFFFF,
        0x7FFFDFFFFFFFFFFF,
        0x7FFFBFFFFFFFFFFF,
        0x7FFF7FFFFFFFFFFF,
        0x7FFEFFFFFFFFFFFF,
        0x7FFDFFFFFFFFFFFF,
        0x7FFBFFFFFFFFFFFF,
        0x7FF7FFFFFFFFFFFF,
        0x7FEFFFFFFFFFFFFF,
        0x7FDFFFFFFFFFFFFF,
        0x7FBFFFFFFFFFFFFF,
        0x7F7FFFFFFFFFFFFF,
        0x7EFFFFFFFFFFFFFF,
        0x7DFFFFFFFFFFFFFF,
        0x7BFFFFFFFFFFFFFF,
        0x77FFFFFFFFFFFFFF,
        0x6FFFFFFFFFFFFFFF,
        0x5FFFFFFFFFFFFFFF,
        0x3FFFFFFFFFFFFFFF,
        0x1FFFFFFFFFFFFFFF,
        0x0FFFFFFFFFFFFFFF,
        0x07FFFFFFFFFFFFFF,
        0x03FFFFFFFFFFFFFF,
        0x01FFFFFFFFFFFFFF,
        0x00FFFFFFFFFFFFFF,
        0x007FFFFFFFFFFFFF,
        0x003FFFFFFFFFFFFF,
        0x001FFFFFFFFFFFFF,
        0x000FFFFFFFFFFFFF,
        0x0007FFFFFFFFFFFF,
        0x0003FFFFFFFFFFFF,
        0x0001FFFFFFFFFFFF,
        0x0000FFFFFFFFFFFF,
        0x00007FFFFFFFFFFF,
        0x00003FFFFFFFFFFF,
        0x00001FFFFFFFFFFF,
        0x00000FFFFFFFFFFF,
        0x000007FFFFFFFFFF,
        0x000003FFFFFFFFFF,
        0x000001FFFFFFFFFF,
        0x000000FFFFFFFFFF,
        0x0000007FFFFFFFFF,
        0x0000003FFFFFFFFF,
        0x0000001FFFFFFFFF,
        0x0000000FFFFFFFFF,
        0x00000007FFFFFFFF,
        0x00000003FFFFFFFF,
        0x00000001FFFFFFFF,
        0x00000000FFFFFFFF,
        0x000000007FFFFFFF,
        0x000000003FFFFFFF,
        0x000000001FFFFFFF,
        0x000000000FFFFFFF,
        0x0000000007FFFFFF,
        0x0000000003FFFFFF,
        0x0000000001FFFFFF,
        0x0000000000FFFFFF,
        0x00000000007FFFFF,
        0x00000000003FFFFF,
        0x00000000001FFFFF,
        0x00000000000FFFFF,
        0x000000000007FFFF,
        0x000000000003FFFF,
        0x000000000001FFFF,
        0x000000000000FFFF,
        0x0000000000007FFF,
        0x0000000000003FFF,
        0x0000000000001FFF,
        0x0000000000000FFF,
        0x00000000000007FF,
        0x00000000000003FF,
        0x00000000000001FF,
        0x00000000000000FF,
        0x000000000000007F,
        0x000000000000003F,
        0x000000000000001F,
        0x000000000000000F,
        0x0000000000000007,
        0x0000000000000003
    };

    private const int NumQInfWeightMasks = 14;

    private static readonly ushort[] QInfWeightMasks = new ushort[NumQInfWeightMasks]
    {
        0xFFFF,
        0xFFFF,
        0xBFFF,
        0x9FFF,
        0x87FF,
        0x87FF,
        0x83FF,
        0x81FF,
        0x80FF,
        0x807F,
        0x803F,
        0x801F,
        0x800F,
        0x8007
    };

    private static readonly ushort[] QInfWeightOffsets = new ushort[NumQInfWeightMasks]
    {
        0x0000,
        0x0000,
        0x2000,
        0x3000,
        0x3800,
        0x3C00,
        0x3E00,
        0x3F00,
        0x3F80,
        0x3FC0,
        0x3FE0,
        0x3FF0,
        0x3FF8,
        0x3FFC
    };

    #endregion

    #region Methods

    private static ExtFloat80 NextQInP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumQInP1);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP1);
        Debug.Assert(sigNum is >= 0 and < NumP1);
        Debug.Assert(expNum is >= 0 and < NumQIn);

        ushort uiZ64 = QIn[expNum];
        ulong uiZ0 = P1[sigNum];

        if ((uiZ64 & 0x7FFF) != 0)
            uiZ0 |= 0x8000000000000000;

        return ExtFloat80.FromUIntBits(uiZ64, uiZ0);
    }

    private static ExtFloat80 NextQOutP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumQOutP1);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP1);
        Debug.Assert(sigNum is >= 0 and < NumP1);
        Debug.Assert(expNum is >= 0 and < NumQOut);

        ushort uiZ64 = QOut[expNum];
        ulong uiZ0 = P1[sigNum];

        if ((uiZ64 & 0x7FFF) != 0)
            uiZ0 |= 0x8000000000000000;

        return ExtFloat80.FromUIntBits(uiZ64, uiZ0);
    }

    private static ExtFloat80 NextQInP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumQInP2);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP2);
        Debug.Assert(sigNum is >= 0 and < NumP2);
        Debug.Assert(expNum is >= 0 and < NumQIn);

        ushort uiZ64 = QIn[expNum];
        ulong uiZ0 = P2[sigNum];

        if ((uiZ64 & 0x7FFF) != 0)
            uiZ0 |= 0x8000000000000000;

        return ExtFloat80.FromUIntBits(uiZ64, uiZ0);
    }

    private static ExtFloat80 NextQOutP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumQOutP2);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP2);
        Debug.Assert(sigNum is >= 0 and < NumP2);
        Debug.Assert(expNum is >= 0 and < NumQOut);

        ushort uiZ64 = QOut[expNum];
        ulong uiZ0 = P2[sigNum];

        if ((uiZ64 & 0x7FFF) != 0)
            uiZ0 |= 0x8000000000000000;

        return ExtFloat80.FromUIntBits(uiZ64, uiZ0);
    }

    private static ExtFloat80 RandomQOutP3(ref ThreefryRandom rng)
    {
        ushort uiZ64 = QOut[rng.NextInt32(NumQOut)];
        ulong uiZ0 = (P2[rng.NextInt32(NumP2)] + P2[rng.NextInt32(NumP2)]) & 0x7FFFFFFFFFFFFFFF;

        if ((uiZ64 & 0x7FFF) != 0)
            uiZ0 |= 0x8000000000000000;

        return ExtFloat80.FromUIntBits(uiZ64, uiZ0);
    }

    private static ExtFloat80 RandomQOutPInf(ref ThreefryRandom rng)
    {
        ushort uiZ64 = QOut[rng.NextInt32(NumQOut)];
        ulong uiZ0 = rng.NextUInt64() & 0x7FFFFFFFFFFFFFFF;

        if ((uiZ64 & 0x7FFF) != 0)
            uiZ0 |= 0x8000000000000000;

        return ExtFloat80.FromUIntBits(uiZ64, uiZ0);
    }

    private static ExtFloat80 RandomQInfP3(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumQInfWeightMasks);
        ushort uiZ64 = (ushort)((rng.NextUInt32() & QInfWeightMasks[weightMaskNum]) + QInfWeightOffsets[weightMaskNum]);
        ulong uiZ0 = (P2[rng.NextInt32(NumP2)] + P2[rng.NextInt32(NumP2)]) & 0x7FFFFFFFFFFFFFFF;

        if ((uiZ64 & 0x7FFF) != 0)
            uiZ0 |= 0x8000000000000000;

        return ExtFloat80.FromUIntBits(uiZ64, uiZ0);
    }

    private static ExtFloat80 RandomQInfPInf(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumQInfWeightMasks);
        ushort uiZ64 = (ushort)((rng.NextUInt32() & QInfWeightMasks[weightMaskNum]) + QInfWeightOffsets[weightMaskNum]);
        ulong uiZ0 = rng.NextUInt64() & 0x7FFFFFFFFFFFFFFF;

        if ((uiZ64 & 0x7FFF) != 0)
            uiZ0 |= 0x8000000000000000;

        return ExtFloat80.FromUIntBits(uiZ64, uiZ0);
    }

    private static ExtFloat80 Random(ref ThreefryRandom rng) => rng.NextInt32(8) switch
    {
        0 or 1 or 2 => RandomQOutP3(ref rng),
        3 => RandomQOutPInf(ref rng),
        4 or 5 or 6 => RandomQInfP3(ref rng),
        7 => RandomQInfPInf(ref rng),
        _ => throw new InvalidOperationException("Invalid switch case.")
    };

    #endregion

    #region Nested Types

    internal class A : GenerateCasesBase
    {
        #region Constructors

        public A() { }

        public A(int level) : base(level) { }

        #endregion

        #region Properties

        public override int ArgumentCount => 1;

        #endregion

        #region Methods

        public override TestRunnerArguments GenerateTestCase(long index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            long subCase;
            long subCaseIndex;
            var rng = new ThreefryRandom(index, Program.ThreefrySeed4x64, stackalloc ulong[ThreefryRandom.ResultsSize]);

            ExtFloat80 extF80_a;

            switch (Level)
            {
                case 1:
                {
                    (subCaseIndex, subCase) = Math.DivRem(index, 3);
                    switch ((int)subCase)
                    {
                        case 0:
                        case 1:
                        {
                            extF80_a = Random(ref rng);
                            break;
                        }
                        case 2:
                        {
                            extF80_a = NextQOutP1((int)(subCaseIndex % NumQOutP1));
                            break;
                        }
                        default:
                        {
                            throw new InvalidOperationException("Invalid switch case.");
                        }
                    }

                    break;
                }

                case 2:
                {
                    subCase = index & 1;
                    subCaseIndex = index >> 1;
                    switch ((int)subCase)
                    {
                        case 0:
                        {
                            extF80_a = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            extF80_a = NextQOutP2((int)(subCaseIndex % NumQOutP2));
                            break;
                        }
                        default:
                        {
                            throw new InvalidOperationException("Invalid switch case.");
                        }
                    }

                    break;
                }

                default:
                {
                    throw new NotImplementedException("Test level not implemented.");
                }
            }

            return new TestRunnerArguments(extF80_a);
        }

        protected override long CalculateTotalCases() => Level switch
        {
            1 => 3 * NumQOutP1,
            2 => 2 * NumQOutP2,
            _ => throw new NotImplementedException("Test level not implemented.")
        };

        #endregion
    }

    internal class AB : GenerateCasesBase
    {
        #region Constructors

        public AB() { }

        public AB(int level) : base(level) { }

        #endregion

        #region Properties

        public override int ArgumentCount => 2;

        #endregion

        #region Methods

        public override TestRunnerArguments GenerateTestCase(long index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            long subCase;
            long subCaseIndex;
            var rng = new ThreefryRandom(index, Program.ThreefrySeed4x64, stackalloc ulong[ThreefryRandom.ResultsSize]);

            long currentAIndex, currentBIndex;
            ExtFloat80 extF80_a, extF80_b;

            switch (Level)
            {
                case 1:
                {
                    (subCaseIndex, subCase) = Math.DivRem(index, 6);

                    // Calculate "currentA" and "currentB" indexes.
                    currentAIndex = subCaseIndex;
                    (currentBIndex, currentAIndex) = Math.DivRem(currentAIndex, NumQInP1);
                    currentBIndex %= NumQInP1;

                    switch ((int)subCase)
                    {
                        case 0:
                        case 2:
                        case 4:
                        {
                            extF80_a = Random(ref rng);
                            extF80_b = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            extF80_a = NextQInP1((int)currentAIndex);
                            extF80_b = Random(ref rng);
                            break;
                        }
                        case 3:
                        {
                            extF80_a = Random(ref rng);
                            extF80_b = NextQInP1((int)currentBIndex);
                            break;
                        }
                        case 5:
                        {
                            extF80_a = NextQInP1((int)currentAIndex);
                            extF80_b = NextQInP1((int)currentBIndex);
                            break;
                        }
                        default:
                        {
                            throw new InvalidOperationException("Invalid switch case.");
                        }
                    }

                    break;
                }

                case 2:
                {
                    subCase = index & 1;
                    subCaseIndex = index >> 1;

                    // Calculate "currentA" and "currentB" indexes.
                    (currentBIndex, currentAIndex) = Math.DivRem(subCaseIndex, NumQInP2);
                    currentBIndex %= NumQInP2;

                    switch ((int)subCase)
                    {
                        case 0:
                        {
                            extF80_a = Random(ref rng);
                            extF80_b = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            extF80_a = NextQInP2((int)currentAIndex);
                            extF80_b = NextQInP2((int)currentBIndex);
                            break;
                        }
                        default:
                        {
                            throw new InvalidOperationException("Invalid switch case.");
                        }
                    }

                    break;
                }

                default:
                {
                    throw new NotImplementedException("Test level not implemented.");
                }
            }

            return new TestRunnerArguments(extF80_a, extF80_b);
        }

        protected override long CalculateTotalCases() => Level switch
        {
            1 => 6 * NumQInP1 * NumQInP1,
            2 => 2 * NumQInP2 * NumQInP2,
            _ => throw new NotImplementedException("Test level not implemented.")
        };

        #endregion
    }

    internal class ABC : GenerateCasesBase
    {
        #region Constructors

        public ABC() { }

        public ABC(int level) : base(level) { }

        #endregion

        #region Properties

        public override int ArgumentCount => 3;

        #endregion

        #region Methods

        public override TestRunnerArguments GenerateTestCase(long index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            long subCase;
            long subCaseIndex;
            var rng = new ThreefryRandom(index, Program.ThreefrySeed4x64, stackalloc ulong[ThreefryRandom.ResultsSize]);

            long currentAIndex, currentBIndex, currentCIndex;
            ExtFloat80 extF80_a, extF80_b, extF80_c;

            switch (Level)
            {
                case 1:
                {
                    (subCaseIndex, subCase) = Math.DivRem(index, 6);

                    // Calculate "currentA", "currentB", and "currentC" indexes.
                    currentAIndex = subCaseIndex;
                    (currentBIndex, currentAIndex) = Math.DivRem(currentAIndex, NumQInP1);
                    (currentCIndex, currentBIndex) = Math.DivRem(currentBIndex, NumQInP1);
                    currentCIndex %= NumQInP1;

                    switch ((int)subCase)
                    {
                        case 0:
                        {
                            extF80_a = Random(ref rng);
                            extF80_b = Random(ref rng);
                            extF80_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 1:
                        {
                            extF80_a = NextQInP1((int)currentAIndex);
                            extF80_b = NextQInP1((int)currentBIndex);
                            extF80_c = Random(ref rng);
                            break;
                        }
                        case 2:
                        case 7:
                        {
                            extF80_a = Random(ref rng);
                            extF80_b = Random(ref rng);
                            extF80_c = Random(ref rng);
                            break;
                        }
                        case 3:
                        {
                            extF80_a = Random(ref rng);
                            extF80_b = NextQInP1((int)currentBIndex);
                            extF80_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 4:
                        {
                            extF80_a = NextQInP1((int)currentAIndex);
                            extF80_b = Random(ref rng);
                            extF80_c = Random(ref rng);
                            break;
                        }
                        case 5:
                        {
                            extF80_a = Random(ref rng);
                            extF80_b = NextQInP1((int)currentBIndex);
                            extF80_c = Random(ref rng);
                            break;
                        }
                        case 6:
                        {
                            extF80_a = NextQInP1((int)currentAIndex);
                            extF80_b = Random(ref rng);
                            extF80_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 8:
                        {
                            extF80_a = NextQInP1((int)currentAIndex);
                            extF80_b = NextQInP1((int)currentBIndex);
                            extF80_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        default:
                        {
                            throw new InvalidOperationException("Invalid switch case.");
                        }
                    }

                    break;
                }

                case 2:
                {
                    subCase = index & 1;
                    subCaseIndex = index >> 1;

                    // Calculate "currentA" and "currentB" indexes.
                    currentAIndex = subCaseIndex;
                    (currentBIndex, currentAIndex) = Math.DivRem(currentAIndex, NumQInP2);
                    (currentCIndex, currentBIndex) = Math.DivRem(currentBIndex, NumQInP2);
                    currentCIndex %= NumQInP2;

                    switch ((int)subCase)
                    {
                        case 0:
                        {
                            extF80_a = Random(ref rng);
                            extF80_b = Random(ref rng);
                            extF80_c = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            extF80_a = NextQInP2((int)currentAIndex);
                            extF80_b = NextQInP2((int)currentBIndex);
                            extF80_c = NextQInP2((int)currentCIndex);
                            break;
                        }
                        default:
                        {
                            throw new InvalidOperationException("Invalid switch case.");
                        }
                    }

                    break;
                }

                default:
                {
                    throw new NotImplementedException("Test level not implemented.");
                }
            }

            return new TestRunnerArguments(extF80_a, extF80_b, extF80_c);
        }

        protected override long CalculateTotalCases() => Level switch
        {
            1 => 9 * NumQInP1 * NumQInP1 * NumQInP1,
            2 => 2L * NumQInP2 * NumQInP2 * NumQInP2, // larger than signed 32-bit
            _ => throw new NotImplementedException("Test level not implemented.")
        };

        #endregion
    }

    #endregion
}
