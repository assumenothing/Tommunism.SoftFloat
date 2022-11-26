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

using static Specialize;

// TODO: Add a static user-settable factory for creating custom instances when using the static properties?

// Improve Visual Studio's readability a little bit by "redefining" the standard integer types to C99 stdint types.

using int32_t = Int32;
using int64_t = Int64;

using uint32_t = UInt32;
using uint64_t = UInt64;

public class SoftFloatState
{
    #region Fields

    [ThreadStatic]
    private static SoftFloatState? _currentThreadState;

    private static readonly SoftFloatState _sharedState = new();

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
    public static SoftFloatState SharedState => _sharedState;

    // softfloat_detectTininess
    /// <summary>
    /// Gets or sets software floating-point underflow tininess-detection mode.
    /// </summary>
    public Tininess DetectTininess { get; set; } = InitialDetectTininess;

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
}
