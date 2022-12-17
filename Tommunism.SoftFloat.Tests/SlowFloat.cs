// NOTE: This was ported from the TestFloat software. And has been "enhanced" with basic support for signaling NaN values (not perfect, but
// passes all of the tests that have been thrown at it).

namespace Tommunism.SoftFloat.Tests;

// TODO: Add a way to quickly and easily save and restore these properties? Should be able to pack these properties into a 64-bit integer.
public sealed class SlowFloatContext
{
    // slowfloat_detectTininess
    public TininessMode DetectTininess { get; set; } = TininessMode.BeforeRounding;

    // slowfloat_roundingMode
    public RoundingMode RoundingMode { get; set; } = RoundingMode.NearEven;

    // slowfloat_exceptionFlags
    public ExceptionFlags ExceptionFlags { get; set; } = ExceptionFlags.None;

    // slow_extF80_roundingPrecision
    public ExtFloat80RoundingPrecision RoundingPrecisionExtFloat80 { get; set; } = ExtFloat80RoundingPrecision._80;
}

// floatX
public partial struct SlowFloat : IEquatable<SlowFloat>
{
    #region Fields

    // TODO: Are there any other obvious ones to create?

    // floatXNaN
    public static readonly SlowFloat NaN = new(
        isNaN: true,
        isInfinity: false,
        isZero: false,
        sign: false,
        exponent: 0,
        significand: UInt128.Zero,
        isSignaling: false
    );

    // floatXPositiveZero
    public static readonly SlowFloat PositiveZero = new(
        isNaN: false,
        isInfinity: false,
        isZero: true,
        sign: false,
        exponent: 0,
        significand: UInt128.Zero,
        isSignaling: false
    );

    // floatXNegativeZero
    public static readonly SlowFloat NegativeZero = new(
        isNaN: false,
        isInfinity: false,
        isZero: true,
        sign: true,
        exponent: 0,
        significand: UInt128.Zero,
        isSignaling: false
    );

    // Various 128-bit constants used throughout the code. Hopefully 128-bit literals are added to C# at some point in the future.
    private static readonly UInt128 _x0080000000000000_0000000000000000 = new(upper: 0x0080000000000000, lower: 0x0000000000000000);
    private static readonly UInt128 _x0000FFFFFFFFFFFF_FFFFFFFFFFFFFFFF = new(upper: 0x0000FFFFFFFFFFFF, lower: 0xFFFFFFFFFFFFFFFF);
    private static readonly UInt128 _x8000000000000000_0000000000000000 = new(upper: 0x8000000000000000, lower: 0x0000000000000000);

    internal UInt128 _sig;
    internal int _exp;
    internal bool _sign;
    internal bool _isZero;
    internal bool _isInf;
    internal bool _isNaN;
    internal bool _isSignaling;

    #endregion

    #region Constructors

    internal SlowFloat(UInt128 significand, int exponent, bool sign, bool isZero, bool isInfinity, bool isNaN, bool isSignaling)
    {
        _sig = significand;
        _exp = exponent;
        _sign = sign;
        _isZero = isZero;
        _isInf = isInfinity;
        _isNaN = isNaN;
        _isSignaling = isSignaling;
    }

    // ui32ToFloatX
    public SlowFloat(uint value)
    {
        ulong sig64;
        int exp;

        _isNaN = false;
        _isInf = false;
        _sign = false;
        sig64 = value;
        if (value != 0)
        {
            _isZero = false;
            exp = 31;
            sig64 <<= 24;
            while (sig64 < 0x0080000000000000)
            {
                --exp;
                sig64 <<= 1;
            }

            _exp = exp;
        }
        else
        {
            _isZero = true;
        }

        _sig = new(upper: sig64, lower: 0);
    }

    // ui64ToFloatX
    public SlowFloat(ulong value)
    {
        UInt128 sig;
        int exp;

        _isNaN = false;
        _isInf = false;
        _sign = false;
        sig = (UInt128)value;
        if (value != 0)
        {
            _isZero = false;
            exp = 63;
            sig <<= 56;
            while (V64(sig) < 0x0080000000000000)
            {
                --exp;
                sig <<= 1;
            }

            _exp = exp;
        }
        else
        {
            _isZero = true;
        }

        _sig = sig;
    }

    // i32ToFloatX
    public SlowFloat(int value)
    {
        bool sign;
        ulong sig64;
        int exp;

        _isNaN = false;
        _isInf = false;
        sign = value < 0;
        _sign = sign;
        sig64 = sign ? (ulong)(-(long)value) : (ulong)value;
        if (value != 0)
        {
            _isZero = false;
            exp = 31;
            sig64 <<= 24;
            while (sig64 < 0x0080000000000000)
            {
                --exp;
                sig64 <<= 1;
            }

            _exp = exp;
        }
        else
        {
            _isZero = true;
        }

        _sig = new(upper: sig64, lower: 0);
    }

    // i64ToFloatX
    public SlowFloat(long value)
    {
        bool sign;
        UInt128 sig;
        int exp;

        _isNaN = false;
        _isInf = false;
        sign = value < 0;
        _sign = sign;
        sig = sign ? (ulong)(-value) : (ulong)value;
        if (value != 0)
        {
            _isZero = false;
            exp = 63;
            sig <<= 56;
            while (V64(sig) < 0x0080000000000000)
            {
                --exp;
                sig <<= 1;
            }

            _exp = exp;
        }
        else
        {
            _isZero = true;
        }

        _sig = sig;
    }

    // f16ToFloatX
    public SlowFloat(Float16 value)
    {
        uint uiA;
        int exp;
        ulong sig64;

        uiA = value.ToUInt16Bits();
        _isNaN = false;
        _isInf = false;
        _isZero = false;
        _sign = (uiA & 0x8000) != 0;
        exp = (int)((uiA >> 10) & 0x1F);
        sig64 = uiA & 0x03FF;
        sig64 <<= 45;
        if (exp == 0x1F)
        {
            if (sig64 != 0)
            {
                _isNaN = true;

                // HACK: This is technically "specialized", but every version defined in SoftFloat 3e uses the same bit pattern.
                if ((uiA & 0x7E00) == 0x7C00)
                    _isSignaling = true;
            }
            else
            {
                _isInf = true;
            }
        }
        else if (exp == 0)
        {
            if (sig64 == 0)
            {
                _isZero = true;
            }
            else
            {
                exp = 1 - 0xF;
                do
                {
                    --exp;
                    sig64 <<= 1;
                } while (sig64 < 0x0080000000000000);
                _exp = exp;
            }
        }
        else
        {
            _exp = exp - 0xF;
            sig64 |= 0x0080000000000000;
        }

        _sig = new(upper: sig64, lower: 0);
    }

    public SlowFloat(Half value)
        : this(new Float16(value))
    {
    }

    // f32ToFloatX
    public SlowFloat(Float32 value)
    {
        uint uiA;
        int exp;
        ulong sig64;

        uiA = value.ToUInt32Bits();
        _isNaN = false;
        _isInf = false;
        _isZero = false;
        _sign = (uiA & 0x80000000) != 0;
        exp = (int)((uiA >> 23) & 0xFF);
        sig64 = uiA & 0x007FFFFF;
        sig64 <<= 32;
        if (exp == 0xFF)
        {
            if (sig64 != 0)
            {
                _isNaN = true;

                // HACK: This is technically "specialized", but every version defined in SoftFloat 3e uses the same bit pattern.
                if ((uiA & 0x7FC00000) == 0x7F800000)
                    _isSignaling = true;
            }
            else
            {
                _isInf = true;
            }
        }
        else if (exp == 0)
        {
            if (sig64 == 0)
            {
                _isZero = true;
            }
            else
            {
                exp = 1 - 0x7F;
                do
                {
                    --exp;
                    sig64 <<= 1;
                } while (sig64 < 0x0080000000000000);
                _exp = exp;
            }
        }
        else
        {
            _exp = exp - 0x7F;
            sig64 |= 0x0080000000000000;
        }

        _sig = new(upper: sig64, lower: 0);
    }

    public SlowFloat(float value)
        : this(new Float32(value))
    {
    }

    // f64ToFloatX
    public SlowFloat(Float64 value)
    {
        ulong uiA;
        int exp;
        ulong sig64;

        uiA = value.ToUInt64Bits();
        _isNaN = false;
        _isInf = false;
        _isZero = false;
        _sign = (uiA & 0x8000000000000000) != 0;
        exp = (int)((uiA >> 52) & 0x7FF);
        sig64 = uiA & 0x000FFFFFFFFFFFFF;
        if (exp == 0x7FF)
        {
            if (sig64 != 0)
            {
                _isNaN = true;

                // HACK: This is technically "specialized", but every version defined in SoftFloat 3e uses the same bit pattern.
                if ((uiA & 0x7FF8000000000000) == 0x7FF0000000000000)
                    _isSignaling = true;
            }
            else
            {
                _isInf = true;
            }
        }
        else if (exp == 0)
        {
            if (sig64 == 0)
            {
                _isZero = true;
            }
            else
            {
                exp = 1 - 0x3FF;
                do
                {
                    --exp;
                    sig64 <<= 1;
                } while (sig64 < 0x0010000000000000);
                _exp = exp;
            }
        }
        else
        {
            _exp = exp - 0x3FF;
            sig64 |= 0x0010000000000000;
        }

        _sig = new(upper: sig64 << 3, lower: 0);
    }

    public SlowFloat(double value)
        : this(new Float64(value))
    {
    }

    // extF80MToFloatX
    public SlowFloat(ExtFloat80 value)
    {
        ulong uiA64;
        int exp;
        UInt128 sig;

        _isNaN = false;
        _isInf = false;
        _isZero = false;
        uiA64 = value.SignAndExponent;
        _sign = (uiA64 & 0x8000) != 0;
        exp = (int)(uiA64 & 0x7FFF);
        sig = value.Significand;
        if (exp == 0x7FFF)
        {
            if (((ulong)sig & 0x7FFFFFFFFFFFFFFF) != 0)
            {
                _isNaN = true;

                // HACK: This is technically "specialized", but every version defined in SoftFloat 3e uses the same bit pattern.
                if (((ulong)sig & 0x4000000000000000) == 0 && ((ulong)sig & 0x3FFFFFFFFFFFFFFF) != 0)
                    _isSignaling = true;
            }
            else
            {
                _isInf = true;
            }
        }
        else
        {
            if (exp == 0) ++exp;
            exp -= 0x3FFF;
            if (((ulong)sig & 0x8000000000000000) == 0)
            {
                if ((ulong)sig == 0)
                {
                    _isZero = true;
                }
                else
                {
                    do
                    {
                        --exp;
                        sig <<= 1;
                    } while ((ulong)sig < 0x8000000000000000);
                }
            }

            _exp = exp;
        }

        _sig = sig << 56;
    }

    // f128MToFloatX
    public SlowFloat(Float128 value)
    {
        ulong uiA64;
        int exp;
        UInt128 sig;

        var uiA = value.ToUInt128Bits();
        _isNaN = false;
        _isInf = false;
        _isZero = false;
        uiA64 = V64(uiA);
        _sign = (uiA64 & 0x8000000000000000) != 0;
        exp = (int)((uiA64 >> 48) & 0x7FFF);
        sig = new(upper: uiA64 & 0x0000FFFFFFFFFFFF, lower: (ulong)uiA);
        if (exp == 0x7FFF)
        {
            if (sig != UInt128.Zero)
            {
                _isNaN = true;

                // HACK: This is technically "specialized", but every version defined in SoftFloat 3e uses the same bit pattern.
                if ((uiA64 & 0x7FFF800000000000) == 0x7FFF000000000000 && ((ulong)sig != 0 || (uiA64 & 0x00007FFFFFFFFFFF) != 0))
                    _isSignaling = true;
            }
            else
            {
                _isInf = true;
            }
        }
        else if (exp == 0)
        {
            if (sig == UInt128.Zero)
            {
                _isZero = true;
            }
            else
            {
                exp = 1 - 0x3FFF;
                do
                {
                    --exp;
                    sig <<= 1;
                } while (V64(sig) < 0x0001000000000000);
                _exp = exp;
            }
        }
        else
        {
            _exp = exp - 0x3FFF;
            sig |= new UInt128(0x0001000000000000, 0x0000000000000000);
        }

        _sig = sig << 7;
    }

