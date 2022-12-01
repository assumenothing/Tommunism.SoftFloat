﻿#region Copyright
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

// TODO: Add a static user-settable factory for creating custom instances when using the static properties?

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

public class SoftFloatState
{
    #region Fields

    [ThreadStatic]
    private static SoftFloatState? _currentThreadState;

    private static SoftFloatState? _sharedState;

    #endregion

    #region Constructors

    public SoftFloatState() : this(SoftFloatSpecialize.Default)
    {
    }

    public SoftFloatState(SoftFloatSpecialize specialize)
    {
        Specialize = specialize;
        DetectTininess = specialize.InitialDetectTininess;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="RaiseFlags(ExceptionFlags)"/> should throw a <see cref="SoftFloatException"/>
    /// exception when called.
    /// </summary>
    /// <remarks>
    /// This is not thread safe! Alternatively this class can be derived to provide more consistency. Ideally this should be set once at
    /// the start of the program.
    /// </remarks>
    public static bool ThrowOnRaiseFlags { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating which state should be returned by <see cref="Default"/>.
    /// </summary>
    /// <remarks>
    /// This is not thread safe! It should not be changed inside or between software floating-point operations. Ideally this should be set
    /// once at the start of the program.
    /// </remarks>
    public static bool UseThreadStaticStateByDefault { get; set; } = true;

    /// <summary>
    /// Gets the default software floating-point state.
    /// </summary>
    /// <remarks>
    /// The returned value is determined by the <see cref="UseThreadStaticStateByDefault"/> property. If
    /// <see cref="UseThreadStaticStateByDefault"/> is true, then <see cref="CurrentThreadState"/> is returned; otherwise,
    /// <see cref="SharedState"/> is returned.
    /// </remarks>
    public static SoftFloatState Default => UseThreadStaticStateByDefault ? CurrentThreadState : SharedState;

    /// <summary>
    /// Gets a software floating-point state that is unique to each thread.
    /// </summary>
    public static SoftFloatState CurrentThreadState
    {
        get
        {
            // Get or create state for current thread.
            var state = _currentThreadState;
            if (state == null)
            {
                state = new SoftFloatState();
                _currentThreadState = state;
            }

            return state;
        }
    }

    /// <summary>
    /// Gets a software floating-point state that is shared with all threads. This is not thread safe!
    /// </summary>
    public static SoftFloatState SharedState
    {
        get
        {
            // Get or create shared state.
            var state = _sharedState;
            if (state == null)
            {
                state = new SoftFloatState();
                _sharedState = state;
            }

            return state;
        }
    }

    /// <summary>
    /// Gets or sets the specialization details to use when performing floating-point operations.
    /// </summary>
    public SoftFloatSpecialize Specialize { get; set; }

    // softfloat_detectTininess
    /// <summary>
    /// Gets or sets software floating-point underflow tininess-detection mode.
    /// </summary>
    public Tininess DetectTininess { get; set; }

    // softfloat_roundingMode
    /// <summary>
    /// Gets or sets software floating-point rounding mode.
    /// </summary>
    public RoundingMode RoundingMode { get; set; } = RoundingMode.NearEven;

    // softfloat_exceptionFlags
    /// <summary>
    /// Gets or sets software floating-point exception flags.
    /// </summary>
    /// <remarks>
    /// This can be overridden to implement custom actions to perform when floating point exceptions flags change (such as logging).
    /// 
    /// The default implementation of <see cref="RaiseFlags(ExceptionFlags)"/> gets and sets this property before optionally throwing an exception.
    /// </remarks>
    public virtual ExceptionFlags ExceptionFlags { get; set; } = ExceptionFlags.None;

    /// <summary>
    /// Gets or sets the rounding precision for 80-bit extended double-precision floating-point.
    /// </summary>
    public ExtFloat80RoundingPrecision ExtFloat80RoundingPrecision { get; set; } = ExtFloat80RoundingPrecision._80;

    #endregion

    #region Methods

    /// <summary>
    /// Routine to raise any or all of the software floating-point exception flags.
    /// </summary>
    /// <remarks>
    /// This can be overridden to implement custom actions to perform when floating point exceptions occur (such as logging).
    /// 
    /// By default, this implementation simply adds the set <paramref name="exceptionFlags"/> to the <see cref="ExceptionFlags"/> property
    /// and optionally throws a <see cref="SoftFloatException"/> exception if <see cref="ThrowOnRaiseFlags"/> is true.
    /// 
    /// Some of the internal code will set the <see cref="ExceptionFlags"/> property directly and bypass this call. This method is
    /// generally called when setting error flags.
    /// </remarks>
    public virtual void RaiseFlags(ExceptionFlags exceptionFlags)
    {
        ExceptionFlags |= exceptionFlags;

        // Should an exception be thrown.
        if (ThrowOnRaiseFlags)
        {
            // Only throw the flags passed in, ignore any other flags that may have already been set.
            throw new SoftFloatException(exceptionFlags);
        }
    }

    #region Float16 Shortcut Methods

    public Float16 ToFloat16(uint32_t value) => Float16.FromUInt32(value, this);
    public Float16 ToFloat16(uint64_t value) => Float16.FromUInt64(value, this);
    public Float16 ToFloat16(int32_t value) => Float16.FromInt32(value, this);
    public Float16 ToFloat16(int64_t value) => Float16.FromInt64(value, this);

    public uint32_t ToUInt32(Float16 value, bool exact) => value.ToUInt32(RoundingMode, exact, this);
    public uint32_t ToUInt32(Float16 value, RoundingMode roundingMode, bool exact) => value.ToUInt32(roundingMode, exact, this);
    public uint32_t ToUInt32RoundMinMag(Float16 value, bool exact) => value.ToUInt32RoundMinMag(exact, this);

    public uint64_t ToUInt64(Float16 value, bool exact) => value.ToUInt64(RoundingMode, exact, this);
    public uint64_t ToUInt64(Float16 value, RoundingMode roundingMode, bool exact) => value.ToUInt64(roundingMode, exact, this);
    public uint64_t ToUInt64RoundMinMag(Float16 value, bool exact) => value.ToUInt64RoundMinMag(exact, this);

    public int32_t ToInt32(Float16 value, bool exact) => value.ToInt32(RoundingMode, exact, this);
    public int32_t ToInt32(Float16 value, RoundingMode roundingMode, bool exact) => value.ToInt32(roundingMode, exact, this);
    public int32_t ToInt32RoundMinMag(Float16 value, bool exact) => value.ToInt32RoundMinMag(exact, this);

    public int64_t ToInt64(Float16 value, bool exact) => value.ToInt64(RoundingMode, exact, this);
    public int64_t ToInt64(Float16 value, RoundingMode roundingMode, bool exact) => value.ToInt64(roundingMode, exact, this);
    public int64_t ToInt64RoundMinMag(Float16 value, bool exact) => value.ToInt64RoundMinMag(exact, this);

    public Float32 ToFloat32(Float16 value) => value.ToFloat32(this);
    public Float64 ToFloat64(Float16 value) => value.ToFloat64(this);
    public ExtFloat80 ToExtFloat80(Float16 value) => value.ToExtFloat80(this);
    public Float128 ToFloat128(Float16 value) => value.ToFloat128(this);

    public Float16 RoundToInt(Float16 value, bool exact) => value.RoundToInt(RoundingMode, exact, this);
    public Float16 RoundToInt(Float16 value, RoundingMode roundingMode, bool exact) => value.RoundToInt(roundingMode, exact, this);

    public Float16 Add(Float16 a, Float16 b) => Float16.Add(a, b, this);
    public Float16 Subtract(Float16 a, Float16 b) => Float16.Subtract(a, b, this);
    public Float16 Multiply(Float16 a, Float16 b) => Float16.Multiply(a, b, this);
    public Float16 MultiplyAndAdd(Float16 a, Float16 b, Float16 c) => Float16.MultiplyAndAdd(a, b, c, this);
    public Float16 Divide(Float16 a, Float16 b) => Float16.Divide(a, b, this);
    public Float16 Modulus(Float16 a, Float16 b) => Float16.Modulus(a, b, this);
    public Float16 SquareRoot(Float16 value) => value.SquareRoot(this);

    public bool CompareEqual(Float16 a, Float16 b, bool quiet) => Float16.CompareEqual(a, b, quiet, this);
    public bool CompareLessThan(Float16 a, Float16 b, bool quiet) => Float16.CompareLessThan(a, b, quiet, this);
    public bool CompareLessThanOrEqual(Float16 a, Float16 b, bool quiet) => Float16.CompareLessThanOrEqual(a, b, quiet, this);

    #endregion

    #region Float32 Shortcut Methods

    public Float32 ToFloat32(uint32_t value) => Float32.FromUInt32(value, this);
    public Float32 ToFloat32(uint64_t value) => Float32.FromUInt64(value, this);
    public Float32 ToFloat32(int32_t value) => Float32.FromInt32(value, this);
    public Float32 ToFloat32(int64_t value) => Float32.FromInt64(value, this);

    public uint32_t ToUInt32(Float32 value, bool exact) => value.ToUInt32(RoundingMode, exact, this);
    public uint32_t ToUInt32(Float32 value, RoundingMode roundingMode, bool exact) => value.ToUInt32(roundingMode, exact, this);
    public uint32_t ToUInt32RoundMinMag(Float32 value, bool exact) => value.ToUInt32RoundMinMag(exact, this);

    public uint64_t ToUInt64(Float32 value, bool exact) => value.ToUInt64(RoundingMode, exact, this);
    public uint64_t ToUInt64(Float32 value, RoundingMode roundingMode, bool exact) => value.ToUInt64(roundingMode, exact, this);
    public uint64_t ToUInt64RoundMinMag(Float32 value, bool exact) => value.ToUInt64RoundMinMag(exact, this);

    public int32_t ToInt32(Float32 value, bool exact) => value.ToInt32(RoundingMode, exact, this);
    public int32_t ToInt32(Float32 value, RoundingMode roundingMode, bool exact) => value.ToInt32(roundingMode, exact, this);
    public int32_t ToInt32RoundMinMag(Float32 value, bool exact) => value.ToInt32RoundMinMag(exact, this);

    public int64_t ToInt64(Float32 value, bool exact) => value.ToInt64(RoundingMode, exact, this);
    public int64_t ToInt64(Float32 value, RoundingMode roundingMode, bool exact) => value.ToInt64(roundingMode, exact, this);
    public int64_t ToInt64RoundMinMag(Float32 value, bool exact) => value.ToInt64RoundMinMag(exact, this);

    public Float16 ToFloat16(Float32 value) => value.ToFloat16(this);
    public Float64 ToFloat64(Float32 value) => value.ToFloat64(this);
    public ExtFloat80 ToExtFloat80(Float32 value) => value.ToExtFloat80(this);
    public Float128 ToFloat128(Float32 value) => value.ToFloat128(this);

    public Float32 RoundToInt(Float32 value, bool exact) => value.RoundToInt(RoundingMode, exact, this);
    public Float32 RoundToInt(Float32 value, RoundingMode roundingMode, bool exact) => value.RoundToInt(roundingMode, exact, this);

    public Float32 Add(Float32 a, Float32 b) => Float32.Add(a, b, this);
    public Float32 Subtract(Float32 a, Float32 b) => Float32.Subtract(a, b, this);
    public Float32 Multiply(Float32 a, Float32 b) => Float32.Multiply(a, b, this);
    public Float32 MultiplyAndAdd(Float32 a, Float32 b, Float32 c) => Float32.MultiplyAndAdd(a, b, c, this);
    public Float32 Divide(Float32 a, Float32 b) => Float32.Divide(a, b, this);
    public Float32 Modulus(Float32 a, Float32 b) => Float32.Modulus(a, b, this);
    public Float32 SquareRoot(Float32 value) => value.SquareRoot(this);

    public bool CompareEqual(Float32 a, Float32 b, bool quiet) => Float32.CompareEqual(a, b, quiet, this);
    public bool CompareLessThan(Float32 a, Float32 b, bool quiet) => Float32.CompareLessThan(a, b, quiet, this);
    public bool CompareLessThanOrEqual(Float32 a, Float32 b, bool quiet) => Float32.CompareLessThanOrEqual(a, b, quiet, this);

    #endregion

    #region Float64 Shortcut Methods

    public Float64 ToFloat64(uint32_t value) => Float64.FromUInt32(value, this);
    public Float64 ToFloat64(uint64_t value) => Float64.FromUInt64(value, this);
    public Float64 ToFloat64(int32_t value) => Float64.FromInt32(value, this);
    public Float64 ToFloat64(int64_t value) => Float64.FromInt64(value, this);

    public uint32_t ToUInt32(Float64 value, bool exact) => value.ToUInt32(RoundingMode, exact, this);
    public uint32_t ToUInt32(Float64 value, RoundingMode roundingMode, bool exact) => value.ToUInt32(roundingMode, exact, this);
    public uint32_t ToUInt32RoundMinMag(Float64 value, bool exact) => value.ToUInt32RoundMinMag(exact, this);

    public uint64_t ToUInt64(Float64 value, bool exact) => value.ToUInt64(RoundingMode, exact, this);
    public uint64_t ToUInt64(Float64 value, RoundingMode roundingMode, bool exact) => value.ToUInt64(roundingMode, exact, this);
    public uint64_t ToUInt64RoundMinMag(Float64 value, bool exact) => value.ToUInt64RoundMinMag(exact, this);

    public int32_t ToInt32(Float64 value, bool exact) => value.ToInt32(RoundingMode, exact, this);
    public int32_t ToInt32(Float64 value, RoundingMode roundingMode, bool exact) => value.ToInt32(roundingMode, exact, this);
    public int32_t ToInt32RoundMinMag(Float64 value, bool exact) => value.ToInt32RoundMinMag(exact, this);

    public int64_t ToInt64(Float64 value, bool exact) => value.ToInt64(RoundingMode, exact, this);
    public int64_t ToInt64(Float64 value, RoundingMode roundingMode, bool exact) => value.ToInt64(roundingMode, exact, this);
    public int64_t ToInt64RoundMinMag(Float64 value, bool exact) => value.ToInt64RoundMinMag(exact, this);

    public Float16 ToFloat16(Float64 value) => value.ToFloat16(this);
    public Float32 ToFloat32(Float64 value) => value.ToFloat32(this);
    public ExtFloat80 ToExtFloat80(Float64 value) => value.ToExtFloat80(this);
    public Float128 ToFloat128(Float64 value) => value.ToFloat128(this);

    public Float64 RoundToInt(Float64 value, bool exact) => value.RoundToInt(RoundingMode, exact, this);
    public Float64 RoundToInt(Float64 value, RoundingMode roundingMode, bool exact) => value.RoundToInt(roundingMode, exact, this);

    public Float64 Add(Float64 a, Float64 b) => Float64.Add(a, b, this);
    public Float64 Subtract(Float64 a, Float64 b) => Float64.Subtract(a, b, this);
    public Float64 Multiply(Float64 a, Float64 b) => Float64.Multiply(a, b, this);
    public Float64 MultiplyAndAdd(Float64 a, Float64 b, Float64 c) => Float64.MultiplyAndAdd(a, b, c, this);
    public Float64 Divide(Float64 a, Float64 b) => Float64.Divide(a, b, this);
    public Float64 Modulus(Float64 a, Float64 b) => Float64.Modulus(a, b, this);
    public Float64 SquareRoot(Float64 value) => value.SquareRoot(this);

    public bool CompareEqual(Float64 a, Float64 b, bool quiet) => Float64.CompareEqual(a, b, quiet, this);
    public bool CompareLessThan(Float64 a, Float64 b, bool quiet) => Float64.CompareLessThan(a, b, quiet, this);
    public bool CompareLessThanOrEqual(Float64 a, Float64 b, bool quiet) => Float64.CompareLessThanOrEqual(a, b, quiet, this);

    #endregion

    #region ExtFloat80 Shortcut Methods

    public ExtFloat80 ToExtFloat80(uint32_t value) => ExtFloat80.FromUInt32(value, this);
    public ExtFloat80 ToExtFloat80(uint64_t value) => ExtFloat80.FromUInt64(value, this);
    public ExtFloat80 ToExtFloat80(int32_t value) => ExtFloat80.FromInt32(value, this);
    public ExtFloat80 ToExtFloat80(int64_t value) => ExtFloat80.FromInt64(value, this);

    public uint32_t ToUInt32(ExtFloat80 value, bool exact) => value.ToUInt32(RoundingMode, exact, this);
    public uint32_t ToUInt32(ExtFloat80 value, RoundingMode roundingMode, bool exact) => value.ToUInt32(roundingMode, exact, this);
    public uint32_t ToUInt32RoundMinMag(ExtFloat80 value, bool exact) => value.ToUInt32RoundMinMag(exact, this);

    public uint64_t ToUInt64(ExtFloat80 value, bool exact) => value.ToUInt64(RoundingMode, exact, this);
    public uint64_t ToUInt64(ExtFloat80 value, RoundingMode roundingMode, bool exact) => value.ToUInt64(roundingMode, exact, this);
    public uint64_t ToUInt64RoundMinMag(ExtFloat80 value, bool exact) => value.ToUInt64RoundMinMag(exact, this);

    public int32_t ToInt32(ExtFloat80 value, bool exact) => value.ToInt32(RoundingMode, exact, this);
    public int32_t ToInt32(ExtFloat80 value, RoundingMode roundingMode, bool exact) => value.ToInt32(roundingMode, exact, this);
    public int32_t ToInt32RoundMinMag(ExtFloat80 value, bool exact) => value.ToInt32RoundMinMag(exact, this);

    public int64_t ToInt64(ExtFloat80 value, bool exact) => value.ToInt64(RoundingMode, exact, this);
    public int64_t ToInt64(ExtFloat80 value, RoundingMode roundingMode, bool exact) => value.ToInt64(roundingMode, exact, this);
    public int64_t ToInt64RoundMinMag(ExtFloat80 value, bool exact) => value.ToInt64RoundMinMag(exact, this);

    public Float16 ToFloat16(ExtFloat80 value) => value.ToFloat16(this);
    public Float32 ToFloat32(ExtFloat80 value) => value.ToFloat32(this);
    public Float64 ToFloat64(ExtFloat80 value) => value.ToFloat64(this);
    public Float128 ToFloat128(ExtFloat80 value) => value.ToFloat128(this);

    public ExtFloat80 RoundToInt(ExtFloat80 value, bool exact) => value.RoundToInt(RoundingMode, exact, this);
    public ExtFloat80 RoundToInt(ExtFloat80 value, RoundingMode roundingMode, bool exact) => value.RoundToInt(roundingMode, exact, this);

    public ExtFloat80 Add(ExtFloat80 a, ExtFloat80 b) => ExtFloat80.Add(a, b, this);
    public ExtFloat80 Subtract(ExtFloat80 a, ExtFloat80 b) => ExtFloat80.Subtract(a, b, this);
    public ExtFloat80 Multiply(ExtFloat80 a, ExtFloat80 b) => ExtFloat80.Multiply(a, b, this);
    public ExtFloat80 Divide(ExtFloat80 a, ExtFloat80 b) => ExtFloat80.Divide(a, b, this);
    public ExtFloat80 Modulus(ExtFloat80 a, ExtFloat80 b) => ExtFloat80.Modulus(a, b, this);
    public ExtFloat80 SquareRoot(ExtFloat80 value) => value.SquareRoot(this);

    public bool CompareEqual(ExtFloat80 a, ExtFloat80 b, bool quiet) => ExtFloat80.CompareEqual(a, b, quiet, this);
    public bool CompareLessThan(ExtFloat80 a, ExtFloat80 b, bool quiet) => ExtFloat80.CompareLessThan(a, b, quiet, this);
    public bool CompareLessThanOrEqual(ExtFloat80 a, ExtFloat80 b, bool quiet) => ExtFloat80.CompareLessThanOrEqual(a, b, quiet, this);

    #endregion

    #region Float128 Shortcut Methods

    public Float128 ToFloat128(uint32_t value) => Float128.FromUInt32(value, this);
    public Float128 ToFloat128(uint64_t value) => Float128.FromUInt64(value, this);
    public Float128 ToFloat128(int32_t value) => Float128.FromInt32(value, this);
    public Float128 ToFloat128(int64_t value) => Float128.FromInt64(value, this);

    public uint32_t ToUInt32(Float128 value, bool exact) => value.ToUInt32(RoundingMode, exact, this);
    public uint32_t ToUInt32(Float128 value, RoundingMode roundingMode, bool exact) => value.ToUInt32(roundingMode, exact, this);
    public uint32_t ToUInt32RoundMinMag(Float128 value, bool exact) => value.ToUInt32RoundMinMag(exact, this);

    public uint64_t ToUInt64(Float128 value, bool exact) => value.ToUInt64(RoundingMode, exact, this);
    public uint64_t ToUInt64(Float128 value, RoundingMode roundingMode, bool exact) => value.ToUInt64(roundingMode, exact, this);
    public uint64_t ToUInt64RoundMinMag(Float128 value, bool exact) => value.ToUInt64RoundMinMag(exact, this);

    public int32_t ToInt32(Float128 value, bool exact) => value.ToInt32(RoundingMode, exact, this);
    public int32_t ToInt32(Float128 value, RoundingMode roundingMode, bool exact) => value.ToInt32(roundingMode, exact, this);
    public int32_t ToInt32RoundMinMag(Float128 value, bool exact) => value.ToInt32RoundMinMag(exact, this);

    public int64_t ToInt64(Float128 value, bool exact) => value.ToInt64(RoundingMode, exact, this);
    public int64_t ToInt64(Float128 value, RoundingMode roundingMode, bool exact) => value.ToInt64(roundingMode, exact, this);
    public int64_t ToInt64RoundMinMag(Float128 value, bool exact) => value.ToInt64RoundMinMag(exact, this);

    public Float16 ToFloat16(Float128 value) => value.ToFloat16(this);
    public Float32 ToFloat32(Float128 value) => value.ToFloat32(this);
    public Float64 ToFloat64(Float128 value) => value.ToFloat64(this);
    public ExtFloat80 ToExtFloat80(Float128 value) => value.ToExtFloat80(this);

    public Float128 RoundToInt(Float128 value, bool exact) => value.RoundToInt(RoundingMode, exact, this);
    public Float128 RoundToInt(Float128 value, RoundingMode roundingMode, bool exact) => value.RoundToInt(roundingMode, exact, this);

    public Float128 Add(Float128 a, Float128 b) => Float128.Add(a, b, this);
    public Float128 Subtract(Float128 a, Float128 b) => Float128.Subtract(a, b, this);
    public Float128 Multiply(Float128 a, Float128 b) => Float128.Multiply(a, b, this);
    public Float128 MultiplyAndAdd(Float128 a, Float128 b, Float128 c) => Float128.MultiplyAndAdd(a, b, c, this);
    public Float128 Divide(Float128 a, Float128 b) => Float128.Divide(a, b, this);
    public Float128 Modulus(Float128 a, Float128 b) => Float128.Modulus(a, b, this);
    public Float128 SquareRoot(Float128 value) => value.SquareRoot(this);

    public bool CompareEqual(Float128 a, Float128 b, bool quiet) => Float128.CompareEqual(a, b, quiet, this);
    public bool CompareLessThan(Float128 a, Float128 b, bool quiet) => Float128.CompareLessThan(a, b, quiet, this);
    public bool CompareLessThanOrEqual(Float128 a, Float128 b, bool quiet) => Float128.CompareLessThanOrEqual(a, b, quiet, this);

    #endregion

    #endregion

    #region Specialize Helpers

    #region Integer Conversion Constants

    public uint UInt32FromPosOverflow => Specialize.UInt32FromPosOverflow;
    public uint UInt32FromNegOverflow => Specialize.UInt32FromNegOverflow;
    public uint UInt32FromNaN => Specialize.UInt32FromNaN;

    public int Int32FromPosOverflow => Specialize.Int32FromPosOverflow;
    public int Int32FromNegOverflow => Specialize.Int32FromNegOverflow;
    public int Int32FromNaN => Specialize.Int32FromNaN;

    public ulong UInt64FromPosOverflow => Specialize.UInt64FromPosOverflow;
    public ulong UInt64FromNegOverflow => Specialize.UInt64FromNegOverflow;
    public ulong UInt64FromNaN => Specialize.UInt64FromNaN;

    public long Int64FromPosOverflow => Specialize.Int64FromPosOverflow;
    public long Int64FromNegOverflow => Specialize.Int64FromNegOverflow;
    public long Int64FromNaN => Specialize.Int64FromNaN;

    public uint UInt32FromOverflow(bool isNegative) => Specialize.UInt32FromOverflow(isNegative);
    public int Int32FromOverflow(bool isNegative) => Specialize.Int32FromOverflow(isNegative);
    public ulong UInt64FromOverflow(bool isNegative) => Specialize.UInt64FromOverflow(isNegative);
    public long Int64FromOverflow(bool isNegative) => Specialize.Int64FromOverflow(isNegative);

    #endregion

    #region Float16

    public uint16_t DefaultNaNFloat16Bits => Specialize.DefaultNaNFloat16Bits;
    public Float16 DefaultNaNFloat16 => Specialize.DefaultNaNFloat16;

    public bool IsSignalNaNFloat16Bits(uint_fast16_t bits) => Specialize.IsSignalNaNFloat16Bits(bits);
    public void Float16BitsToCommonNaN(uint_fast16_t bits, out SoftFloatCommonNaN commonNaN) => Specialize.Float16BitsToCommonNaN(this, bits, out commonNaN);
    public Float16 CommonNaNToFloat16(in SoftFloatCommonNaN commonNaN) => Float16.FromBitsUI16(Specialize.CommonNaNToFloat16Bits(commonNaN));
    public Float16 PropagateNaNFloat16(uint_fast16_t bitsA, uint_fast16_t bitsB) => Float16.FromBitsUI16(Specialize.PropagateNaNFloat16Bits(this, bitsA, bitsB));

    public Float16 PropagateNaNFloat16(uint_fast16_t bitsA, uint_fast16_t bitsB, uint_fast16_t bitsC)
    {
        uint16_t result;
        result = Specialize.PropagateNaNFloat16Bits(this, bitsA, bitsB);
        result = Specialize.PropagateNaNFloat16Bits(this, result, bitsC);
        return Float16.FromBitsUI16(result);
    }

    #endregion

    #region Float32

    public uint32_t DefaultNaNFloat32Bits => Specialize.DefaultNaNFloat32Bits;
    public Float32 DefaultNaNFloat32 => Specialize.DefaultNaNFloat32;

    public bool IsSigNaNFloat32Bits(uint_fast32_t bits) => Specialize.IsSigNaNFloat32Bits(bits);
    public void Float32BitsToCommonNaN(uint_fast32_t bits, out SoftFloatCommonNaN commonNaN) => Specialize.Float32BitsToCommonNaN(this, bits, out commonNaN);
    public Float32 CommonNaNToFloat32(in SoftFloatCommonNaN commonNaN) => Float32.FromBitsUI32(Specialize.CommonNaNToFloat32Bits(in commonNaN));
    public Float32 PropagateNaNFloat32Bits(uint_fast32_t bitsA, uint_fast32_t bitsB) => Float32.FromBitsUI32(Specialize.PropagateNaNFloat32Bits(this, bitsA, bitsB));

    public Float32 PropagateNaNFloat32Bits(uint_fast32_t bitsA, uint_fast32_t bitsB, uint_fast32_t bitsC)
    {
        uint32_t result;
        result = Specialize.PropagateNaNFloat32Bits(this, bitsA, bitsB);
        result = Specialize.PropagateNaNFloat32Bits(this, result, bitsC);
        return Float32.FromBitsUI32(result);
    }

    #endregion

    #region Float64

    public uint64_t DefaultNaNFloat64Bits => Specialize.DefaultNaNFloat64Bits;
    public Float64 DefaultNaNFloat64 => Specialize.DefaultNaNFloat64;

    public bool IsSigNaNFloat64Bits(uint_fast64_t bits) => Specialize.IsSigNaNFloat64Bits(bits);
    public void Float64BitsToCommonNaN(uint_fast64_t bits, out SoftFloatCommonNaN commonNaN) => Specialize.Float64BitsToCommonNaN(this, bits, out commonNaN);
    public Float64 CommonNaNToFloat64(in SoftFloatCommonNaN commonNaN) => Float64.FromBitsUI64(Specialize.CommonNaNToFloat64Bits(in commonNaN));
    public Float64 PropagateNaNFloat64Bits(uint_fast64_t bitsA, uint_fast64_t bitsB) => Float64.FromBitsUI64(Specialize.PropagateNaNFloat64Bits(this, bitsA, bitsB));

    public Float64 PropagateNaNFloat64Bits(uint_fast64_t bitsA, uint_fast64_t bitsB, uint_fast64_t bitsC)
    {
        uint64_t result;
        result = Specialize.PropagateNaNFloat64Bits(this, bitsA, bitsB);
        result = Specialize.PropagateNaNFloat64Bits(this, result, bitsC);
        return Float64.FromBitsUI64(result);
    }

    #endregion

    #region ExtFloat80

    public UInt128 DefaultNaNExtFloat80Bits => Specialize.DefaultNaNExtFloat80Bits;
    public ExtFloat80 DefaultNaNExtFloat80 => Specialize.DefaultNaNExtFloat80;

    public bool IsSigNaNExtFloat80Bits(uint_fast16_t bits64, uint_fast64_t bits0) => Specialize.IsSigNaNExtFloat80Bits(bits64, bits0);
    public void ExtFloat80BitsToCommonNaN(uint_fast16_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN) => Specialize.ExtFloat80BitsToCommonNaN(this, bits64, bits0, out commonNaN);
    public ExtFloat80 CommonNaNToExtFloat80(in SoftFloatCommonNaN commonNaN) => ExtFloat80.FromBitsUI128(Specialize.CommonNaNToExtFloat80Bits(in commonNaN));
    public ExtFloat80 PropagateNaNExtFloat80Bits(uint_fast16_t bitsA64, uint_fast64_t bitsA0, uint_fast16_t bitsB64, uint_fast64_t bitsB0) => ExtFloat80.FromBitsUI128(Specialize.PropagateNaNExtFloat80Bits(this, bitsA64, bitsA0, bitsB64, bitsB0));

    public ExtFloat80 PropagateNaNExtFloat80Bits(uint_fast16_t bitsA64, uint_fast64_t bitsA0, uint_fast16_t bitsB64, uint_fast64_t bitsB0, uint_fast16_t bitsC64, uint_fast64_t bitsC0)
    {
        UInt128 result;
        result = Specialize.PropagateNaNExtFloat80Bits(this, bitsA64, bitsA0, bitsB64, bitsB0);
        result = Specialize.PropagateNaNExtFloat80Bits(this, (uint_fast16_t)result.GetUpperUI64(), result.GetLowerUI64(), bitsC64, bitsC0);
        return ExtFloat80.FromBitsUI128(result);
    }

    #endregion

    #region Float128

    public UInt128 DefaultNaNFloat128Bits => Specialize.DefaultNaNFloat128Bits;
    public Float128 DefaultNaNFloat128 => Specialize.DefaultNaNFloat128;

    public bool IsSigNaNFloat128Bits(uint_fast64_t bits64, uint_fast64_t bits0) => Specialize.IsSigNaNFloat128Bits(bits64, bits0);
    public void Float128BitsToCommonNaN(uint_fast64_t bits64, uint_fast64_t bits0, out SoftFloatCommonNaN commonNaN) => Specialize.Float128BitsToCommonNaN(this, bits64, bits0, out commonNaN);
    public Float128 CommonNaNToFloat128(in SoftFloatCommonNaN commonNaN) => Float128.FromBitsUI128(Specialize.CommonNaNToFloat128Bits(in commonNaN));
    public Float128 PropagateNaNFloat128Bits(uint_fast64_t bitsA64, uint_fast64_t bitsA0, uint_fast64_t bitsB64, uint_fast64_t bitsB0) => Float128.FromBitsUI128(Specialize.PropagateNaNFloat128Bits(this, bitsA64, bitsA0, bitsB64, bitsB0));

    public Float128 PropagateNaNFloat128Bits(uint_fast64_t bitsA64, uint_fast64_t bitsA0, uint_fast64_t bitsB64, uint_fast64_t bitsB0, uint_fast64_t bitsC64, uint_fast64_t bitsC0)
    {
        UInt128 result;
        result = Specialize.PropagateNaNFloat128Bits(this, bitsA64, bitsA0, bitsB64, bitsB0);
        result = Specialize.PropagateNaNFloat128Bits(this, result.GetUpperUI64(), result.GetLowerUI64(), bitsC64, bitsC0);
        return Float128.FromBitsUI128(result);
    }

    #endregion

    #endregion
}
