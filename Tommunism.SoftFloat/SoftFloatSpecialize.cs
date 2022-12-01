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

public abstract partial class SoftFloatSpecialize
{
    #region Default Instance

    private static SoftFloatSpecialize _default = X86.Default;

    /// <summary>
    /// Gets or sets the default instance to use for specialized implementation details.
    /// </summary>
    /// <remarks>
    /// This value will be used on new instances of <see cref="SoftFloatState"/> (after this property has changed). The default value is
    /// <see cref="X86.Default"/>.
    /// </remarks>
    public static SoftFloatSpecialize Default
    {
        get => _default;
        set => _default = value;
    }

    #endregion

    #region Fields

    private Float16 _defaultNaNFloat16;
    private Float32 _defaultNaNFloat32;
    private Float64 _defaultNaNFloat64;
    private ExtFloat80 _defaultNaNExtFloat80;
    private Float128 _defaultNaNFloat128;
    private bool _defaultNaNsInitialized;

    #endregion

    #region Helpers

    // Inherited classes can call this if getting the default NaN bits does not cause side effects.
    protected void InitializeDefaultNaNs()
    {
        // Using lazy-initialization to avoid potential issues with calling virtual members inside constructors.
        // This is not thread-safe. But it shouldn't matter, as the default NaN bits should return the same values every time.
        if (!_defaultNaNsInitialized)
        {
            _defaultNaNFloat16 = Float16.FromBitsUI16(DefaultNaNFloat16Bits);
            _defaultNaNFloat32 = Float32.FromBitsUI32(DefaultNaNFloat32Bits);
            _defaultNaNFloat64 = Float64.FromBitsUI64(DefaultNaNFloat64Bits);
            _defaultNaNExtFloat80 = ExtFloat80.FromBitsUI80(DefaultNaNExtFloat80BitsUpper, DefaultNaNExtFloat80BitsLower);
            _defaultNaNFloat128 = Float128.FromBitsUI128(v64: DefaultNaNFloat128BitsUpper, v0: DefaultNaNFloat128BitsLower);
            _defaultNaNsInitialized = true;
        }
    }

    #endregion

    // init_detectTininess
    /// <summary>
    /// Default value for 'softfloat_detectTininess'.
    /// </summary>
    public abstract Tininess InitialDetectTininess { get; }

    #region Integer Conversion Constants

    // ui32_fromPosOverflow
    public abstract uint UInt32FromPosOverflow { get; }

    // ui32_fromNegOverflow
    public abstract uint UInt32FromNegOverflow { get; }

    // ui32_fromNaN
    public abstract uint UInt32FromNaN { get; }

    // i32_fromPosOverflow
    public abstract int Int32FromPosOverflow { get; }

    // i32_fromNegOverflow
    public abstract int Int32FromNegOverflow { get; }

    // i32_fromNaN
    public abstract int Int32FromNaN { get; }

    // ui64_fromPosOverflow
    public abstract ulong UInt64FromPosOverflow { get; }

    // ui64_fromNegOverflow
    public abstract ulong UInt64FromNegOverflow { get; }

    // ui64_fromNaN
    public abstract ulong UInt64FromNaN { get; }

    // i64_fromPosOverflow
    public abstract long Int64FromPosOverflow { get; }

    // i64_fromNegOverflow
    public abstract long Int64FromNegOverflow { get; }

    // i64_fromNaN
    public abstract long Int64FromNaN { get; }

    public uint UInt32FromOverflow(bool isNegative) => isNegative ? UInt32FromNegOverflow : UInt32FromPosOverflow;

    public int Int32FromOverflow(bool isNegative) => isNegative ? Int32FromNegOverflow : Int32FromPosOverflow;

    public ulong UInt64FromOverflow(bool isNegative) => isNegative ? UInt64FromNegOverflow : UInt64FromPosOverflow;

    public long Int64FromOverflow(bool isNegative) => isNegative ? Int64FromNegOverflow : Int64FromPosOverflow;

