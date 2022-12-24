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
/// Container type for generating signed 64-bit integer test cases.
/// </summary>
internal static class TestCaseGeneratorInt64
{
    #region Fields

    private const int NumP1 = 252;
    private const int NumP2 = (NumP1 * NumP1 + NumP1) / 2;

    private static readonly int[] P1BinarySearchIndexes = new int[NumP1];
    private static readonly ulong[] P1 = new ulong[NumP1]
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
        0x8000000000000000,
        0xC000000000000000,
        0xE000000000000000,
        0xF000000000000000,
        0xF800000000000000,
        0xFC00000000000000,
        0xFE00000000000000,
        0xFF00000000000000,
        0xFF80000000000000,
        0xFFC0000000000000,
        0xFFE0000000000000,
        0xFFF0000000000000,
        0xFFF8000000000000,
        0xFFFC000000000000,
        0xFFFE000000000000,
        0xFFFF000000000000,
        0xFFFF800000000000,
        0xFFFFC00000000000,
        0xFFFFE00000000000,
        0xFFFFF00000000000,
        0xFFFFF80000000000,
        0xFFFFFC0000000000,
        0xFFFFFE0000000000,
        0xFFFFFF0000000000,
        0xFFFFFF8000000000,
        0xFFFFFFC000000000,
        0xFFFFFFE000000000,
        0xFFFFFFF000000000,
        0xFFFFFFF800000000,
        0xFFFFFFFC00000000,
        0xFFFFFFFE00000000,
        0xFFFFFFFF00000000,
        0xFFFFFFFF80000000,
        0xFFFFFFFFC0000000,
        0xFFFFFFFFE0000000,
        0xFFFFFFFFF0000000,
        0xFFFFFFFFF8000000,
        0xFFFFFFFFFC000000,
        0xFFFFFFFFFE000000,
        0xFFFFFFFFFF000000,
        0xFFFFFFFFFF800000,
        0xFFFFFFFFFFC00000,
        0xFFFFFFFFFFE00000,
        0xFFFFFFFFFFF00000,
        0xFFFFFFFFFFF80000,
        0xFFFFFFFFFFFC0000,
        0xFFFFFFFFFFFE0000,
        0xFFFFFFFFFFFF0000,
        0xFFFFFFFFFFFF8000,
        0xFFFFFFFFFFFFC000,
        0xFFFFFFFFFFFFE000,
        0xFFFFFFFFFFFFF000,
        0xFFFFFFFFFFFFF800,
        0xFFFFFFFFFFFFFC00,
        0xFFFFFFFFFFFFFE00,
        0xFFFFFFFFFFFFFF00,
        0xFFFFFFFFFFFFFF80,
        0xFFFFFFFFFFFFFFC0,
        0xFFFFFFFFFFFFFFE0,
        0xFFFFFFFFFFFFFFF0,
        0xFFFFFFFFFFFFFFF8,
        0xFFFFFFFFFFFFFFFC,
        0xFFFFFFFFFFFFFFFE,
        0xFFFFFFFFFFFFFFFF,
        0xFFFFFFFFFFFFFFFD,
        0xFFFFFFFFFFFFFFFB,
        0xFFFFFFFFFFFFFFF7,
        0xFFFFFFFFFFFFFFEF,
        0xFFFFFFFFFFFFFFDF,
        0xFFFFFFFFFFFFFFBF,
        0xFFFFFFFFFFFFFF7F,
        0xFFFFFFFFFFFFFEFF,
        0xFFFFFFFFFFFFFDFF,
        0xFFFFFFFFFFFFFBFF,
        0xFFFFFFFFFFFFF7FF,
        0xFFFFFFFFFFFFEFFF,
        0xFFFFFFFFFFFFDFFF,
        0xFFFFFFFFFFFFBFFF,
        0xFFFFFFFFFFFF7FFF,
        0xFFFFFFFFFFFEFFFF,
        0xFFFFFFFFFFFDFFFF,
        0xFFFFFFFFFFFBFFFF,
        0xFFFFFFFFFFF7FFFF,
        0xFFFFFFFFFFEFFFFF,
        0xFFFFFFFFFFDFFFFF,
        0xFFFFFFFFFFBFFFFF,
        0xFFFFFFFFFF7FFFFF,
        0xFFFFFFFFFEFFFFFF,
        0xFFFFFFFFFDFFFFFF,
        0xFFFFFFFFFBFFFFFF,
        0xFFFFFFFFF7FFFFFF,
        0xFFFFFFFFEFFFFFFF,
        0xFFFFFFFFDFFFFFFF,
        0xFFFFFFFFBFFFFFFF,
        0xFFFFFFFF7FFFFFFF,
        0xFFFFFFFEFFFFFFFF,
        0xFFFFFFFDFFFFFFFF,
        0xFFFFFFFBFFFFFFFF,
        0xFFFFFFF7FFFFFFFF,
        0xFFFFFFEFFFFFFFFF,
        0xFFFFFFDFFFFFFFFF,
        0xFFFFFFBFFFFFFFFF,
        0xFFFFFF7FFFFFFFFF,
        0xFFFFFEFFFFFFFFFF,
        0xFFFFFDFFFFFFFFFF,
        0xFFFFFBFFFFFFFFFF,
        0xFFFFF7FFFFFFFFFF,
        0xFFFFEFFFFFFFFFFF,
        0xFFFFDFFFFFFFFFFF,
        0xFFFFBFFFFFFFFFFF,
        0xFFFF7FFFFFFFFFFF,
        0xFFFEFFFFFFFFFFFF,
        0xFFFDFFFFFFFFFFFF,
        0xFFFBFFFFFFFFFFFF,
        0xFFF7FFFFFFFFFFFF,
        0xFFEFFFFFFFFFFFFF,
        0xFFDFFFFFFFFFFFFF,
        0xFFBFFFFFFFFFFFFF,
        0xFF7FFFFFFFFFFFFF,
        0xFEFFFFFFFFFFFFFF,
        0xFDFFFFFFFFFFFFFF,
        0xFBFFFFFFFFFFFFFF,
        0xF7FFFFFFFFFFFFFF,
        0xEFFFFFFFFFFFFFFF,
        0xDFFFFFFFFFFFFFFF,
        0xBFFFFFFFFFFFFFFF,
        0x7FFFFFFFFFFFFFFF,
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

