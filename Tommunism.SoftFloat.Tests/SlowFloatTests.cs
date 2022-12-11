namespace Tommunism.SoftFloat.Tests;

internal static class SlowFloatTests
{
    // NOTE: If function in the dictionary is null, then it is not currently implemented and should be skipped.
    // NOTE: Many of these tests will fail in "strict" mode (-checkNaNs and -checkInvInts / -checkAll), because NaNs are represented in a
    // very simple mostly non-specialized way with SlowFloat.

    public static readonly Dictionary<string, Func<TestRunnerState, TestRunnerArguments, TestRunnerResult>?> Functions = new()
    {
        { "ui32_to_f16", TestIntegerToFloat },          // level 2 "all" passes
        { "ui32_to_f32", TestIntegerToFloat },          // level 2 "all" passes
        { "ui32_to_f64", TestIntegerToFloat },          // level 2 "all" passes
        { "ui32_to_extF80", TestIntegerToFloat },       // level 2 "all" passes
        { "ui32_to_f128", TestIntegerToFloat },         // level 2 "all" passes
        { "ui64_to_f16", TestIntegerToFloat },          // level 2 "all" passes
        { "ui64_to_f32", TestIntegerToFloat },          // level 2 "all" passes
        { "ui64_to_f64", TestIntegerToFloat },          // level 2 "all" passes
        { "ui64_to_extF80", TestIntegerToFloat },       // level 2 "all" passes
        { "ui64_to_f128", TestIntegerToFloat },         // level 2 "all" passes
        { "i32_to_f16", TestIntegerToFloat },           // level 2 "all" passes
        { "i32_to_f32", TestIntegerToFloat },           // level 2 "all" passes
        { "i32_to_f64", TestIntegerToFloat },           // level 2 "all" passes
        { "i32_to_extF80", TestIntegerToFloat },        // level 2 "all" passes
        { "i32_to_f128", TestIntegerToFloat },          // level 2 "all" passes
        { "i64_to_f16", TestIntegerToFloat },           // level 2 "all" passes
        { "i64_to_f32", TestIntegerToFloat },           // level 2 "all" passes
        { "i64_to_f64", TestIntegerToFloat },           // level 2 "all" passes
        { "i64_to_extF80", TestIntegerToFloat },        // level 2 "all" passes
        { "i64_to_f128", TestIntegerToFloat },          // level 2 "all" passes

        { "f16_to_ui32", TestFloatToInteger },          // level 2 "all" passes
        { "f16_to_ui64", TestFloatToInteger },          // level 2 "all" passes
        { "f16_to_i32", TestFloatToInteger },           // level 2 "all" passes
        { "f16_to_i64", TestFloatToInteger },           // level 2 "all" passes
        { "f16_to_f32", TestFloatToFloat },             // level 2 "all" passes
        { "f16_to_f64", TestFloatToFloat },             // level 2 "all" passes
        { "f16_to_extF80", TestFloatToFloat },          // level 2 "all" passes
        { "f16_to_f128", TestFloatToFloat },            // level 2 "all" passes
        { "f16_roundToInt", TestRoundToInt },           // level 2 "all" passes
        { "f16_add", TestAdd },                         // level 2 "all" passes
        { "f16_sub", TestSubtract },                    // level 2 "all" passes
        { "f16_mul", TestMultiply },                    // level 2 "all" passes
        { "f16_mulAdd", TestMultiplyAndAdd },           // level 1 "all" passes -- long test, even on level 1 (over 6 million test cases for each config)
        { "f16_div", TestDivide },                      // level 2 "all" passes
        { "f16_rem", TestModulus },                     // level 2 "all" passes
        { "f16_sqrt", TestSquareRoot },                 // level 2 "all" passes
        { "f16_eq", TestEquals },                       // level 2 "all" passes
        { "f16_le", TestLessThanOrEquals },             // level 2 "all" passes
        { "f16_lt", TestLessThan },                     // level 2 "all" passes
        { "f16_eq_signaling", TestEquals },             // level 2 "all" passes
        { "f16_le_quiet", TestLessThanOrEquals },       // level 2 "all" passes
        { "f16_lt_quiet", TestLessThan },               // level 2 "all" passes

        { "f32_to_ui32", TestFloatToInteger },          // level 2 "all" passes
        { "f32_to_ui64", TestFloatToInteger },          // level 2 "all" passes
        { "f32_to_i32", TestFloatToInteger },           // level 2 "all" passes
        { "f32_to_i64", TestFloatToInteger },           // level 2 "all" passes
        { "f32_to_f16", TestFloatToFloat },             // level 2 "all" passes
        { "f32_to_f64", TestFloatToFloat },             // level 2 "all" passes
        { "f32_to_extF80", TestFloatToFloat },          // level 2 "all" passes
        { "f32_to_f128", TestFloatToFloat },            // level 2 "all" passes
        { "f32_roundToInt", TestRoundToInt },           // level 2 "all" passes
        { "f32_add", TestAdd },                         // level 2 "all" passes
        { "f32_sub", TestSubtract },                    // level 2 "all" passes
        { "f32_mul", TestMultiply },                    // level 2 "all" passes
        { "f32_mulAdd", TestMultiplyAndAdd },           // level 1 "all" passes
        { "f32_div", TestDivide },                      // level 1 "all" passes
        { "f32_rem", TestModulus },                     // level 2 "all" passes
        { "f32_sqrt", TestSquareRoot },                 // level 2 "all" passes
        { "f32_eq", TestEquals },                       // level 2 "all" passes
        { "f32_le", TestLessThanOrEquals },             // level 2 "all" passes
        { "f32_lt", TestLessThan },                     // level 2 "all" passes
        { "f32_eq_signaling", TestEquals },             // level 2 "all" passes
        { "f32_le_quiet", TestLessThanOrEquals },       // level 2 "all" passes
        { "f32_lt_quiet", TestLessThan },               // level 2 "all" passes

        { "f64_to_ui32", TestFloatToInteger },          // level 2 "all" passes
        { "f64_to_ui64", TestFloatToInteger },          // level 2 "all" passes
        { "f64_to_i32", TestFloatToInteger },           // level 2 "all" passes
        { "f64_to_i64", TestFloatToInteger },           // level 2 "all" passes
        { "f64_to_f16", TestFloatToFloat },             // level 2 "all" passes
        { "f64_to_f32", TestFloatToFloat },             // level 2 "all" passes
        { "f64_to_extF80", TestFloatToFloat },          // level 2 "all" passes
        { "f64_to_f128", TestFloatToFloat },            // level 2 "all" passes
        { "f64_roundToInt", TestRoundToInt },           // level 2 "all" passes
        { "f64_add", TestAdd },                         // level 2 "all" passes
        { "f64_sub", TestSubtract },                    // level 2 "all" passes
        { "f64_mul", TestMultiply },                    // level 2 "all" passes
        { "f64_mulAdd", TestMultiplyAndAdd },           // level 1 "all" passes
        { "f64_div", TestDivide },                      // level 1 "all" passes
        { "f64_rem", TestModulus },                     // level 2 "all" passes
        { "f64_sqrt", TestSquareRoot },                 // level 2 "all" passes
        { "f64_eq", TestEquals },                       // level 2 "all" passes
        { "f64_le", TestLessThanOrEquals },             // level 2 "all" passes
        { "f64_lt", TestLessThan },                     // level 2 "all" passes
        { "f64_eq_signaling", TestEquals },             // level 2 "all" passes
        { "f64_le_quiet", TestLessThanOrEquals },       // level 2 "all" passes
        { "f64_lt_quiet", TestLessThan },               // level 2 "all" passes

        { "extF80_to_ui32", TestFloatToInteger },       // level 2 "all" passes
        { "extF80_to_ui64", TestFloatToInteger },       // level 2 "all" passes
        { "extF80_to_i32", TestFloatToInteger },        // level 2 "all" passes
        { "extF80_to_i64", TestFloatToInteger },        // level 2 "all" passes
        { "extF80_to_f16", TestFloatToFloat },          // level 2 "all" passes
        { "extF80_to_f32", TestFloatToFloat },          // level 2 "all" passes
        { "extF80_to_f64", TestFloatToFloat },          // level 2 "all" passes
        { "extF80_to_f128", TestFloatToFloat },         // level 2 "all" passes
        { "extF80_roundToInt", TestRoundToInt },        // level 2 "all" passes
        { "extF80_add", TestAdd },                      // level 2 "all" passes
        { "extF80_sub", TestSubtract },                 // level 2 "all" passes
        { "extF80_mul", TestMultiply },                 // level 2 "all" passes
        { "extF80_div", TestDivide },                   // level 1 "all" passes
        { "extF80_rem", TestModulus },                  // level 2 "all" passes -- some 1M tasks take a few minutes each
        { "extF80_sqrt", TestSquareRoot },              // level 2 "all" passes
        { "extF80_eq", TestEquals },                    // level 2 "all" passes
        { "extF80_le", TestLessThanOrEquals },          // level 2 "all" passes
        { "extF80_lt", TestLessThan },                  // level 2 "all" passes
        { "extF80_eq_signaling", TestEquals },          // level 2 "all" passes
        { "extF80_le_quiet", TestLessThanOrEquals },    // level 2 "all" passes
        { "extF80_lt_quiet", TestLessThan },            // level 2 "all" passes

        { "f128_to_ui32", TestFloatToInteger },         // level 2 "all" passes
        { "f128_to_ui64", TestFloatToInteger },         // level 2 "all" passes
        { "f128_to_i32", TestFloatToInteger },          // level 2 "all" passes
        { "f128_to_i64", TestFloatToInteger },          // level 2 "all" passes
        { "f128_to_f16", TestFloatToFloat },            // level 2 "all" passes
        { "f128_to_f32", TestFloatToFloat },            // level 2 "all" passes
        { "f128_to_f64", TestFloatToFloat },            // level 2 "all" passes
        { "f128_to_extF80", TestFloatToFloat },         // level 2 "all" passes
        { "f128_roundToInt", TestRoundToInt },          // level 2 "all" passes
        { "f128_add", TestAdd },                        // level 2 "all" passes
        { "f128_sub", TestSubtract },                   // level 2 "all" passes
        { "f128_mul", TestMultiply },                   // level 1 "all" passes
        { "f128_mulAdd", TestMultiplyAndAdd },          // level 1 "all" passes -- running at level 2 with many threads and it is still on the first configuration after nearly 7 hours!
        { "f128_div", TestDivide },                     // level 1 "all" passes
        { "f128_rem", TestModulus },                    // level 2 "all" passes -- some 1M tasks take a few minutes each -- ~11 minutes for all on max 24 threads
        { "f128_sqrt", TestSquareRoot },                // level 2 "all" passes
        { "f128_eq", TestEquals },                      // level 2 "all" passes
        { "f128_le", TestLessThanOrEquals },            // level 2 "all" passes
        { "f128_lt", TestLessThan },                    // level 2 "all" passes
        { "f128_eq_signaling", TestEquals },            // level 2 "all" passes
        { "f128_le_quiet", TestLessThanOrEquals },      // level 2 "all" passes
        { "f128_lt_quiet", TestLessThan },              // level 2 "all" passes
    };

