﻿namespace Tommunism.SoftFloat.Tests;

using static FunctionInfoFlags;

internal record FunctionRewriteInfo(string Function, ExtFloat80RoundingPrecision? Precision = null, RoundingMode? Rounding = null, TininessMode? Tininess = null, bool? Exact = null);

internal static class FunctionInfo
{
    // NOTE: If ARG_1 or ARG_2 bits are not set, then three operands is implied (e.g., *_mulAdd are three operand functions).
    public const uint ARG_1 = ARG_UNARY;
    public const uint ARG_2 = ARG_BINARY;
    public const uint ARG_3 = ARG_TERNARY;
    public const uint ARG_R = ARG_ROUNDINGMODE;
    public const uint ARG_E = ARG_EXACT;
    public const uint EFF_P = EFF_ROUNDINGPRECISION; // only used by ExtFloat80
    public const uint EFF_R = EFF_ROUNDINGMODE;
    public const uint EFF_T = EFF_TININESSMODE;
    public const uint EFF_T_REDP = EFF_TININESSMODE_REDUCEDPREC; // only used by ExtFloat80
    public const uint F80 = EXTF80;

    public static IReadOnlyDictionary<string, FunctionInfoFlags> Functions { get; } = new Dictionary<string, FunctionInfoFlags>()
    {
        { "ui32_to_f16",    UI32 | F16  | ARG_1 | EFF_R },
        { "ui32_to_f32",    UI32 | F32  | ARG_1 | EFF_R },
        { "ui32_to_f64",    UI32 | F64  | ARG_1         },
        { "ui32_to_extF80", UI32 | F80  | ARG_1         },
        { "ui32_to_f128",   UI32 | F128 | ARG_1         },
        { "ui64_to_f16",    UI64 | F16  | ARG_1 | EFF_R },
        { "ui64_to_f32",    UI64 | F32  | ARG_1 | EFF_R },
        { "ui64_to_f64",    UI64 | F64  | ARG_1 | EFF_R },
        { "ui64_to_extF80", UI64 | F80  | ARG_1         },
        { "ui64_to_f128",   UI64 | F128 | ARG_1         },
        { "i32_to_f16",     I32  | F16  | ARG_1 | EFF_R },
        { "i32_to_f32",     I32  | F32  | ARG_1 | EFF_R },
        { "i32_to_f64",     I32  | F64  | ARG_1         },
        { "i32_to_extF80",  I32  | F80  | ARG_1         },
        { "i32_to_f128",    I32  | F128 | ARG_1         },
        { "i64_to_f16",     I64  | F16  | ARG_1 | EFF_R },
        { "i64_to_f32",     I64  | F32  | ARG_1 | EFF_R },
        { "i64_to_f64",     I64  | F64  | ARG_1 | EFF_R },
        { "i64_to_extF80",  I64  | F80  | ARG_1         },
        { "i64_to_f128",    I64  | F128 | ARG_1         },

        { "f16_to_ui32",          F16 | UI32 | ARG_1 | ARG_R | ARG_E },
        { "f16_to_ui64",          F16 | UI64 | ARG_1 | ARG_R | ARG_E },
        { "f16_to_i32",           F16 | I32  | ARG_1 | ARG_R | ARG_E },
        { "f16_to_i64",           F16 | I64  | ARG_1 | ARG_R | ARG_E },
        { "f16_to_ui32_r_minMag", F16 | UI32 | ARG_1 | ARG_E | SOFTFLOAT },
        { "f16_to_ui64_r_minMag", F16 | UI64 | ARG_1 | ARG_E | SOFTFLOAT },
        { "f16_to_i32_r_minMag",  F16 | I32  | ARG_1 | ARG_E | SOFTFLOAT },
        { "f16_to_i64_r_minMag",  F16 | I64  | ARG_1 | ARG_E | SOFTFLOAT },
        { "f16_to_f32",           F16 | F32  | ARG_1 },
        { "f16_to_f64",           F16 | F64  | ARG_1 },
        { "f16_to_extF80",        F16 | F80  | ARG_1 },
        { "f16_to_f128",          F16 | F128 | ARG_1 },
        { "f16_roundToInt",       F16 | ARG_1 | ARG_R | ARG_E },
        { "f16_add",              F16 | ARG_2 | EFF_R         },
        { "f16_sub",              F16 | ARG_2 | EFF_R         },
        { "f16_mul",              F16 | ARG_2 | EFF_R | EFF_T },
        { "f16_mulAdd",           F16 | ARG_3 | EFF_R | EFF_T },
        { "f16_div",              F16 | ARG_2 | EFF_R         },
        { "f16_rem",              F16 | ARG_2                 },
        { "f16_sqrt",             F16 | ARG_1 | EFF_R         },
        { "f16_eq",               F16 | ARG_2                 },
        { "f16_le",               F16 | ARG_2                 },
        { "f16_lt",               F16 | ARG_2                 },
        { "f16_eq_signaling",     F16 | ARG_2                 },
        { "f16_le_quiet",         F16 | ARG_2                 },
        { "f16_lt_quiet",         F16 | ARG_2                 },

        { "f32_to_ui32",          F32 | UI32 | ARG_1 | ARG_R | ARG_E },
        { "f32_to_ui64",          F32 | UI64 | ARG_1 | ARG_R | ARG_E },
        { "f32_to_i32",           F32 | I32  | ARG_1 | ARG_R | ARG_E },
        { "f32_to_i64",           F32 | I64  | ARG_1 | ARG_R | ARG_E },
        { "f32_to_ui32_r_minMag", F32 | UI32 | ARG_1 | ARG_E | SOFTFLOAT },
        { "f32_to_ui64_r_minMag", F32 | UI64 | ARG_1 | ARG_E | SOFTFLOAT },
        { "f32_to_i32_r_minMag",  F32 | I32  | ARG_1 | ARG_E | SOFTFLOAT },
        { "f32_to_i64_r_minMag",  F32 | I64  | ARG_1 | ARG_E | SOFTFLOAT },
        { "f32_to_f16",           F32 | F16  | ARG_1 | EFF_R | EFF_T },
        { "f32_to_f64",           F32 | F64  | ARG_1 },
        { "f32_to_extF80",        F32 | F80  | ARG_1 },
        { "f32_to_f128",          F32 | F128 | ARG_1 },
        { "f32_roundToInt",       F32 | ARG_1 | ARG_R | ARG_E },
        { "f32_add",              F32 | ARG_2 | EFF_R         },
        { "f32_sub",              F32 | ARG_2 | EFF_R         },
        { "f32_mul",              F32 | ARG_2 | EFF_R | EFF_T },
        { "f32_mulAdd",           F32 | ARG_3 | EFF_R | EFF_T },
        { "f32_div",              F32 | ARG_2 | EFF_R         },
        { "f32_rem",              F32 | ARG_2                 },
        { "f32_sqrt",             F32 | ARG_1 | EFF_R         },
        { "f32_eq",               F32 | ARG_2                 },
        { "f32_le",               F32 | ARG_2                 },
        { "f32_lt",               F32 | ARG_2                 },
        { "f32_eq_signaling",     F32 | ARG_2                 },
        { "f32_le_quiet",         F32 | ARG_2                 },
        { "f32_lt_quiet",         F32 | ARG_2                 },

        { "f64_to_ui32",          F64 | UI32 | ARG_1 | ARG_R | ARG_E },
        { "f64_to_ui64",          F64 | UI64 | ARG_1 | ARG_R | ARG_E },
        { "f64_to_i32",           F64 | I32  | ARG_1 | ARG_R | ARG_E },
        { "f64_to_i64",           F64 | I64  | ARG_1 | ARG_R | ARG_E },
        { "f64_to_ui32_r_minMag", F64 | UI32 | ARG_1 | ARG_E | SOFTFLOAT },
        { "f64_to_ui64_r_minMag", F64 | UI64 | ARG_1 | ARG_E | SOFTFLOAT },
        { "f64_to_i32_r_minMag",  F64 | I32  | ARG_1 | ARG_E | SOFTFLOAT },
        { "f64_to_i64_r_minMag",  F64 | I64  | ARG_1 | ARG_E | SOFTFLOAT },
        { "f64_to_f16",           F64 | F16  | ARG_1 | EFF_R | EFF_T },
        { "f64_to_f32",           F64 | F32  | ARG_1 | EFF_R | EFF_T },
        { "f64_to_extF80",        F64 | F80  | ARG_1 },
        { "f64_to_f128",          F64 | F128 | ARG_1 },
        { "f64_roundToInt",       F64 | ARG_1 | ARG_R | ARG_E },
        { "f64_add",              F64 | ARG_2 | EFF_R         },
        { "f64_sub",              F64 | ARG_2 | EFF_R         },
        { "f64_mul",              F64 | ARG_2 | EFF_R | EFF_T },
        { "f64_mulAdd",           F64 | ARG_3 | EFF_R | EFF_T },
        { "f64_div",              F64 | ARG_2 | EFF_R         },
        { "f64_rem",              F64 | ARG_2                 },
        { "f64_sqrt",             F64 | ARG_1 | EFF_R         },
        { "f64_eq",               F64 | ARG_2                 },
        { "f64_le",               F64 | ARG_2                 },
        { "f64_lt",               F64 | ARG_2                 },
        { "f64_eq_signaling",     F64 | ARG_2                 },
        { "f64_le_quiet",         F64 | ARG_2                 },
        { "f64_lt_quiet",         F64 | ARG_2                 },

        { "extF80_to_ui32",          F80 | UI32 | ARG_1 | ARG_R | ARG_E },
        { "extF80_to_ui64",          F80 | UI64 | ARG_1 | ARG_R | ARG_E },
        { "extF80_to_i32",           F80 | I32  | ARG_1 | ARG_R | ARG_E },
        { "extF80_to_i64",           F80 | I64  | ARG_1 | ARG_R | ARG_E },
        { "extF80_to_ui32_r_minMag", F80 | UI32 | ARG_1 | ARG_E | SOFTFLOAT },
        { "extF80_to_ui64_r_minMag", F80 | UI64 | ARG_1 | ARG_E | SOFTFLOAT },
        { "extF80_to_i32_r_minMag",  F80 | I32  | ARG_1 | ARG_E | SOFTFLOAT },
        { "extF80_to_i64_r_minMag",  F80 | I64  | ARG_1 | ARG_E | SOFTFLOAT },
        { "extF80_to_f16",           F80 | F16  | ARG_1 | EFF_R | EFF_T },
        { "extF80_to_f32",           F80 | F32  | ARG_1 | EFF_R | EFF_T },
        { "extF80_to_f64",           F80 | F64  | ARG_1 | EFF_R | EFF_T },
        { "extF80_to_f128",          F80 | F128 | ARG_1 },
        { "extF80_roundToInt",       F80 | ARG_1 | ARG_R | ARG_E },
        { "extF80_add",              F80 | ARG_2 | EFF_P | EFF_R         | EFF_T_REDP },
        { "extF80_sub",              F80 | ARG_2 | EFF_P | EFF_R         | EFF_T_REDP },
        { "extF80_mul",              F80 | ARG_2 | EFF_P | EFF_R | EFF_T | EFF_T_REDP },
        { "extF80_div",              F80 | ARG_2 | EFF_P | EFF_R         | EFF_T_REDP },
        { "extF80_rem",              F80 | ARG_2                                      },
        { "extF80_sqrt",             F80 | ARG_1 | EFF_P | EFF_R                      },
        { "extF80_eq",               F80 | ARG_2                                      },
        { "extF80_le",               F80 | ARG_2                                      },
        { "extF80_lt",               F80 | ARG_2                                      },
        { "extF80_eq_signaling",     F80 | ARG_2                                      },
        { "extF80_le_quiet",         F80 | ARG_2                                      },
        { "extF80_lt_quiet",         F80 | ARG_2                                      },

        { "f128_to_ui32",          F128 | UI32 | ARG_1 | ARG_R | ARG_E },
        { "f128_to_ui64",          F128 | UI64 | ARG_1 | ARG_R | ARG_E },
        { "f128_to_i32",           F128 | I32  | ARG_1 | ARG_R | ARG_E },
        { "f128_to_i64",           F128 | I64  | ARG_1 | ARG_R | ARG_E },
        { "f128_to_ui32_r_minMag", F128 | UI32 | ARG_1 | ARG_E | SOFTFLOAT },
        { "f128_to_ui64_r_minMag", F128 | UI64 | ARG_1 | ARG_E | SOFTFLOAT },
        { "f128_to_i32_r_minMag",  F128 | I32  | ARG_1 | ARG_E | SOFTFLOAT },
        { "f128_to_i64_r_minMag",  F128 | I64  | ARG_1 | ARG_E | SOFTFLOAT },
        { "f128_to_f16",           F128 | F16 | ARG_1 | EFF_R | EFF_T },
        { "f128_to_f32",           F128 | F32 | ARG_1 | EFF_R | EFF_T },
        { "f128_to_f64",           F128 | F64 | ARG_1 | EFF_R | EFF_T },
        { "f128_to_extF80",        F128 | F80 | ARG_1 | EFF_R | EFF_T },
        { "f128_roundToInt",       F128 | ARG_1 | ARG_R | ARG_E },
        { "f128_add",              F128 | ARG_2 | EFF_R         },
        { "f128_sub",              F128 | ARG_2 | EFF_R         },
        { "f128_mul",              F128 | ARG_2 | EFF_R | EFF_T },
        { "f128_mulAdd",           F128 | ARG_3 | EFF_R | EFF_T },
        { "f128_div",              F128 | ARG_2 | EFF_R         },
        { "f128_rem",              F128 | ARG_2                 },
        { "f128_sqrt",             F128 | ARG_1 | EFF_R         },
        { "f128_eq",               F128 | ARG_2                 },
        { "f128_le",               F128 | ARG_2                 },
        { "f128_lt",               F128 | ARG_2                 },
        { "f128_eq_signaling",     F128 | ARG_2                 },
        { "f128_le_quiet",         F128 | ARG_2                 },
        { "f128_lt_quiet",         F128 | ARG_2                 },
    };

