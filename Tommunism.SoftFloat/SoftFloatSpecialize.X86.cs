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

        internal override SpecializeNaNIntegerKind UInt32NaNKind => SpecializeNaNIntegerKind.NaNIsPosAndNegOverflow;

        internal override SpecializeNaNIntegerKind Int32NaNKind => SpecializeNaNIntegerKind.NaNIsPosAndNegOverflow;

        #endregion

        #region Float16

        public override uint16_t DefaultNaNFloat16Bits => 0xFE00;

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
                    if (!isSigNaNB)
                        return (uint16_t)(IsNaNF16UI(bitsB) ? uiNonsigB : uiNonsigA);
                }
                else
                {
                    return (uint16_t)(IsNaNF16UI(bitsA) ? uiNonsigA : uiNonsigB);
                }
            }

            var uiMagA = bitsA & 0x7FFF;
            var uiMagB = bitsB & 0x7FFF;
            if (uiMagA < uiMagB) return (uint16_t)uiNonsigB;
            if (uiMagB < uiMagA) return (uint16_t)uiNonsigA;
            return (uint16_t)((uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB);
        }

        #endregion

        #region Float32

        public override uint32_t DefaultNaNFloat32Bits => 0xFFC00000;

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
                    if (!isSigNaNB)
                        return IsNaNF32UI(bitsB) ? uiNonsigB : uiNonsigA;
                }
                else
                {
                    return IsNaNF32UI(bitsA) ? uiNonsigA : uiNonsigB;
                }
            }

            var uiMagA = bitsA & 0x7FFFFFFF;
            var uiMagB = bitsB & 0x7FFFFFFF;
            if (uiMagA < uiMagB) return uiNonsigB;
            if (uiMagB < uiMagA) return uiNonsigA;
            return (uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB;
        }

        #endregion

        #region Float64

        public override uint64_t DefaultNaNFloat64Bits => 0xFFF8000000000000;

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
                    if (!isSigNaNB)
                        return IsNaNF64UI(bitsB) ? uiNonsigB : uiNonsigA;
                }
                else
                {
                    return IsNaNF64UI(bitsA) ? uiNonsigA : uiNonsigB;
                }
            }

            var uiMagA = bitsA & 0x7FFFFFFFFFFFFFFF;
            var uiMagB = bitsB & 0x7FFFFFFFFFFFFFFF;
            if (uiMagA < uiMagB) return uiNonsigB;
            if (uiMagB < uiMagA) return uiNonsigA;
            return (uiNonsigA < uiNonsigB) ? uiNonsigA : uiNonsigB;
        }

        #endregion

        #region ExtFloat80

        public override UInt128 DefaultNaNExtFloat80Bits => new(upper: 0xFFFF, lower: 0xC000000000000000);

        #endregion

        #region Float128

        public override UInt128 DefaultNaNFloat128Bits => new(upper: 0xFFFF800000000000, lower: 0x0000000000000000);

        public override UInt128 PropagateNaNFloat128Bits(SoftFloatState state, uint_fast64_t bitsA64, uint_fast64_t bitsA0, uint_fast64_t bitsB64, uint_fast64_t bitsB0)
        {
            var isSigNaNA = IsSigNaNFloat128Bits(bitsA64, bitsA0);
            var isSigNaNB = IsSigNaNFloat128Bits(bitsB64, bitsB0);

            // Make NaNs non-signaling.
            var uiNonsigA64 = bitsA64 | 0x0000800000000000;
            var uiNonsigB64 = bitsB64 | 0x0000800000000000;

            if (isSigNaNA | isSigNaNB)
            {
                state.RaiseFlags(ExceptionFlags.Invalid);
                if (isSigNaNA)
                {
                    if (!isSigNaNB)
                    {
                        return IsNaNF128UI(bitsB64, bitsB0)
                            ? new UInt128(upper: uiNonsigB64, lower: bitsB0)
                            : new UInt128(upper: uiNonsigA64, lower: bitsA0);
                    }
                }
                else
                {
                    return IsNaNF128UI(bitsA64, bitsA0)
                        ? new UInt128(upper: bitsA64, lower: bitsB0)
                        : new UInt128(upper: uiNonsigA64, lower: bitsA0);
                }
            }

            var uiMagA64 = bitsA64 & 0x7FFFFFFFFFFFFFFF;
            var uiMagB64 = bitsB64 & 0x7FFFFFFFFFFFFFFF;

            int cmp = uiMagA64.CompareTo(uiMagB64);
            if (cmp == 0)
            {
                cmp = bitsA0.CompareTo(bitsB0);
                if (cmp == 0)
                    cmp = uiNonsigB64.CompareTo(uiNonsigA64);
            }

            return cmp <= 0
                ? new UInt128(upper: uiNonsigB64, lower: bitsB0)
                : new UInt128(upper: uiNonsigA64, lower: bitsA0);
        }

        #endregion
    }
}