    // NOTE: These are virtual in the off-chance that better performance can be achieved by overridding them and avoiding comparisons with other virtual members.
    // (These should only be overridden and returning a fixed value if the derived class or all of its [U]Int*From* properties are sealed.)
    // Only implementing the ones which are commonly compared.

    internal virtual SpecializeNaNIntegerKind UInt32NaNKind
    {
        get
        {
            SpecializeNaNIntegerKind result = 0;
            var valueNaN = UInt32FromNaN;
            if (valueNaN == UInt32FromPosOverflow) result |= SpecializeNaNIntegerKind.NaNIsPosOverflow;
            if (valueNaN == UInt32FromNegOverflow) result |= SpecializeNaNIntegerKind.NaNIsNegOverflow;
            return result;
        }
    }

    internal virtual SpecializeNaNIntegerKind Int32NaNKind
    {
        get
        {
            SpecializeNaNIntegerKind result = 0;
            var valueNaN = Int32FromNaN;
            if (valueNaN == Int32FromPosOverflow) result |= SpecializeNaNIntegerKind.NaNIsPosOverflow;
            if (valueNaN == Int32FromNegOverflow) result |= SpecializeNaNIntegerKind.NaNIsNegOverflow;
            return result;
        }
    }

    #endregion

    #region Float16

    // defaultNaNF16UI
    /// <summary>
    /// The bit pattern for a default generated 16-bit floating-point NaN.
    /// </summary>
    public abstract uint16_t DefaultNaNFloat16Bits { get; }

    public Float16 DefaultNaNFloat16
    {
        get
        {
            InitializeDefaultNaNs();
            return _defaultNaNFloat16;
        }
    }

    // softfloat_isSigNaNF16UI
    /// <summary>
    /// Returns true when 16-bit unsigned integer <paramref name="bits"/> has the bit pattern of a 16-bit floating-point signaling NaN.
    /// </summary>
    public virtual bool IsSignalNaNFloat16Bits(uint_fast16_t bits) => (bits & 0x7E00) == 0x7C00 && (bits & 0x01FF) != 0;

