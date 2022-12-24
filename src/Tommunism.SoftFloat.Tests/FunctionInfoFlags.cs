namespace Tommunism.SoftFloat.Tests;

internal readonly struct FunctionInfoFlags : IEquatable<FunctionInfoFlags>
{
    #region Fields

    public const uint ARG_TERNARY = 0; // implied when ARG_UNARY and ARG_BINARY flags are not set
    public const uint ARG_UNARY = 1 << 0;
    public const uint ARG_BINARY = 1 << 1;
    public const uint ARG_ROUNDINGMODE = 1 << 2;
    public const uint ARG_EXACT = 1 << 3;
    public const uint EFF_ROUNDINGPRECISION = 1 << 4;
    public const uint EFF_ROUNDINGMODE = 1 << 5;
    public const uint EFF_TININESSMODE = 1 << 6;
    public const uint EFF_TININESSMODE_REDUCEDPREC = 1 << 7;
    public const uint SOFTFLOAT = 1 << 8; // extension, for testing SoftFloat-specific functions (not supported by "testfloat_gen")

    // Extensions for identifying float-specific functions.
    public const uint F16 = 1 << 9;
    public const uint F32 = 1 << 10;
    public const uint F64 = 1 << 11;
    public const uint EXTF80 = 1 << 12;
    public const uint F128 = 1 << 13;

    // Extensions for identifying integer-specific functions.
    public const uint UI32 = 1 << 14;
    public const uint UI64 = 1 << 15;
    public const uint I32 = 1 << 16;
    public const uint I64 = 1 << 17;

    public const uint ALL_FLAGS = ARG_UNARY | ARG_BINARY | ARG_ROUNDINGMODE | ARG_EXACT |
        EFF_ROUNDINGPRECISION | EFF_ROUNDINGMODE | EFF_TININESSMODE | EFF_TININESSMODE_REDUCEDPREC |
        SOFTFLOAT;

    private readonly uint _flags;

    #endregion

    #region Constructors

    public FunctionInfoFlags(uint value) => _flags = value;

    #endregion

    #region Properties

    public uint Flags => _flags;

    public int ArgumentCount => (_flags & (ARG_UNARY | ARG_BINARY)) switch
    {
        ARG_UNARY => 1,
        ARG_BINARY => 2,
        ARG_TERNARY => 3,
        _ => throw new InvalidOperationException("Invalid arguments flags.")
    };

    public bool HasArgumentRoundingMode => (_flags & ARG_ROUNDINGMODE) != 0;

    public bool HasArgumentExact => (_flags & ARG_EXACT) != 0;

    public bool AffectedByRoundingPrecision => (_flags & EFF_ROUNDINGPRECISION) != 0;

    public bool AffectedByRoundingMode => (_flags & EFF_ROUNDINGMODE) != 0;

    public bool AffectedByTininessMode => (_flags & EFF_TININESSMODE) != 0;

    public bool AffectedByTininessWithReducedPrecision => (_flags & EFF_TININESSMODE_REDUCEDPREC) != 0;

    public bool TestSoftFloatOnly => (_flags & SOFTFLOAT) != 0;

    public bool IsUInt32 => (_flags & UI32) != 0;

    public bool IsUInt64 => (_flags & UI64) != 0;

    public bool IsInt32 => (_flags & I32) != 0;

    public bool IsInt64 => (_flags & I64) != 0;

    public bool IsFloat16 => (_flags & F16) != 0;

    public bool IsFloat32 => (_flags & F32) != 0;

    public bool IsFloat64 => (_flags & F64) != 0;

    public bool IsExtFloat80 => (_flags & EXTF80) != 0;

    public bool IsFloat128 => (_flags & F128) != 0;

    #endregion

    #region Methods

    public static implicit operator FunctionInfoFlags(uint value) => new(value);

    public static bool operator ==(FunctionInfoFlags left, FunctionInfoFlags right) => left.Equals(right);

    public static bool operator !=(FunctionInfoFlags left, FunctionInfoFlags right) => !(left == right);

    public override bool Equals(object? obj) => obj is FunctionInfoFlags flags && Equals(flags);

    public bool Equals(FunctionInfoFlags other) => _flags == other._flags;

    public override int GetHashCode() => _flags.GetHashCode();

    public override string ToString()
    {
        // The common cases could probably be embedded, but this is mostly for debugging purposes anyways.
        var builder = new ValueStringBuilder(stackalloc char[128]);

        builder.Append((_flags & (ARG_UNARY | ARG_BINARY)) switch
        {
            ARG_UNARY => "ARG_UNARY",
            ARG_BINARY => "ARG_BINARY",
            ARG_TERNARY => "ARG_TERNARY",
            _ => "ARG_UNKNOWN_OPERANDS"
        });

        if ((_flags & ARG_ROUNDINGMODE) != 0)
            builder.Append(" | ARG_ROUNDINGMODE");

        if ((_flags & ARG_EXACT) != 0)
            builder.Append(" | ARG_EXACT");

        if ((_flags & EFF_ROUNDINGPRECISION) != 0)
            builder.Append(" | EFF_ROUNDINGPRECISION");

        if ((_flags & EFF_ROUNDINGMODE) != 0)
            builder.Append(" | EFF_ROUNDINGMODE");

        if ((_flags & EFF_TININESSMODE) != 0)
            builder.Append(" | EFF_TININESSMODE");

        if ((_flags & EFF_TININESSMODE_REDUCEDPREC) != 0)
            builder.Append(" | EFF_TININESSMODE_REDUCEDPREC");

        if ((_flags & SOFTFLOAT) != 0)
            builder.Append(" | SOFTFLOAT");

        if ((_flags & UI32) != 0)
            builder.Append(" | UI32");

        if ((_flags & UI64) != 0)
            builder.Append(" | UI64");

        if ((_flags & I32) != 0)
            builder.Append(" | I32");

        if ((_flags & I64) != 0)
            builder.Append(" | I64");

        if ((_flags & F16) != 0)
            builder.Append(" | F16");

        if ((_flags & F32) != 0)
            builder.Append(" | F32");

        if ((_flags & F64) != 0)
            builder.Append(" | F64");

        if ((_flags & EXTF80) != 0)
            builder.Append(" | EXTF80");

        if ((_flags & F128) != 0)
            builder.Append(" | F128");

        // Anything other bits are considered "unimplemented" or "invalid". (This should be extremely rare and ideally never occur, so the
        // performance penalty of string interpolation shouldn't matter much.)
        if ((_flags & ~ALL_FLAGS) != 0)
            builder.Append($" | {_flags & ~ALL_FLAGS}");

        return builder.ToString();
    }

    #endregion
}
