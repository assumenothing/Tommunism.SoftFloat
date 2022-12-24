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

/// <summary>
/// Container type for generating 16-bit floating-point test cases.
/// </summary>
internal static class TestCaseGeneratorFloat32
{
    #region Fields

    private const int NumQIn = 22;
    private const int NumQOut = 50;
    private const int NumP1 = 4;
    private const int NumP2 = 88;

    private const int NumQInP1 = NumQIn * NumP1;
    private const int NumQOutP1 = NumQOut * NumP1;

    private const int NumQInP2 = NumQIn * NumP2;
    private const int NumQOutP2 = NumQOut * NumP2;

    private static readonly uint[] QIn = new uint[NumQIn]
    {
        0x00000000,    // positive, subnormal
        0x00800000,    // positive, -126
        0x33800000,    // positive,  -24
        0x3E800000,    // positive,   -2
        0x3F000000,    // positive,   -1
        0x3F800000,    // positive,    0
        0x40000000,    // positive,    1
        0x40800000,    // positive,    2
        0x4B800000,    // positive,   24
        0x7F000000,    // positive,  127
        0x7F800000,    // positive, infinity or NaN
        0x80000000,    // negative, subnormal
        0x80800000,    // negative, -126
        0xB3800000,    // negative,  -24
        0xBE800000,    // negative,   -2
        0xBF000000,    // negative,   -1
        0xBF800000,    // negative,    0
        0xC0000000,    // negative,    1
        0xC0800000,    // negative,    2
        0xCB800000,    // negative,   24
        0xFE800000,    // negative,  126
        0xFF800000     // negative, infinity or NaN
    };

    private static readonly uint[] QOut = new uint[NumQOut]
    {
        0x00000000,    // positive, subnormal
        0x00800000,    // positive, -126
        0x01000000,    // positive, -125
        0x33800000,    // positive,  -24
        0x3D800000,    // positive,   -4
        0x3E000000,    // positive,   -3
        0x3E800000,    // positive,   -2
        0x3F000000,    // positive,   -1
        0x3F800000,    // positive,    0
        0x40000000,    // positive,    1
        0x40800000,    // positive,    2
        0x41000000,    // positive,    3
        0x41800000,    // positive,    4
        0x4B800000,    // positive,   24
        0x4E000000,    // positive,   29
        0x4E800000,    // positive,   30
        0x4F000000,    // positive,   31
        0x4F800000,    // positive,   32
        0x5E000000,    // positive,   61
        0x5E800000,    // positive,   62
        0x5F000000,    // positive,   63
        0x5F800000,    // positive,   64
        0x7E800000,    // positive,  126
        0x7F000000,    // positive,  127
        0x7F800000,    // positive, infinity or NaN
        0x80000000,    // negative, subnormal
        0x80800000,    // negative, -126
        0x81000000,    // negative, -125
        0xB3800000,    // negative,  -24
        0xBD800000,    // negative,   -4
        0xBE000000,    // negative,   -3
        0xBE800000,    // negative,   -2
        0xBF000000,    // negative,   -1
        0xBF800000,    // negative,    0
        0xC0000000,    // negative,    1
        0xC0800000,    // negative,    2
        0xC1000000,    // negative,    3
        0xC1800000,    // negative,    4
        0xCB800000,    // negative,   24
        0xCE000000,    // negative,   29
        0xCE800000,    // negative,   30
        0xCF000000,    // negative,   31
        0xCF800000,    // negative,   32
        0xDE000000,    // negative,   61
        0xDE800000,    // negative,   62
        0xDF000000,    // negative,   63
        0xDF800000,    // negative,   64
        0xFE800000,    // negative,  126
        0xFF000000,    // negative,  127
        0xFF800000     // negative, infinity or NaN
    };

    private static readonly uint[] P1 = new uint[NumP1]
    {
        0x00000000,
        0x00000001,
        0x007FFFFF,
        0x007FFFFE
    };