    // This is a map of test functions that can be generated & verified using other test functions (with specific options set). Entries in
    // here must also be defined in the main functions dictionary and should have entries in the generator types. These are generally
    // intended for testing optimized variations of existing functions (e.g., the float-to-int functions with fixed rounding modes).
    public static IReadOnlyDictionary<string, FunctionRewriteInfo> RewriteFunctions { get; } = new Dictionary<string, FunctionRewriteInfo>()
    {
        { "f16_to_ui32_r_minMag", new("f16_to_ui32", Rounding: RoundingMode.MinMag) },
        { "f16_to_ui64_r_minMag", new("f16_to_ui64", Rounding: RoundingMode.MinMag) },
        { "f16_to_i32_r_minMag",  new("f16_to_i32", Rounding: RoundingMode.MinMag) },
        { "f16_to_i64_r_minMag",  new("f16_to_i64", Rounding: RoundingMode.MinMag) },

        { "f32_to_ui32_r_minMag", new("f32_to_ui32", Rounding: RoundingMode.MinMag) },
        { "f32_to_ui64_r_minMag", new("f32_to_ui64", Rounding: RoundingMode.MinMag) },
        { "f32_to_i32_r_minMag",  new("f32_to_i32", Rounding: RoundingMode.MinMag) },
        { "f32_to_i64_r_minMag",  new("f32_to_i64", Rounding: RoundingMode.MinMag) },

        { "f64_to_ui32_r_minMag", new("f64_to_ui32", Rounding: RoundingMode.MinMag) },
        { "f64_to_ui64_r_minMag", new("f64_to_ui64", Rounding: RoundingMode.MinMag) },
        { "f64_to_i32_r_minMag",  new("f64_to_i32", Rounding: RoundingMode.MinMag) },
        { "f64_to_i64_r_minMag",  new("f64_to_i64", Rounding: RoundingMode.MinMag) },

        { "extF80_to_ui32_r_minMag", new("extF80_to_ui32", Rounding: RoundingMode.MinMag) },
        { "extF80_to_ui64_r_minMag", new("extF80_to_ui64", Rounding: RoundingMode.MinMag) },
        { "extF80_to_i32_r_minMag",  new("extF80_to_i32", Rounding: RoundingMode.MinMag) },
        { "extF80_to_i64_r_minMag",  new("extF80_to_i64", Rounding: RoundingMode.MinMag) },

        { "f128_to_ui32_r_minMag", new("f128_to_ui32", Rounding: RoundingMode.MinMag) },
        { "f128_to_ui64_r_minMag", new("f128_to_ui64", Rounding: RoundingMode.MinMag) },
        { "f128_to_i32_r_minMag",  new("f128_to_i32", Rounding: RoundingMode.MinMag) },
        { "f128_to_i64_r_minMag",  new("f128_to_i64", Rounding: RoundingMode.MinMag) },
    };

