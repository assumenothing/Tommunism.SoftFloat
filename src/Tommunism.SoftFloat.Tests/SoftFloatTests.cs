namespace Tommunism.SoftFloat.Tests;

internal static class SoftFloatTests
{
    // NOTE: If function in the dictionary is null, then it is not currently implemented and should be skipped.

    public static readonly Dictionary<string, Func<TestRunnerState, TestRunnerArguments, TestRunnerResult>?> Functions = new()
    {
        { "ui32_to_f16", TestIntegerToFloat },      // level 2 "all" passes (strict)
        { "ui32_to_f32", TestIntegerToFloat },      // level 2 "all" passes (strict)
        { "ui32_to_f64", TestIntegerToFloat },      // level 2 "all" passes (strict)
        { "ui32_to_extF80", TestIntegerToFloat },   // level 2 "all" passes (strict)
        { "ui32_to_f128", TestIntegerToFloat },     // level 2 "all" passes (strict)
        { "ui64_to_f16", TestIntegerToFloat },      // level 2 "all" passes (strict)
        { "ui64_to_f32", TestIntegerToFloat },      // level 2 "all" passes (strict)
        { "ui64_to_f64", TestIntegerToFloat },      // level 2 "all" passes (strict)
        { "ui64_to_extF80", TestIntegerToFloat },   // level 2 "all" passes (strict)
        { "ui64_to_f128", TestIntegerToFloat },     // level 2 "all" passes (strict)
        { "i32_to_f16", TestIntegerToFloat },       // level 2 "all" passes (strict)
        { "i32_to_f32", TestIntegerToFloat },       // level 2 "all" passes (strict)
        { "i32_to_f64", TestIntegerToFloat },       // level 2 "all" passes (strict)
        { "i32_to_extF80", TestIntegerToFloat },    // level 2 "all" passes (strict)
        { "i32_to_f128", TestIntegerToFloat },      // level 2 "all" passes (strict)
        { "i64_to_f16", TestIntegerToFloat },       // level 2 "all" passes (strict)
        { "i64_to_f32", TestIntegerToFloat },       // level 2 "all" passes (strict)
        { "i64_to_f64", TestIntegerToFloat },       // level 2 "all" passes (strict)
        { "i64_to_extF80", TestIntegerToFloat },    // level 2 "all" passes (strict)
        { "i64_to_f128", TestIntegerToFloat },      // level 2 "all" passes (strict)

        { "f16_to_ui32", TestFloatToInteger },      // level 2 "all" passes (strict)
        { "f16_to_ui64", TestFloatToInteger },      // level 2 "all" passes (strict)
        { "f16_to_i32", TestFloatToInteger },       // level 2 "all" passes (strict)
        { "f16_to_i64", TestFloatToInteger },       // level 2 "all" passes (strict)
        { "f16_to_ui32_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "f16_to_ui64_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "f16_to_i32_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "f16_to_i64_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "f16_to_f32", TestFloatToFloat },         // level 2 "all" passes (strict)
        { "f16_to_f64", TestFloatToFloat },         // level 2 "all" passes (strict)
        { "f16_to_extF80", TestFloatToFloat },      // level 2 "all" passes (strict)
        { "f16_to_f128", TestFloatToFloat },        // level 2 "all" passes (strict)
        { "f16_roundToInt", TestRoundToInt },       // level 2 "all" passes (strict)
        { "f16_add", TestAdd },                     // level 2 "all" passes (strict)
        { "f16_sub", TestSubtract },                // level 2 "all" passes (strict)
        { "f16_mul", TestMultiply },                // level 2 "all" passes (strict)
        { "f16_mulAdd", TestMultiplyAndAdd },       // level 1 "all" passes (strict)
        { "f16_div", TestDivide },                  // level 2 "all" passes (strict)
        { "f16_rem", TestModulus },                 // level 2 "all" passes (strict)
        { "f16_sqrt", TestSquareRoot },             // level 2 "all" passes (strict)
        { "f16_eq", TestEquals },                   // level 2 "all" passes (strict)
        { "f16_le", TestLessThanOrEquals },         // level 2 "all" passes (strict)
        { "f16_lt", TestLessThan },                 // level 2 "all" passes (strict)
        { "f16_eq_signaling", TestEquals },         // level 2 "all" passes (strict)
        { "f16_le_quiet", TestLessThanOrEquals },   // level 2 "all" passes (strict)
        { "f16_lt_quiet", TestLessThan },           // level 2 "all" passes (strict)

        { "f32_to_ui32", TestFloatToInteger },      // level 2 "all" passes (strict)
        { "f32_to_ui64", TestFloatToInteger },      // level 2 "all" passes (strict)
        { "f32_to_i32", TestFloatToInteger },       // level 2 "all" passes (strict)
        { "f32_to_i64", TestFloatToInteger },       // level 2 "all" passes (strict)
        { "f32_to_ui32_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "f32_to_ui64_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "f32_to_i32_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "f32_to_i64_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "f32_to_f16", TestFloatToFloat },         // level 2 "all" passes (strict)
        { "f32_to_f64", TestFloatToFloat },         // level 2 "all" passes (strict)
        { "f32_to_extF80", TestFloatToFloat },      // level 2 "all" passes (strict)
        { "f32_to_f128", TestFloatToFloat },        // level 2 "all" passes (strict)
        { "f32_roundToInt", TestRoundToInt },       // level 2 "all" passes (strict)
        { "f32_add", TestAdd },                     // level 2 "all" passes (strict)
        { "f32_sub", TestSubtract },                // level 2 "all" passes (strict)
        { "f32_mul", TestMultiply },                // level 2 "all" passes (strict)
        { "f32_mulAdd", TestMultiplyAndAdd },       // level 1 "all" passes (strict)
        { "f32_div", TestDivide },                  // level 2 "all" passes (strict)
        { "f32_rem", TestModulus },                 // level 2 "all" passes (strict)
        { "f32_sqrt", TestSquareRoot },             // level 2 "all" passes (strict)
        { "f32_eq", TestEquals },                   // level 2 "all" passes (strict)
        { "f32_le", TestLessThanOrEquals },         // level 2 "all" passes (strict)
        { "f32_lt", TestLessThan },                 // level 2 "all" passes (strict)
        { "f32_eq_signaling", TestEquals },         // level 2 "all" passes (strict)
        { "f32_le_quiet", TestLessThanOrEquals },   // level 2 "all" passes (strict)
        { "f32_lt_quiet", TestLessThan },           // level 2 "all" passes (strict)

        { "f64_to_ui32", TestFloatToInteger },      // level 2 "all" passes (strict)
        { "f64_to_ui64", TestFloatToInteger },      // level 2 "all" passes (strict)
        { "f64_to_i32", TestFloatToInteger },       // level 2 "all" passes (strict)
        { "f64_to_i64", TestFloatToInteger },       // level 2 "all" passes (strict)
        { "f64_to_ui32_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "f64_to_ui64_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "f64_to_i32_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "f64_to_i64_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "f64_to_f16", TestFloatToFloat },         // level 2 "all" passes (strict)
        { "f64_to_f32", TestFloatToFloat },         // level 2 "all" passes (strict)
        { "f64_to_extF80", TestFloatToFloat },      // level 2 "all" passes (strict)
        { "f64_to_f128", TestFloatToFloat },        // level 2 "all" passes (strict)
        { "f64_roundToInt", TestRoundToInt },       // level 2 "all" passes (strict)
        { "f64_add", TestAdd },                     // level 2 "all" passes (strict)
        { "f64_sub", TestSubtract },                // level 2 "all" passes (strict)
        { "f64_mul", TestMultiply },                // level 2 "all" passes (strict)
        { "f64_mulAdd", TestMultiplyAndAdd },       // level 1 "all" passes (strict)
        { "f64_div", TestDivide },                  // level 2 "all" passes (strict)
        { "f64_rem", TestModulus },                 // level 2 "all" passes (strict)
        { "f64_sqrt", TestSquareRoot },             // level 2 "all" passes (strict)
        { "f64_eq", TestEquals },                   // level 2 "all" passes (strict)
        { "f64_le", TestLessThanOrEquals },         // level 2 "all" passes (strict)
        { "f64_lt", TestLessThan },                 // level 2 "all" passes (strict)
        { "f64_eq_signaling", TestEquals },         // level 2 "all" passes (strict)
        { "f64_le_quiet", TestLessThanOrEquals },   // level 2 "all" passes (strict)
        { "f64_lt_quiet", TestLessThan },           // level 2 "all" passes (strict)

        { "extF80_to_ui32", TestFloatToInteger },   // level 2 "all" passes (strict)
        { "extF80_to_ui64", TestFloatToInteger },   // level 2 "all" passes (strict)
        { "extF80_to_i32", TestFloatToInteger },    // level 2 "all" passes (strict)
        { "extF80_to_i64", TestFloatToInteger },    // level 2 "all" passes (strict)
        { "extF80_to_ui32_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "extF80_to_ui64_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "extF80_to_i32_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "extF80_to_i64_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "extF80_to_f16", TestFloatToFloat },      // level 2 "all" passes (strict)
        { "extF80_to_f32", TestFloatToFloat },      // level 2 "all" passes (strict)
        { "extF80_to_f64", TestFloatToFloat },      // level 2 "all" passes (strict)
        { "extF80_to_f128", TestFloatToFloat },     // level 2 "all" passes (strict)
        { "extF80_roundToInt", TestRoundToInt },    // level 2 "all" passes (strict)
        { "extF80_add", TestAdd },                  // level 2 "all" passes (strict)
        { "extF80_sub", TestSubtract },             // level 2 "all" passes (strict)
        { "extF80_mul", TestMultiply },             // level 2 "all" passes (strict)
        { "extF80_div", TestDivide },               // level 2 "all" passes (strict)
        { "extF80_rem", TestModulus },              // level 2 "all" passes (strict)
        { "extF80_sqrt", TestSquareRoot },          // level 2 "all" passes (strict)
        { "extF80_eq", TestEquals },                // level 2 "all" passes (strict)
        { "extF80_le", TestLessThanOrEquals },      // level 2 "all" passes (strict)
        { "extF80_lt", TestLessThan },              // level 2 "all" passes (strict)
        { "extF80_eq_signaling", TestEquals },      // level 2 "all" passes (strict)
        { "extF80_le_quiet", TestLessThanOrEquals },// level 2 "all" passes (strict)
        { "extF80_lt_quiet", TestLessThan },        // level 2 "all" passes (strict)

        { "f128_to_ui32", TestFloatToInteger },     // level 2 "all" passes (strict)
        { "f128_to_ui64", TestFloatToInteger },     // level 2 "all" passes (strict)
        { "f128_to_i32", TestFloatToInteger },      // level 2 "all" passes (strict)
        { "f128_to_i64", TestFloatToInteger },      // level 2 "all" passes (strict)
        { "f128_to_ui32_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "f128_to_ui64_r_minMag", TestFloatToIntegerMinMag }, // level 2 "all" passes (strict)
        { "f128_to_i32_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "f128_to_i64_r_minMag", TestFloatToIntegerMinMag },  // level 2 "all" passes (strict)
        { "f128_to_f16", TestFloatToFloat },        // level 2 "all" passes (strict)
        { "f128_to_f32", TestFloatToFloat },        // level 2 "all" passes (strict)
        { "f128_to_f64", TestFloatToFloat },        // level 2 "all" passes (strict)
        { "f128_to_extF80", TestFloatToFloat },     // level 2 "all" passes (strict)
        { "f128_roundToInt", TestRoundToInt },      // level 2 "all" passes (strict)
        { "f128_add", TestAdd },                    // level 1 "all" passes (strict)
        { "f128_sub", TestSubtract },               // level 1 "all" passes (strict)
        { "f128_mul", TestMultiply },               // level 1 "all" passes (strict)
        { "f128_mulAdd", TestMultiplyAndAdd },      // level 1 "all" passes (strict)
        { "f128_div", TestDivide },                 // level 2 "all" passes (strict)
        { "f128_rem", TestModulus },                // level 2 "all" passes (strict)
        { "f128_sqrt", TestSquareRoot },            // level 2 "all" passes (strict)
        { "f128_eq", TestEquals },                  // level 2 "all" passes (strict)
        { "f128_le", TestLessThanOrEquals },        // level 2 "all" passes (strict)
        { "f128_lt", TestLessThan },                // level 2 "all" passes (strict)
        { "f128_eq_signaling", TestEquals },        // level 2 "all" passes (strict)
        { "f128_le_quiet", TestLessThanOrEquals },  // level 2 "all" passes (strict)
        { "f128_lt_quiet", TestLessThan },          // level 2 "all" passes (strict)
    };

    public static TestRunnerResult TestIntegerToFloat(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 1 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        uint ui32;
        ulong ui64;
        int i32;
        long i64;

        Float16 f16;
        Float32 f32;
        Float64 f64;
        ExtFloat80 extF80;
        Float128 f128;

        switch (runner.TestFunction)
        {
            case "ui32_to_f16":
            {
                ui32 = arguments.Argument1.ToUInt32();
                f16 = Float16.FromUInt32(context, ui32);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "ui32_to_f32":
            {
                ui32 = arguments.Argument1.ToUInt32();
                f32 = Float32.FromUInt32(context, ui32);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "ui32_to_f64":
            {
                ui32 = arguments.Argument1.ToUInt32();
                f64 = Float64.FromUInt32(context, ui32);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "ui32_to_extF80":
            {
                ui32 = arguments.Argument1.ToUInt32();
                extF80 = ExtFloat80.FromUInt32(context, ui32);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "ui32_to_f128":
            {
                ui32 = arguments.Argument1.ToUInt32();
                f128 = Float128.FromUInt32(context, ui32);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "ui64_to_f16":
            {
                ui64 = arguments.Argument1.ToUInt64();
                f16 = Float16.FromUInt64(context, ui64);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "ui64_to_f32":
            {
                ui64 = arguments.Argument1.ToUInt64();
                f32 = Float32.FromUInt64(context, ui64);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "ui64_to_f64":
            {
                ui64 = arguments.Argument1.ToUInt64();
                f64 = Float64.FromUInt64(context, ui64);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "ui64_to_extF80":
            {
                ui64 = arguments.Argument1.ToUInt64();
                extF80 = ExtFloat80.FromUInt64(context, ui64);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "ui64_to_f128":
            {
                ui64 = arguments.Argument1.ToUInt64();
                f128 = Float128.FromUInt64(context, ui64);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "i32_to_f16":
            {
                i32 = arguments.Argument1.ToInt32();
                f16 = Float16.FromInt32(context, i32);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "i32_to_f32":
            {
                i32 = arguments.Argument1.ToInt32();
                f32 = Float32.FromInt32(context, i32);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "i32_to_f64":
            {
                i32 = arguments.Argument1.ToInt32();
                f64 = Float64.FromInt32(context, i32);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "i32_to_extF80":
            {
                i32 = arguments.Argument1.ToInt32();
                extF80 = ExtFloat80.FromInt32(context, i32);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "i32_to_f128":
            {
                i32 = arguments.Argument1.ToInt32();
                f128 = Float128.FromInt32(context, i32);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "i64_to_f16":
            {
                i64 = arguments.Argument1.ToInt64();
                f16 = Float16.FromInt64(context, i64);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "i64_to_f32":
            {
                i64 = arguments.Argument1.ToInt64();
                f32 = Float32.FromInt64(context, i64);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "i64_to_f64":
            {
                i64 = arguments.Argument1.ToInt64();
                f64 = Float64.FromInt64(context, i64);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "i64_to_extF80":
            {
                i64 = arguments.Argument1.ToInt64();
                extF80 = ExtFloat80.FromInt64(context, i64);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "i64_to_f128":
            {
                i64 = arguments.Argument1.ToInt64();
                f128 = Float128.FromInt64(context, i64);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestFloatToInteger(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 1 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // These are the defaults that testfloat_ver expects if arguments are not explicitly set.
        var roundingMode = runner.Options.Rounding ?? RoundingMode.NearEven;
        var exact = runner.Options.Exact ?? false;

        // Make sure input and output types are correct.

        Float16 f16;
        Float32 f32;
        Float64 f64;
        ExtFloat80 extF80;
        Float128 f128;

        uint ui32;
        ulong ui64;
        int i32;
        long i64;

        switch (runner.TestFunction)
        {
            case "f16_to_ui32":
            {
                f16 = arguments.Argument1.ToFloat16();
                ui32 = f16.ToUInt32(context, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f16_to_ui64":
            {
                f16 = arguments.Argument1.ToFloat16();
                ui64 = f16.ToUInt64(context, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f16_to_i32":
            {
                f16 = arguments.Argument1.ToFloat16();
                i32 = f16.ToInt32(context, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f16_to_i64":
            {
                f16 = arguments.Argument1.ToFloat16();
                i64 = f16.ToInt64(context, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f32_to_ui32":
            {
                f32 = arguments.Argument1.ToFloat32();
                ui32 = f32.ToUInt32(context, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f32_to_ui64":
            {
                f32 = arguments.Argument1.ToFloat32();
                ui64 = f32.ToUInt64(context, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f32_to_i32":
            {
                f32 = arguments.Argument1.ToFloat32();
                i32 = f32.ToInt32(context, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f32_to_i64":
            {
                f32 = arguments.Argument1.ToFloat32();
                i64 = f32.ToInt64(context, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f64_to_ui32":
            {
                f64 = arguments.Argument1.ToFloat64();
                ui32 = f64.ToUInt32(context, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f64_to_ui64":
            {
                f64 = arguments.Argument1.ToFloat64();
                ui64 = f64.ToUInt64(context, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f64_to_i32":
            {
                f64 = arguments.Argument1.ToFloat64();
                i32 = f64.ToInt32(context, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f64_to_i64":
            {
                f64 = arguments.Argument1.ToFloat64();
                i64 = f64.ToInt64(context, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "extF80_to_ui32":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                ui32 = extF80.ToUInt32(context, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "extF80_to_ui64":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                ui64 = extF80.ToUInt64(context, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "extF80_to_i32":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                i32 = extF80.ToInt32(context, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "extF80_to_i64":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                i64 = extF80.ToInt64(context, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f128_to_ui32":
            {
                f128 = arguments.Argument1.ToFloat128();
                ui32 = f128.ToUInt32(context, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f128_to_ui64":
            {
                f128 = arguments.Argument1.ToFloat128();
                ui64 = f128.ToUInt64(context, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f128_to_i32":
            {
                f128 = arguments.Argument1.ToFloat128();
                i32 = f128.ToInt32(context, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f128_to_i64":
            {
                f128 = arguments.Argument1.ToFloat128();
                i64 = f128.ToInt64(context, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestFloatToIntegerMinMag(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 1 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        Debug.Assert(runner.Options.Rounding == RoundingMode.MinMag, "Rounding mode is not defined.");

        // These are the defaults that testfloat_ver expects if arguments are not explicitly set.
        var exact = runner.Options.Exact ?? false;

        // Make sure input and output types are correct.

        Float16 f16;
        Float32 f32;
        Float64 f64;
        ExtFloat80 extF80;
        Float128 f128;

        uint ui32;
        ulong ui64;
        int i32;
        long i64;

        switch (runner.TestFunction)
        {
            case "f16_to_ui32_r_minMag":
            {
                f16 = arguments.Argument1.ToFloat16();
                ui32 = f16.ToUInt32RoundMinMag(context, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f16_to_ui64_r_minMag":
            {
                f16 = arguments.Argument1.ToFloat16();
                ui64 = f16.ToUInt64RoundMinMag(context, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f16_to_i32_r_minMag":
            {
                f16 = arguments.Argument1.ToFloat16();
                i32 = f16.ToInt32RoundMinMag(context, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f16_to_i64_r_minMag":
            {
                f16 = arguments.Argument1.ToFloat16();
                i64 = f16.ToInt64RoundMinMag(context, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f32_to_ui32_r_minMag":
            {
                f32 = arguments.Argument1.ToFloat32();
                ui32 = f32.ToUInt32RoundMinMag(context, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f32_to_ui64_r_minMag":
            {
                f32 = arguments.Argument1.ToFloat32();
                ui64 = f32.ToUInt64RoundMinMag(context, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f32_to_i32_r_minMag":
            {
                f32 = arguments.Argument1.ToFloat32();
                i32 = f32.ToInt32RoundMinMag(context, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f32_to_i64_r_minMag":
            {
                f32 = arguments.Argument1.ToFloat32();
                i64 = f32.ToInt64RoundMinMag(context, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f64_to_ui32_r_minMag":
            {
                f64 = arguments.Argument1.ToFloat64();
                ui32 = f64.ToUInt32RoundMinMag(context, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f64_to_ui64_r_minMag":
            {
                f64 = arguments.Argument1.ToFloat64();
                ui64 = f64.ToUInt64RoundMinMag(context, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f64_to_i32_r_minMag":
            {
                f64 = arguments.Argument1.ToFloat64();
                i32 = f64.ToInt32RoundMinMag(context, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f64_to_i64_r_minMag":
            {
                f64 = arguments.Argument1.ToFloat64();
                i64 = f64.ToInt64RoundMinMag(context, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "extF80_to_ui32_r_minMag":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                ui32 = extF80.ToUInt32RoundMinMag(context, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "extF80_to_ui64_r_minMag":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                ui64 = extF80.ToUInt64RoundMinMag(context, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "extF80_to_i32_r_minMag":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                i32 = extF80.ToInt32RoundMinMag(context, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "extF80_to_i64_r_minMag":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                i64 = extF80.ToInt64RoundMinMag(context, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f128_to_ui32_r_minMag":
            {
                f128 = arguments.Argument1.ToFloat128();
                ui32 = f128.ToUInt32RoundMinMag(context, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f128_to_ui64_r_minMag":
            {
                f128 = arguments.Argument1.ToFloat128();
                ui64 = f128.ToUInt64RoundMinMag(context, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f128_to_i32_r_minMag":
            {
                f128 = arguments.Argument1.ToFloat128();
                i32 = f128.ToInt32RoundMinMag(context, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f128_to_i64_r_minMag":
            {
                f128 = arguments.Argument1.ToFloat128();
                i64 = f128.ToInt64RoundMinMag(context, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestFloatToFloat(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 1 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16;
        Float32 f32;
        Float64 f64;
        ExtFloat80 extF80;
        Float128 f128;

        switch (runner.TestFunction)
        {
            case "f16_to_f32":
            {
                f16 = arguments.Argument1.ToFloat16();
                f32 = f16.ToFloat32(context);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "f16_to_f64":
            {
                f16 = arguments.Argument1.ToFloat16();
                f64 = f16.ToFloat64(context);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "f16_to_extF80":
            {
                f16 = arguments.Argument1.ToFloat16();
                extF80 = f16.ToExtFloat80(context);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "f16_to_f128":
            {
                f16 = arguments.Argument1.ToFloat16();
                f128 = f16.ToFloat128(context);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "f32_to_f16":
            {
                f32 = arguments.Argument1.ToFloat32();
                f16 = f32.ToFloat16(context);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "f32_to_f64":
            {
                f32 = arguments.Argument1.ToFloat32();
                f64 = f32.ToFloat64(context);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "f32_to_extF80":
            {
                f32 = arguments.Argument1.ToFloat32();
                extF80 = f32.ToExtFloat80(context);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "f32_to_f128":
            {
                f32 = arguments.Argument1.ToFloat32();
                f128 = f32.ToFloat128(context);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "f64_to_f16":
            {
                f64 = arguments.Argument1.ToFloat64();
                f16 = f64.ToFloat16(context);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "f64_to_f32":
            {
                f64 = arguments.Argument1.ToFloat64();
                f32 = f64.ToFloat32(context);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "f64_to_extF80":
            {
                f64 = arguments.Argument1.ToFloat64();
                extF80 = f64.ToExtFloat80(context);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "f64_to_f128":
            {
                f64 = arguments.Argument1.ToFloat64();
                f128 = f64.ToFloat128(context);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "extF80_to_f16":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                f16 = extF80.ToFloat16(context);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "extF80_to_f32":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                f32 = extF80.ToFloat32(context);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "extF80_to_f64":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                f64 = extF80.ToFloat64(context);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "extF80_to_f128":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                f128 = extF80.ToFloat128(context);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "f128_to_f16":
            {
                f128 = arguments.Argument1.ToFloat128();
                f16 = f128.ToFloat16(context);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "f128_to_f32":
            {
                f128 = arguments.Argument1.ToFloat128();
                f32 = f128.ToFloat32(context);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "f128_to_f64":
            {
                f128 = arguments.Argument1.ToFloat128();
                f64 = f128.ToFloat64(context);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "f128_to_extF80":
            {
                f128 = arguments.Argument1.ToFloat128();
                extF80 = f128.ToExtFloat80(context);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestRoundToInt(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 1 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // These are the defaults that testfloat_ver expects if arguments are not explicitly set.
        var roundingMode = runner.Options.Rounding ?? RoundingMode.NearEven;
        var exact = runner.Options.Exact ?? false;

        // Make sure input and output types are correct.

        Float16 f16_a, f16_z;
        Float32 f32_a, f32_z;
        Float64 f64_a, f64_z;
        ExtFloat80 extF80_a, extF80_z;
        Float128 f128_a, f128_z;

        switch (runner.TestFunction)
        {
            case "f16_roundToInt":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_z = f16_a.RoundToInt(context, roundingMode, exact);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_roundToInt":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_z = f32_a.RoundToInt(context, roundingMode, exact);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_roundToInt":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_z = f64_a.RoundToInt(context, roundingMode, exact);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_roundToInt":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_z = extF80_a.RoundToInt(context, roundingMode, exact);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_roundToInt":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_z = f128_a.RoundToInt(context, roundingMode, exact);
                return new TestRunnerResult(f128_z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestAdd(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 2 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b, f16_z;
        Float32 f32_a, f32_b, f32_z;
        Float64 f64_a, f64_b, f64_z;
        ExtFloat80 extF80_a, extF80_b, extF80_z;
        Float128 f128_a, f128_b, f128_z;

        switch (runner.TestFunction)
        {
            case "f16_add":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                f16_z = Float16.Add(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_add":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = Float32.Add(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_add":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = Float64.Add(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_add":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = ExtFloat80.Add(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_add":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = Float128.Add(context, f128_a, f128_b);
                return new TestRunnerResult(f128_z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestSubtract(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 2 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b, f16_z;
        Float32 f32_a, f32_b, f32_z;
        Float64 f64_a, f64_b, f64_z;
        ExtFloat80 extF80_a, extF80_b, extF80_z;
        Float128 f128_a, f128_b, f128_z;

        switch (runner.TestFunction)
        {
            case "f16_sub":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                f16_z = Float16.Subtract(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_sub":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = Float32.Subtract(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_sub":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = Float64.Subtract(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_sub":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = ExtFloat80.Subtract(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_sub":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = Float128.Subtract(context, f128_a, f128_b);
                return new TestRunnerResult(f128_z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestMultiply(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 2 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b, f16_z;
        Float32 f32_a, f32_b, f32_z;
        Float64 f64_a, f64_b, f64_z;
        ExtFloat80 extF80_a, extF80_b, extF80_z;
        Float128 f128_a, f128_b, f128_z;

        switch (runner.TestFunction)
        {
            case "f16_mul":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                f16_z = Float16.Multiply(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_mul":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = Float32.Multiply(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_mul":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = Float64.Multiply(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_mul":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = ExtFloat80.Multiply(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_mul":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = Float128.Multiply(context, f128_a, f128_b);
                return new TestRunnerResult(f128_z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestMultiplyAndAdd(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 3 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b, f16_c, f16_z;
        Float32 f32_a, f32_b, f32_c, f32_z;
        Float64 f64_a, f64_b, f64_c, f64_z;
        Float128 f128_a, f128_b, f128_c, f128_z;

        switch (runner.TestFunction)
        {
            case "f16_mulAdd":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                f16_c = arguments.Argument3.ToFloat16();
                f16_z = Float16.MultiplyAndAdd(context, f16_a, f16_b, f16_c);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_mulAdd":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_c = arguments.Argument3.ToFloat32();
                f32_z = Float32.MultiplyAndAdd(context, f32_a, f32_b, f32_c);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_mulAdd":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_c = arguments.Argument3.ToFloat64();
                f64_z = Float64.MultiplyAndAdd(context, f64_a, f64_b, f64_c);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "f128_mulAdd":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_c = arguments.Argument3.ToFloat128();
                f128_z = Float128.MultiplyAndAdd(context, f128_a, f128_b, f128_c);
                return new TestRunnerResult(f128_z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestDivide(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 2 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b, f16_z;
        Float32 f32_a, f32_b, f32_z;
        Float64 f64_a, f64_b, f64_z;
        ExtFloat80 extF80_a, extF80_b, extF80_z;
        Float128 f128_a, f128_b, f128_z;

        switch (runner.TestFunction)
        {
            case "f16_div":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                f16_z = Float16.Divide(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_div":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = Float32.Divide(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_div":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = Float64.Divide(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_div":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = ExtFloat80.Divide(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_div":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = Float128.Divide(context, f128_a, f128_b);
                return new TestRunnerResult(f128_z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestModulus(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 2 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b, f16_z;
        Float32 f32_a, f32_b, f32_z;
        Float64 f64_a, f64_b, f64_z;
        ExtFloat80 extF80_a, extF80_b, extF80_z;
        Float128 f128_a, f128_b, f128_z;

        switch (runner.TestFunction)
        {
            case "f16_rem":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                f16_z = Float16.Remainder(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_rem":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = Float32.Remainder(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_rem":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = Float64.Remainder(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_rem":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = ExtFloat80.Remainder(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_rem":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = Float128.Remainder(context, f128_a, f128_b);
                return new TestRunnerResult(f128_z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestSquareRoot(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 1 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_z;
        Float32 f32_a, f32_z;
        Float64 f64_a, f64_z;
        ExtFloat80 extF80_a, extF80_z;
        Float128 f128_a, f128_z;

        switch (runner.TestFunction)
        {
            case "f16_sqrt":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_z = f16_a.SquareRoot(context);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_sqrt":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_z = f32_a.SquareRoot(context);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_sqrt":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_z = f64_a.SquareRoot(context);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_sqrt":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_z = extF80_a.SquareRoot(context);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_sqrt":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_z = f128_a.SquareRoot(context);
                return new TestRunnerResult(f128_z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestEquals(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 2 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b;
        Float32 f32_a, f32_b;
        Float64 f64_a, f64_b;
        ExtFloat80 extF80_a, extF80_b;
        Float128 f128_a, f128_b;
        bool z;

        switch (runner.TestFunction)
        {
            case "f16_eq":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = Float16.CompareEqual(context, f16_a, f16_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_eq":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = Float32.CompareEqual(context, f32_a, f32_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_eq":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = Float64.CompareEqual(context, f64_a, f64_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_eq":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = ExtFloat80.CompareEqual(context, extF80_a, extF80_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_eq":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = Float128.CompareEqual(context, f128_a, f128_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            case "f16_eq_signaling":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = Float16.CompareEqual(context, f16_a, f16_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_eq_signaling":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = Float32.CompareEqual(context, f32_a, f32_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_eq_signaling":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = Float64.CompareEqual(context, f64_a, f64_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_eq_signaling":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = ExtFloat80.CompareEqual(context, extF80_a, extF80_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_eq_signaling":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = Float128.CompareEqual(context, f128_a, f128_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestLessThan(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 2 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b;
        Float32 f32_a, f32_b;
        Float64 f64_a, f64_b;
        ExtFloat80 extF80_a, extF80_b;
        Float128 f128_a, f128_b;
        bool z;

        switch (runner.TestFunction)
        {
            case "f16_lt":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = Float16.CompareLessThan(context, f16_a, f16_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_lt":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = Float32.CompareLessThan(context, f32_a, f32_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_lt":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = Float64.CompareLessThan(context, f64_a, f64_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_lt":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = ExtFloat80.CompareLessThan(context, extF80_a, extF80_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_lt":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = Float128.CompareLessThan(context, f128_a, f128_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            case "f16_lt_quiet":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = Float16.CompareLessThan(context, f16_a, f16_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_lt_quiet":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = Float32.CompareLessThan(context, f32_a, f32_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_lt_quiet":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = Float64.CompareLessThan(context, f64_a, f64_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_lt_quiet":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = ExtFloat80.CompareLessThan(context, extF80_a, extF80_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_lt_quiet":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = Float128.CompareLessThan(context, f128_a, f128_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }

    public static TestRunnerResult TestLessThanOrEquals(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 2 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SoftFloatContext;
        runner.ResetContext(context);

        // Make sure input and output types are correct.

        Float16 f16_a, f16_b;
        Float32 f32_a, f32_b;
        Float64 f64_a, f64_b;
        ExtFloat80 extF80_a, extF80_b;
        Float128 f128_a, f128_b;
        bool z;

        switch (runner.TestFunction)
        {
            case "f16_le":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = Float16.CompareLessThanOrEqual(context, f16_a, f16_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_le":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = Float32.CompareLessThanOrEqual(context, f32_a, f32_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_le":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = Float64.CompareLessThanOrEqual(context, f64_a, f64_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_le":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = ExtFloat80.CompareLessThanOrEqual(context, extF80_a, extF80_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_le":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = Float128.CompareLessThanOrEqual(context, f128_a, f128_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            case "f16_le_quiet":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = Float16.CompareLessThanOrEqual(context, f16_a, f16_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_le_quiet":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = Float32.CompareLessThanOrEqual(context, f32_a, f32_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_le_quiet":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = Float64.CompareLessThanOrEqual(context, f64_a, f64_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_le_quiet":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = ExtFloat80.CompareLessThanOrEqual(context, extF80_a, extF80_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_le_quiet":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = Float128.CompareLessThanOrEqual(context, f128_a, f128_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }
}
