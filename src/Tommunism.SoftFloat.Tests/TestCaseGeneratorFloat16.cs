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
internal static class TestCaseGeneratorFloat16
{
    #region Fields

    private const int NumQIn = 22;
    private const int NumQOut = 34;
    private const int NumP1 = 4;
    private const int NumP2 = 36;

    private const int NumQInP1 = NumQIn * NumP1;
    private const int NumQOutP1 = NumQOut * NumP1;

    private const int NumQInP2 = NumQIn * NumP2;
    private const int NumQOutP2 = NumQOut * NumP2;

    private static readonly ushort[] QIn = new ushort[NumQIn]
    {
        0x0000,    // positive, subnormal
        0x0400,    // positive, -14
        0x1000,    // positive, -11
        0x3400,    // positive,  -2
        0x3800,    // positive,  -1
        0x3C00,    // positive,   0
        0x4000,    // positive,   1
        0x4400,    // positive,   2
        0x6800,    // positive,  11
        0x7800,    // positive,  15
        0x7C00,    // positive, infinity or NaN
        0x8000,    // negative, subnormal
        0x8400,    // negative, -14
        0x9000,    // negative, -11
        0xB400,    // negative,  -2
        0xB800,    // negative,  -1
        0xBC00,    // negative,   0
        0xC000,    // negative,   1
        0xC400,    // negative,   2
        0xE800,    // negative,  11
        0xF800,    // negative,  15
        0xFC00     // negative, infinity or NaN
    };

    private static readonly ushort[] QOut = new ushort[NumQOut]
    {
        0x0000,    // positive, subnormal
        0x0400,    // positive, -14
        0x0800,    // positive, -13
        0x1000,    // positive, -11
        0x2C00,    // positive,  -4
        0x3000,    // positive,  -3
        0x3400,    // positive,  -2
        0x3800,    // positive,  -1
        0x3C00,    // positive,   0
        0x4000,    // positive,   1
        0x4400,    // positive,   2
        0x4800,    // positive,   3
        0x4C00,    // positive,   4
        0x6800,    // positive,  11
        0x7400,    // positive,  14
        0x7800,    // positive,  15
        0x7C00,    // positive, infinity or NaN
        0x8000,    // negative, subnormal
        0x8400,    // negative, -14
        0x8800,    // negative, -13
        0x9000,    // negative, -11
        0xAC00,    // negative,  -4
        0xB000,    // negative,  -3
        0xB400,    // negative,  -2
        0xB800,    // negative,  -1
        0xBC00,    // negative,   0
        0xC000,    // negative,   1
        0xC400,    // negative,   2
        0xC800,    // negative,   3
        0xCC00,    // negative,   4
        0xE800,    // negative,  11
        0xF400,    // negative,  14
        0xF800,    // negative,  15
        0xFC00     // negative, infinity or NaN
    };

    private static readonly ushort[] P1 = new ushort[NumP1]
    {
        0x0000,
        0x0001,
        0x03FF,
        0x03FE
    };

    private static readonly ushort[] P2 = new ushort[NumP2]
    {
        0x0000,
        0x0001,
        0x0002,
        0x0004,
        0x0008,
        0x0010,
        0x0020,
        0x0040,
        0x0080,
        0x0100,
        0x0200,
        0x0300,
        0x0380,
        0x03C0,
        0x03E0,
        0x03F0,
        0x03F8,
        0x03FC,
        0x03FE,
        0x03FF,
        0x03FD,
        0x03FB,
        0x03F7,
        0x03EF,
        0x03DF,
        0x03BF,
        0x037F,
        0x02FF,
        0x01FF,
        0x00FF,
        0x007F,
        0x003F,
        0x001F,
        0x000F,
        0x0007,
        0x0003
    };

    private const int NumQInfWeightMasks = 4;

    private static readonly ushort[] QInfWeightMasks = new ushort[NumQInfWeightMasks]
    {
        0xFC00, 0xFC00, 0xBC00, 0x9C00
    };