    // softfloat_f16UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="bits"/> has the bit pattern of a 16-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the
    /// invalid exception is raised.
    /// </summary>
    public virtual void Float16BitsToCommonNaN(SoftFloatState state, uint_fast16_t bits, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSignalNaNFloat16Bits(bits))
            state.RaiseFlags(ExceptionFlags.Invalid);

        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits >> 15) != 0,
            Value = new UInt128(upper: (ulong)bits << 54, lower: 0)
        };
    }

    // softfloat_commonNaNToF16UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="commonNaN"/> into a 16-bit floating-point NaN, and returns the bit pattern of
    /// this value as an unsigned integer.
    /// </summary>
    public virtual uint16_t CommonNaNToFloat16Bits(in SoftFloatCommonNaN commonNaN) =>
            (uint16_t)((commonNaN.Sign ? (1U << 15) : 0) | 0x7E00 | (uint_fast16_t)(commonNaN.Value >> 118));

    // softfloat_propagateNaNF16UI
    /// <summary>
    /// Interpreting <paramref name="bitsA"/> and <paramref name="bitsB"/> as the bit patterns of two 16-bit floating-point values, at
    /// least one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="bitsA"/> or
    /// <paramref name="bitsB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public abstract uint16_t PropagateNaNFloat16Bits(SoftFloatState state, uint_fast16_t bitsA, uint_fast16_t bitsB);

    #endregion

    #region Float32

    // defaultNaNF32UI
    /// <summary>
    /// The bit pattern for a default generated 32-bit floating-point NaN.
    /// </summary>
    public abstract uint32_t DefaultNaNFloat32Bits { get; }

    public Float32 DefaultNaNFloat32
    {
        get
        {
            InitializeDefaultNaNs();
            return _defaultNaNFloat32;
        }
    }

    // softfloat_isSigNaNF32UI
    /// <summary>
    /// Returns true when 32-bit unsigned integer <paramref name="bits"/> has the bit pattern of a 32-bit floating-point signaling NaN.
    /// </summary>
    public virtual bool IsSigNaNFloat32Bits(uint_fast32_t bits) => (bits & 0x7FC00000) == 0x7FC00000 && (bits & 0x003FFFFF) != 0;

    // softfloat_f32UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="bits"/> has the bit pattern of a 32-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the
    /// invalid exception is raised.
    /// </summary>
    public virtual void Float32BitsToCommonNaN(SoftFloatState state, uint_fast32_t bits, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSigNaNFloat32Bits(bits))
            state.RaiseFlags(ExceptionFlags.Invalid);

        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits >> 31) != 0,
            Value = new UInt128(upper: (ulong)bits << 41, lower: 0)
        };
    }

    // softfloat_commonNaNToF32UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="commonNaN"/> into a 32-bit floating-point NaN, and returns the bit pattern of
    /// this value as an unsigned integer.
    /// </summary>
    public virtual uint32_t CommonNaNToFloat32Bits(in SoftFloatCommonNaN commonNaN) =>
        (commonNaN.Sign ? (1U << 31) : 0U) | 0x7FC00000 | (uint_fast32_t)(commonNaN.Value >> 105);

    // softfloat_propagateNaNF32UI
    /// <summary>
    /// Interpreting <paramref name="bitsA"/> and <paramref name="bitsB"/> as the bit patterns of two 32-bit floating-point values, at
    /// least one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="bitsA"/> or
    /// <paramref name="bitsB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public abstract uint32_t PropagateNaNFloat32Bits(SoftFloatState state, uint_fast32_t bitsA, uint_fast32_t bitsB);

    #endregion

    #region Float64

    // defaultNaNF64UI
    /// <summary>
    /// The bit pattern for a default generated 64-bit floating-point NaN.
    /// </summary>
    public abstract uint64_t DefaultNaNFloat64Bits { get; }

    public Float64 DefaultNaNFloat64
    {
        get
        {
            InitializeDefaultNaNs();
            return _defaultNaNFloat64;
        }
    }

    // softfloat_isSigNaNF64UI
    /// <summary>
    /// Returns true when 64-bit unsigned integer <paramref name="bits"/> has the bit pattern of a 64-bit floating-point signaling NaN.
    /// </summary>
    public virtual bool IsSigNaNFloat64Bits(uint_fast64_t bits) =>
        (bits & 0x7FF8000000000000) == 0x7FF8000000000000 && (bits & 0x0007FFFFFFFFFFFF) != 0;

    // softfloat_f64UIToCommonNaN
    /// <summary>
    /// Assuming <paramref name="bits"/> has the bit pattern of a 64-bit floating-point NaN, converts this NaN to the common NaN form, and
    /// stores the resulting common NaN at the location pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the
    /// invalid exception is raised.
    /// </summary>
    public virtual void Float64BitsToCommonNaN(SoftFloatState state, uint_fast64_t bits, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSigNaNFloat64Bits(bits))
            state.RaiseFlags(ExceptionFlags.Invalid);

        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits >> 63) != 0,
            Value = new UInt128(upper: bits << 12, lower: 0)
        };
    }

    // softfloat_commonNaNToF64UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="commonNaN"/> into a 64-bit floating-point NaN, and returns the bit pattern of
    /// this value as an unsigned integer.
    /// </summary>
    public virtual uint64_t CommonNaNToFloat64Bits(in SoftFloatCommonNaN commonNaN) =>
        (commonNaN.Sign ? (1UL << 63) : 0) | 0x7FF8000000000000 | (uint_fast64_t)(commonNaN.Value >> 76);

    // softfloat_propagateNaNF64UI
    /// <summary>
    /// Interpreting <paramref name="bitsA"/> and <paramref name="bitsB"/> as the bit patterns of two 64-bit floating-point values, at
    /// least one of which is a NaN, returns the bit pattern of the combined NaN result. If either <paramref name="bitsA"/> or
    /// <paramref name="bitsB"/> has the pattern of a signaling NaN, the invalid exception is raised.
    /// </summary>
    public abstract uint64_t PropagateNaNFloat64Bits(SoftFloatState state, uint_fast64_t bitsA, uint_fast64_t bitsB);

    #endregion

    #region ExtFloat80

    // defaultNaNExtF80UI64
    /// <summary>
    /// The bit pattern for the upper 16 bits of a default generated 80-bit extended floating-point NaN.
    /// </summary>
    public virtual uint16_t DefaultNaNExtFloat80BitsUpper => 0xFFFF;

    // defaultNaNExtF80UI0
    /// <summary>
    /// The bit pattern for the lower 64 bits of a default generated 80-bit extended floating-point NaN.
    /// </summary>
    public virtual uint64_t DefaultNaNExtFloat80BitsLower => 0xC000000000000000;

    public ExtFloat80 DefaultNaNExtFloat80
    {
        get
        {
            InitializeDefaultNaNs();
            return _defaultNaNExtFloat80;
        }
    }

    // softfloat_isSigNaNExtF80UI
    /// <summary>
    /// Returns true when the 80-bit unsigned integer formed from concatenating 16-bit <paramref name="bits64"/> and 64-bit
    /// <paramref name="bits0"/> has the bit pattern of an 80-bit extended floating-point signaling NaN.
    /// </summary>
    public virtual bool IsSigNaNExtFloat80Bits(uint_fast16_t bits64, uint_fast64_t bits0) =>
        (bits64 & 0x7FFF) == 0x7FFF && (bits0 & 0x4000000000000000) == 0 && (bits0 & 0x3FFFFFFFFFFFFFFF) != 0;

    // softfloat_extF80UIToCommonNaN
    /// <summary>
    /// Assuming the unsigned integer formed from concatenating <paramref name="bits64"/> and <paramref name="bits0"/> has the bit pattern
    /// of an 80-bit extended floating-point NaN, converts this NaN to the common NaN form, and stores the resulting common NaN at the
    /// location pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the invalid exception is raised.
    /// </summary>
    public virtual void ExtFloat80BitsToCommonNaN(SoftFloatState state, uint_fast16_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN)
    {
        if (IsSigNaNExtFloat80Bits(bits64, bits0))
            state.RaiseFlags(ExceptionFlags.Invalid);

        commonNaN = new SoftFloatCommonNaN()
        {
            Sign = (bits64 >> 15) != 0,
            Value = new UInt128(upper: bits0 << 1, lower: 0)
        };
    }

    // softfloat_commonNaNToExtF80UI
    /// <summary>
    /// Converts the common NaN pointed to by <paramref name="commonNaN"/> into an 80-bit extended floating-point NaN, and returns the bit
    /// pattern of this value as an unsigned integer.
    /// </summary>
    public virtual UInt128 CommonNaNToExtFloat80Bits(in SoftFloatCommonNaN commonNaN) => new(
        upper: (commonNaN.Sign ? (1UL << 15) : 0) | 0x7FFF,
        lower: 0xC000000000000000 | (uint64_t)(commonNaN.Value >> 65)
    );

    // softfloat_propagateNaNExtF80UI
    /// <summary>
    /// Interpreting the unsigned integer formed from concatenating <paramref name="bitsA64"/> and <paramref name="bitsA0"/> as an 80-bit
    /// extended floating-point value, and likewise interpreting the unsigned integer formed from concatenating <paramref name="bitsB64"/>
    /// and <paramref name="bitsB0"/> as another 80-bit extended floating-point value, and assuming at least on of these floating-point
    /// values is a NaN, returns the bit pattern of the combined NaN result. If either original floating-point value is a signaling NaN,
    /// the invalid exception is raised.
    /// </summary>
    public virtual UInt128 PropagateNaNExtFloat80Bits(SoftFloatState state, uint_fast16_t bitsA64, uint_fast64_t bitsA0, uint_fast16_t bitsB64, uint_fast64_t bitsB0)
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
                if (!isSigNaNB)
                {
                    return IsNaNExtF80UI((int_fast16_t)bitsB64, bitsB0)
                        ? new UInt128(upper: bitsB64, lower: uiNonsigB0)
                        : new UInt128(upper: bitsA64, lower: uiNonsigA0);
                }
            }
            else
            {
                return IsNaNExtF80UI((int_fast16_t)bitsA64, bitsA0)
                    ? new UInt128(upper: bitsA64, lower: uiNonsigA0)
                    : new UInt128(upper: bitsB64, lower: uiNonsigB0);
            }
        }

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

    // defaultNaNF128UI64
    /// <summary>
    /// The bit pattern for the upper 64 bits of a default generated 128-bit floating-point NaN.
    /// </summary>
    public abstract uint_fast64_t DefaultNaNFloat128BitsUpper { get; }

    // defaultNaNF128UI0
    /// <summary>
    /// The bit pattern for the lowper 64 bits of a default generated 128-bit floating-point NaN.
    /// </summary>
    public abstract uint_fast64_t DefaultNaNFloat128BitsLower { get; }

    public Float128 DefaultNaNFloat128
    {
        get
        {
            InitializeDefaultNaNs();
            return _defaultNaNFloat128;
        }
    }

    // softfloat_isSigNaNF128UI
    /// <summary>
    /// Returns true when the 128-bit unsigned integer formed from concatenating 64-bit <paramref name="bits64"/> and 64-bit
    /// <paramref name="bits0"/> has the bit pattern of a 128-bit floating-point signaling NaN.
    /// </summary>
    public virtual bool IsSigNaNFloat128Bits(uint_fast64_t bits64, uint_fast64_t bits0) =>
        (bits64 & 0x7FFF800000000000) == 0x7FFF000000000000 && (bits0 != 0 || (bits64 & 0x00007FFFFFFFFFFF) != 0);

    // softfloat_f128UIToCommonNaN
    /// <summary>
    /// Assuming the unsigned integer formed from concatenating <paramref name="bits64"/> and <paramref name="bits0"/> has the bit pattern
    /// of an 128-bit floating-point NaN, converts this NaN to the common NaN form, and stores the resulting common NaN at the location
    /// pointed to by <paramref name="commonNaN"/>. If the NaN is a signaling NaN, the invalid exception is raised.
    /// </summary>
    public virtual void Float128BitsToCommonNaN(SoftFloatState state, uint_fast64_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN)
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

    // softfloat_commonNaNToF128UI
    /// <summary>
    /// Converts the common NaN pointed to by 'aPtr' into a 128-bit floating-point NaN, and returns the bit pattern of this value as an
    /// unsigned integer.
    /// </summary>
    public virtual UInt128 CommonNaNToFloat128Bits(in SoftFloatCommonNaN commonNaN)
    {
        var uiZ = commonNaN.Value >> 16;
        uiZ |= new UInt128(upper: (commonNaN.Sign ? (1UL << 63) : 0) | 0x7FFF800000000000, lower: 0);
        return uiZ;
    }

    // softfloat_propagateNaNF128UI
    /// <summary>
    /// Interpreting the unsigned integer formed from concatenating <paramref name="bitsA64"/> and <paramref name="bitsA0"/> as a 128-bit
    /// floating-point value, and likewise interpreting the unsigned integer formed from concatenating <paramref name="bitsB64"/> and
    /// <paramref name="bitsB0"/> as another 128-bit floating-point value, and assuming at least on of these floating-point values is a NaN,
    /// returns the bit pattern of the combined NaN result. If either original floating-point value is a signaling NaN, the invalid
    /// exception is raised.
    /// </summary>
    public abstract UInt128 PropagateNaNFloat128Bits(SoftFloatState state, uint_fast64_t bitsA64, uint_fast64_t bitsA0, uint_fast64_t bitsB64, uint_fast64_t bitsB0);

    #endregion
}

[Flags]
internal enum SpecializeNaNIntegerKind : uint
{
    NaNIsUnique = 0,
    NaNIsPosOverflow = 1 << 0,
    NaNIsNegOverflow = 1 << 1,
    NaNIsPosAndNegOverflow = NaNIsPosOverflow | NaNIsNegOverflow
}