    private const int NumPInfWeightMasks = 61;

    private static readonly ulong[] PInfWeightMasks = new ulong[NumPInfWeightMasks]
    {
        0xFFFFFFFFFFFFFFFF,
        0x7FFFFFFFFFFFFFFF,
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
        0x000000000000000F
    };

    private static readonly ulong[] PInfWeightOffsets = new ulong[NumPInfWeightMasks]
    {
        0x0000000000000000,
        0xC000000000000000,
        0xE000000000000000,
        0xF000000000000000,
        0xF800000000000000,
        0xFC00000000000000,
        0xFE00000000000000,
        0xFF00000000000000,
        0xFF80000000000000,
        0xFFC0000000000000,
        0xFFE0000000000000,
        0xFFF0000000000000,
        0xFFF8000000000000,
        0xFFFC000000000000,
        0xFFFE000000000000,
        0xFFFF000000000000,
        0xFFFF800000000000,
        0xFFFFC00000000000,
        0xFFFFE00000000000,
        0xFFFFF00000000000,
        0xFFFFF80000000000,
        0xFFFFFC0000000000,
        0xFFFFFE0000000000,
        0xFFFFFF0000000000,
        0xFFFFFF8000000000,
        0xFFFFFFC000000000,
        0xFFFFFFE000000000,
        0xFFFFFFF000000000,
        0xFFFFFFF800000000,
        0xFFFFFFFC00000000,
        0xFFFFFFFE00000000,
        0xFFFFFFFF00000000,
        0xFFFFFFFF80000000,
        0xFFFFFFFFC0000000,
        0xFFFFFFFFE0000000,
        0xFFFFFFFFF0000000,
        0xFFFFFFFFF8000000,
        0xFFFFFFFFFC000000,
        0xFFFFFFFFFE000000,
        0xFFFFFFFFFF000000,
        0xFFFFFFFFFF800000,
        0xFFFFFFFFFFC00000,
        0xFFFFFFFFFFE00000,
        0xFFFFFFFFFFF00000,
        0xFFFFFFFFFFF80000,
        0xFFFFFFFFFFFC0000,
        0xFFFFFFFFFFFE0000,
        0xFFFFFFFFFFFF0000,
        0xFFFFFFFFFFFF8000,
        0xFFFFFFFFFFFFC000,
        0xFFFFFFFFFFFFE000,
        0xFFFFFFFFFFFFF000,
        0xFFFFFFFFFFFFF800,
        0xFFFFFFFFFFFFFC00,
        0xFFFFFFFFFFFFFE00,
        0xFFFFFFFFFFFFFF00,
        0xFFFFFFFFFFFFFF80,
        0xFFFFFFFFFFFFFFC0,
        0xFFFFFFFFFFFFFFE0,
        0xFFFFFFFFFFFFFFF0,
        0xFFFFFFFFFFFFFFF8
    };