    #endregion

    #region Properties

    public UInt128 Significand => _sig;

    public int Exponent => _exp;

    public bool Sign => _sign;

    public bool IsZero => _isZero;

    public bool IsInfinity => _isInf;

    public bool IsNaN => _isNaN;

    public bool IsSignaling => _isSignaling;

    #endregion

    #region Methods

    #region UInt128 Helpers

    // Too bad the upper & lower fields on UInt128 are private... makes sense though.

    internal static ulong V64(UInt128 value) => (ulong)(value >> 64);
    internal static ulong V0(UInt128 value) => (ulong)value;

    internal static UInt128 ShortShiftRightJam128(UInt128 value, int count) => (value >> count)
        | (((ulong)value << (-count)) != 0 ? UInt128.One : UInt128.Zero);

    internal static UInt128 Neg128(UInt128 value)
    {
        // TODO: Is this identical to the unary negation operator in UInt128? Nothing changed, so it is likely the same...
        if (V0(value) != 0)
        {
            return new UInt128(
                ~V64(value),
                (ulong)(-(long)V0(value))
            );
        }
        else
        {
            return new UInt128(
                (ulong)(-(long)V64(value)),
                V0(value)
            );
        }
    }

    // NOTE: "add128" from TestFloat appears to have code that is identical to the binary addition operator in UInt128.

    #endregion

    #region RoundToX