    public static TestRunnerResult TestIntegerToFloat(TestRunnerState runner, TestRunnerArguments arguments)
    {
        // Remember the last two arguments are the generator's expected result.
        if (arguments.Count < 1 + (runner.AppendResultsToArguments ? 0 : 2))
            throw new InvalidOperationException("Not enough arguments to perform operation.");

        // Get context and reset according to runner options.
        var context = runner.SlowFloatContext;
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
                f16 = SlowFloat.ToFloat16(context, ui32);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "ui32_to_f32":
            {
                ui32 = arguments.Argument1.ToUInt32();
                f32 = SlowFloat.ToFloat32(context, ui32);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "ui32_to_f64":
            {
                ui32 = arguments.Argument1.ToUInt32();
                f64 = SlowFloat.ToFloat64(context, ui32);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "ui32_to_extF80":
            {
                ui32 = arguments.Argument1.ToUInt32();
                extF80 = SlowFloat.ToExtFloat80(context, ui32);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "ui32_to_f128":
            {
                ui32 = arguments.Argument1.ToUInt32();
                f128 = SlowFloat.ToFloat128(context, ui32);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "ui64_to_f16":
            {
                ui64 = arguments.Argument1.ToUInt64();
                f16 = SlowFloat.ToFloat16(context, ui64);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "ui64_to_f32":
            {
                ui64 = arguments.Argument1.ToUInt64();
                f32 = SlowFloat.ToFloat32(context, ui64);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "ui64_to_f64":
            {
                ui64 = arguments.Argument1.ToUInt64();
                f64 = SlowFloat.ToFloat64(context, ui64);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "ui64_to_extF80":
            {
                ui64 = arguments.Argument1.ToUInt64();
                extF80 = SlowFloat.ToExtFloat80(context, ui64);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "ui64_to_f128":
            {
                ui64 = arguments.Argument1.ToUInt64();
                f128 = SlowFloat.ToFloat128(context, ui64);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "i32_to_f16":
            {
                i32 = arguments.Argument1.ToInt32();
                f16 = SlowFloat.ToFloat16(context, i32);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "i32_to_f32":
            {
                i32 = arguments.Argument1.ToInt32();
                f32 = SlowFloat.ToFloat32(context, i32);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "i32_to_f64":
            {
                i32 = arguments.Argument1.ToInt32();
                f64 = SlowFloat.ToFloat64(context, i32);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "i32_to_extF80":
            {
                i32 = arguments.Argument1.ToInt32();
                extF80 = SlowFloat.ToExtFloat80(context, i32);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "i32_to_f128":
            {
                i32 = arguments.Argument1.ToInt32();
                f128 = SlowFloat.ToFloat128(context, i32);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "i64_to_f16":
            {
                i64 = arguments.Argument1.ToInt64();
                f16 = SlowFloat.ToFloat16(context, i64);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "i64_to_f32":
            {
                i64 = arguments.Argument1.ToInt64();
                f32 = SlowFloat.ToFloat32(context, i64);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "i64_to_f64":
            {
                i64 = arguments.Argument1.ToInt64();
                f64 = SlowFloat.ToFloat64(context, i64);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "i64_to_extF80":
            {
                i64 = arguments.Argument1.ToInt64();
                extF80 = SlowFloat.ToExtFloat80(context, i64);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "i64_to_f128":
            {
                i64 = arguments.Argument1.ToInt64();
                f128 = SlowFloat.ToFloat128(context, i64);
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
        var context = runner.SlowFloatContext;
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
                ui32 = SlowFloat.ToUInt32(context, f16, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f16_to_ui64":
            {
                f16 = arguments.Argument1.ToFloat16();
                ui64 = SlowFloat.ToUInt64(context, f16, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f16_to_i32":
            {
                f16 = arguments.Argument1.ToFloat16();
                i32 = SlowFloat.ToInt32(context, f16, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f16_to_i64":
            {
                f16 = arguments.Argument1.ToFloat16();
                i64 = SlowFloat.ToInt64(context, f16, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f32_to_ui32":
            {
                f32 = arguments.Argument1.ToFloat32();
                ui32 = SlowFloat.ToUInt32(context, f32, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f32_to_ui64":
            {
                f32 = arguments.Argument1.ToFloat32();
                ui64 = SlowFloat.ToUInt64(context, f32, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f32_to_i32":
            {
                f32 = arguments.Argument1.ToFloat32();
                i32 = SlowFloat.ToInt32(context, f32, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f32_to_i64":
            {
                f32 = arguments.Argument1.ToFloat32();
                i64 = SlowFloat.ToInt64(context, f32, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f64_to_ui32":
            {
                f64 = arguments.Argument1.ToFloat64();
                ui32 = SlowFloat.ToUInt32(context, f64, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f64_to_ui64":
            {
                f64 = arguments.Argument1.ToFloat64();
                ui64 = SlowFloat.ToUInt64(context, f64, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f64_to_i32":
            {
                f64 = arguments.Argument1.ToFloat64();
                i32 = SlowFloat.ToInt32(context, f64, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f64_to_i64":
            {
                f64 = arguments.Argument1.ToFloat64();
                i64 = SlowFloat.ToInt64(context, f64, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "extF80_to_ui32":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                ui32 = SlowFloat.ToUInt32(context, extF80, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "extF80_to_ui64":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                ui64 = SlowFloat.ToUInt64(context, extF80, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "extF80_to_i32":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                i32 = SlowFloat.ToInt32(context, extF80, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "extF80_to_i64":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                i64 = SlowFloat.ToInt64(context, extF80, roundingMode, exact);
                return new TestRunnerResult(i64, context.ExceptionFlags);
            }

            case "f128_to_ui32":
            {
                f128 = arguments.Argument1.ToFloat128();
                ui32 = SlowFloat.ToUInt32(context, f128, roundingMode, exact);
                return new TestRunnerResult(ui32, context.ExceptionFlags);
            }
            case "f128_to_ui64":
            {
                f128 = arguments.Argument1.ToFloat128();
                ui64 = SlowFloat.ToUInt64(context, f128, roundingMode, exact);
                return new TestRunnerResult(ui64, context.ExceptionFlags);
            }
            case "f128_to_i32":
            {
                f128 = arguments.Argument1.ToFloat128();
                i32 = SlowFloat.ToInt32(context, f128, roundingMode, exact);
                return new TestRunnerResult(i32, context.ExceptionFlags);
            }
            case "f128_to_i64":
            {
                f128 = arguments.Argument1.ToFloat128();
                i64 = SlowFloat.ToInt64(context, f128, roundingMode, exact);
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
        var context = runner.SlowFloatContext;
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
                f32 = SlowFloat.ToFloat32(context, f16);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "f16_to_f64":
            {
                f16 = arguments.Argument1.ToFloat16();
                f64 = SlowFloat.ToFloat64(context, f16);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "f16_to_extF80":
            {
                f16 = arguments.Argument1.ToFloat16();
                extF80 = SlowFloat.ToExtFloat80(context, f16);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "f16_to_f128":
            {
                f16 = arguments.Argument1.ToFloat16();
                f128 = SlowFloat.ToFloat128(context, f16);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "f32_to_f16":
            {
                f32 = arguments.Argument1.ToFloat32();
                f16 = SlowFloat.ToFloat16(context, f32);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "f32_to_f64":
            {
                f32 = arguments.Argument1.ToFloat32();
                f64 = SlowFloat.ToFloat64(context, f32);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "f32_to_extF80":
            {
                f32 = arguments.Argument1.ToFloat32();
                extF80 = SlowFloat.ToExtFloat80(context, f32);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "f32_to_f128":
            {
                f32 = arguments.Argument1.ToFloat32();
                f128 = SlowFloat.ToFloat128(context, f32);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "f64_to_f16":
            {
                f64 = arguments.Argument1.ToFloat64();
                f16 = SlowFloat.ToFloat16(context, f64);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "f64_to_f32":
            {
                f64 = arguments.Argument1.ToFloat64();
                f32 = SlowFloat.ToFloat32(context, f64);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "f64_to_extF80":
            {
                f64 = arguments.Argument1.ToFloat64();
                extF80 = SlowFloat.ToExtFloat80(context, f64);
                return new TestRunnerResult(extF80, context.ExceptionFlags);
            }
            case "f64_to_f128":
            {
                f64 = arguments.Argument1.ToFloat64();
                f128 = SlowFloat.ToFloat128(context, f64);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "extF80_to_f16":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                f16 = SlowFloat.ToFloat16(context, extF80);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "extF80_to_f32":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                f32 = SlowFloat.ToFloat32(context, extF80);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "extF80_to_f64":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                f64 = SlowFloat.ToFloat64(context, extF80);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "extF80_to_f128":
            {
                extF80 = arguments.Argument1.ToExtFloat80();
                f128 = SlowFloat.ToFloat128(context, extF80);
                return new TestRunnerResult(f128, context.ExceptionFlags);
            }

            case "f128_to_f16":
            {
                f128 = arguments.Argument1.ToFloat128();
                f16 = SlowFloat.ToFloat16(context, f128);
                return new TestRunnerResult(f16, context.ExceptionFlags);
            }
            case "f128_to_f32":
            {
                f128 = arguments.Argument1.ToFloat128();
                f32 = SlowFloat.ToFloat32(context, f128);
                return new TestRunnerResult(f32, context.ExceptionFlags);
            }
            case "f128_to_f64":
            {
                f128 = arguments.Argument1.ToFloat128();
                f64 = SlowFloat.ToFloat64(context, f128);
                return new TestRunnerResult(f64, context.ExceptionFlags);
            }
            case "f128_to_extF80":
            {
                f128 = arguments.Argument1.ToFloat128();
                extF80 = SlowFloat.ToExtFloat80(context, f128);
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
        var context = runner.SlowFloatContext;
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
                f16_z = SlowFloat.RoundToInt(context, f16_a, roundingMode, exact);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_roundToInt":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_z = SlowFloat.RoundToInt(context, f32_a, roundingMode, exact);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_roundToInt":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_z = SlowFloat.RoundToInt(context, f64_a, roundingMode, exact);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_roundToInt":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_z = SlowFloat.RoundToInt(context, extF80_a, roundingMode, exact);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_roundToInt":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_z = SlowFloat.RoundToInt(context, f128_a, roundingMode, exact);
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
        var context = runner.SlowFloatContext;
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
                f16_z = SlowFloat.Add(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_add":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = SlowFloat.Add(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_add":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = SlowFloat.Add(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_add":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = SlowFloat.Add(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_add":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = SlowFloat.Add(context, f128_a, f128_b);
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
        var context = runner.SlowFloatContext;
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
                f16_z = SlowFloat.Subtract(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_sub":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = SlowFloat.Subtract(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_sub":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = SlowFloat.Subtract(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_sub":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = SlowFloat.Subtract(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_sub":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = SlowFloat.Subtract(context, f128_a, f128_b);
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
        var context = runner.SlowFloatContext;
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
                f16_z = SlowFloat.Multiply(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_mul":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = SlowFloat.Multiply(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_mul":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = SlowFloat.Multiply(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_mul":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = SlowFloat.Multiply(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_mul":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = SlowFloat.Multiply(context, f128_a, f128_b);
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
        var context = runner.SlowFloatContext;
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
                f16_z = SlowFloat.MultiplyAndAdd(context, f16_a, f16_b, f16_c);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_mulAdd":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_c = arguments.Argument3.ToFloat32();
                f32_z = SlowFloat.MultiplyAndAdd(context, f32_a, f32_b, f32_c);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_mulAdd":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_c = arguments.Argument3.ToFloat64();
                f64_z = SlowFloat.MultiplyAndAdd(context, f64_a, f64_b, f64_c);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "f128_mulAdd":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_c = arguments.Argument3.ToFloat128();
                f128_z = SlowFloat.MultiplyAndAdd(context, f128_a, f128_b, f128_c);
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
        var context = runner.SlowFloatContext;
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
                f16_z = SlowFloat.Divide(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_div":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = SlowFloat.Divide(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_div":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = SlowFloat.Divide(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_div":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = SlowFloat.Divide(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_div":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = SlowFloat.Divide(context, f128_a, f128_b);
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
        var context = runner.SlowFloatContext;
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
                f16_z = SlowFloat.Modulus(context, f16_a, f16_b);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_rem":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                f32_z = SlowFloat.Modulus(context, f32_a, f32_b);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_rem":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                f64_z = SlowFloat.Modulus(context, f64_a, f64_b);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_rem":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                extF80_z = SlowFloat.Modulus(context, extF80_a, extF80_b);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_rem":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                f128_z = SlowFloat.Modulus(context, f128_a, f128_b);
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
        var context = runner.SlowFloatContext;
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
                f16_z = SlowFloat.SquareRoot(context, f16_a);
                return new TestRunnerResult(f16_z, context.ExceptionFlags);
            }
            case "f32_sqrt":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_z = SlowFloat.SquareRoot(context, f32_a);
                return new TestRunnerResult(f32_z, context.ExceptionFlags);
            }
            case "f64_sqrt":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_z = SlowFloat.SquareRoot(context, f64_a);
                return new TestRunnerResult(f64_z, context.ExceptionFlags);
            }
            case "extF80_sqrt":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_z = SlowFloat.SquareRoot(context, extF80_a);
                return new TestRunnerResult(extF80_z, context.ExceptionFlags);
            }
            case "f128_sqrt":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_z = SlowFloat.SquareRoot(context, f128_a);
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
        var context = runner.SlowFloatContext;
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
                z = SlowFloat.Equals(context, f16_a, f16_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_eq":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = SlowFloat.Equals(context, f32_a, f32_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_eq":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = SlowFloat.Equals(context, f64_a, f64_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_eq":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = SlowFloat.Equals(context, extF80_a, extF80_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_eq":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = SlowFloat.Equals(context, f128_a, f128_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            case "f16_eq_signaling":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = SlowFloat.Equals(context, f16_a, f16_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_eq_signaling":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = SlowFloat.Equals(context, f32_a, f32_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_eq_signaling":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = SlowFloat.Equals(context, f64_a, f64_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_eq_signaling":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = SlowFloat.Equals(context, extF80_a, extF80_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_eq_signaling":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = SlowFloat.Equals(context, f128_a, f128_b, signaling: true);
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
        var context = runner.SlowFloatContext;
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
                z = SlowFloat.LessThan(context, f16_a, f16_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_lt":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = SlowFloat.LessThan(context, f32_a, f32_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_lt":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = SlowFloat.LessThan(context, f64_a, f64_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_lt":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = SlowFloat.LessThan(context, extF80_a, extF80_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_lt":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = SlowFloat.LessThan(context, f128_a, f128_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            case "f16_lt_quiet":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = SlowFloat.LessThan(context, f16_a, f16_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_lt_quiet":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = SlowFloat.LessThan(context, f32_a, f32_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_lt_quiet":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = SlowFloat.LessThan(context, f64_a, f64_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_lt_quiet":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = SlowFloat.LessThan(context, extF80_a, extF80_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_lt_quiet":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = SlowFloat.LessThan(context, f128_a, f128_b, signaling: false);
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
        var context = runner.SlowFloatContext;
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
                z = SlowFloat.LessThanOrEquals(context, f16_a, f16_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_le":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = SlowFloat.LessThanOrEquals(context, f32_a, f32_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_le":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = SlowFloat.LessThanOrEquals(context, f64_a, f64_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_le":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = SlowFloat.LessThanOrEquals(context, extF80_a, extF80_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_le":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = SlowFloat.LessThanOrEquals(context, f128_a, f128_b, signaling: true);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            case "f16_le_quiet":
            {
                f16_a = arguments.Argument1.ToFloat16();
                f16_b = arguments.Argument2.ToFloat16();
                z = SlowFloat.LessThanOrEquals(context, f16_a, f16_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f32_le_quiet":
            {
                f32_a = arguments.Argument1.ToFloat32();
                f32_b = arguments.Argument2.ToFloat32();
                z = SlowFloat.LessThanOrEquals(context, f32_a, f32_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f64_le_quiet":
            {
                f64_a = arguments.Argument1.ToFloat64();
                f64_b = arguments.Argument2.ToFloat64();
                z = SlowFloat.LessThanOrEquals(context, f64_a, f64_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "extF80_le_quiet":
            {
                extF80_a = arguments.Argument1.ToExtFloat80();
                extF80_b = arguments.Argument2.ToExtFloat80();
                z = SlowFloat.LessThanOrEquals(context, extF80_a, extF80_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }
            case "f128_le_quiet":
            {
                f128_a = arguments.Argument1.ToFloat128();
                f128_b = arguments.Argument2.ToFloat128();
                z = SlowFloat.LessThanOrEquals(context, f128_a, f128_b, signaling: false);
                return new TestRunnerResult(z, context.ExceptionFlags);
            }

            default:
                throw new NotImplementedException();
        }
    }
}