    #endregion

    #region Constructors

    static TestCaseGeneratorInt64()
    {
        // Generate binary search indexes for P1.
        var remaining = NumP1;
        var index = remaining;
        for (var i = 1; i < NumP1; i++)
        {
            P1BinarySearchIndexes[i] = index;
            remaining--;
            index += remaining;
        }

        Debug.Assert(remaining == 1);
        Debug.Assert(index == NumP2);
    }

    #endregion

    #region Methods

    public static TestCaseGenerator Create(int argumentCount, int level = 1) => argumentCount switch
    {
        0 or 1 => new Args1(level),
        _ => throw new NotImplementedException()
    };

    private static long NextP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumP1);
        return (long)P1[index];
    }

    private static long NextP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumP2);

        // Convert the test run index into k-combination indexes.
        int term1Num = Array.BinarySearch(P1BinarySearchIndexes, index);
        if (term1Num < 0) term1Num = ~term1Num - 1;
        int term2Num = index - P1BinarySearchIndexes[term1Num] + term1Num;

        Debug.Assert(term1Num is >= 0 and < NumP1);
        Debug.Assert(term2Num >= term1Num && term2Num < NumP1);

        return (long)(P1[term1Num] + P1[term2Num]);
    }

    private static long RandomP3(ref ThreefryRandom rng) =>
        (long)(P1[rng.NextInt32(NumP1)] + P1[rng.NextInt32(NumP1)] + P1[rng.NextInt32(NumP1)]);

    private static long RandomPInf(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumPInfWeightMasks);
        return (long)((rng.NextUInt64() & PInfWeightMasks[weightMaskNum]) + PInfWeightOffsets[weightMaskNum]);
    }

    #endregion

    #region Nested Types

    /// <summary>
    /// Generates single argument signed 64-bit integer test cases.
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

            long i64_a;

            switch (Level)
            {
                case 1:
                {
                    (subCaseIndex, subCase) = Math.DivRem(index, 3);
                    switch ((int)subCase)
                    {
                        case 0:
                        {
                            i64_a = RandomP3(ref rng);
                            break;
                        }
                        case 1:
                        {
                            i64_a = RandomPInf(ref rng);
                            break;
                        }
                        case 2:
                        {
                            i64_a = NextP1((int)(subCaseIndex % NumP1));
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
                    subCase = index & 3;
                    subCaseIndex = index >> 2;
                    switch ((int)subCase)
                    {
                        case 0:
                        {
                            i64_a = RandomP3(ref rng);
                            break;
                        }
                        case 2:
                        {
                            i64_a = RandomPInf(ref rng);
                            break;
                        }
                        case 3:
                        case 1:
                        {
                            // Calculate the combined index from the two sub-test cases.
                            subCaseIndex <<= 1;
                            if ((int)subCase != 1)
                                subCaseIndex |= 1;

                            i64_a = NextP2((int)(subCaseIndex % NumP2));
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

            return new TestRunnerArguments(i64_a);
        }

        protected override long CalculateTotalCases() => Level switch
        {
            1 => 3 * NumP1,
            2 => 2 * NumP2,
            _ => throw new NotImplementedException("Test level not implemented.")
        };

        #endregion
    }

    #endregion
}