    private static readonly uint[] P2 = new uint[NumP2]
    {
        0x00000000,
        0x00000001,
        0x00000002,
        0x00000004,
        0x00000008,
        0x00000010,
        0x00000020,
        0x00000040,
        0x00000080,
        0x00000100,
        0x00000200,
        0x00000400,
        0x00000800,
        0x00001000,
        0x00002000,
        0x00004000,
        0x00008000,
        0x00010000,
        0x00020000,
        0x00040000,
        0x00080000,
        0x00100000,
        0x00200000,
        0x00400000,
        0x00600000,
        0x00700000,
        0x00780000,
        0x007C0000,
        0x007E0000,
        0x007F0000,
        0x007F8000,
        0x007FC000,
        0x007FE000,
        0x007FF000,
        0x007FF800,
        0x007FFC00,
        0x007FFE00,
        0x007FFF00,
        0x007FFF80,
        0x007FFFC0,
        0x007FFFE0,
        0x007FFFF0,
        0x007FFFF8,
        0x007FFFFC,
        0x007FFFFE,
        0x007FFFFF,
        0x007FFFFD,
        0x007FFFFB,
        0x007FFFF7,
        0x007FFFEF,
        0x007FFFDF,
        0x007FFFBF,
        0x007FFF7F,
        0x007FFEFF,
        0x007FFDFF,
        0x007FFBFF,
        0x007FF7FF,
        0x007FEFFF,
        0x007FDFFF,
        0x007FBFFF,
        0x007F7FFF,
        0x007EFFFF,
        0x007DFFFF,
        0x007BFFFF,
        0x0077FFFF,
        0x006FFFFF,
        0x005FFFFF,
        0x003FFFFF,
        0x001FFFFF,
        0x000FFFFF,
        0x0007FFFF,
        0x0003FFFF,
        0x0001FFFF,
        0x0000FFFF,
        0x00007FFF,
        0x00003FFF,
        0x00001FFF,
        0x00000FFF,
        0x000007FF,
        0x000003FF,
        0x000001FF,
        0x000000FF,
        0x0000007F,
        0x0000003F,
        0x0000001F,
        0x0000000F,
        0x00000007,
        0x00000003
    };

    private const int NumQInfWeightMasks = 7;

    private static readonly uint[] QInfWeightMasks = new uint[NumQInfWeightMasks]
    {
        0xFF800000,
        0xFF800000,
        0xBF800000,
        0x9F800000,
        0x8F800000,
        0x87800000,
        0x83800000
    };

    private static readonly uint[] QInfWeightOffsets = new uint[NumQInfWeightMasks]
    {
        0x00000000,
        0x00000000,
        0x20000000,
        0x30000000,
        0x38000000,
        0x3C000000,
        0x3E000000
    };

    #endregion

    #region Methods

    public static TestCaseGenerator Create(int argumentCount, int level = 1) => argumentCount switch
    {
        1 => new Args1(level),
        2 => new Args2(level),
        3 => new Args3(level),
        _ => throw new NotImplementedException()
    };

