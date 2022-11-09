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

    #endregion
}