    private static readonly ushort[] QInfWeightOffsets = new ushort[NumQInfWeightMasks]
    {
        0x0000, 0x0000, 0x2000, 0x3000
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

    private static Float16 NextQInP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumQInP1);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP1);
        Debug.Assert(sigNum is >= 0 and < NumP1);
        Debug.Assert(expNum is >= 0 and < NumQIn);

        return Float16.FromUIntBits((ushort)(QIn[expNum] | P1[sigNum]));
    }

    private static Float16 NextQOutP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumQOutP1);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP1);
        Debug.Assert(sigNum is >= 0 and < NumP1);
        Debug.Assert(expNum is >= 0 and < NumQOut);

        return Float16.FromUIntBits((ushort)(QOut[expNum] | P1[sigNum]));
    }

    private static Float16 NextQInP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumQInP2);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP2);
        Debug.Assert(sigNum is >= 0 and < NumP2);
        Debug.Assert(expNum is >= 0 and < NumQIn);

        return Float16.FromUIntBits((ushort)(QIn[expNum] | P2[sigNum]));
    }

    private static Float16 NextQOutP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumQOutP2);

        // Convert the index into array permutation indexes.
        var (expNum, sigNum) = Math.DivRem(index, NumP2);
        Debug.Assert(sigNum is >= 0 and < NumP2);
        Debug.Assert(expNum is >= 0 and < NumQOut);

        return Float16.FromUIntBits((ushort)(QOut[expNum] | P2[sigNum]));
    }

    private static Float16 RandomQOutP3(ref ThreefryRandom rng) =>
        Float16.FromUIntBits((ushort)(QOut[rng.NextInt32(NumQOut)]
            | ((P2[rng.NextInt32(NumP2)] + P2[rng.NextInt32(NumP2)]) & 0x03FF)));

    private static Float16 RandomQOutPInf(ref ThreefryRandom rng) =>
        Float16.FromUIntBits((ushort)(QOut[rng.NextInt32(NumQOut)] | (rng.NextUInt32() & 0x03FF)));

    private static Float16 RandomQInfP3(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumQInfWeightMasks);
        return Float16.FromUIntBits((ushort)(
            ((rng.NextUInt32() & QInfWeightMasks[weightMaskNum]) + QInfWeightOffsets[weightMaskNum])
            | (((uint)P2[rng.NextInt32(NumP2)] + P2[rng.NextInt32(NumP2)]) & 0x03FF)
        ));
    }

    private static Float16 RandomQInfPInf(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumQInfWeightMasks);
        return Float16.FromUIntBits((ushort)(
            (rng.NextUInt32() & (QInfWeightMasks[weightMaskNum] | 0x03FF)) + QInfWeightOffsets[weightMaskNum]
        ));
    }

    private static Float16 Random(ref ThreefryRandom rng) => rng.NextInt32(8) switch
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
    /// Generates single argument 16-bit floating-point test cases.
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

            Float16 f16_a;

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
                            f16_a = Random(ref rng);
                            break;
                        }
                        case 2:
                        {
                            f16_a = NextQOutP1((int)(subCaseIndex % NumQOutP1));
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
                            f16_a = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f16_a = NextQOutP2((int)(subCaseIndex % NumQOutP2));
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

            return new TestRunnerArguments(f16_a);
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
    /// Generates double argument 16-bit floating-point test cases.
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
            Float16 f16_a, f16_b;

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
                            f16_a = Random(ref rng);
                            f16_b = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f16_a = NextQInP1((int)currentAIndex);
                            f16_b = Random(ref rng);
                            break;
                        }
                        case 3:
                        {
                            f16_a = Random(ref rng);
                            f16_b = NextQInP1((int)currentBIndex);
                            break;
                        }
                        case 5:
                        {
                            f16_a = NextQInP1((int)currentAIndex);
                            f16_b = NextQInP1((int)currentBIndex);
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
                            f16_a = Random(ref rng);
                            f16_b = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f16_a = NextQInP2((int)currentAIndex);
                            f16_b = NextQInP2((int)currentBIndex);
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

            return new TestRunnerArguments(f16_a, f16_b);
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
    /// Generates triple argument 16-bit floating-point test cases.
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
            Float16 f16_a, f16_b, f16_c;

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
                            f16_a = Random(ref rng);
                            f16_b = Random(ref rng);
                            f16_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 1:
                        {
                            f16_a = NextQInP1((int)currentAIndex);
                            f16_b = NextQInP1((int)currentBIndex);
                            f16_c = Random(ref rng);
                            break;
                        }
                        case 2:
                        case 7:
                        {
                            f16_a = Random(ref rng);
                            f16_b = Random(ref rng);
                            f16_c = Random(ref rng);
                            break;
                        }
                        case 3:
                        {
                            f16_a = Random(ref rng);
                            f16_b = NextQInP1((int)currentBIndex);
                            f16_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 4:
                        {
                            f16_a = NextQInP1((int)currentAIndex);
                            f16_b = Random(ref rng);
                            f16_c = Random(ref rng);
                            break;
                        }
                        case 5:
                        {
                            f16_a = Random(ref rng);
                            f16_b = NextQInP1((int)currentBIndex);
                            f16_c = Random(ref rng);
                            break;
                        }
                        case 6:
                        {
                            f16_a = NextQInP1((int)currentAIndex);
                            f16_b = Random(ref rng);
                            f16_c = NextQInP1((int)currentCIndex);
                            break;
                        }
                        case 8:
                        {
                            f16_a = NextQInP1((int)currentAIndex);
                            f16_b = NextQInP1((int)currentBIndex);
                            f16_c = NextQInP1((int)currentCIndex);
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
                            f16_a = Random(ref rng);
                            f16_b = Random(ref rng);
                            f16_c = Random(ref rng);
                            break;
                        }
                        case 1:
                        {
                            f16_a = NextQInP2((int)currentAIndex);
                            f16_b = NextQInP2((int)currentBIndex);
                            f16_c = NextQInP2((int)currentCIndex);
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

            return new TestRunnerArguments(f16_a, f16_b, f16_c);
        }

        protected override long CalculateTotalCases() => Level switch
        {
            1 => 9 * NumQInP1 * NumQInP1 * NumQInP1,
            2 => 2 * NumQInP2 * NumQInP2 * NumQInP2,
            _ => throw new NotImplementedException("Test level not implemented.")
        };

        #endregion
    }

    #endregion
}
