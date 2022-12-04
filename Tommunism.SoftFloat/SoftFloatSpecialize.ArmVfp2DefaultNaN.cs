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
    public sealed class ArmVfp2DefaultNaN : SoftFloatSpecialize
    {
        #region Default Instance & Constructor

        /// <summary>
        /// Gets or sets the default instance to use for specialized implementation details.
        /// </summary>
        public static new ArmVfp2DefaultNaN Default { get; } = new();

        // This is a sealed class with constant default NaN bits, so it should be safe to cache them.
        public ArmVfp2DefaultNaN() => InitializeDefaultNaNs();

        #endregion

        public override Tininess InitialDetectTininess => Tininess.BeforeRounding;

        #region Integer Conversion Constants

        public override uint UInt32FromPositiveOverflow => 0xFFFFFFFF;

        public override uint UInt32FromNegativeOverflow => 0;

        public override uint UInt32FromNaN => 0;

        public override int Int32FromPositiveOverflow => 0x7FFFFFFF;

        public override int Int32FromNegativeOverflow => -0x7FFFFFFF - 1;

        public override int Int32FromNaN => 0;

        public override ulong UInt64FromPositiveOverflow => 0xFFFFFFFFFFFFFFFF;

        public override ulong UInt64FromNegativeOverflow => 0;

        public override ulong UInt64FromNaN => 0;

        public override long Int64FromPositiveOverflow => 0x7FFFFFFFFFFFFFFF;

        public override long Int64FromNegativeOverflow => -0x7FFFFFFFFFFFFFFF - 1;

        public override long Int64FromNaN => 0;

        internal override SpecializeNaNIntegerKind UInt32NaNKind => SpecializeNaNIntegerKind.NaNIsNegOverflow;

        internal override SpecializeNaNIntegerKind Int32NaNKind => SpecializeNaNIntegerKind.NaNIsUnique;

        #endregion

        #region Float16

        public override uint16_t DefaultNaNFloat16Bits => 0x7E00;

        public override void Float16BitsToCommonNaN(SoftFloatContext context, uint_fast16_t bits, out SoftFloatCommonNaN commonNaN)
        {
            commonNaN = default;
            if ((bits & 0x0200) == 0)
                context.RaiseFlags(ExceptionFlags.Invalid);
        }

        public override uint16_t CommonNaNToFloat16Bits(in SoftFloatCommonNaN commonNaN) => DefaultNaNFloat16Bits;

        public override uint16_t PropagateNaNFloat16Bits(SoftFloatContext context, uint_fast16_t bitsA, uint_fast16_t bitsB)
        {
            if (IsSignalingNaNFloat16Bits(bitsA) || IsSignalingNaNFloat16Bits(bitsB))
                context.RaiseFlags(ExceptionFlags.Invalid);

            return DefaultNaNFloat16Bits;
        }

        #endregion

        #region Float32

        public override uint32_t DefaultNaNFloat32Bits => 0x7FC00000;

        public override void Float32BitsToCommonNaN(SoftFloatContext context, uint_fast32_t bits, out SoftFloatCommonNaN commonNaN)
        {
            commonNaN = default;
            if ((bits & 0x00400000) == 0)
                context.RaiseFlags(ExceptionFlags.Invalid);
        }

        public override uint_fast32_t CommonNaNToFloat32Bits(in SoftFloatCommonNaN commonNaN) => DefaultNaNFloat32Bits;

        public override uint32_t PropagateNaNFloat32Bits(SoftFloatContext context, uint_fast32_t bitsA, uint_fast32_t bitsB)
        {
            if (IsSignalingNaNFloat32Bits(bitsA) || IsSignalingNaNFloat32Bits(bitsB))
                context.RaiseFlags(ExceptionFlags.Invalid);

            return DefaultNaNFloat32Bits;
        }

        #endregion

        #region Float64

        public override uint64_t DefaultNaNFloat64Bits => 0x7FF8000000000000;

        public override void Float64BitsToCommonNaN(SoftFloatContext context, uint_fast64_t bits, out SoftFloatCommonNaN commonNaN)
        {
            commonNaN = default;
            if ((bits & 0x0008000000000000) == 0)
                context.RaiseFlags(ExceptionFlags.Invalid);
        }

        public override uint64_t CommonNaNToFloat64Bits(in SoftFloatCommonNaN commonNaN) => DefaultNaNFloat64Bits;

        public override uint64_t PropagateNaNFloat64Bits(SoftFloatContext context, uint_fast64_t bitsA, uint_fast64_t bitsB)
        {
            if (IsSignalingNaNFloat64Bits(bitsA) || IsSignalingNaNFloat64Bits(bitsB))
                context.RaiseFlags(ExceptionFlags.Invalid);

            return DefaultNaNFloat64Bits;
        }

        #endregion

        #region ExtFloat80

        public override UInt128 DefaultNaNExtFloat80Bits => new(upper: 0x7FFF, lower: 0xC000000000000000);

        public override void ExtFloat80BitsToCommonNaN(SoftFloatContext context, uint_fast16_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN)
        {
            commonNaN = default;
            if ((bits0 & 0x4000000000000000) == 0)
                context.RaiseFlags(ExceptionFlags.Invalid);
        }

        public override UInt128 CommonNaNToExtFloat80Bits(in SoftFloatCommonNaN commonNaN) => DefaultNaNExtFloat80Bits;

        public override UInt128 PropagateNaNExtFloat80Bits(SoftFloatContext context, uint bitsA64, ulong bitsA0, uint bitsB64, ulong bitsB0)
        {
            if (IsSignalingNaNExtFloat80Bits(bitsA64, bitsA0) || IsSignalingNaNExtFloat80Bits(bitsB64, bitsB0))
                context.RaiseFlags(ExceptionFlags.Invalid);

            return DefaultNaNExtFloat80Bits;
        }

        #endregion

        #region Float128

        public override UInt128 DefaultNaNFloat128Bits => new(upper: 0x7FFF800000000000, lower: 0x0000000000000000);

        public override void Float128BitsToCommonNaN(SoftFloatContext context, uint_fast64_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN)
        {
            commonNaN = default;
            if ((bits64 & 0x0000800000000000) == 0)
                context.RaiseFlags(ExceptionFlags.Invalid);
        }

        public override UInt128 CommonNaNToFloat128Bits(in SoftFloatCommonNaN commonNaN) => DefaultNaNFloat128Bits;

        public override UInt128 PropagateNaNFloat128Bits(SoftFloatContext context, uint_fast64_t bitsA64, uint_fast64_t bitsA0, uint_fast64_t bitsB64, uint_fast64_t bitsB0)
        {
            if (IsSignalingNaNFloat128Bits(bitsA64, bitsA0) || IsSignalingNaNFloat128Bits(bitsB64, bitsB0))
                context.RaiseFlags(ExceptionFlags.Invalid);

            return DefaultNaNFloat128Bits;
        }

        #endregion
    }
}
