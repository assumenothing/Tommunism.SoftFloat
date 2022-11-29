using System;

namespace Tommunism.SoftFloat;

using static Primitives;
using static Internals;

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

partial class SoftFloatSpecialize
{
    public sealed class X86 : SoftFloatSpecialize
    {
        #region Default Instance & Constructor

        /// <summary>
        /// Gets or sets the default instance to use for specialized implementation details.
        /// </summary>
        public static new X86 Default { get; } = new();

        // This is a sealed class with constant default NaN bits, so it should be safe to cache them.
        public X86() => InitializeDefaultNaNs();

        #endregion

        public override Tininess InitialDetectTininess => Tininess.AfterRounding;

        #region Integer Conversion Constants

        public override uint UInt32FromPosOverflow => 0xFFFFFFFF;

        public override uint UInt32FromNegOverflow => 0xFFFFFFFF;

        public override uint UInt32FromNaN => 0xFFFFFFFF;

        public override int Int32FromPosOverflow => -0x7FFFFFFF - 1;

        public override int Int32FromNegOverflow => -0x7FFFFFFF - 1;

        public override int Int32FromNaN => -0x7FFFFFFF - 1;

        public override ulong UInt64FromPosOverflow => 0xFFFFFFFFFFFFFFFF;

        public override ulong UInt64FromNegOverflow => 0xFFFFFFFFFFFFFFFF;

        public override ulong UInt64FromNaN => 0xFFFFFFFFFFFFFFFF;

        public override long Int64FromPosOverflow => -0x7FFFFFFFFFFFFFFF - 1;

        public override long Int64FromNegOverflow => -0x7FFFFFFFFFFFFFFF - 1;

        public override long Int64FromNaN => -0x7FFFFFFFFFFFFFFF - 1;

        #endregion

        #region Float16

        public override uint16_t DefaultNaNFloat16Bits => 0xFE00;

        public override bool IsSignalNaNFloat16Bits(uint_fast16_t bits) => (bits & 0x7E00) == 0x7C00 && (bits & 0x01FF) != 0;

        public override void Float16BitsToCommonNaN(SoftFloatState state, uint_fast16_t bits, out SoftFloatCommonNaN commonNaN)
        {
            if (IsSignalNaNFloat16Bits(bits))
                state.RaiseFlags(ExceptionFlags.Invalid);

            commonNaN = new SoftFloatCommonNaN()
            {
                Sign = (bits >> 15) != 0,
                Value = new UInt128(upper: (ulong)bits << 54, lower: 0)
            };
        }

        public override uint16_t CommonNaNToFloat16Bits(in SoftFloatCommonNaN commonNaN) =>
            (uint16_t)((commonNaN.Sign ? (1U << 15) : 0) | 0x7E00 | (uint_fast16_t)(commonNaN.Value >> 118));