    private static Float32 NextQInP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumQInP1);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP1);
        Debug.Assert(sigNum is >= 0 and < NumP1);
        Debug.Assert(expNum is >= 0 and < NumQIn);

        return Float32.FromUIntBits(QIn[expNum] | P1[sigNum]);
    }

    private static Float32 NextQOutP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumQOutP1);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP1);
        Debug.Assert(sigNum is >= 0 and < NumP1);
        Debug.Assert(expNum is >= 0 and < NumQOut);

        return Float32.FromUIntBits(QOut[expNum] | P1[sigNum]);
    }

    private static Float32 NextQInP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumQInP2);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP2);
        Debug.Assert(sigNum is >= 0 and < NumP2);
        Debug.Assert(expNum is >= 0 and < NumQIn);

        return Float32.FromUIntBits(QIn[expNum] | P2[sigNum]);
    }

    private static Float32 NextQOutP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumQOutP2);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP2);
        Debug.Assert(sigNum is >= 0 and < NumP2);
        Debug.Assert(expNum is >= 0 and < NumQOut);

        return Float32.FromUIntBits(QOut[expNum] | P2[sigNum]);
    }

    private static Float32 RandomQOutP3(ref ThreefryRandom rng) =>
        Float32.FromUIntBits(QOut[rng.NextInt32(NumQOut)]
            | ((P2[rng.NextInt32(NumP2)] + P2[rng.NextInt32(NumP2)]) & 0x007FFFFF));

    private static Float32 RandomQOutPInf(ref ThreefryRandom rng) =>
        Float32.FromUIntBits(QOut[rng.NextInt32(NumQOut)] | (rng.NextUInt32() & 0x007FFFFF));

    private static Float32 RandomQInfP3(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumQInfWeightMasks);
        return Float32.FromUIntBits(
            ((rng.NextUInt32() & QInfWeightMasks[weightMaskNum]) + QInfWeightOffsets[weightMaskNum])
            | ((P2[rng.NextInt32(NumP2)] + P2[rng.NextInt32(NumP2)]) & 0x007FFFFF)
        );
    }

    private static Float32 RandomQInfPInf(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumQInfWeightMasks);
        return Float32.FromUIntBits(
            (rng.NextUInt32() & (QInfWeightMasks[weightMaskNum] | 0x007FFFFF)) + QInfWeightOffsets[weightMaskNum]
        );
    }

    private static Float32 Random(ref ThreefryRandom rng) => rng.NextInt32(8) switch
    {
        0 or 1 or 2 => RandomQOutP3(ref rng),
        3 => RandomQOutPInf(ref rng),
        4 or 5 or 6 => RandomQInfP3(ref rng),
        7 => RandomQInfPInf(ref rng),
        _ => throw new InvalidOperationException("Invalid switch case.")
    };

    #endregion

    #region Nested Types

    /// <summary>
    /// Generates single argument 32-bit floating-point test cases.
    /// </summary>
    public class Args1 : TestCaseGenerator
    {
        #region Constructors

        public Args1() { }

        public Args1(int level) : base(level) { }

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

            Float32 f32_a;

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
                            f32_a = Random(ref rng);
                            break;
                        }
                        case 2:
                        {
                            f32_a = NextQOutP1((int)(subCaseIndex % NumQOutP1));
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
                            f32_a = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f32_a = NextQOutP2((int)(subCaseIndex % NumQOutP2));
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

            return new TestRunnerArguments(f32_a);
        }

        protected override long CalculateTotalCases() => Level switch
        {
            1 => 3 * NumQOutP1,
            2 => 2 * NumQOutP2,
            _ => throw new NotImplementedException("Test level not implemented.")
        };

        #endregion
    }

    /// <summary>
    /// Generates double argument 32-bit floating-point test cases.
    /// </summary>
    public class Args2 : TestCaseGenerator
    {
        #region Constructors

        public Args2() { }

        public Args2(int level) : base(level) { }

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
            Float32 f32_a, f32_b;

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
                            f32_a = Random(ref rng);
                            f32_b = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f32_a = NextQInP1((int)currentAIndex);
                            f32_b = Random(ref rng);
                            break;
                        }
                        case 3:
                        {
                            f32_a = Random(ref rng);
                            f32_b = NextQInP1((int)currentBIndex);
                            break;
                        }
                        case 5:
                        {
                            f32_a = NextQInP1((int)currentAIndex);
                            f32_b = NextQInP1((int)currentBIndex);
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
                            f32_a = Random(ref rng);
                            f32_b = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f32_a = NextQInP2((int)currentAIndex);
                            f32_b = NextQInP2((int)currentBIndex);
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

            return new TestRunnerArguments(f32_a, f32_b);
        }

        protected override long CalculateTotalCases() => Level switch
        {
            1 => 6 * NumQInP1 * NumQInP1,
            2 => 2 * NumQInP2 * NumQInP2,
            _ => throw new NotImplementedException("Test level not implemented.")
        };

        #endregion
    }

    /// <summary>
    /// Generates double argument 32-bit floating-point test cases.
    /// </summary>
    public class Args3 : TestCaseGenerator
    {
        #region Constructors

        public Args3() { }

        public Args3(int level) : base(level) { }

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
            Float32 f32_a, f32_b, f32_c;

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
                            f32_a = Random(ref rng);
                            f32_b = Random(ref rng);
                            f32_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 1:
                        {
                            f32_a = NextQInP1((int)currentAIndex);
                            f32_b = NextQInP1((int)currentBIndex);
                            f32_c = Random(ref rng);
                            break;
                        }
                        case 2:
                        case 7:
                        {
                            f32_a = Random(ref rng);
                            f32_b = Random(ref rng);
                            f32_c = Random(ref rng);
                            break;
                        }
                        case 3:
                        {
                            f32_a = Random(ref rng);
                            f32_b = NextQInP1((int)currentBIndex);
                            f32_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 4:
                        {
                            f32_a = NextQInP1((int)currentAIndex);
                            f32_b = Random(ref rng);
                            f32_c = Random(ref rng);
                            break;
                        }
                        case 5:
                        {
                            f32_a = Random(ref rng);
                            f32_b = NextQInP1((int)currentBIndex);
                            f32_c = Random(ref rng);
                            break;
                        }
                        case 6:
                        {
                            f32_a = NextQInP1((int)currentAIndex);
                            f32_b = Random(ref rng);
                            f32_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 8:
                        {
                            f32_a = NextQInP1((int)currentAIndex);
                            f32_b = NextQInP1((int)currentBIndex);
                            f32_c = NextQInP1((int)currentCIndex);
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
                            f32_a = Random(ref rng);
                            f32_b = Random(ref rng);
                            f32_c = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f32_a = NextQInP2((int)currentAIndex);
                            f32_b = NextQInP2((int)currentBIndex);
                            f32_c = NextQInP2((int)currentCIndex);
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

            return new TestRunnerArguments(f32_a, f32_b, f32_c);
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
