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

internal static class GenerateCasesFloat64
{
    #region Fields

    private const int NumQIn = 22;
    private const int NumQOut = 64;
    private const int NumP1 = 4;
    private const int NumP2 = 204;

    private const int NumQInP1 = NumQIn * NumP1;
    private const int NumQOutP1 = NumQOut * NumP1;

    private const int NumQInP2 = NumQIn * NumP2;
    private const int NumQOutP2 = NumQOut * NumP2;

    private static readonly ulong[] QIn = new ulong[NumQIn]
    {
        0x0000000000000000,    // positive, subnormal
        0x0010000000000000,    // positive, -1022
        0x3CA0000000000000,    // positive,   -53
        0x3FD0000000000000,    // positive,    -2
        0x3FE0000000000000,    // positive,    -1
        0x3FF0000000000000,    // positive,     0
        0x4000000000000000,    // positive,     1
        0x4010000000000000,    // positive,     2
        0x4340000000000000,    // positive,    53
        0x7FE0000000000000,    // positive,  1023
        0x7FF0000000000000,    // positive, infinity or NaN
        0x8000000000000000,    // negative, subnormal
        0x8010000000000000,    // negative, -1022
        0xBCA0000000000000,    // negative,   -53
        0xBFD0000000000000,    // negative,    -2
        0xBFE0000000000000,    // negative,    -1
        0xBFF0000000000000,    // negative,     0
        0xC000000000000000,    // negative,     1
        0xC010000000000000,    // negative,     2
        0xC340000000000000,    // negative,    53
        0xFFE0000000000000,    // negative,  1023
        0xFFF0000000000000     // negative, infinity or NaN
    };

    private static readonly ulong[] QOut = new ulong[NumQOut]
    {
        0x0000000000000000,    // positive, subnormal
        0x0010000000000000,    // positive, -1022
        0x0020000000000000,    // positive, -1021
        0x37E0000000000000,    // positive,  -129
        0x37F0000000000000,    // positive,  -128
        0x3800000000000000,    // positive,  -127
        0x3810000000000000,    // positive,  -126
        0x3CA0000000000000,    // positive,   -53
        0x3FB0000000000000,    // positive,    -4
        0x3FC0000000000000,    // positive,    -3
        0x3FD0000000000000,    // positive,    -2
        0x3FE0000000000000,    // positive,    -1
        0x3FF0000000000000,    // positive,     0
        0x4000000000000000,    // positive,     1
        0x4010000000000000,    // positive,     2
        0x4020000000000000,    // positive,     3
        0x4030000000000000,    // positive,     4
        0x41C0000000000000,    // positive,    29
        0x41D0000000000000,    // positive,    30
        0x41E0000000000000,    // positive,    31
        0x41F0000000000000,    // positive,    32
        0x4340000000000000,    // positive,    53
        0x43C0000000000000,    // positive,    61
        0x43D0000000000000,    // positive,    62
        0x43E0000000000000,    // positive,    63
        0x43F0000000000000,    // positive,    64
        0x47E0000000000000,    // positive,   127
        0x47F0000000000000,    // positive,   128
        0x4800000000000000,    // positive,   129
        0x7FD0000000000000,    // positive,  1022
        0x7FE0000000000000,    // positive,  1023
        0x7FF0000000000000,    // positive, infinity or NaN
        0x8000000000000000,    // negative, subnormal
        0x8010000000000000,    // negative, -1022
        0x8020000000000000,    // negative, -1021
        0xB7E0000000000000,    // negative,  -129
        0xB7F0000000000000,    // negative,  -128
        0xB800000000000000,    // negative,  -127
        0xB810000000000000,    // negative,  -126
        0xBCA0000000000000,    // negative,   -53
        0xBFB0000000000000,    // negative,    -4
        0xBFC0000000000000,    // negative,    -3
        0xBFD0000000000000,    // negative,    -2
        0xBFE0000000000000,    // negative,    -1
        0xBFF0000000000000,    // negative,     0
        0xC000000000000000,    // negative,     1
        0xC010000000000000,    // negative,     2
        0xC020000000000000,    // negative,     3
        0xC030000000000000,    // negative,     4
        0xC1C0000000000000,    // negative,    29
        0xC1D0000000000000,    // negative,    30
        0xC1E0000000000000,    // negative,    31
        0xC1F0000000000000,    // negative,    32
        0xC340000000000000,    // negative,    53
        0xC3C0000000000000,    // negative,    61
        0xC3D0000000000000,    // negative,    62
        0xC3E0000000000000,    // negative,    63
        0xC3F0000000000000,    // negative,    64
        0xC7E0000000000000,    // negative,   127
        0xC7F0000000000000,    // negative,   128
        0xC800000000000000,    // negative,   129
        0xFFD0000000000000,    // negative,  1022
        0xFFE0000000000000,    // negative,  1023
        0xFFF0000000000000     // negative, infinity or NaN
    };