        public override uint16_t PropagateNaNFloat16Bits(SoftFloatState state, uint_fast16_t bitsA, uint_fast16_t bitsB)
        {
            var isSigNaNA = IsSignalNaNFloat16Bits(bitsA);
            var isSigNaNB = IsSignalNaNFloat16Bits(bitsB);

            // Make NaNs non-signaling.
            var uiNonsigA = bitsA | 0x0200;
            var uiNonsigB = bitsB | 0x0200;

            if (isSigNaNA | isSigNaNB)
            {
                state.RaiseFlags(ExceptionFlags.Invalid);
                if (isSigNaNA)
                {
                    if (isSigNaNB)
                        goto returnLargerMag;

                    return (uint16_t)(IsNaNF16UI(bitsB) ? uiNonsigB : uiNonsigA);
                }
                else
                {
                    return (uint16_t)(IsNaNF16UI(bitsA) ? uiNonsigA : uiNonsigB);
                }
            }

        returnLargerMag:
            var uiMagA = bitsA & 0x7FFF;
            var uiMagB = bitsB & 0x7FFF;
            if (uiMagA < uiMagB) return (uint16_t)uiNonsigB;
            if (uiMagB < uiMagA) return (uint16_t)uiNonsigA;
            return (uint16_t)((uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB);
        }

        #endregion

        #region Float32

        public override uint32_t DefaultNaNFloat32Bits => 0xFFC00000;

        public override bool IsSigNaNFloat32Bits(uint_fast32_t bits) => (bits & 0x7FC00000) == 0x7FC00000 && (bits & 0x003FFFFF) != 0;

        public override void Float32BitsToCommonNaN(SoftFloatState state, uint_fast32_t bits, out SoftFloatCommonNaN commonNaN)
        {
            if (IsSigNaNFloat32Bits(bits))
                state.RaiseFlags(ExceptionFlags.Invalid);

            commonNaN = new SoftFloatCommonNaN()
            {
                Sign = (bits >> 31) != 0,
                Value = new UInt128(upper: (ulong)bits << 41, lower: 0)
            };
        }

        public override uint32_t CommonNaNToFloat32Bits(in SoftFloatCommonNaN commonNaN) =>
            (commonNaN.Sign ? (1U << 31) : 0U) | 0x7FC00000 | (uint_fast32_t)(commonNaN.Value >> 105);

        public override uint32_t PropagateNaNFloat32Bits(SoftFloatState state, uint_fast32_t bitsA, uint_fast32_t bitsB)
        {
            var isSigNaNA = IsSigNaNFloat32Bits(bitsA);
            var isSigNaNB = IsSigNaNFloat32Bits(bitsB);

            // Make NaNs non-signaling.
            var uiNonsigA = bitsA | 0x00400000;
            var uiNonsigB = bitsB | 0x00400000;

            if (isSigNaNA | isSigNaNB)
            {
                state.RaiseFlags(ExceptionFlags.Invalid);
                if (isSigNaNA)
                {
                    if (isSigNaNB)
                        goto returnLargerMag;

                    return IsNaNF32UI(bitsB) ? uiNonsigB : uiNonsigA;
                }
                else
                {
                    return IsNaNF32UI(bitsA) ? uiNonsigA : uiNonsigB;
                }
            }

        returnLargerMag:
            var uiMagA = bitsA & 0x7FFFFFFF;
            var uiMagB = bitsB & 0x7FFFFFFF;
            if (uiMagA < uiMagB) return uiNonsigB;
            if (uiMagB < uiMagA) return uiNonsigA;
            return (uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB;
        }

        #endregion

        #region Float64

        public override uint64_t DefaultNaNFloat64Bits => 0xFFF8000000000000;

        public override bool IsSigNaNFloat64Bits(uint_fast64_t bits) =>
            (bits & 0x7FF8000000000000) == 0x7FF8000000000000 && (bits & 0x0007FFFFFFFFFFFF) != 0;

        public override void Float64BitsToCommonNaN(SoftFloatState state, uint_fast64_t bits, out SoftFloatCommonNaN commonNaN)
        {
            if (IsSigNaNFloat64Bits(bits))
                state.RaiseFlags(ExceptionFlags.Invalid);

            commonNaN = new SoftFloatCommonNaN()
            {
                Sign = (bits >> 63) != 0,
                Value = new UInt128(upper: bits << 12, lower: 0)
            };
        }

        public override uint64_t CommonNaNToFloat64Bits(in SoftFloatCommonNaN commonNaN) =>
            (commonNaN.Sign ? (1UL << 63) : 0) | 0x7FF8000000000000 | (uint_fast64_t)(commonNaN.Value >> 76);

        public override uint64_t PropagateNaNFloat64Bits(SoftFloatState state, uint_fast64_t bitsA, uint_fast64_t bitsB)
        {
            var isSigNaNA = IsSigNaNFloat64Bits(bitsA);
            var isSigNaNB = IsSigNaNFloat64Bits(bitsB);

            // Make NaNs non-signaling.
            var uiNonsigA = bitsA | 0x0008000000000000;
            var uiNonsigB = bitsB | 0x0008000000000000;

            if (isSigNaNA | isSigNaNB)
            {
                state.RaiseFlags(ExceptionFlags.Invalid);
                if (isSigNaNA)
                {
                    if (isSigNaNB)
                        goto returnLargerMag;

                    return IsNaNF64UI(bitsB) ? uiNonsigB : uiNonsigA;
                }
                else
                {
                    return IsNaNF64UI(bitsA) ? uiNonsigA : uiNonsigB;
                }
            }

        returnLargerMag:
            var uiMagA = bitsA & 0x7FFFFFFFFFFFFFFF;
            var uiMagB = bitsB & 0x7FFFFFFFFFFFFFFF;
            if (uiMagA < uiMagB) return uiNonsigB;
            if (uiMagB < uiMagA) return uiNonsigA;
            return (uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB;
        }

        #endregion

        #region ExtFloat80

        public override uint16_t DefaultNaNExtFloat80BitsUpper => 0xFFFF;

        public override uint64_t DefaultNaNExtFloat80BitsLower => 0xC000000000000000;

        public override bool IsSigNaNExtFloat80Bits(uint_fast16_t bits64, uint_fast64_t bits0) =>
            (bits64 & 0x7FFF) == 0x7FFF && (bits0 & 0x4000000000000000) == 0 && (bits0 & 0x3FFFFFFFFFFFFFFF) != 0;

        public override void ExtFloat80BitsToCommonNaN(SoftFloatState state, uint_fast16_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN)
        {
            if (IsSigNaNExtFloat80Bits(bits64, bits0))
                state.RaiseFlags(ExceptionFlags.Invalid);

            commonNaN = new SoftFloatCommonNaN()
            {
                Sign = (bits64 >> 15) != 0,
                Value = new UInt128(upper: bits0 << 1, lower: 0)
            };
        }

        public override UInt128 CommonNaNToExtFloat80Bits(in SoftFloatCommonNaN commonNaN) => new(
            upper: (commonNaN.Sign ? (1UL << 15) : 0) | 0x7FFF,
            lower: 0xC000000000000000 | (uint64_t)(commonNaN.Value >> 65)
        );

        public override UInt128 PropagateNaNExtFloat80Bits(SoftFloatState state, uint_fast16_t bitsA64, uint_fast64_t bitsA0, uint_fast16_t bitsB64, uint_fast64_t bitsB0)
        {
            var isSigNaNA = IsSigNaNExtFloat80Bits(bitsA64, bitsA0);
            var isSigNaNB = IsSigNaNExtFloat80Bits(bitsB64, bitsB0);

            // Make NaNs non-signaling.
            var uiNonsigA0 = bitsA0 | 0xC000000000000000;
            var uiNonsigB0 = bitsB0 | 0xC000000000000000;

            if (isSigNaNA | isSigNaNB)
            {
                state.RaiseFlags(ExceptionFlags.Invalid);
                if (isSigNaNA)
                {
                    if (isSigNaNB)
                        goto returnLargerMag;

                    return IsNaNExtF80UI((int_fast16_t)bitsB64, bitsB0)
                        ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
                        : new UInt128(upper: bitsA64, lower: uiNonsigA0);
                }
                else
                {
                    return IsNaNExtF80UI((int_fast16_t)bitsA64, bitsA0)
                        ? new UInt128(upper: bitsA64, lower: uiNonsigA0)
                        : new UInt128(upper: bitsB64, lower: uiNonsigB0);
                }
            }

        returnLargerMag:
            var uiMagA64 = bitsA64 & 0x7FFF;
            var uiMagB64 = bitsB64 & 0x7FFF;

            int cmp = uiMagA64.CompareTo(uiMagB64);
            if (cmp == 0) cmp = bitsA0.CompareTo(bitsB0);
            if (cmp == 0) cmp = bitsB64.CompareTo(bitsA64);
            return cmp <= 0
                ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
                : new UInt128(upper: bitsA64, lower: uiNonsigA0);
        }

        #endregion

        #region Float128

        public override uint_fast64_t DefaultNaNFloat128BitsUpper => 0xFFFF800000000000;

        public override uint_fast64_t DefaultNaNFloat128BitsLower => 0x0000000000000000;

        public override bool IsSigNaNFloat128Bits(uint_fast64_t bits64, uint_fast64_t bits0) =>
            (bits64 & 0x7FFF800000000000) == 0x7FFF000000000000 && (bits0 != 0 || (bits64 & 0x00007FFFFFFFFFFF) != 0);

        public override void Float128BitsToCommonNaN(SoftFloatState state, uint_fast64_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN)
        {
            if (IsSigNaNFloat128Bits(bits64, bits0))
                state.RaiseFlags(ExceptionFlags.Invalid);

            var NaNSig = ShortShiftLeft128(bits64, bits0, 16);
            commonNaN = new SoftFloatCommonNaN()
            {
                Sign = (bits64 >> 63) != 0,
                Value = new UInt128(upper: NaNSig.V64, lower: NaNSig.V00)
            };
        }

        public override UInt128 CommonNaNToFloat128Bits(in SoftFloatCommonNaN commonNaN)
        {
            var uiZ = commonNaN.Value >> 16;
            uiZ |= new UInt128(upper: (commonNaN.Sign ? (1UL << 63) : 0) | 0x7FFF800000000000, lower: 0);
            return uiZ;
        }

        public override UInt128 PropagateNaNFloat128Bits(SoftFloatState state, uint_fast64_t bitsA64, uint_fast64_t bitsA0, uint_fast64_t bitsB64, uint_fast64_t bitsB0)
        {
            var isSigNaNA = IsSigNaNFloat128Bits(bitsA64, bitsA0);
            var isSigNaNB = IsSigNaNFloat128Bits(bitsB64, bitsB0);

            // Make NaNs non-signaling.
            var uiNonsigA0 = bitsA0 | 0x0000800000000000;
            var uiNonsigB0 = bitsB0 | 0x0000800000000000;

            if (isSigNaNA | isSigNaNB)
            {
                state.RaiseFlags(ExceptionFlags.Invalid);
                if (isSigNaNA)
                {
                    if (isSigNaNB)
                        goto returnLargerMag;

                    return IsNaNF128UI(bitsB64, bitsB0)
                        ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
                        : new UInt128(upper: bitsA64, lower: uiNonsigA0);
                }
                else
                {
                    return IsNaNF128UI(bitsA64, bitsA0)
                        ? new UInt128(upper: bitsA64, lower: uiNonsigA0)
                        : new UInt128(upper: bitsB64, lower: uiNonsigB0);
                }
            }

        returnLargerMag:
            var uiMagA64 = bitsA64 & 0x7FFFFFFFFFFFFFFF;
            var uiMagB64 = bitsB64 & 0x7FFFFFFFFFFFFFFF;

            int cmp = uiMagA64.CompareTo(uiMagB64);
            if (cmp == 0) cmp = bitsA0.CompareTo(bitsB0);
            if (cmp == 0) cmp = bitsB64.CompareTo(bitsA64);
            return cmp <= 0
                ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
                : new UInt128(upper: bitsA64, lower: uiNonsigA0);
        }

        #endregion
    }
}
