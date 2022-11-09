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
using System.Runtime.InteropServices;

namespace Tommunism.SoftFloat;

[Obsolete("Use new Float16(ushort) instead.")]
[StructLayout(LayoutKind.Explicit)]
internal struct UInt16_Float16
{
    [FieldOffset(0)]
    public UInt16 UInt16;

    [FieldOffset(0)]
    public Float16 Float16;
}

[Obsolete("Use new Float32(uint) instead.")]
[StructLayout(LayoutKind.Explicit)]
internal struct UInt32_Float32
{
    [FieldOffset(0)]
    public UInt32 UInt32;

    [FieldOffset(0)]
    public Float32 Float32;
}

[Obsolete("Use new Float64(ulong) instead.")]
[StructLayout(LayoutKind.Explicit)]
internal struct UInt64_Float64
{
    [FieldOffset(0)]
    public UInt64 UInt64;

    [FieldOffset(0)]
    public Float64 Float64;
}

[Obsolete("Use new Float128(ulong,ulong) instead.")]
[StructLayout(LayoutKind.Explicit)]
internal struct UInt128_Float128
{
    [FieldOffset(0)]
    public UInt128 UInt128;

    [FieldOffset(0)]
    public Float128 Float128;
}