    private static readonly ulong[] P1 = new ulong[NumP1]
    {
        0x0000000000000000,
        0x0000000000000001,
        0x000FFFFFFFFFFFFF,
        0x000FFFFFFFFFFFFE
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
        0x000C000000000000,
        0x000E000000000000,
        0x000F000000000000,
        0x000F800000000000,
        0x000FC00000000000,
        0x000FE00000000000,
        0x000FF00000000000,
        0x000FF80000000000,
        0x000FFC0000000000,
        0x000FFE0000000000,
        0x000FFF0000000000,
        0x000FFF8000000000,
        0x000FFFC000000000,
        0x000FFFE000000000,
        0x000FFFF000000000,
        0x000FFFF800000000,
        0x000FFFFC00000000,
        0x000FFFFE00000000,
        0x000FFFFF00000000,
        0x000FFFFF80000000,
        0x000FFFFFC0000000,
        0x000FFFFFE0000000,
        0x000FFFFFF0000000,
        0x000FFFFFF8000000,
        0x000FFFFFFC000000,
        0x000FFFFFFE000000,
        0x000FFFFFFF000000,
        0x000FFFFFFF800000,
        0x000FFFFFFFC00000,
        0x000FFFFFFFE00000,
        0x000FFFFFFFF00000,
        0x000FFFFFFFF80000,
        0x000FFFFFFFFC0000,
        0x000FFFFFFFFE0000,
        0x000FFFFFFFFF0000,
        0x000FFFFFFFFF8000,
        0x000FFFFFFFFFC000,
        0x000FFFFFFFFFE000,
        0x000FFFFFFFFFF000,
        0x000FFFFFFFFFF800,
        0x000FFFFFFFFFFC00,
        0x000FFFFFFFFFFE00,
        0x000FFFFFFFFFFF00,
        0x000FFFFFFFFFFF80,
        0x000FFFFFFFFFFFC0,
        0x000FFFFFFFFFFFE0,
        0x000FFFFFFFFFFFF0,
        0x000FFFFFFFFFFFF8,
        0x000FFFFFFFFFFFFC,
        0x000FFFFFFFFFFFFE,
        0x000FFFFFFFFFFFFF,
        0x000FFFFFFFFFFFFD,
        0x000FFFFFFFFFFFFB,
        0x000FFFFFFFFFFFF7,
        0x000FFFFFFFFFFFEF,
        0x000FFFFFFFFFFFDF,
        0x000FFFFFFFFFFFBF,
        0x000FFFFFFFFFFF7F,
        0x000FFFFFFFFFFEFF,
        0x000FFFFFFFFFFDFF,
        0x000FFFFFFFFFFBFF,
        0x000FFFFFFFFFF7FF,
        0x000FFFFFFFFFEFFF,
        0x000FFFFFFFFFDFFF,
        0x000FFFFFFFFFBFFF,
        0x000FFFFFFFFF7FFF,
        0x000FFFFFFFFEFFFF,
        0x000FFFFFFFFDFFFF,
        0x000FFFFFFFFBFFFF,
        0x000FFFFFFFF7FFFF,
        0x000FFFFFFFEFFFFF,
        0x000FFFFFFFDFFFFF,
        0x000FFFFFFFBFFFFF,
        0x000FFFFFFF7FFFFF,
        0x000FFFFFFEFFFFFF,
        0x000FFFFFFDFFFFFF,
        0x000FFFFFFBFFFFFF,
        0x000FFFFFF7FFFFFF,
        0x000FFFFFEFFFFFFF,
        0x000FFFFFDFFFFFFF,
        0x000FFFFFBFFFFFFF,
        0x000FFFFF7FFFFFFF,
        0x000FFFFEFFFFFFFF,
        0x000FFFFDFFFFFFFF,
        0x000FFFFBFFFFFFFF,
        0x000FFFF7FFFFFFFF,
        0x000FFFEFFFFFFFFF,
        0x000FFFDFFFFFFFFF,
        0x000FFFBFFFFFFFFF,
        0x000FFF7FFFFFFFFF,
        0x000FFEFFFFFFFFFF,
        0x000FFDFFFFFFFFFF,
        0x000FFBFFFFFFFFFF,
        0x000FF7FFFFFFFFFF,
        0x000FEFFFFFFFFFFF,
        0x000FDFFFFFFFFFFF,
        0x000FBFFFFFFFFFFF,
        0x000F7FFFFFFFFFFF,
        0x000EFFFFFFFFFFFF,
        0x000DFFFFFFFFFFFF,
        0x000BFFFFFFFFFFFF,
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

    private const int NumQInfWeightMasks = 10;

    private static readonly ulong[] QInfWeightMasks = new ulong[NumQInfWeightMasks]
    {
        0xFFF0000000000000,
        0xFFF0000000000000,
        0xBFF0000000000000,
        0x9FF0000000000000,
        0x8FF0000000000000,
        0x87F0000000000000,
        0x83F0000000000000,
        0x81F0000000000000,
        0x80F0000000000000,
        0x8070000000000000
    };

    private static readonly ulong[] QInfWeightOffsets = new ulong[NumQInfWeightMasks]
    {
        0x0000000000000000,
        0x0000000000000000,
        0x2000000000000000,
        0x3000000000000000,
        0x3800000000000000,
        0x3C00000000000000,
        0x3E00000000000000,
        0x3F00000000000000,
        0x3F80000000000000,
        0x3FC0000000000000
    };

    #endregion

    #region Methods

    private static Float64 NextQInP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumQInP1);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP1);
        Debug.Assert(sigNum is >= 0 and < NumP1);
        Debug.Assert(expNum is >= 0 and < NumQIn);

        return Float64.FromUIntBits(QIn[expNum] | P1[sigNum]);
    }

    private static Float64 NextQOutP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumQOutP1);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP1);
        Debug.Assert(sigNum is >= 0 and < NumP1);
        Debug.Assert(expNum is >= 0 and < NumQOut);

        return Float64.FromUIntBits(QOut[expNum] | P1[sigNum]);
    }

    private static Float64 NextQInP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumQInP2);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP2);
        Debug.Assert(sigNum is >= 0 and < NumP2);
        Debug.Assert(expNum is >= 0 and < NumQIn);

        return Float64.FromUIntBits(QIn[expNum] | P2[sigNum]);
    }

    private static Float64 NextQOutP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumQOutP2);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP2);
        Debug.Assert(sigNum is >= 0 and < NumP2);
        Debug.Assert(expNum is >= 0 and < NumQOut);

        return Float64.FromUIntBits(QOut[expNum] | P2[sigNum]);
    }

    private static Float64 RandomQOutP3(ref ThreefryRandom rng) =>
        Float64.FromUIntBits(QOut[rng.NextInt32(NumQOut)]
            | ((P2[rng.NextInt32(NumP2)] + P2[rng.NextInt32(NumP2)]) & 0x000FFFFFFFFFFFFF));

    private static Float64 RandomQOutPInf(ref ThreefryRandom rng) =>
        Float64.FromUIntBits(QOut[rng.NextInt32(NumQOut)] | (rng.NextUInt64() & 0x000FFFFFFFFFFFFF));

    private static Float64 RandomQInfP3(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumQInfWeightMasks);
        return Float64.FromUIntBits(
            (((rng.NextUInt32() << 48) & QInfWeightMasks[weightMaskNum]) + QInfWeightOffsets[weightMaskNum])
            | ((P2[rng.NextInt32(NumP2)] + P2[rng.NextInt32(NumP2)]) & 0x000FFFFFFFFFFFFF)
        );
    }

    private static Float64 RandomQInfPInf(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumQInfWeightMasks);
        return Float64.FromUIntBits(
            (rng.NextUInt64() & (QInfWeightMasks[weightMaskNum] | 0x000FFFFFFFFFFFFF)) + QInfWeightOffsets[weightMaskNum]
        );
    }

    private static Float64 Random(ref ThreefryRandom rng) => rng.NextInt32(8) switch
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

            Float64 f64_a;

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
                            f64_a = Random(ref rng);
                            break;
                        }
                        case 2:
                        {
                            f64_a = NextQOutP1((int)(subCaseIndex % NumQOutP1));
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
                            f64_a = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f64_a = NextQOutP2((int)(subCaseIndex % NumQOutP2));
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

            return new TestRunnerArguments(f64_a);
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
            Float64 f64_a, f64_b;

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
                            f64_a = Random(ref rng);
                            f64_b = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f64_a = NextQInP1((int)currentAIndex);
                            f64_b = Random(ref rng);
                            break;
                        }
                        case 3:
                        {
                            f64_a = Random(ref rng);
                            f64_b = NextQInP1((int)currentBIndex);
                            break;
                        }
                        case 5:
                        {
                            f64_a = NextQInP1((int)currentAIndex);
                            f64_b = NextQInP1((int)currentBIndex);
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
                            f64_a = Random(ref rng);
                            f64_b = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f64_a = NextQInP2((int)currentAIndex);
                            f64_b = NextQInP2((int)currentBIndex);
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

            return new TestRunnerArguments(f64_a, f64_b);
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
            Float64 f64_a, f64_b, f64_c;

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
                            f64_a = Random(ref rng);
                            f64_b = Random(ref rng);
                            f64_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 1:
                        {
                            f64_a = NextQInP1((int)currentAIndex);
                            f64_b = NextQInP1((int)currentBIndex);
                            f64_c = Random(ref rng);
                            break;
                        }
                        case 2:
                        case 7:
                        {
                            f64_a = Random(ref rng);
                            f64_b = Random(ref rng);
                            f64_c = Random(ref rng);
                            break;
                        }
                        case 3:
                        {
                            f64_a = Random(ref rng);
                            f64_b = NextQInP1((int)currentBIndex);
                            f64_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 4:
                        {
                            f64_a = NextQInP1((int)currentAIndex);
                            f64_b = Random(ref rng);
                            f64_c = Random(ref rng);
                            break;
                        }
                        case 5:
                        {
                            f64_a = Random(ref rng);
                            f64_b = NextQInP1((int)currentBIndex);
                            f64_c = Random(ref rng);
                            break;
                        }
                        case 6:
                        {
                            f64_a = NextQInP1((int)currentAIndex);
                            f64_b = Random(ref rng);
                            f64_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 8:
                        {
                            f64_a = NextQInP1((int)currentAIndex);
                            f64_b = NextQInP1((int)currentBIndex);
                            f64_c = NextQInP1((int)currentCIndex);
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
                            f64_a = Random(ref rng);
                            f64_b = Random(ref rng);
                            f64_c = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f64_a = NextQInP2((int)currentAIndex);
                            f64_b = NextQInP2((int)currentBIndex);
                            f64_c = NextQInP2((int)currentCIndex);
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

            return new TestRunnerArguments(f64_a, f64_b, f64_c);
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