    // roundFloatXTo11
    public void RoundTo11(SlowFloatContext context, bool isTiny, RoundingMode roundingMode, bool exact)
    {
        ulong roundBits, sigX64;

        sigX64 = V64(_sig);
        roundBits = (sigX64 & 0x1FFFFFFFFFFF) | (V0(_sig) != 0 ? 1U : 0);
        if (roundBits != 0)
        {
            sigX64 &= 0xFFFFE00000000000;
            if (exact) context.ExceptionFlags |= ExceptionFlags.Inexact;
            if (isTiny) context.ExceptionFlags |= ExceptionFlags.Underflow;

            switch (roundingMode)
            {
                case RoundingMode.NearEven:
                {
                    if (roundBits < 0x100000000000 || (roundBits == 0x100000000000 && (sigX64 & 0x200000000000) == 0))
                        goto noIncrement;

                    break;
                }
                case RoundingMode.MinMag:
                {
                    goto noIncrement;
                }
                case RoundingMode.Min:
                {
                    if (!_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Max:
                {
                    if (_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.NearMaxMag:
                {
                    if (roundBits < 0x100000000000)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Odd:
                {
                    sigX64 |= 0x200000000000;
                    goto noIncrement;
                }
            }

            sigX64 += 0x200000000000;
            if (sigX64 == 0x0100000000000000)
            {
                ++_exp;
                sigX64 = 0x0080000000000000;
            }

        noIncrement:
            _sig = new(upper: sigX64, lower: 0);
        }
    }

    // roundFloatXTo24
    public void RoundTo24(SlowFloatContext context, bool isTiny, RoundingMode roundingMode, bool exact)
    {
        ulong sigX64;
        uint roundBits;

        sigX64 = V64(_sig);
        roundBits = (uint)sigX64 | (V0(_sig) != 0 ? 1U : 0);
        if (roundBits != 0)
        {
            sigX64 &= 0xFFFFFFFF00000000;
            if (exact) context.ExceptionFlags |= ExceptionFlags.Inexact;
            if (isTiny) context.ExceptionFlags |= ExceptionFlags.Underflow;

            switch (roundingMode)
            {
                case RoundingMode.NearEven:
                {
                    if (roundBits < 0x80000000 || (roundBits == 0x80000000 && (sigX64 & 0x100000000) == 0))
                        goto noIncrement;

                    break;
                }
                case RoundingMode.MinMag:
                {
                    goto noIncrement;
                }
                case RoundingMode.Min:
                {
                    if (!_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Max:
                {
                    if (_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.NearMaxMag:
                {
                    if (roundBits < 0x80000000)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Odd:
                {
                    sigX64 |= 0x100000000;
                    goto noIncrement;
                }
            }

            sigX64 += 0x100000000;
            if (sigX64 == 0x0100000000000000)
            {
                ++_exp;
                sigX64 = 0x0080000000000000;
            }

        noIncrement:
            _sig = new(upper: sigX64, lower: 0);
        }
    }

    // roundFloatXTo53
    public void RoundTo53(SlowFloatContext context, bool isTiny, RoundingMode roundingMode, bool exact)
    {
        ulong sigX64;
        uint roundBits;

        sigX64 = V64(_sig);
        roundBits = (uint)(sigX64 & 7) | (V0(_sig) != 0 ? 1U : 0);
        if (roundBits != 0)
        {
            sigX64 &= 0xFFFFFFFFFFFFFFF8;
            if (exact) context.ExceptionFlags |= ExceptionFlags.Inexact;
            if (isTiny) context.ExceptionFlags |= ExceptionFlags.Underflow;

            switch (roundingMode)
            {
                case RoundingMode.NearEven:
                {
                    if (roundBits < 4 || (roundBits == 4 && (sigX64 & 8) == 0))
                        goto noIncrement;

                    break;
                }
                case RoundingMode.MinMag:
                {
                    goto noIncrement;
                }
                case RoundingMode.Min:
                {
                    if (!_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Max:
                {
                    if (_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.NearMaxMag:
                {
                    if (roundBits < 4)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Odd:
                {
                    sigX64 |= 8;
                    goto noIncrement;
                }
            }

            sigX64 += 8;
            if (sigX64 == 0x0100000000000000)
            {
                ++_exp;
                sigX64 = 0x0080000000000000;
            }

        noIncrement:
            _sig = new(upper: sigX64, lower: 0);
        }
    }

    // roundFloatXTo64
    public void RoundTo64(SlowFloatContext context, bool isTiny, RoundingMode roundingMode, bool exact)
    {
        ulong sigX0, roundBits, sigX64;

        sigX0 = V0(_sig);
        roundBits = sigX0 & 0x00FFFFFFFFFFFFFF;
        if (roundBits != 0)
        {
            sigX0 &= 0xFF00000000000000;
            if (exact) context.ExceptionFlags |= ExceptionFlags.Inexact;
            if (isTiny) context.ExceptionFlags |= ExceptionFlags.Underflow;

            switch (roundingMode)
            {
                case RoundingMode.NearEven:
                {
                    if (roundBits < 0x0080000000000000 || (roundBits == 0x0080000000000000 && (sigX0 & 0x0100000000000000) == 0))
                        goto noIncrement;

                    break;
                }
                case RoundingMode.MinMag:
                {
                    goto noIncrement;
                }
                case RoundingMode.Min:
                {
                    if (!_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Max:
                {
                    if (_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.NearMaxMag:
                {
                    if (roundBits < 0x0080000000000000)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Odd:
                {
                    sigX0 |= 0x100000000000000;
                    goto noIncrement;
                }
            }

            sigX0 += 0x100000000000000;
            sigX64 = V64(_sig) + (sigX0 == 0 ? 1U : 0);
            if (sigX64 == 0x0100000000000000)
            {
                ++_exp;
                sigX64 = 0x0080000000000000;
            }

            _sig = new(upper: sigX64, lower: sigX0);
            return;

        noIncrement:
            _sig = new(upper: V64(_sig), lower: sigX0);
        }
    }

    // roundFloatXTo113
    public void RoundTo113(SlowFloatContext context, bool isTiny, RoundingMode roundingMode, bool exact)
    {
        ulong sigX0, sigX64;
        uint roundBits;

        sigX0 = V0(_sig);
        roundBits = (uint)(sigX0 & 0x7F);
        if (roundBits != 0)
        {
            sigX0 &= 0xFFFFFFFFFFFFFF80;
            if (exact) context.ExceptionFlags |= ExceptionFlags.Inexact;
            if (isTiny) context.ExceptionFlags |= ExceptionFlags.Underflow;

            switch (roundingMode)
            {
                case RoundingMode.NearEven:
                {
                    if (roundBits < 0x40 || (roundBits == 0x40 && (sigX0 & 0x80) == 0))
                        goto noIncrement;

                    break;
                }
                case RoundingMode.MinMag:
                {
                    goto noIncrement;
                }
                case RoundingMode.Min:
                {
                    if (!_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Max:
                {
                    if (_sign)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.NearMaxMag:
                {
                    if (roundBits < 0x40)
                        goto noIncrement;

                    break;
                }
                case RoundingMode.Odd:
                {
                    sigX0 |= 0x80;
                    goto noIncrement;
                }
            }

            sigX0 += 0x80;
            sigX64 = V64(_sig) + (sigX0 == 0 ? 1U : 0);
            if (sigX64 == 0x0100000000000000)
            {
                ++_exp;
                sigX64 = 0x0080000000000000;
            }

            _sig = new(upper: sigX64, lower: sigX0);
            return;

        noIncrement:
            _sig = new(upper: V64(_sig), lower: sigX0);
        }
    }

    // floatXRoundToInt
    public void RoundToInt(SlowFloatContext context, RoundingMode roundingMode, bool exact)
    {
        int exp, shiftDist;
        UInt128 sig;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN. (Probably not necessary as this is also done in ToFloat* methods.)
            if (_isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
            return;
        }

        if (_isInf)
            return;

        exp = _exp;
        shiftDist = 112 - exp;
        if (shiftDist <= 0)
            return;

        if (119 < shiftDist)
        {
            _exp = 112;
            _sig = _isZero ? UInt128.Zero : UInt128.One;
        }
        else
        {
            sig = _sig;
            while (0 < shiftDist)
            {
                ++exp;
                sig = ShortShiftRightJam128(sig, 1);
                --shiftDist;
            }

            _exp = exp;
            _sig = sig;
        }

        RoundTo113(context, false, roundingMode, exact);
        if (_sig == UInt128.Zero)
            _isZero = true;
    }

    #endregion

    #region Integer To Float / Float To Integer

    // ui32ToFloatX (ctor alias)
    public static SlowFloat FromUInt32(uint a) => new(a);

    // ui64ToFloatX (ctor alias)
    public static SlowFloat FromUInt64(ulong a) => new(a);

    // i32ToFloatX (ctor alias)
    public static SlowFloat FromInt32(int a) => new(a);

    // i64ToFloatX (ctor alias)
    public static SlowFloat FromInt64(long a) => new(a);

    // floatXToUI32
    public uint ToUInt32(SlowFloatContext context, RoundingMode roundingMode, bool exact)
    {
        ExceptionFlags savedExceptionFlags;
        SlowFloat x;
        int shiftDist;
        uint z;

        if (_isInf || _isNaN)
        {
            context.ExceptionFlags |= ExceptionFlags.Invalid;
            return (_isInf && _sign) ? uint.MinValue : uint.MaxValue;
        }

        if (_isZero)
            return 0;

        savedExceptionFlags = context.ExceptionFlags; // ignore result from rounding?
        x = this; // make a copy
        shiftDist = 52 - x._exp;
        if (56 < shiftDist)
        {
            x._sig = UInt128.One;
        }
        else
        {
            while (0 < shiftDist)
            {
                x._sig = ShortShiftRightJam128(x._sig, 1);
                --shiftDist;
            }
        }

        x.RoundTo53(context, false, roundingMode, exact);
        x._sig = ShortShiftRightJam128(x._sig, 3);
        z = (uint)V64(x._sig);
        if (shiftDist < 0 || (V64(x._sig) >> 32) != 0 || (x._sign && z != 0))
        {
            context.ExceptionFlags = savedExceptionFlags | ExceptionFlags.Invalid;
            return x._sign ? uint.MinValue : uint.MaxValue;
        }

        return z;
    }

    // floatXToUI64
    public ulong ToUInt64(SlowFloatContext context, RoundingMode roundingMode, bool exact)
    {
        ExceptionFlags savedExceptionFlags;
        SlowFloat x;
        int shiftDist;
        ulong z;

        if (_isInf || _isNaN)
        {
            context.ExceptionFlags |= ExceptionFlags.Invalid;
            return (_isInf && _sign) ? ulong.MinValue : ulong.MaxValue;
        }

        if (_isZero)
            return 0;

        savedExceptionFlags = context.ExceptionFlags; // ignore result from rounding?
        x = this; // make a copy
        shiftDist = 112 - x._exp;
        if (116 < shiftDist)
        {
            x._sig = UInt128.One;
        }
        else
        {
            while (0 < shiftDist)
            {
                x._sig = ShortShiftRightJam128(x._sig, 1);
                --shiftDist;
            }
        }

        x.RoundTo113(context, false, roundingMode, exact);
        x._sig = ShortShiftRightJam128(x._sig, 7);
        z = V0(x._sig);
        if (shiftDist < 0 || V64(x._sig) != 0 || (x._sign && z != 0))
        {
            context.ExceptionFlags = savedExceptionFlags | ExceptionFlags.Invalid;
            return x._sign ? ulong.MinValue : ulong.MaxValue;
        }

        return z;
    }

    // floatXToI32
    public int ToInt32(SlowFloatContext context, RoundingMode roundingMode, bool exact)
    {
        ExceptionFlags savedExceptionFlags;
        SlowFloat x;
        int shiftDist;
        int z;

        if (_isInf || _isNaN)
        {
            context.ExceptionFlags |= ExceptionFlags.Invalid;
            return (_isInf && _sign) ? int.MinValue : int.MaxValue;
        }

        if (_isZero)
            return 0;

        savedExceptionFlags = context.ExceptionFlags; // ignore result from rounding?
        x = this; // make a copy
        shiftDist = 52 - x._exp;
        if (56 < shiftDist)
        {
            x._sig = UInt128.One;
        }
        else
        {
            while (0 < shiftDist)
            {
                x._sig = ShortShiftRightJam128(x._sig, 1);
                --shiftDist;
            }
        }

        x.RoundTo53(context, false, roundingMode, exact);
        x._sig = ShortShiftRightJam128(x._sig, 3);
        z = (int)(uint)V64(x._sig);
        if (x._sign) z = -z;
        if (shiftDist < 0 || (V64(x._sig) >> 32) != 0 || (z != 0 && x._sign != (z < 0)))
        {
            context.ExceptionFlags = savedExceptionFlags | ExceptionFlags.Invalid;
            return x._sign ? int.MinValue : int.MaxValue;
        }

        return z;
    }

    // floatXToI64
    public long ToInt64(SlowFloatContext context, RoundingMode roundingMode, bool exact)
    {
        ExceptionFlags savedExceptionFlags;
        SlowFloat x;
        int shiftDist;
        long z;

        if (_isInf || _isNaN)
        {
            context.ExceptionFlags |= ExceptionFlags.Invalid;
            return (_isInf && _sign) ? long.MinValue : long.MaxValue;
        }

        if (_isZero)
            return 0;

        savedExceptionFlags = context.ExceptionFlags; // ignore result from rounding?
        x = this; // make a copy
        shiftDist = 112 - x._exp;
        if (116 < shiftDist)
        {
            x._sig = UInt128.One;
        }
        else
        {
            while (0 < shiftDist)
            {
                x._sig = ShortShiftRightJam128(x._sig, 1);
                --shiftDist;
            }
        }

        x.RoundTo113(context, false, roundingMode, exact);
        x._sig = ShortShiftRightJam128(x._sig, 7);
        z = (long)V0(x._sig);
        if (x._sign) z = -z;
        if (shiftDist < 0 || V64(x._sig) != 0 || (z != 0 && x._sign != (z < 0)))
        {
            context.ExceptionFlags = savedExceptionFlags | ExceptionFlags.Invalid;
            return x._sign ? long.MinValue : long.MaxValue;
        }

        return z;
    }

    #endregion

    #region Float16

    // f16ToFloatX (ctor alias)
    public static SlowFloat FromFloat16(Float16 value) => new(value);
    public static SlowFloat FromFloat16(Half value) => new(value);

    // floatXToF16
    public Float16 ToFloat16(SlowFloatContext context)
    {
        uint uiZ;
        SlowFloat x, savedX;
        bool isTiny;
        int exp;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
            // TODO: Set value depending on whether it is signaling NaN.
            return Float16.FromUIntBits(0xFFFF);
        }

        if (_isInf)
            return Float16.FromUIntBits((ushort)(_sign ? 0xFC00 : 0x7C00));

        if (_isZero)
            return Float16.FromUIntBits((ushort)(_sign ? 0x8000 : 0));

        x = this;
        while (0x0100000000000000 <= V64(x._sig))
        {
            ++x._exp;
            x._sig = ShortShiftRightJam128(x._sig, 1);
        }

        while (V64(x._sig) < 0x0080000000000000)
        {
            --x._exp;
            x._sig <<= 1;
        }

        savedX = x;
        isTiny = context.DetectTininess == TininessMode.BeforeRounding && x._exp + 0xF <= 0;
        x.RoundTo11(context, isTiny, context.RoundingMode, true);
        exp = x._exp + 0xF;
        if (0x1F <= exp)
        {
            context.ExceptionFlags |= ExceptionFlags.Overflow | ExceptionFlags.Inexact;
            if (x._sign)
            {
                return Float16.FromUIntBits(context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Min or RoundingMode.NearMaxMag => 0xFC00,
                    RoundingMode.MinMag or RoundingMode.Max or RoundingMode.Odd => 0xFBFF,
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                });
            }
            else
            {
                return Float16.FromUIntBits(context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Max or RoundingMode.NearMaxMag => 0x7C00,
                    RoundingMode.MinMag or RoundingMode.Min or RoundingMode.Odd => 0x7BFF,
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                });
            }
        }

        if (exp <= 0)
        {
            isTiny = true;
            x = savedX;
            exp = x._exp + 0xF;
            if (exp < -14)
            {
                x._sig = (x._sig != UInt128.Zero) ? UInt128.One : UInt128.Zero;
            }
            else
            {
                while (exp <= 0)
                {
                    ++exp;
                    x._sig = ShortShiftRightJam128(x._sig, 1);
                }
            }

            x.RoundTo11(context, isTiny, context.RoundingMode, true);
            exp = 0x0080000000000000 <= V64(x._sig) ? 1 : 0;
        }

        uiZ = (uint)exp << 10;
        if (x._sign) uiZ |= 0x8000;
        uiZ |= (uint)(x._sig >> 109) & 0x03FF;
        return Float16.FromUIntBits((ushort)uiZ);
    }

    #region Float to Integer

    // slow_f16_to_ui32
    public static uint ToUInt32(SlowFloatContext context, Float16 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt32(context, roundingMode, exact);
    // slow_f16_to_ui64
    public static ulong ToUInt64(SlowFloatContext context, Float16 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt64(context, roundingMode, exact);
    // slow_f16_to_i32
    public static int ToInt32(SlowFloatContext context, Float16 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt32(context, roundingMode, exact);
    // slow_f16_to_i64
    public static long ToInt64(SlowFloatContext context, Float16 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt64(context, roundingMode, exact);

    // slow_f16_to_ui32_r_minMag
    public static uint ToUInt32RoundMinMag(SlowFloatContext context, Float16 value, bool exact) => new SlowFloat(value).ToUInt32(context, RoundingMode.MinMag, exact);
    // slow_f16_to_ui64_r_minMag
    public static ulong ToUInt64RoundMinMag(SlowFloatContext context, Float16 value, bool exact) => new SlowFloat(value).ToUInt64(context, RoundingMode.MinMag, exact);
    // slow_f16_to_i32_r_minMag
    public static int ToInt32RoundMinMag(SlowFloatContext context, Float16 value, bool exact) => new SlowFloat(value).ToInt32(context, RoundingMode.MinMag, exact);
    // slow_f16_to_i64_r_minMag
    public static long ToInt64RoundMinMag(SlowFloatContext context, Float16 value, bool exact) => new SlowFloat(value).ToInt64(context, RoundingMode.MinMag, exact);

    #endregion

    #region Float to Float

    // slow_f16_to_f32
    public static Float32 ToFloat32(SlowFloatContext context, Float16 value) => new SlowFloat(value).ToFloat32(context);
    // slow_f16_to_f64
    public static Float64 ToFloat64(SlowFloatContext context, Float16 value) => new SlowFloat(value).ToFloat64(context);
    // slow_f16_to_extF80M
    public static ExtFloat80 ToExtFloat80(SlowFloatContext context, Float16 value) => new SlowFloat(value).ToExtFloat80(context);
    // slow_f16_to_f128M
    public static Float128 ToFloat128(SlowFloatContext context, Float16 value) => new SlowFloat(value).ToFloat128(context);

    #endregion

    // slow_f16_roundToInt
    public static Float16 RoundToInt(SlowFloatContext context, Float16 value, RoundingMode roundingMode, bool exact)
    {
        var x = new SlowFloat(value);
        x.RoundToInt(context, roundingMode, exact);
        return x.ToFloat16(context);
    }

    #region Arithmetic

    // slow_f16_add
    public static Float16 Add(SlowFloatContext context, Float16 a, Float16 b) => Add(context, new SlowFloat(a), new SlowFloat(b)).ToFloat16(context);
    // slow_f16_sub
    public static Float16 Subtract(SlowFloatContext context, Float16 a, Float16 b) => Subtract(context, new SlowFloat(a), new SlowFloat(b)).ToFloat16(context);
    // slow_f16_mul
    public static Float16 Multiply(SlowFloatContext context, Float16 a, Float16 b) => Multiply(context, new SlowFloat(a), new SlowFloat(b)).ToFloat16(context);
    // slow_f16_mulAdd
    public static Float16 MultiplyAndAdd(SlowFloatContext context, Float16 a, Float16 b, Float16 c) => MultiplyAndAdd(context, new SlowFloat(a), new SlowFloat(b), new SlowFloat(c)).ToFloat16(context);
    // slow_f16_div
    public static Float16 Divide(SlowFloatContext context, Float16 a, Float16 b) => Divide(context, new SlowFloat(a), new SlowFloat(b)).ToFloat16(context);
    // slow_f16_rem
    public static Float16 Modulus(SlowFloatContext context, Float16 a, Float16 b) => Modulus(context, new SlowFloat(a), new SlowFloat(b)).ToFloat16(context);
    // slow_f16_sqrt
    public static Float16 SquareRoot(SlowFloatContext context, Float16 a) => SquareRoot(context, new SlowFloat(a)).ToFloat16(context);

    #endregion

    #region Logical Comparisons

    // slow_f16_eq / slow_f16_eq_signaling
    public static bool Equals(SlowFloatContext context, Float16 a, Float16 b, bool signaling) => Equals(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_f16_lt / slow_f16_lt_quiet
    public static bool LessThan(SlowFloatContext context, Float16 a, Float16 b, bool signaling) => LessThan(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_f16_le / slow_f16_le_quiet
    public static bool LessThanOrEquals(SlowFloatContext context, Float16 a, Float16 b, bool signaling) => LessThanOrEquals(context, new SlowFloat(a), new SlowFloat(b), signaling);

    #endregion

    #endregion

    #region Float32

    // f32ToFloatX (ctor alias)
    public static SlowFloat FromFloat32(Float32 value) => new(value);
    public static SlowFloat FromFloat32(float value) => new(value);

    // floatXToF32
    public Float32 ToFloat32(SlowFloatContext context)
    {
        uint uiZ;
        SlowFloat x, savedX;
        bool isTiny;
        int exp;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
            // TODO: Set value depending on whether it is signaling NaN.
            return Float32.FromUIntBits(0xFFFFFFFF);
        }

        if (_isInf)
            return Float32.FromUIntBits(_sign ? 0xFF800000 : 0x7F800000);

        if (_isZero)
            return Float32.FromUIntBits(_sign ? 0x80000000 : 0);

        x = this;
        while (0x0100000000000000 <= V64(x._sig))
        {
            ++x._exp;
            x._sig = ShortShiftRightJam128(x._sig, 1);
        }

        while (V64(x._sig) < 0x0080000000000000)
        {
            --x._exp;
            x._sig <<= 1;
        }

        savedX = x;
        isTiny = context.DetectTininess == TininessMode.BeforeRounding && x._exp + 0x7F <= 0;
        x.RoundTo24(context, isTiny, context.RoundingMode, true);
        exp = x._exp + 0x7F;
        if (0xFF <= exp)
        {
            context.ExceptionFlags |= ExceptionFlags.Overflow | ExceptionFlags.Inexact;
            if (x._sign)
            {
                return Float32.FromUIntBits(context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Min or RoundingMode.NearMaxMag => 0xFF800000,
                    RoundingMode.MinMag or RoundingMode.Max or RoundingMode.Odd => 0xFF7FFFFF,
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                });
            }
            else
            {
                return Float32.FromUIntBits(context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Max or RoundingMode.NearMaxMag => 0x7F800000,
                    RoundingMode.MinMag or RoundingMode.Min or RoundingMode.Odd => 0x7F7FFFFF,
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                });
            }
        }

        if (exp <= 0)
        {
            isTiny = true;
            x = savedX;
            exp = x._exp + 0x7F;
            if (exp < -27)
            {
                x._sig = (x._sig != UInt128.Zero) ? UInt128.One : UInt128.Zero;
            }
            else
            {
                while (exp <= 0)
                {
                    ++exp;
                    x._sig = ShortShiftRightJam128(x._sig, 1);
                }
            }

            x.RoundTo24(context, isTiny, context.RoundingMode, true);
            exp = 0x0080000000000000 <= V64(x._sig) ? 1 : 0;
        }

        uiZ = (uint)exp << 23;
        if (x._sign) uiZ |= 0x80000000;
        uiZ |= (uint)(x._sig >> 96) & 0x007FFFFF;
        return Float32.FromUIntBits(uiZ);
    }

    #region Float to Integer

    // slow_f32_to_ui32
    public static uint ToUInt32(SlowFloatContext context, Float32 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt32(context, roundingMode, exact);
    // slow_f32_to_ui64
    public static ulong ToUInt64(SlowFloatContext context, Float32 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt64(context, roundingMode, exact);
    // slow_f32_to_i32
    public static int ToInt32(SlowFloatContext context, Float32 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt32(context, roundingMode, exact);
    // slow_f32_to_i64
    public static long ToInt64(SlowFloatContext context, Float32 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt64(context, roundingMode, exact);

    // slow_f32_to_ui32_r_minMag
    public static uint ToUInt32RoundMinMag(SlowFloatContext context, Float32 value, bool exact) => new SlowFloat(value).ToUInt32(context, RoundingMode.MinMag, exact);
    // slow_f32_to_ui64_r_minMag
    public static ulong ToUInt64RoundMinMag(SlowFloatContext context, Float32 value, bool exact) => new SlowFloat(value).ToUInt64(context, RoundingMode.MinMag, exact);
    // slow_f32_to_i32_r_minMag
    public static int ToInt32RoundMinMag(SlowFloatContext context, Float32 value, bool exact) => new SlowFloat(value).ToInt32(context, RoundingMode.MinMag, exact);
    // slow_f32_to_i64_r_minMag
    public static long ToInt64RoundMinMag(SlowFloatContext context, Float32 value, bool exact) => new SlowFloat(value).ToInt64(context, RoundingMode.MinMag, exact);

    #endregion

    #region Float to Float

    // slow_f32_to_f16
    public static Float16 ToFloat16(SlowFloatContext context, Float32 value) => new SlowFloat(value).ToFloat16(context);
    // slow_f32_to_f64
    public static Float64 ToFloat64(SlowFloatContext context, Float32 value) => new SlowFloat(value).ToFloat64(context);
    // slow_f32_to_extF80M
    public static ExtFloat80 ToExtFloat80(SlowFloatContext context, Float32 value) => new SlowFloat(value).ToExtFloat80(context);
    // slow_f32_to_f128M
    public static Float128 ToFloat128(SlowFloatContext context, Float32 value) => new SlowFloat(value).ToFloat128(context);

    #endregion

    // slow_f32_roundToInt
    public static Float32 RoundToInt(SlowFloatContext context, Float32 value, RoundingMode roundingMode, bool exact)
    {
        var x = new SlowFloat(value);
        x.RoundToInt(context, roundingMode, exact);
        return x.ToFloat32(context);
    }

    #region Arithmetic

    // slow_f32_add
    public static Float32 Add(SlowFloatContext context, Float32 a, Float32 b) => Add(context, new SlowFloat(a), new SlowFloat(b)).ToFloat32(context);
    // slow_f32_sub
    public static Float32 Subtract(SlowFloatContext context, Float32 a, Float32 b) => Subtract(context, new SlowFloat(a), new SlowFloat(b)).ToFloat32(context);
    // slow_f32_mul
    public static Float32 Multiply(SlowFloatContext context, Float32 a, Float32 b) => Multiply(context, new SlowFloat(a), new SlowFloat(b)).ToFloat32(context);
    // slow_f32_mulAdd
    public static Float32 MultiplyAndAdd(SlowFloatContext context, Float32 a, Float32 b, Float32 c) => MultiplyAndAdd(context, new SlowFloat(a), new SlowFloat(b), new SlowFloat(c)).ToFloat32(context);
    // slow_f32_div
    public static Float32 Divide(SlowFloatContext context, Float32 a, Float32 b) => Divide(context, new SlowFloat(a), new SlowFloat(b)).ToFloat32(context);
    // slow_f32_rem
    public static Float32 Modulus(SlowFloatContext context, Float32 a, Float32 b) => Modulus(context, new SlowFloat(a), new SlowFloat(b)).ToFloat32(context);
    // slow_f32_sqrt
    public static Float32 SquareRoot(SlowFloatContext context, Float32 a) => SquareRoot(context, new SlowFloat(a)).ToFloat32(context);

    #endregion

    #region Logical Comparisons

    // slow_f32_eq / slow_f32_eq_signaling
    public static bool Equals(SlowFloatContext context, Float32 a, Float32 b, bool signaling) => Equals(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_f32_lt / slow_f32_lt_quiet
    public static bool LessThan(SlowFloatContext context, Float32 a, Float32 b, bool signaling) => LessThan(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_f32_le / slow_f32_le_quiet
    public static bool LessThanOrEquals(SlowFloatContext context, Float32 a, Float32 b, bool signaling) => LessThanOrEquals(context, new SlowFloat(a), new SlowFloat(b), signaling);

    #endregion

    #endregion

    #region Float64

    // f64ToFloatX (ctor alias)
    public static SlowFloat FromFloat64(Float64 value) => new(value);
    public static SlowFloat FromFloat64(double value) => new(value);

    // floatXToF64
    public Float64 ToFloat64(SlowFloatContext context)
    {
        ulong uiZ;
        SlowFloat x, savedX;
        bool isTiny;
        int exp;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
            // TODO: Set value depending on whether it is signaling NaN.
            return Float64.FromUIntBits(0xFFFFFFFFFFFFFFFF);
        }

        if (_isInf)
            return Float64.FromUIntBits(_sign ? 0xFFF0000000000000 : 0x7FF0000000000000);

        if (_isZero)
            return Float64.FromUIntBits(_sign ? 0x8000000000000000 : 0);

        x = this;
        while (0x0100000000000000 <= V64(x._sig))
        {
            ++x._exp;
            x._sig = ShortShiftRightJam128(x._sig, 1);
        }

        while (V64(x._sig) < 0x0080000000000000)
        {
            --x._exp;
            x._sig <<= 1;
        }

        savedX = x;
        isTiny = context.DetectTininess == TininessMode.BeforeRounding && x._exp + 0x3FF <= 0;
        x.RoundTo53(context, isTiny, context.RoundingMode, true);
        exp = x._exp + 0x3FF;
        if (0x7FF <= exp)
        {
            context.ExceptionFlags |= ExceptionFlags.Overflow | ExceptionFlags.Inexact;
            if (x._sign)
            {
                return Float64.FromUIntBits(context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Min or RoundingMode.NearMaxMag => 0xFFF0000000000000,
                    RoundingMode.MinMag or RoundingMode.Max or RoundingMode.Odd => 0xFFEFFFFFFFFFFFFF,
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                });
            }
            else
            {
                return Float64.FromUIntBits(context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Max or RoundingMode.NearMaxMag => 0x7FF0000000000000,
                    RoundingMode.MinMag or RoundingMode.Min or RoundingMode.Odd => 0x7FEFFFFFFFFFFFFF,
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                });
            }
        }

        if (exp <= 0)
        {
            isTiny = true;
            x = savedX;
            exp = x._exp + 0x3FF;
            if (exp < -56)
            {
                x._sig = (x._sig != UInt128.Zero) ? UInt128.One : UInt128.Zero;
            }
            else
            {
                while (exp <= 0)
                {
                    ++exp;
                    x._sig = ShortShiftRightJam128(x._sig, 1);
                }
            }

            x.RoundTo53(context, isTiny, context.RoundingMode, true);
            exp = 0x0080000000000000 <= V64(x._sig) ? 1 : 0;
        }

        uiZ = (ulong)exp << 52;
        if (x._sign) uiZ |= 0x8000000000000000;
        uiZ |= (ulong)(x._sig >> 67) & 0x000FFFFFFFFFFFFF;
        return Float64.FromUIntBits(uiZ);
    }

    #region Float to Integer

    // slow_f64_to_ui32
    public static uint ToUInt32(SlowFloatContext context, Float64 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt32(context, roundingMode, exact);
    // slow_f64_to_ui64
    public static ulong ToUInt64(SlowFloatContext context, Float64 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt64(context, roundingMode, exact);
    // slow_f64_to_i32
    public static int ToInt32(SlowFloatContext context, Float64 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt32(context, roundingMode, exact);
    // slow_f64_to_i64
    public static long ToInt64(SlowFloatContext context, Float64 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt64(context, roundingMode, exact);

    // slow_f64_to_ui32_r_minMag
    public static uint ToUInt32RoundMinMag(SlowFloatContext context, Float64 value, bool exact) => new SlowFloat(value).ToUInt32(context, RoundingMode.MinMag, exact);
    // slow_f64_to_ui64_r_minMag
    public static ulong ToUInt64RoundMinMag(SlowFloatContext context, Float64 value, bool exact) => new SlowFloat(value).ToUInt64(context, RoundingMode.MinMag, exact);
    // slow_f64_to_i32_r_minMag
    public static int ToInt32RoundMinMag(SlowFloatContext context, Float64 value, bool exact) => new SlowFloat(value).ToInt32(context, RoundingMode.MinMag, exact);
    // slow_f64_to_i64_r_minMag
    public static long ToInt64RoundMinMag(SlowFloatContext context, Float64 value, bool exact) => new SlowFloat(value).ToInt64(context, RoundingMode.MinMag, exact);

    #endregion

    #region Float to Float

    // slow_f64_to_f16
    public static Float16 ToFloat16(SlowFloatContext context, Float64 value) => new SlowFloat(value).ToFloat16(context);
    // slow_f64_to_f32
    public static Float32 ToFloat32(SlowFloatContext context, Float64 value) => new SlowFloat(value).ToFloat32(context);
    // slow_f64_to_extF80M
    public static ExtFloat80 ToExtFloat80(SlowFloatContext context, Float64 value) => new SlowFloat(value).ToExtFloat80(context);
    // slow_f64_to_f128M
    public static Float128 ToFloat128(SlowFloatContext context, Float64 value) => new SlowFloat(value).ToFloat128(context);

    #endregion

    // slow_f64_roundToInt
    public static Float64 RoundToInt(SlowFloatContext context, Float64 value, RoundingMode roundingMode, bool exact)
    {
        var x = new SlowFloat(value);
        x.RoundToInt(context, roundingMode, exact);
        return x.ToFloat64(context);
    }

    #region Arithmetic

    // slow_f64_add
    public static Float64 Add(SlowFloatContext context, Float64 a, Float64 b) => Add(context, new SlowFloat(a), new SlowFloat(b)).ToFloat64(context);
    // slow_f64_sub
    public static Float64 Subtract(SlowFloatContext context, Float64 a, Float64 b) => Subtract(context, new SlowFloat(a), new SlowFloat(b)).ToFloat64(context);
    // slow_f64_mul
    public static Float64 Multiply(SlowFloatContext context, Float64 a, Float64 b) => Multiply(context, new SlowFloat(a), new SlowFloat(b)).ToFloat64(context);
    // slow_f64_mulAdd
    public static Float64 MultiplyAndAdd(SlowFloatContext context, Float64 a, Float64 b, Float64 c) => MultiplyAndAdd(context, new SlowFloat(a), new SlowFloat(b), new SlowFloat(c)).ToFloat64(context);
    // slow_f64_div
    public static Float64 Divide(SlowFloatContext context, Float64 a, Float64 b) => Divide(context, new SlowFloat(a), new SlowFloat(b)).ToFloat64(context);
    // slow_f64_rem
    public static Float64 Modulus(SlowFloatContext context, Float64 a, Float64 b) => Modulus(context, new SlowFloat(a), new SlowFloat(b)).ToFloat64(context);
    // slow_f64_sqrt
    public static Float64 SquareRoot(SlowFloatContext context, Float64 a) => SquareRoot(context, new SlowFloat(a)).ToFloat64(context);

    #endregion

    #region Logical Comparisons

    // slow_f64_eq / slow_f64_eq_signaling
    public static bool Equals(SlowFloatContext context, Float64 a, Float64 b, bool signaling) => Equals(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_f64_lt / slow_f64_lt_quiet
    public static bool LessThan(SlowFloatContext context, Float64 a, Float64 b, bool signaling) => LessThan(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_f64_le / slow_f64_le_quiet
    public static bool LessThanOrEquals(SlowFloatContext context, Float64 a, Float64 b, bool signaling) => LessThanOrEquals(context, new SlowFloat(a), new SlowFloat(b), signaling);

    #endregion

    #endregion

    #region ExtFloat80

    // extF80MToFloatX (ctor alias)
    public static SlowFloat FromExtFloat80(ExtFloat80 value) => new(value);

    // floatXToExtF80M
    public ExtFloat80 ToExtFloat80(SlowFloatContext context)
    {
        SlowFloat x, savedX;
        bool isTiny;
        int exp;
        uint uiZ64;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
            // TODO: Set value depending on whether it is signaling NaN.
            return ExtFloat80.FromUIntBits(0xFFFF, 0xFFFFFFFFFFFFFFFF);
        }

        if (_isInf)
            return ExtFloat80.FromUIntBits((ushort)(_sign ? 0xFFFF : 0x7FFF), 0x8000000000000000);

        if (_isZero)
            return ExtFloat80.FromUIntBits((ushort)(_sign ? 0x8000 : 0), 0);

        x = this;
        while (0x0100000000000000 <= V64(x._sig))
        {
            ++x._exp;
            x._sig = ShortShiftRightJam128(x._sig, 1);
        }

        while (V64(x._sig) < 0x0080000000000000)
        {
            --x._exp;
            x._sig <<= 1;
        }

        savedX = x;
        isTiny = context.DetectTininess == TininessMode.BeforeRounding && x._exp + 0x3FFF <= 0;
        switch (context.RoundingPrecisionExtFloat80)
        {
            case ExtFloat80RoundingPrecision._32:
            {
                x.RoundTo24(context, isTiny, context.RoundingMode, true);
                break;
            }
            case ExtFloat80RoundingPrecision._64:
            {
                x.RoundTo53(context, isTiny, context.RoundingMode, true);
                break;
            }
            default:
            {
                x.RoundTo64(context, isTiny, context.RoundingMode, true);
                break;
            }
        }

        exp = x._exp + 0x3FFF;
        if (0x7FFF <= exp)
        {
            context.ExceptionFlags |= ExceptionFlags.Overflow | ExceptionFlags.Inexact;
            if (x._sign)
            {
                return context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Min or RoundingMode.NearMaxMag => ExtFloat80.FromUIntBits(0xFFFF, 0x8000000000000000),
                    RoundingMode.MinMag or RoundingMode.Max or RoundingMode.Odd => ExtFloat80.FromUIntBits(
                        0xFFFE,
                        context.RoundingPrecisionExtFloat80 switch
                        {
                            ExtFloat80RoundingPrecision._32 => 0xFFFFFF0000000000,
                            ExtFloat80RoundingPrecision._64 => 0xFFFFFFFFFFFFF800,
                            _ => 0xFFFFFFFFFFFFFFFF
                        }
                    ),
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                };
            }
            else
            {
                return context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Max or RoundingMode.NearMaxMag => ExtFloat80.FromUIntBits(0x7FFF, 0x8000000000000000),
                    RoundingMode.MinMag or RoundingMode.Min or RoundingMode.Odd => ExtFloat80.FromUIntBits(
                        0x7FFE,
                        context.RoundingPrecisionExtFloat80 switch
                        {
                            ExtFloat80RoundingPrecision._32 => 0xFFFFFF0000000000,
                            ExtFloat80RoundingPrecision._64 => 0xFFFFFFFFFFFFF800,
                            _ => 0xFFFFFFFFFFFFFFFF
                        }
                    ),
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                };
            }
        }

        if (exp <= 0)
        {
            isTiny = true;
            x = savedX;
            exp = x._exp + 0x3FFF;
            if (exp < -70)
            {
                x._sig = (x._sig != UInt128.Zero) ? UInt128.One : UInt128.Zero;
            }
            else
            {
                while (exp <= 0)
                {
                    ++exp;
                    x._sig = ShortShiftRightJam128(x._sig, 1);
                }
            }

            switch (context.RoundingPrecisionExtFloat80)
            {
                case ExtFloat80RoundingPrecision._32:
                {
                    x.RoundTo24(context, isTiny, context.RoundingMode, true);
                    break;
                }
                case ExtFloat80RoundingPrecision._64:
                {
                    x.RoundTo53(context, isTiny, context.RoundingMode, true);
                    break;
                }
                default:
                {
                    x.RoundTo64(context, isTiny, context.RoundingMode, true);
                    break;
                }
            }

            exp = (0x0080000000000000 <= V64(x._sig)) ? 1 : 0;
        }

        uiZ64 = (uint)exp;
        if (x._sign) uiZ64 |= 0x8000;
        return ExtFloat80.FromUIntBits((ushort)uiZ64, (ulong)ShortShiftRightJam128(x._sig, 56));
    }

    #region Float to Integer

    // slow_extF80M_to_ui32
    public static uint ToUInt32(SlowFloatContext context, ExtFloat80 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt32(context, roundingMode, exact);
    // slow_extF80M_to_ui64
    public static ulong ToUInt64(SlowFloatContext context, ExtFloat80 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt64(context, roundingMode, exact);
    // slow_extF80M_to_i32
    public static int ToInt32(SlowFloatContext context, ExtFloat80 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt32(context, roundingMode, exact);
    // slow_extF80M_to_i64
    public static long ToInt64(SlowFloatContext context, ExtFloat80 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt64(context, roundingMode, exact);

    // slow_extF80M_to_ui32_r_minMag
    public static uint ToUInt32RoundMinMag(SlowFloatContext context, ExtFloat80 value, bool exact) => new SlowFloat(value).ToUInt32(context, RoundingMode.MinMag, exact);
    // slow_extF80M_to_ui64_r_minMag
    public static ulong ToUInt64RoundMinMag(SlowFloatContext context, ExtFloat80 value, bool exact) => new SlowFloat(value).ToUInt64(context, RoundingMode.MinMag, exact);
    // slow_extF80M_to_i32_r_minMag
    public static int ToInt32RoundMinMag(SlowFloatContext context, ExtFloat80 value, bool exact) => new SlowFloat(value).ToInt32(context, RoundingMode.MinMag, exact);
    // slow_extF80M_to_i64_r_minMag
    public static long ToInt64RoundMinMag(SlowFloatContext context, ExtFloat80 value, bool exact) => new SlowFloat(value).ToInt64(context, RoundingMode.MinMag, exact);

    #endregion

    #region Float to Float

    // slow_extF80M_to_f16
    public static Float16 ToFloat16(SlowFloatContext context, ExtFloat80 value) => new SlowFloat(value).ToFloat16(context);
    // slow_extF80M_to_f32
    public static Float32 ToFloat32(SlowFloatContext context, ExtFloat80 value) => new SlowFloat(value).ToFloat32(context);
    // slow_extF80M_to_f64
    public static Float64 ToFloat64(SlowFloatContext context, ExtFloat80 value) => new SlowFloat(value).ToFloat64(context);
    // slow_extF80M_to_f128M
    public static Float128 ToFloat128(SlowFloatContext context, ExtFloat80 value) => new SlowFloat(value).ToFloat128(context);

    #endregion

    // slow_extF80M_roundToInt
    public static ExtFloat80 RoundToInt(SlowFloatContext context, ExtFloat80 value, RoundingMode roundingMode, bool exact)
    {
        var x = new SlowFloat(value);
        x.RoundToInt(context, roundingMode, exact);
        return x.ToExtFloat80(context);
    }

    #region Arithmetic

    // slow_extF80M_add
    public static ExtFloat80 Add(SlowFloatContext context, ExtFloat80 a, ExtFloat80 b) => Add(context, new SlowFloat(a), new SlowFloat(b)).ToExtFloat80(context);
    // slow_extF80M_sub
    public static ExtFloat80 Subtract(SlowFloatContext context, ExtFloat80 a, ExtFloat80 b) => Subtract(context, new SlowFloat(a), new SlowFloat(b)).ToExtFloat80(context);
    // slow_extF80M_mul
    public static ExtFloat80 Multiply(SlowFloatContext context, ExtFloat80 a, ExtFloat80 b) => Multiply(context, new SlowFloat(a), new SlowFloat(b)).ToExtFloat80(context);
    // slow_extF80M_div
    public static ExtFloat80 Divide(SlowFloatContext context, ExtFloat80 a, ExtFloat80 b) => Divide(context, new SlowFloat(a), new SlowFloat(b)).ToExtFloat80(context);
    // slow_extF80M_rem
    public static ExtFloat80 Modulus(SlowFloatContext context, ExtFloat80 a, ExtFloat80 b) => Modulus(context, new SlowFloat(a), new SlowFloat(b)).ToExtFloat80(context);
    // slow_extF80M_sqrt
    public static ExtFloat80 SquareRoot(SlowFloatContext context, ExtFloat80 a) => SquareRoot(context, new SlowFloat(a)).ToExtFloat80(context);

    #endregion

    #region Logical Comparisons

    // slow_extF80M_eq / slow_extF80M_eq_signaling
    public static bool Equals(SlowFloatContext context, ExtFloat80 a, ExtFloat80 b, bool signaling) => Equals(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_extF80M_lt / slow_extF80M_lt_quiet
    public static bool LessThan(SlowFloatContext context, ExtFloat80 a, ExtFloat80 b, bool signaling) => LessThan(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_extF80M_le / slow_extF80M_le_quiet
    public static bool LessThanOrEquals(SlowFloatContext context, ExtFloat80 a, ExtFloat80 b, bool signaling) => LessThanOrEquals(context, new SlowFloat(a), new SlowFloat(b), signaling);

    #endregion

    #endregion

    #region Float128

    // f128MToFloatX (ctor alias)
    public static SlowFloat FromFloat128(Float128 value) => new(value);

    // floatXToF128M
    public Float128 ToFloat128(SlowFloatContext context)
    {
        ulong uiZ64;
        SlowFloat x, savedX;
        bool isTiny;
        int exp;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
            // TODO: Set value depending on whether it is signaling NaN.
            return Float128.FromUIntBits(upper: 0xFFFFFFFFFFFFFFFF, lower: 0xFFFFFFFFFFFFFFFF);
        }

        if (_isInf)
            return Float128.FromUIntBits(upper: _sign ? 0xFFFF000000000000 : 0x7FFF000000000000, lower: 0x0000000000000000);

        if (_isZero)
            return Float128.FromUIntBits(upper: _sign ? 0x8000000000000000 : 0x0000000000000000, lower: 0x0000000000000000);

        x = this;
        while (0x0100000000000000 <= V64(x._sig))
        {
            ++x._exp;
            x._sig = ShortShiftRightJam128(x._sig, 1);
        }

        while (V64(x._sig) < 0x0080000000000000)
        {
            --x._exp;
            x._sig <<= 1;
        }

        savedX = x;
        isTiny = context.DetectTininess == TininessMode.BeforeRounding && x._exp + 0x3FFF <= 0;
        x.RoundTo113(context, isTiny, context.RoundingMode, true);
        exp = x._exp + 0x3FFF;
        if (0x7FFF <= exp)
        {
            context.ExceptionFlags |= ExceptionFlags.Overflow | ExceptionFlags.Inexact;
            if (x._sign)
            {
                return context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Min or RoundingMode.NearMaxMag => Float128.FromUIntBits(upper: 0xFFFF000000000000, lower: 0x0000000000000000),
                    RoundingMode.MinMag or RoundingMode.Max or RoundingMode.Odd => Float128.FromUIntBits(upper: 0xFFFEFFFFFFFFFFFF, lower: 0xFFFFFFFFFFFFFFFF),
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                };
            }
            else
            {
                return context.RoundingMode switch
                {
                    RoundingMode.NearEven or RoundingMode.Max or RoundingMode.NearMaxMag => Float128.FromUIntBits(upper: 0x7FFF000000000000, lower: 0x0000000000000000),
                    RoundingMode.MinMag or RoundingMode.Min or RoundingMode.Odd => Float128.FromUIntBits(upper: 0x7FFEFFFFFFFFFFFF, lower: 0xFFFFFFFFFFFFFFFF),
                    _ => throw new InvalidOperationException("Invalid rounding mode.")
                };
            }
        }

        if (exp <= 0)
        {
            isTiny = true;
            x = savedX;
            exp = x._exp + 0x3FFF;
            if (exp < -120)
            {
                x._sig = (x._sig != UInt128.Zero) ? UInt128.One : UInt128.Zero;
            }
            else
            {
                while (exp <= 0)
                {
                    ++exp;
                    x._sig = ShortShiftRightJam128(x._sig, 1);
                }
            }

            x.RoundTo113(context, isTiny, context.RoundingMode, true);
            exp = 0x0080000000000000 <= V64(x._sig) ? 1 : 0;
        }

        uiZ64 = (ulong)exp << 48;
        if (x._sign) uiZ64 |= 0x8000000000000000;
        x._sig = ShortShiftRightJam128(x._sig, 7);
        return Float128.FromUIntBits((x._sig & _x0000FFFFFFFFFFFF_FFFFFFFFFFFFFFFF) | new UInt128(upper: uiZ64, lower: 0x0000000000000000));
    }

    #region Float to Integer

    // slow_f128M_to_ui32
    public static uint ToUInt32(SlowFloatContext context, Float128 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt32(context, roundingMode, exact);
    // slow_f128M_to_ui64
    public static ulong ToUInt64(SlowFloatContext context, Float128 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToUInt64(context, roundingMode, exact);
    // slow_f128M_to_i32
    public static int ToInt32(SlowFloatContext context, Float128 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt32(context, roundingMode, exact);
    // slow_f128M_to_i64
    public static long ToInt64(SlowFloatContext context, Float128 value, RoundingMode roundingMode, bool exact) => new SlowFloat(value).ToInt64(context, roundingMode, exact);

    // slow_f128M_to_ui32_r_minMag
    public static uint ToUInt32RoundMinMag(SlowFloatContext context, Float128 value, bool exact) => new SlowFloat(value).ToUInt32(context, RoundingMode.MinMag, exact);
    // slow_f128M_to_ui64_r_minMag
    public static ulong ToUInt64RoundMinMag(SlowFloatContext context, Float128 value, bool exact) => new SlowFloat(value).ToUInt64(context, RoundingMode.MinMag, exact);
    // slow_f128M_to_i32_r_minMag
    public static int ToInt32RoundMinMag(SlowFloatContext context, Float128 value, bool exact) => new SlowFloat(value).ToInt32(context, RoundingMode.MinMag, exact);
    // slow_f128M_to_i64_r_minMag
    public static long ToInt64RoundMinMag(SlowFloatContext context, Float128 value, bool exact) => new SlowFloat(value).ToInt64(context, RoundingMode.MinMag, exact);

    #endregion

    #region Float to Float

    // slow_f128M_to_f16
    public static Float16 ToFloat16(SlowFloatContext context, Float128 value) => new SlowFloat(value).ToFloat16(context);
    // slow_f128M_to_f32
    public static Float32 ToFloat32(SlowFloatContext context, Float128 value) => new SlowFloat(value).ToFloat32(context);
    // slow_f128M_to_f64
    public static Float64 ToFloat64(SlowFloatContext context, Float128 value) => new SlowFloat(value).ToFloat64(context);
    // slow_f128M_to_extF80M
    public static ExtFloat80 ToExtFloat80(SlowFloatContext context, Float128 value) => new SlowFloat(value).ToExtFloat80(context);

    #endregion

    // slow_f128M_roundToInt
    public static Float128 RoundToInt(SlowFloatContext context, Float128 value, RoundingMode roundingMode, bool exact)
    {
        var x = new SlowFloat(value);
        x.RoundToInt(context, roundingMode, exact);
        return x.ToFloat128(context);
    }

    #region Arithmetic

    // slow_f128M_add
    public static Float128 Add(SlowFloatContext context, Float128 a, Float128 b) => Add(context, new SlowFloat(a), new SlowFloat(b)).ToFloat128(context);
    // slow_f128M_sub
    public static Float128 Subtract(SlowFloatContext context, Float128 a, Float128 b) => Subtract(context, new SlowFloat(a), new SlowFloat(b)).ToFloat128(context);
    // slow_f128M_mul
    public static Float128 Multiply(SlowFloatContext context, Float128 a, Float128 b) => Multiply(context, new SlowFloat(a), new SlowFloat(b)).ToFloat128(context);
    // slow_f128M_div
    public static Float128 Divide(SlowFloatContext context, Float128 a, Float128 b) => Divide(context, new SlowFloat(a), new SlowFloat(b)).ToFloat128(context);
    // slow_f128M_rem
    public static Float128 Modulus(SlowFloatContext context, Float128 a, Float128 b) => Modulus(context, new SlowFloat(a), new SlowFloat(b)).ToFloat128(context);
    // slow_f128M_sqrt
    public static Float128 SquareRoot(SlowFloatContext context, Float128 a) => SquareRoot(context, new SlowFloat(a)).ToFloat128(context);

    // This is different, because it uses a 256-bit float internally for calculations instead of the normal 128-bit float.
    // slow_f128M_mulAdd
    public static Float128 MultiplyAndAdd(SlowFloatContext context, Float128 a, Float128 b, Float128 c)
    {
        var x = new SlowFloat256(a);
        x.Multiply(context, new(b));
        x.Add(context, new(c));
        return x.ToFloat128(context);
    }

    #endregion

    #region Logical Comparisons

    // slow_f128M_eq / slow_f128M_eq_signaling
    public static bool Equals(SlowFloatContext context, Float128 a, Float128 b, bool signaling) => Equals(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_f128M_lt / slow_f128M_lt_quiet
    public static bool LessThan(SlowFloatContext context, Float128 a, Float128 b, bool signaling) => LessThan(context, new SlowFloat(a), new SlowFloat(b), signaling);
    // slow_f128M_le / slow_f128M_le_quiet
    public static bool LessThanOrEquals(SlowFloatContext context, Float128 a, Float128 b, bool signaling) => LessThanOrEquals(context, new SlowFloat(a), new SlowFloat(b), signaling);

    #endregion

    #endregion

    // floatXInvalid
    public void Invalid(SlowFloatContext context)
    {
        context.ExceptionFlags |= ExceptionFlags.Invalid;
        this = NaN;
    }

    #region Arithmetic

    // floatXAdd (static alias)
    public static SlowFloat Add(SlowFloatContext context, SlowFloat a, SlowFloat b)
    {
        a.Add(context, b);
        return a;
    }

    public static SlowFloat Subtract(SlowFloatContext context, SlowFloat a, SlowFloat b)
    {
        b._sign = !b._sign;
        a.Add(context, b);
        return a;
    }

    // floatXMul (static alias)
    public static SlowFloat Multiply(SlowFloatContext context, SlowFloat a, SlowFloat b)
    {
        a.Multiply(context, b);
        return a;
    }

    public static SlowFloat MultiplyAndAdd(SlowFloatContext context, SlowFloat a, SlowFloat b, SlowFloat c)
    {
        a.Multiply(context, b);
        a.Add(context, c);
        return a;
    }

    // floatXDiv (static alias)
    public static SlowFloat Divide(SlowFloatContext context, SlowFloat a, SlowFloat b)
    {
        a.Divide(context, b);
        return a;
    }

    // floatXRem (static alias)
    public static SlowFloat Modulus(SlowFloatContext context, SlowFloat a, SlowFloat b)
    {
        a.Modulus(context, b);
        return a;
    }

    // floatXSqrt (static alias)
    public static SlowFloat SquareRoot(SlowFloatContext context, SlowFloat a)
    {
        a.SquareRoot(context);
        return a;
    }

    // floatXAdd
    public void Add(SlowFloatContext context, SlowFloat y)
    {
        int expX, expY, expDiff;
        UInt128 sigY;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling || (y._isNaN && y._isSignaling))
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._isNaN)
        {
            this = y;

            // HACK: Add support for signaling NaN.
            if (y._isSignaling)
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (_isInf && y._isInf)
        {
            if (_sign != y._sign)
                Invalid(context);

            return;
        }

        if (_isInf)
            return;

        if (y._isInf)
        {
            this = y;
            return;
        }

        if (_isZero && y._isZero)
        {
            if (_sign == y._sign)
                return;

            this = (context.RoundingMode == RoundingMode.Min) ? NegativeZero : PositiveZero;
            return;
        }

        expX = _exp;
        expY = y._exp;

        if (_sign != y._sign && expX == expY && _sig == y._sig)
        {
            this = (context.RoundingMode == RoundingMode.Min) ? NegativeZero : PositiveZero;
            return;
        }

        if (_isZero)
        {
            this = y;
            return;
        }

        if (y._isZero)
            return;

        expDiff = expX - expY;
        if (expDiff < 0)
        {
            _exp = expY;
            if (expDiff < -120)
            {
                _sig = UInt128.One;
            }
            else
            {
                while (expDiff < 0)
                {
                    ++expDiff;
                    _sig = ShortShiftRightJam128(_sig, 1);
                }
            }

            if (_sign != y._sign)
                _sig = Neg128(_sig);

            _sign = y._sign;
            _sig += y._sig;
        }
        else
        {
            sigY = y._sig;
            if (120 < expDiff)
            {
                sigY = UInt128.One;
            }
            else
            {
                while (0 < expDiff)
                {
                    --expDiff;
                    sigY = ShortShiftRightJam128(sigY, 1);
                }
            }

            if (_sign != y._sign)
                sigY = Neg128(sigY);

            _sig += sigY;
        }

        if ((_sig & _x8000000000000000_0000000000000000) != 0)
        {
            _sign = !_sign;
            _sig = Neg128(_sig);
        }
    }

    // floatXMul
    public void Multiply(SlowFloatContext context, SlowFloat y)
    {
        UInt128 sig;
        int bitNum;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling || (y._isNaN && y._isSignaling))
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._isNaN)
        {
            _isNaN = true;
            _isInf = false;
            _isZero = false;
            _sign = y._sign;

            // HACK: Add support for signaling NaN.
            if (y._isNaN && y._isSignaling)
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._sign)
            _sign = !_sign;

        if (_isInf)
        {
            if (y._isZero)
                Invalid(context);

            return;
        }

        if (y._isInf)
        {
            if (_isZero)
                Invalid(context);
            else
                _isInf = true;

            return;
        }

        if (_isZero || y._isZero)
        {
            this = _sign ? NegativeZero : PositiveZero;
            return;
        }

        _exp += y._exp;
        sig = UInt128.Zero;
        for (bitNum = 0; bitNum < 120; ++bitNum)
        {
            sig = ShortShiftRightJam128(sig, 1);
            if (((ulong)_sig & 1) != 0)
                sig += y._sig;

            _sig >>= 1;
        }

        if (0x0100000000000000 <= V64(sig))
        {
            ++_exp;
            sig = ShortShiftRightJam128(sig, 1);
        }

        _sig = sig;
    }

    // floatXDiv
    public void Divide(SlowFloatContext context, SlowFloat y)
    {
        UInt128 sig, negSigY;
        int bitNum;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling || (y._isNaN && y._isSignaling))
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._isNaN)
        {
            _isNaN = true;
            _isInf = false;
            _isZero = false;
            _sign = y._sign;

            // HACK: Add support for signaling NaN.
            if (y._isSignaling)
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._sign)
            _sign = !_sign;

        if (_isInf)
        {
            if (y._isInf)
                Invalid(context);

            return;
        }

        if (y._isZero)
        {
            if (_isZero)
            {
                Invalid(context);
                return;
            }

            context.ExceptionFlags |= ExceptionFlags.Infinite;
            _isInf = true;
            return;
        }

        if (_isZero || y._isInf)
        {
            this = _sign ? NegativeZero : PositiveZero;
            return;
        }

        _exp -= y._exp + 1;
        sig = UInt128.Zero;
        negSigY = Neg128(y._sig);
        for (bitNum = 0; bitNum < 120; ++bitNum)
        {
            if (y._sig <= _sig)
            {
                sig |= UInt128.One;
                _sig += negSigY;
            }

            _sig <<= 1;
            sig <<= 1;
        }

        if (_sig != UInt128.Zero)
            sig |= UInt128.One;

        _sig = sig;
    }

    // floatXRem
    public void Modulus(SlowFloatContext context, SlowFloat y)
    {
        int expX, expY;
        UInt128 sigY, negSigY;
        bool lastQuotientBit;
        UInt128 savedSigX;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling || (y._isNaN && y._isSignaling))
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._isNaN)
        {
            _isNaN = true;
            _isInf = false;
            _isZero = false;
            _sign = y._sign;

            // HACK: Add support for signaling NaN.
            if (y._isSignaling)
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (_isInf || y._isZero)
        {
            Invalid(context);
            return;
        }

        if (_isZero || y._isInf)
            return;

        expX = _exp;
        expY = y._exp - 1;
        if (expX < expY)
            return;

        sigY = y._sig << 1;
        negSigY = Neg128(sigY);
        while (expY < expX)
        {
            --expX;
            if (sigY <= _sig)
                _sig += negSigY;

            _sig <<= 1;
        }

        _exp = expX;
        lastQuotientBit = sigY <= _sig;
        if (lastQuotientBit)
            _sig += negSigY;

        savedSigX = _sig;
        _sig = Neg128(_sig + negSigY);
        if (_sig < savedSigX)
        {
            _sign = !_sign;
        }
        else if (savedSigX <_sig)
        {
            _sig = savedSigX;
        }
        else
        {
            if (lastQuotientBit)
                _sign = !_sign;
            else
                _sig = savedSigX;
        }

        if (_sig == UInt128.Zero)
            _isZero = true;
    }

    // floatXSqrt
    public void SquareRoot(SlowFloatContext context)
    {
        UInt128 sig, bitSig;
        int bitNum;
        UInt128 savedSigX;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
            return;
        }

        if (_isZero)
            return;

        if (_sign)
        {
            Invalid(context);
            return;
        }

        if (_isInf)
            return;

        if ((_exp & 1) == 0)
            _sig = ShortShiftRightJam128(_sig, 1);

        _exp >>= 1;
        sig = UInt128.Zero;
        bitSig = _x0080000000000000_0000000000000000;
        for (bitNum = 0; bitNum < 120; ++bitNum)
        {
            savedSigX = _sig;
            _sig += Neg128(sig);
            _sig <<= 1;
            _sig += Neg128(bitSig);
            if ((_sig & _x8000000000000000_0000000000000000) != 0)
                _sig = savedSigX << 1;
            else
                sig |= bitSig;

            bitSig = ShortShiftRightJam128(bitSig, 1);
        }

        if (_sig != UInt128.Zero)
            sig |= UInt128.One;

        _sig = sig;
    }

    #endregion

    #region Logical Comparisons

    // floatXEq
    public static bool operator ==(SlowFloat x, SlowFloat y)
    {
        if (x._isNaN || y._isNaN) return false;
        if (x._isZero && y._isZero) return true;
        if (x._sign != y._sign) return false;
        if (x._isInf || y._isInf) return x._isInf && y._isInf;
        return x._exp == y._exp && x._sig == y._sig;
    }

    public static bool operator !=(SlowFloat x, SlowFloat y) => !(x == y);

    // floatXLe
    public static bool operator <=(SlowFloat x, SlowFloat y)
    {
        if (x._isNaN || y._isNaN) return false;
        if (x._isZero && y._isZero) return true;
        if (x._sign != y._sign) return x._sign;
        if (x._sign)
        {
            if (x._isInf || y._isZero) return true;
            if (y._isInf || x._isZero) return false;
            if (y._exp < x._exp) return true;
            if (x._exp < y._exp) return false;
            return y._sig <= x._sig;
        }
        else
        {
            if (y._isInf || x._isZero) return true;
            if (x._isInf || y._isZero) return false;
            if (x._exp < y._exp) return true;
            if (y._exp < x._exp) return false;
            return x._sig <= y._sig;
        }
    }

    public static bool operator >=(SlowFloat x, SlowFloat y) => y <= x;

    // floatXLt
    public static bool operator <(SlowFloat x, SlowFloat y)
    {
        if (x._isNaN || y._isNaN) return false;
        if (x._isZero && y._isZero) return false;
        if (x._sign != y._sign) return x._sign;
        if (x._isInf && y._isInf) return false;
        if (x._sign)
        {
            if (x._isInf || y._isZero) return true;
            if (y._isInf || x._isZero) return false;
            if (y._exp < x._exp) return true;
            if (x._exp < y._exp) return false;
            return y._sig < x._sig;
        }
        else
        {
            if (y._isInf || x._isZero) return true;
            if (x._isInf || y._isZero) return false;
            if (x._exp < y._exp) return true;
            if (y._exp < x._exp) return false;
            return x._sig < y._sig;
        }
    }

    public static bool operator >(SlowFloat x, SlowFloat y) => y < x;

    public static bool Equals(SlowFloatContext context, SlowFloat a, SlowFloat b, bool signaling)
    {
        if (a._isNaN || b._isNaN)
        {
            if (signaling || a._isSignaling || b._isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
        }

        return a == b;
    }

    public static bool LessThan(SlowFloatContext context, SlowFloat a, SlowFloat b, bool signaling)
    {
        if (a._isNaN || b._isNaN)
        {
            if (signaling || a._isSignaling || b._isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
        }

        return a < b;
    }

    public static bool LessThanOrEquals(SlowFloatContext context, SlowFloat a, SlowFloat b, bool signaling)
    {
        if (a._isNaN || b._isNaN)
        {
            if (signaling || a._isSignaling || b._isSignaling)
                context.ExceptionFlags |= ExceptionFlags.Invalid;
        }

        return a <= b;
    }

    #endregion

    #region Integer to Float

    // slow_ui32_to_f16
    public static Float16 ToFloat16(SlowFloatContext context, uint a) => new SlowFloat(a).ToFloat16(context);
    // slow_ui32_to_f32
    public static Float32 ToFloat32(SlowFloatContext context, uint a) => new SlowFloat(a).ToFloat32(context);
    // slow_ui32_to_f64
    public static Float64 ToFloat64(SlowFloatContext context, uint a) => new SlowFloat(a).ToFloat64(context);
    // slow_ui32_to_extF80M
    public static ExtFloat80 ToExtFloat80(SlowFloatContext context, uint a) => new SlowFloat(a).ToExtFloat80(context);
    // slow_ui32_to_f128M
    public static Float128 ToFloat128(SlowFloatContext context, uint a) => new SlowFloat(a).ToFloat128(context);

    // slow_ui64_to_f16
    public static Float16 ToFloat16(SlowFloatContext context, ulong a) => new SlowFloat(a).ToFloat16(context);
    // slow_ui64_to_f32
    public static Float32 ToFloat32(SlowFloatContext context, ulong a) => new SlowFloat(a).ToFloat32(context);
    // slow_ui64_to_f64
    public static Float64 ToFloat64(SlowFloatContext context, ulong a) => new SlowFloat(a).ToFloat64(context);
    // slow_ui64_to_extF80M
    public static ExtFloat80 ToExtFloat80(SlowFloatContext context, ulong a) => new SlowFloat(a).ToExtFloat80(context);
    // slow_ui64_to_f128M
    public static Float128 ToFloat128(SlowFloatContext context, ulong a) => new SlowFloat(a).ToFloat128(context);

    // slow_i32_to_f16
    public static Float16 ToFloat16(SlowFloatContext context, int a) => new SlowFloat(a).ToFloat16(context);
    // slow_i32_to_f32
    public static Float32 ToFloat32(SlowFloatContext context, int a) => new SlowFloat(a).ToFloat32(context);
    // slow_i32_to_f64
    public static Float64 ToFloat64(SlowFloatContext context, int a) => new SlowFloat(a).ToFloat64(context);
    // slow_i32_to_extF80M
    public static ExtFloat80 ToExtFloat80(SlowFloatContext context, int a) => new SlowFloat(a).ToExtFloat80(context);
    // slow_i32_to_f128M
    public static Float128 ToFloat128(SlowFloatContext context, int a) => new SlowFloat(a).ToFloat128(context);

    // slow_i64_to_f16
    public static Float16 ToFloat16(SlowFloatContext context, long a) => new SlowFloat(a).ToFloat16(context);
    // slow_i64_to_f32
    public static Float32 ToFloat32(SlowFloatContext context, long a) => new SlowFloat(a).ToFloat32(context);
    // slow_i64_to_f64
    public static Float64 ToFloat64(SlowFloatContext context, long a) => new SlowFloat(a).ToFloat64(context);
    // slow_i64_to_extF80M
    public static ExtFloat80 ToExtFloat80(SlowFloatContext context, long a) => new SlowFloat(a).ToExtFloat80(context);
    // slow_i64_to_f128M
    public static Float128 ToFloat128(SlowFloatContext context, long a) => new SlowFloat(a).ToFloat128(context);

    #endregion

    public override bool Equals(object? obj) => obj is SlowFloat slowFloat && Equals(slowFloat);

    public override int GetHashCode() => HashCode.Combine(_sig, _exp, _sign, _isZero, _isInf, _isNaN, _isSignaling);

    public bool Equals(SlowFloat other) => this == other;

    #endregion
}

// uint256
internal struct UInt256M : IEquatable<UInt256M>
{
    public static readonly UInt256M Zero = default;

    internal ulong _v0, _v64, _v128, _v192;

    // eq256M
    public static bool operator ==(UInt256M x, UInt256M y) => x._v0 == y._v0 && x._v64 == y._v64 && x._v128 == y._v128 && x._v192 == y._v192;

    public static bool operator !=(UInt256M x, UInt256M y) => !(x == y);

    public static UInt256M operator <<(UInt256M value, int shift)
    {
        shift &= 0xFF; // to keep it consistent with other C# operators
        for (var i = 0; i < shift; i++)
            value.ShiftLeft1();

        return value;
    }

    public static UInt256M operator >>(UInt256M value, int shift)
    {
        shift &= 0xFF; // to keep it consistent with other C# operators
        for (var i = 0; i < shift; i++)
            value.ShiftRight1();

        return value;
    }

    public static UInt256M operator +(UInt256M x, UInt256M y)
    {
        x.Add(y);
        return x;
    }

    public static UInt256M operator -(UInt256M value)
    {
        value.Negate();
        return value;
    }

    // shiftLeft1256M
    public void ShiftLeft1()
    {
        ulong dword1, dword2;

        dword1 = _v128;
        _v192 = (_v192 << 1) | (dword1 >> 63);
        dword2 = _v64;
        _v128 = (dword1 << 1) | (dword2 >> 63);
        dword1 = _v0;
        _v64 = (dword2 << 1) | (dword1 >> 63);
        _v0 = dword1 << 1;
    }

    // shiftRight1256M
    public void ShiftRight1()
    {
        ulong dword1, dword2;

        dword1 = _v64;
        _v0 = (dword1 << 63) | (_v0 >> 1);
        dword2 = _v128;
        _v64 = (dword2 << 63) | (dword1 >> 1);
        dword1 = _v192;
        _v128 = (dword1 << 63) | (dword2 >> 1);
        _v192 = dword1 >> 1;
    }

    // shiftRight1Jam256M
    public void ShiftRight1Jam()
    {
        uint extra;

        extra = (uint)_v0 & 1;
        ShiftRight1();
        _v0 |= extra;
    }

    // neg256M
    public void Negate()
    {
        ulong v64, v0, v128;

        v64 = _v64;
        v0 = _v0;
        if ((v64 | v0) != 0)
        {
            _v192 = ~_v192;
            _v128 = ~_v128;
            if (v0 != 0)
            {
                _v64 = ~v64;
                _v0 = (ulong)(-(long)v0);
            }
            else
            {
                _v64 = (ulong)(-(long)v64);
            }
        }
        else
        {
            v128 = _v128;
            if (v128 != 0)
            {
                _v192 = ~_v192;
                _v128 = (ulong)(-(long)v128);
            }
            else
            {
                _v192 = (ulong)(-(long)_v192);
            }
        }
    }

    // add256M
    public void Add(UInt256M b)
    {
        ulong dwordA, dwordZ;
        uint carry1, carry2;

        dwordA = _v0;
        dwordZ = dwordA + b._v0;
        carry1 = (dwordZ < dwordA) ? 1U : 0;
        _v0 = dwordZ;
        dwordA = _v64;
        dwordZ = dwordA + b._v64;
        carry2 = (dwordZ < dwordA) ? 1U : 0;
        dwordZ += carry1;
        carry2 += (dwordZ < carry1) ? 1U : 0;
        _v64 = dwordZ;
        dwordA = _v128;
        dwordZ = dwordA + b._v128;
        carry1 = (dwordZ < dwordA) ? 1U : 0;
        dwordZ += carry2;
        carry1 += (dwordZ < carry2) ? 1U : 0;
        _v128 = dwordZ;
        _v192 = _v192 + b._v192 + carry1;
    }

    public bool Equals(UInt256M other) => this == other;

    public override bool Equals(object? obj) => obj is UInt256M int256 && Equals(int256);

    public override int GetHashCode() => HashCode.Combine(_v0, _v64, _v128, _v192);

    public override string ToString() => $"0x{_v192:X16}_{_v128:X16}_{_v64:X16}_{_v0:X16}";
}

// floatX256
internal partial struct SlowFloat256
{
    #region Fields

    // floatX256NaN
    public static readonly SlowFloat256 NaN = new(
        isNaN: true,
        isInf: false,
        isZero: false,
        sign: false,
        exp: 0,
        sig: UInt256M.Zero,
        isSignaling: false
    );

    // floatX256PositiveZero
    public static readonly SlowFloat256 PositiveZero = new(
        isNaN: false,
        isInf: false,
        isZero: true,
        sign: false,
        exp: 0,
        sig: UInt256M.Zero,
        isSignaling: false
    );

    // floatX256NegativeZero
    public static readonly SlowFloat256 NegativeZero = new(
        isNaN: false,
        isInf: false,
        isZero: true,
        sign: true,
        exp: 0,
        sig: UInt256M.Zero,
        isSignaling: false
    );

    internal UInt256M _sig;
    internal int _exp;
    internal bool _sign;
    internal bool _isZero;
    internal bool _isInf;
    internal bool _isNaN;
    internal bool _isSignaling;

    #endregion

    #region Constructors

    internal SlowFloat256(UInt256M sig, int exp, bool sign, bool isZero, bool isInf, bool isNaN, bool isSignaling)
    {
        _sig = sig;
        _exp = exp;
        _sign = sign;
        _isZero = isZero;
        _isInf = isInf;
        _isNaN = isNaN;
        _isSignaling = isSignaling;
    }

    public SlowFloat256(SlowFloat value)
    {
        _isNaN = value._isNaN;
        _isSignaling = value._isSignaling;
        _isInf = value._isInf;
        _isZero = value._isZero;
        _sign = value._sign;
        _exp = value._exp;
        _sig._v192 = SlowFloat.V64(value._sig);
        _sig._v128 = SlowFloat.V0(value._sig);
        _sig._v64 = 0;
        _sig._v0 = 0;
    }

    // f128MToFloatX256
    public SlowFloat256(Float128 value)
        : this(new SlowFloat(value))
    {
    }

    #endregion

    #region Properties

    public UInt256M Significand => _sig;

    public int Exponent => _exp;

    public bool Sign => _sign;

    public bool IsZero => _isZero;

    public bool IsInfinity => _isInf;

    public bool IsNaN => _isNaN;

    #endregion

    #region Methods

    // floatX256ToF128M
    public Float128 ToFloat128(SlowFloatContext context)
    {
        SlowFloat x;
        int expZ;
        UInt256M sig;

        x._isNaN = _isNaN;
        x._isSignaling = _isSignaling;
        x._isInf = _isInf;
        x._isZero = _isZero;
        x._sign = _sign;
        if (!(x._isNaN | x._isInf | x._isZero))
        {
            expZ = _exp;
            sig = _sig;
            while (sig._v192 == 0)
            {
                expZ -= 64;
                sig._v192 = sig._v128;
                sig._v128 = sig._v64;
                sig._v64 = sig._v0;
                sig._v0 = 0;
            }

            while (sig._v192 < 0x0100000000000000)
            {
                --expZ;
                sig.ShiftLeft1();
            }

            x._exp = expZ;
            x._sig = new(upper: sig._v192, lower: sig._v128 | ((sig._v64 | sig._v0) != 0 ? 1U : 0));
        }
        else
        {
            x._exp = 0;
            x._sig = 0;
        }

        return x.ToFloat128(context);
    }

    // floatX256Invalid
    public void Invalid(SlowFloatContext context)
    {
        context.ExceptionFlags |= ExceptionFlags.Invalid;
        this = NaN;
    }

    // floatX256Add
    public void Add(SlowFloatContext context, SlowFloat256 y)
    {
        int expX, expY, expDiff;
        UInt256M sigY;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling || (y._isNaN && y._isSignaling))
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._isNaN)
        {
            this = y;

            // HACK: Add support for signaling NaN.
            if (y._isSignaling)
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (_isInf && y._isInf)
        {
            if (_sign != y._sign)
                Invalid(context);

            return;
        }

        if (_isInf)
            return;

        if (y._isInf)
        {
            this = y;
            return;
        }

        if (_isZero && y._isZero)
        {
            if (_sign == y._sign)
                return;

            this = context.RoundingMode == RoundingMode.Min ? NegativeZero : PositiveZero;
            return;
        }

        expX = _exp;
        expY = y._exp;
        if (_sign != y._sign && expX == expY && _sig == y._sig)
        {
            this = context.RoundingMode == RoundingMode.Min ? NegativeZero : PositiveZero;
            return;
        }

        if (_isZero)
        {
            this = y;
            return;
        }

        if (y._isZero)
            return;

        expDiff = expX - expY;
        if (expDiff < 0)
        {
            _exp = expY;
            if (expDiff < -248)
            {
                _sig._v192 = 0;
                _sig._v128 = 0;
                _sig._v64 = 0;
                _sig._v0 = 1;
            }
            else
            {
                while (expDiff < 0)
                {
                    ++expDiff;
                    _sig.ShiftRight1Jam();
                }
            }

            if (_sign != y._sign)
                _sig.Negate();

            _sign = y._sign;
            _sig.Add(y._sig);
        }
        else
        {
            sigY = y._sig;
            if (248 < expDiff)
            {
                sigY._v192 = 0;
                sigY._v128 = 0;
                sigY._v64 = 0;
                sigY._v0 = 1;
            }
            else
            {
                while (0 < expDiff)
                {
                    --expDiff;
                    sigY.ShiftRight1Jam();
                }
            }

            if (_sign != y._sign)
                sigY.Negate();

            _sig.Add(sigY);
        }

        if ((_sig._v192 & 0x8000000000000000) != 0)
        {
            _sign = !_sign;
            _sig.Negate();
        }
    }

    // floatX256Mul
    public void Multiply(SlowFloatContext context, SlowFloat256 y)
    {
        UInt256M sig;
        int bitNum;

        if (_isNaN)
        {
            // HACK: Add support for signaling NaN.
            if (_isSignaling || (y._isNaN && y._isSignaling))
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._isNaN)
        {
            _isNaN = true;
            _isInf = false;
            _isZero = false;
            _sign = y._sign;

            // HACK: Add support for signaling NaN.
            if (y._isNaN && y._isSignaling)
            {
                context.ExceptionFlags |= ExceptionFlags.Invalid;
                _isSignaling = true;
            }
            return;
        }

        if (y._sign)
            _sign = !_sign;

        if (_isInf)
        {
            if (y._isZero)
                Invalid(context);

            return;
        }

        if (y._isInf)
        {
            if (_isZero)
                Invalid(context);
            else
                _isInf = true;

            return;
        }

        if (_isZero || y._isZero)
        {
            this = _sign ? NegativeZero : PositiveZero;
            return;
        }

        _exp += y._exp;
        sig._v192 = 0;
        sig._v128 = 0;
        sig._v64 = 0;
        sig._v0 = 0;

        for (bitNum = 0; bitNum < 248; ++bitNum)
        {
            sig.ShiftRight1Jam();
            if ((_sig._v0 & 1) != 0)
                sig.Add(y._sig);

            _sig.ShiftRight1();
        }

        if (0x0100000000000000 <= sig._v192)
        {
            ++_exp;
            sig.ShiftRight1Jam();
        }
        _sig = sig;
    }

    #endregion
}