    // This is a map of all normal functions to equivalent generator types (and required number of operands). Using these should result in
    // much faster test generation, because the results do not need to be computed. Note that integer types always have an operand count of
    // one, but the generator does not allow the count to be specified as an argument (and thus the value is always zero).
    public static IReadOnlyDictionary<string, (string TypeName, int ArgCount)> GeneratorTypes { get; } = new Dictionary<string, (string TypeName, int ArgCount)>()
    {
        { "ui32_to_f16",            ("ui32", 0) },
        { "ui32_to_f32",            ("ui32", 0) },
        { "ui32_to_f64",            ("ui32", 0) },
        { "ui32_to_extF80",         ("ui32", 0) },
        { "ui32_to_f128",           ("ui32", 0) },
        { "ui64_to_f16",            ("ui64", 0) },
        { "ui64_to_f32",            ("ui64", 0) },
        { "ui64_to_f64",            ("ui64", 0) },
        { "ui64_to_extF80",         ("ui64", 0) },
        { "ui64_to_f128",           ("ui64", 0) },
        { "i32_to_f16",             ("i32", 0) },
        { "i32_to_f32",             ("i32", 0) },
        { "i32_to_f64",             ("i32", 0) },
        { "i32_to_extF80",          ("i32", 0) },
        { "i32_to_f128",            ("i32", 0) },
        { "i64_to_f16",             ("i64", 0) },
        { "i64_to_f32",             ("i64", 0) },
        { "i64_to_f64",             ("i64", 0) },
        { "i64_to_extF80",          ("i64", 0) },
        { "i64_to_f128",            ("i64", 0) },

        { "f16_to_ui32",            ("f16", 1) },
        { "f16_to_ui64",            ("f16", 1) },
        { "f16_to_i32",             ("f16", 1) },
        { "f16_to_i64",             ("f16", 1) },
        { "f16_to_ui32_r_minMag",   ("f16", 1) },
        { "f16_to_ui64_r_minMag",   ("f16", 1) },
        { "f16_to_i32_r_minMag",    ("f16", 1) },
        { "f16_to_i64_r_minMag",    ("f16", 1) },
        { "f16_to_f32",             ("f16", 1) },
        { "f16_to_f64",             ("f16", 1) },
        { "f16_to_extF80",          ("f16", 1) },
        { "f16_to_f128",            ("f16", 1) },
        { "f16_roundToInt",         ("f16", 1) },
        { "f16_add",                ("f16", 2) },
        { "f16_sub",                ("f16", 2) },
        { "f16_mul",                ("f16", 2) },
        { "f16_mulAdd",             ("f16", 3) },
        { "f16_div",                ("f16", 2) },
        { "f16_rem",                ("f16", 2) },
        { "f16_sqrt",               ("f16", 1) },
        { "f16_eq",                 ("f16", 2) },
        { "f16_le",                 ("f16", 2) },
        { "f16_lt",                 ("f16", 2) },
        { "f16_eq_signaling",       ("f16", 2) },
        { "f16_le_quiet",           ("f16", 2) },
        { "f16_lt_quiet",           ("f16", 2) },

        { "f32_to_ui32",            ("f32", 1) },
        { "f32_to_ui64",            ("f32", 1) },
        { "f32_to_i32",             ("f32", 1) },
        { "f32_to_i64",             ("f32", 1) },
        { "f32_to_ui32_r_minMag",   ("f32", 1) },
        { "f32_to_ui64_r_minMag",   ("f32", 1) },
        { "f32_to_i32_r_minMag",    ("f32", 1) },
        { "f32_to_i64_r_minMag",    ("f32", 1) },
        { "f32_to_f16",             ("f32", 1) },
        { "f32_to_f64",             ("f32", 1) },
        { "f32_to_extF80",          ("f32", 1) },
        { "f32_to_f128",            ("f32", 1) },
        { "f32_roundToInt",         ("f32", 1) },
        { "f32_add",                ("f32", 2) },
        { "f32_sub",                ("f32", 2) },
        { "f32_mul",                ("f32", 2) },
        { "f32_mulAdd",             ("f32", 3) },
        { "f32_div",                ("f32", 2) },
        { "f32_rem",                ("f32", 2) },
        { "f32_sqrt",               ("f32", 1) },
        { "f32_eq",                 ("f32", 2) },
        { "f32_le",                 ("f32", 2) },
        { "f32_lt",                 ("f32", 2) },
        { "f32_eq_signaling",       ("f32", 2) },
        { "f32_le_quiet",           ("f32", 2) },
        { "f32_lt_quiet",           ("f32", 2) },

        { "f64_to_ui32",            ("f64", 1) },
        { "f64_to_ui64",            ("f64", 1) },
        { "f64_to_i32",             ("f64", 1) },
        { "f64_to_i64",             ("f64", 1) },
        { "f64_to_ui32_r_minMag",   ("f64", 1) },
        { "f64_to_ui64_r_minMag",   ("f64", 1) },
        { "f64_to_i32_r_minMag",    ("f64", 1) },
        { "f64_to_i64_r_minMag",    ("f64", 1) },
        { "f64_to_f16",             ("f64", 1) },
        { "f64_to_f32",             ("f64", 1) },
        { "f64_to_extF80",          ("f64", 1) },
        { "f64_to_f128",            ("f64", 1) },
        { "f64_roundToInt",         ("f64", 1) },
        { "f64_add",                ("f64", 2) },
        { "f64_sub",                ("f64", 2) },
        { "f64_mul",                ("f64", 2) },
        { "f64_mulAdd",             ("f64", 3) },
        { "f64_div",                ("f64", 2) },
        { "f64_rem",                ("f64", 2) },
        { "f64_sqrt",               ("f64", 1) },
        { "f64_eq",                 ("f64", 2) },
        { "f64_le",                 ("f64", 2) },
        { "f64_lt",                 ("f64", 2) },
        { "f64_eq_signaling",       ("f64", 2) },
        { "f64_le_quiet",           ("f64", 2) },
        { "f64_lt_quiet",           ("f64", 2) },

        { "extF80_to_ui32",         ("extF80", 1) },
        { "extF80_to_ui64",         ("extF80", 1) },
        { "extF80_to_i32",          ("extF80", 1) },
        { "extF80_to_i64",          ("extF80", 1) },
        { "extF80_to_ui32_r_minMag",("extF80", 1) },
        { "extF80_to_ui64_r_minMag",("extF80", 1) },
        { "extF80_to_i32_r_minMag", ("extF80", 1) },
        { "extF80_to_i64_r_minMag", ("extF80", 1) },
        { "extF80_to_f16",          ("extF80", 1) },
        { "extF80_to_f32",          ("extF80", 1) },
        { "extF80_to_f64",          ("extF80", 1) },
        { "extF80_to_f128",         ("extF80", 1) },
        { "extF80_roundToInt",      ("extF80", 1) },
        { "extF80_add",             ("extF80", 2) },
        { "extF80_sub",             ("extF80", 2) },
        { "extF80_mul",             ("extF80", 2) },
        { "extF80_div",             ("extF80", 2) },
        { "extF80_rem",             ("extF80", 2) },
        { "extF80_sqrt",            ("extF80", 1) },
        { "extF80_eq",              ("extF80", 2) },
        { "extF80_le",              ("extF80", 2) },
        { "extF80_lt",              ("extF80", 2) },
        { "extF80_eq_signaling",    ("extF80", 2) },
        { "extF80_le_quiet",        ("extF80", 2) },
        { "extF80_lt_quiet",        ("extF80", 2) },

        { "f128_to_ui32",           ("f128", 1) },
        { "f128_to_ui64",           ("f128", 1) },
        { "f128_to_i32",            ("f128", 1) },
        { "f128_to_i64",            ("f128", 1) },
        { "f128_to_ui32_r_minMag",  ("f128", 1) },
        { "f128_to_ui64_r_minMag",  ("f128", 1) },
        { "f128_to_i32_r_minMag",   ("f128", 1) },
        { "f128_to_i64_r_minMag",   ("f128", 1) },
        { "f128_to_f16",            ("f128", 1) },
        { "f128_to_f32",            ("f128", 1) },
        { "f128_to_f64",            ("f128", 1) },
        { "f128_to_extF80",         ("f128", 1) },
        { "f128_roundToInt",        ("f128", 1) },
        { "f128_add",               ("f128", 2) },
        { "f128_sub",               ("f128", 2) },
        { "f128_mul",               ("f128", 2) },
        { "f128_mulAdd",            ("f128", 3) },
        { "f128_div",               ("f128", 2) },
        { "f128_rem",               ("f128", 2) },
        { "f128_sqrt",              ("f128", 1) },
        { "f128_eq",                ("f128", 2) },
        { "f128_le",                ("f128", 2) },
        { "f128_lt",                ("f128", 2) },
        { "f128_eq_signaling",      ("f128", 2) },
        { "f128_le_quiet",          ("f128", 2) },
        { "f128_lt_quiet",          ("f128", 2) },
    };
}
