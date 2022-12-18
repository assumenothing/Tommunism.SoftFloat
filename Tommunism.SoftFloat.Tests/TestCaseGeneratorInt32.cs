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
/// Container type for generating signed 32-bit integer test cases.
/// </summary>
internal static class TestCaseGeneratorInt32
{
    #region Fields

    private const int NumP1 = 124;
    private const int NumP2 = (NumP1 * NumP1 + NumP1) / 2;

    private static readonly int[] P1BinarySearchIndexes = new int[NumP1];
    private static readonly uint[] P1 = new uint[NumP1]
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
        0x00800000,
        0x01000000,
        0x02000000,
        0x04000000,
        0x08000000,
        0x10000000,
        0x20000000,
        0x40000000,
        0x80000000,
        0xC0000000,
        0xE0000000,
        0xF0000000,
        0xF8000000,
        0xFC000000,
        0xFE000000,
        0xFF000000,
        0xFF800000,
        0xFFC00000,
        0xFFE00000,
        0xFFF00000,
        0xFFF80000,
        0xFFFC0000,
        0xFFFE0000,
        0xFFFF0000,
        0xFFFF8000,
        0xFFFFC000,
        0xFFFFE000,
        0xFFFFF000,
        0xFFFFF800,
        0xFFFFFC00,
        0xFFFFFE00,
        0xFFFFFF00,
        0xFFFFFF80,
        0xFFFFFFC0,
        0xFFFFFFE0,
        0xFFFFFFF0,
        0xFFFFFFF8,
        0xFFFFFFFC,
        0xFFFFFFFE,
        0xFFFFFFFF,
        0xFFFFFFFD,
        0xFFFFFFFB,
        0xFFFFFFF7,
        0xFFFFFFEF,
        0xFFFFFFDF,
        0xFFFFFFBF,
        0xFFFFFF7F,
        0xFFFFFEFF,
        0xFFFFFDFF,
        0xFFFFFBFF,
        0xFFFFF7FF,
        0xFFFFEFFF,
        0xFFFFDFFF,
        0xFFFFBFFF,
        0xFFFF7FFF,
        0xFFFEFFFF,
        0xFFFDFFFF,
        0xFFFBFFFF,
        0xFFF7FFFF,
        0xFFEFFFFF,
        0xFFDFFFFF,
        0xFFBFFFFF,
        0xFF7FFFFF,
        0xFEFFFFFF,
        0xFDFFFFFF,
        0xFBFFFFFF,
        0xF7FFFFFF,
        0xEFFFFFFF,
        0xDFFFFFFF,
        0xBFFFFFFF,
        0x7FFFFFFF,
        0x3FFFFFFF,
        0x1FFFFFFF,
        0x0FFFFFFF,
        0x07FFFFFF,
        0x03FFFFFF,
        0x01FFFFFF,
        0x00FFFFFF,
        0x007FFFFF,
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

    private const int NumPInfWeightMasks = 29;

    private static readonly uint[] PInfWeightMasks = new uint[NumPInfWeightMasks]
    {
        0xFFFFFFFF,
        0x7FFFFFFF,
        0x3FFFFFFF,
        0x1FFFFFFF,
        0x0FFFFFFF,
        0x07FFFFFF,
        0x03FFFFFF,
        0x01FFFFFF,
        0x00FFFFFF,
        0x007FFFFF,
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
        0x0000000F
    };

    private static readonly uint[] PInfWeightOffsets = new uint[NumPInfWeightMasks]
    {
        0x00000000,
        0xC0000000,
        0xE0000000,
        0xF0000000,
        0xF8000000,
        0xFC000000,
        0xFE000000,
        0xFF000000,
        0xFF800000,
        0xFFC00000,
        0xFFE00000,
        0xFFF00000,
        0xFFF80000,
        0xFFFC0000,
        0xFFFE0000,
        0xFFFF0000,
        0xFFFF8000,
        0xFFFFC000,
        0xFFFFE000,
        0xFFFFF000,
        0xFFFFF800,
        0xFFFFFC00,
        0xFFFFFE00,
        0xFFFFFF00,
        0xFFFFFF80,
        0xFFFFFFC0,
        0xFFFFFFE0,
        0xFFFFFFF0,
        0xFFFFFFF8
    };

    #endregion

    #region Constructors

    static TestCaseGeneratorInt32()
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

    private static int NextP1(int index)
    {
        Debug.Assert(index is >= 0 and < NumP1);
        return (int)P1[index];
    }

    private static int NextP2(int index)
    {
        Debug.Assert(index is >= 0 and < NumP2);

        // Convert the test run index into k-combination indexes.
        int term1Num = Array.BinarySearch(P1BinarySearchIndexes, index);
        if (term1Num < 0) term1Num = ~term1Num - 1;
        int term2Num = index - P1BinarySearchIndexes[term1Num] + term1Num;

        Debug.Assert(term1Num is >= 0 and < NumP1);
        Debug.Assert(term2Num >= term1Num && term2Num < NumP1);

        return (int)(P1[term1Num] + P1[term2Num]);
    }

    private static int RandomP3(ref ThreefryRandom rng) =>
        (int)(P1[rng.NextInt32(NumP1)] + P1[rng.NextInt32(NumP1)] + P1[rng.NextInt32(NumP1)]);

    private static int RandomPInf(ref ThreefryRandom rng)
    {
        int weightMaskNum = rng.NextInt32(NumPInfWeightMasks);
        return (int)((rng.NextUInt32() & PInfWeightMasks[weightMaskNum]) + PInfWeightOffsets[weightMaskNum]);
    }

    #endregion

    #region Nested Types

    /// <summary>
    /// Generates single argument signed 32-bit integer test cases.
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

            int i32_a;

            switch (Level)
            {
                case 1:
                {
                    (subCaseIndex, subCase) = Math.DivRem(index, 3);
                    switch ((int)subCase)
                    {
                        case 0:
                        {
                            i32_a = RandomP3(ref rng);
                            break;
                        }
                        case 1:
                        {
                            i32_a = RandomPInf(ref rng);
                            break;
                        }
                        case 2:
                        {
                            i32_a = NextP1((int)(subCaseIndex % NumP1));
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
                            i32_a = RandomP3(ref rng);
                            break;
                        }
                        case 2:
                        {
                            i32_a = RandomPInf(ref rng);
                            break;
                        }
                        case 3:
                        case 1:
                        {
                            // Calculate the combined index from the two sub-test cases.
                            subCaseIndex <<= 1;
                            if ((int)subCase != 1)
                                subCaseIndex |= 1;

                            i32_a = NextP2((int)(subCaseIndex % NumP2));
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

            return new TestRunnerArguments(i32_a);
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
