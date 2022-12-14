namespace Tommunism.SoftFloat.Tests;

internal static class Program
{
    public static string GeneratorCommandPath { get; private set; } = "testfloat_gen";

    public static string VerifierCommandPath { get; private set; } = "testfloat_ver";

    // NOTE: If ARG_1 or ARG_2 bits are not set, then three operands is implied (e.g., *_mulAdd are three operand functions).
    public const uint ARG_1 = FunctionInfoFlags.ARG_UNARY;
    public const uint ARG_2 = FunctionInfoFlags.ARG_BINARY;
    public const uint ARG_3 = FunctionInfoFlags.ARG_TERNARY;
    public const uint ARG_R = FunctionInfoFlags.ARG_ROUNDINGMODE;
    public const uint ARG_E = FunctionInfoFlags.ARG_EXACT;
    public const uint EFF_P = FunctionInfoFlags.EFF_ROUNDINGPRECISION; // only used by ExtFloat80
    public const uint EFF_R = FunctionInfoFlags.EFF_ROUNDINGMODE;
    public const uint EFF_T = FunctionInfoFlags.EFF_TININESSMODE;
    public const uint EFF_T_REDP = FunctionInfoFlags.EFF_TININESSMODE_REDUCEDPREC; // only used by ExtFloat80

    // This table was copied from functionInfos.c in TestFloat-3e.
    public static readonly Dictionary<string, FunctionInfoFlags> FunctionInfos = new()
    {
        { "ui32_to_f16",    ARG_1 | EFF_R },
        { "ui32_to_f32",    ARG_1 | EFF_R },
        { "ui32_to_f64",    ARG_1         },
        { "ui32_to_extF80", ARG_1         },
        { "ui32_to_f128",   ARG_1         },
        { "ui64_to_f16",    ARG_1 | EFF_R },
        { "ui64_to_f32",    ARG_1 | EFF_R },
        { "ui64_to_f64",    ARG_1 | EFF_R },
        { "ui64_to_extF80", ARG_1         },
        { "ui64_to_f128",   ARG_1         },
        { "i32_to_f16",     ARG_1 | EFF_R },
        { "i32_to_f32",     ARG_1 | EFF_R },
        { "i32_to_f64",     ARG_1         },
        { "i32_to_extF80",  ARG_1         },
        { "i32_to_f128",    ARG_1         },
        { "i64_to_f16",     ARG_1 | EFF_R },
        { "i64_to_f32",     ARG_1 | EFF_R },
        { "i64_to_f64",     ARG_1 | EFF_R },
        { "i64_to_extF80",  ARG_1         },
        { "i64_to_f128",    ARG_1         },

        { "f16_to_ui32", ARG_1 | ARG_R | ARG_E },
        { "f16_to_ui64", ARG_1 | ARG_R | ARG_E },
        { "f16_to_i32",  ARG_1 | ARG_R | ARG_E },
        { "f16_to_i64",  ARG_1 | ARG_R | ARG_E },
        //{ "f16_to_ui32_r_minMag", ARG_1 | ARG_E },
        //{ "f16_to_ui64_r_minMag", ARG_1 | ARG_E },
        //{ "f16_to_i32_r_minMag",  ARG_1 | ARG_E },
        //{ "f16_to_i64_r_minMag",  ARG_1 | ARG_E },
        { "f16_to_f32",    ARG_1 },
        { "f16_to_f64",    ARG_1 },
        { "f16_to_extF80", ARG_1 },
        { "f16_to_f128",   ARG_1 },
        { "f16_roundToInt", ARG_1 | ARG_R | ARG_E },
        { "f16_add",          ARG_2 | EFF_R         },
        { "f16_sub",          ARG_2 | EFF_R         },
        { "f16_mul",          ARG_2 | EFF_R | EFF_T },
        { "f16_mulAdd",               EFF_R | EFF_T },
        { "f16_div",          ARG_2 | EFF_R         },
        { "f16_rem",          ARG_2                 },
        { "f16_sqrt",         ARG_1 | EFF_R         },
        { "f16_eq",           ARG_2                 },
        { "f16_le",           ARG_2                 },
        { "f16_lt",           ARG_2                 },
        { "f16_eq_signaling", ARG_2                 },
        { "f16_le_quiet",     ARG_2                 },
        { "f16_lt_quiet",     ARG_2                 },

        { "f32_to_ui32", ARG_1 | ARG_R | ARG_E },
        { "f32_to_ui64", ARG_1 | ARG_R | ARG_E },
        { "f32_to_i32",  ARG_1 | ARG_R | ARG_E },
        { "f32_to_i64",  ARG_1 | ARG_R | ARG_E },
        //{ "f32_to_ui32_r_minMag", ARG_1 | ARG_E },
        //{ "f32_to_ui64_r_minMag", ARG_1 | ARG_E },
        //{ "f32_to_i32_r_minMag",  ARG_1 | ARG_E },
        //{ "f32_to_i64_r_minMag",  ARG_1 | ARG_E },
        { "f32_to_f16", ARG_1 | EFF_R | EFF_T },
        { "f32_to_f64",    ARG_1 },
        { "f32_to_extF80", ARG_1 },
        { "f32_to_f128",   ARG_1 },
        { "f32_roundToInt", ARG_1 | ARG_R | ARG_E },
        { "f32_add",          ARG_2 | EFF_R         },
        { "f32_sub",          ARG_2 | EFF_R         },
        { "f32_mul",          ARG_2 | EFF_R | EFF_T },
        { "f32_mulAdd",               EFF_R | EFF_T },
        { "f32_div",          ARG_2 | EFF_R         },
        { "f32_rem",          ARG_2                 },
        { "f32_sqrt",         ARG_1 | EFF_R         },
        { "f32_eq",           ARG_2                 },
        { "f32_le",           ARG_2                 },
        { "f32_lt",           ARG_2                 },
        { "f32_eq_signaling", ARG_2                 },
        { "f32_le_quiet",     ARG_2                 },
        { "f32_lt_quiet",     ARG_2                 },

        { "f64_to_ui32", ARG_1 | ARG_R | ARG_E },
        { "f64_to_ui64", ARG_1 | ARG_R | ARG_E },
        { "f64_to_i32",  ARG_1 | ARG_R | ARG_E },
        { "f64_to_i64",  ARG_1 | ARG_R | ARG_E },
        //{ "f64_to_ui32_r_minMag", ARG_1 | ARG_E },
        //{ "f64_to_ui64_r_minMag", ARG_1 | ARG_E },
        //{ "f64_to_i32_r_minMag",  ARG_1 | ARG_E },
        //{ "f64_to_i64_r_minMag",  ARG_1 | ARG_E },
        { "f64_to_f16", ARG_1 | EFF_R | EFF_T },
        { "f64_to_f32", ARG_1 | EFF_R | EFF_T },
        { "f64_to_extF80", ARG_1 },
        { "f64_to_f128",   ARG_1 },
        { "f64_roundToInt", ARG_1 | ARG_R | ARG_E },
        { "f64_add",          ARG_2 | EFF_R         },
        { "f64_sub",          ARG_2 | EFF_R         },
        { "f64_mul",          ARG_2 | EFF_R | EFF_T },
        { "f64_mulAdd",               EFF_R | EFF_T },
        { "f64_div",          ARG_2 | EFF_R         },
        { "f64_rem",          ARG_2                 },
        { "f64_sqrt",         ARG_1 | EFF_R         },
        { "f64_eq",           ARG_2                 },
        { "f64_le",           ARG_2                 },
        { "f64_lt",           ARG_2                 },
        { "f64_eq_signaling", ARG_2                 },
        { "f64_le_quiet",     ARG_2                 },
        { "f64_lt_quiet",     ARG_2                 },

        { "extF80_to_ui32", ARG_1 | ARG_R | ARG_E },
        { "extF80_to_ui64", ARG_1 | ARG_R | ARG_E },
        { "extF80_to_i32",  ARG_1 | ARG_R | ARG_E },
        { "extF80_to_i64",  ARG_1 | ARG_R | ARG_E },
        //{ "extF80_to_ui32_r_minMag", ARG_1 | ARG_E },
        //{ "extF80_to_ui64_r_minMag", ARG_1 | ARG_E },
        //{ "extF80_to_i32_r_minMag",  ARG_1 | ARG_E },
        //{ "extF80_to_i64_r_minMag",  ARG_1 | ARG_E },
        { "extF80_to_f16", ARG_1 | EFF_R | EFF_T },
        { "extF80_to_f32", ARG_1 | EFF_R | EFF_T },
        { "extF80_to_f64", ARG_1 | EFF_R | EFF_T },
        { "extF80_to_f128", ARG_1 },
        { "extF80_roundToInt", ARG_1 | ARG_R | ARG_E },
        { "extF80_add",          ARG_2 | EFF_P | EFF_R         | EFF_T_REDP },
        { "extF80_sub",          ARG_2 | EFF_P | EFF_R         | EFF_T_REDP },
        { "extF80_mul",          ARG_2 | EFF_P | EFF_R | EFF_T | EFF_T_REDP },
        { "extF80_div",          ARG_2 | EFF_P | EFF_R         | EFF_T_REDP },
        { "extF80_rem",          ARG_2                                      },
        { "extF80_sqrt",         ARG_1 | EFF_P | EFF_R                      },
        { "extF80_eq",           ARG_2                                      },
        { "extF80_le",           ARG_2                                      },
        { "extF80_lt",           ARG_2                                      },
        { "extF80_eq_signaling", ARG_2                                      },
        { "extF80_le_quiet",     ARG_2                                      },
        { "extF80_lt_quiet",     ARG_2                                      },

        { "f128_to_ui32", ARG_1 | ARG_R | ARG_E },
        { "f128_to_ui64", ARG_1 | ARG_R | ARG_E },
        { "f128_to_i32",  ARG_1 | ARG_R | ARG_E },
        { "f128_to_i64",  ARG_1 | ARG_R | ARG_E },
        //{ "f128_to_ui32_r_minMag", ARG_1 | ARG_E },
        //{ "f128_to_ui64_r_minMag", ARG_1 | ARG_E },
        //{ "f128_to_i32_r_minMag",  ARG_1 | ARG_E },
        //{ "f128_to_i64_r_minMag",  ARG_1 | ARG_E },
        { "f128_to_f16",    ARG_1 | EFF_R | EFF_T },
        { "f128_to_f32",    ARG_1 | EFF_R | EFF_T },
        { "f128_to_f64",    ARG_1 | EFF_R | EFF_T },
        { "f128_to_extF80", ARG_1 | EFF_R | EFF_T },
        { "f128_roundToInt", ARG_1 | ARG_R | ARG_E },
        { "f128_add",          ARG_2 | EFF_R         },
        { "f128_sub",          ARG_2 | EFF_R         },
        { "f128_mul",          ARG_2 | EFF_R | EFF_T },
        { "f128_mulAdd",               EFF_R | EFF_T },
        { "f128_div",          ARG_2 | EFF_R         },
        { "f128_rem",          ARG_2                 },
        { "f128_sqrt",         ARG_1 | EFF_R         },
        { "f128_eq",           ARG_2                 },
        { "f128_le",           ARG_2                 },
        { "f128_lt",           ARG_2                 },
        { "f128_eq_signaling", ARG_2                 },
        { "f128_le_quiet",     ARG_2                 },
        { "f128_lt_quiet",     ARG_2                 },
    };

    // This is a map of all normal functions to equivalent generator types (and required number of operands). Using these should result in
    // much faster test generation, because the results do not need to be computed. Note that integer types always have an operand count of
    // one, but the generator does not allow the count to be specified as an argument (and thus the value is always zero).
    public static readonly Dictionary<string, (string, int)> GeneratorTypes = new()
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

    // Enumeration/option combination helper properties (lazy initialized).

    private static ExtFloat80RoundingPrecision[]? _extFloat80RoundingPrecisions;
    public static IReadOnlyList<ExtFloat80RoundingPrecision> ExtFloat80RoundingPrecisions =>
        _extFloat80RoundingPrecisions ??= Enum.GetValues<ExtFloat80RoundingPrecision>();

    private static RoundingMode[]? _roundingModes;
    public static IReadOnlyList<RoundingMode> RoundingModes => _roundingModes ??= Enum.GetValues<RoundingMode>();

    private static TininessMode[]? _tininessModes;
    public static IReadOnlyList<TininessMode> TininessModes => _tininessModes ??= Enum.GetValues<TininessMode>();

    private static bool[]? _exactValues;
    public static IReadOnlyList<bool> ExactModes => _exactValues ??= new bool[] { false, true };

    public static async Task<int> Main(string[] args)
    {
        // This is the original test runner. Single threaded and monolithic. Not great, but it does the job.
        var testRunner = new TestRunner
        {
            ConsoleDebug = 3, // see comments for property for details (for best speeds, use 0 or 1; use 2 if checking verifier STDERR or 3 if we want everything)
        };

        // This is a parallel test runner. Capable of utilizing many cores for running tests and verifying. Test generator is still single threaded though.
        var testRunner2 = new TestRunner2
        {
            //MaxVerifierProcesses = 1, // maybe easier to debug with a single thread
        };

        // Options for configuring test runs. Set common options here (individual test runs may change this slightly, especially state-specific properties).
        var options = new TestRunnerOptions
        {
            // Perform more exhaustive testing on lower bit count operations (level 2 potentially finds bugs that would normally be hidden
            // by a level 1 pass--but it takes a very long time to even generate the test cases for some of the functions).
            GeneratorLevel = 1,

            // Display all errors (may be a lot of results for "buggy" implementations).
            //MaxErrors = 0,

            // Set these to true if SoftFloat specializations match the implementation used by TestFloat.
            CheckNaNs = true,
            CheckInvalidIntegers = true,

            //Rounding = RoundingMode.NearEven,
            //Exact = true,
            //RoundingPrecisionExtFloat80 = ExtFloat80RoundingPrecision._80,
            //DetectTininess = TininessMode.BeforeRounding,
        };

        // TestFloat was compiled with a SoftFloat implementation using the X86-SSE specializations.
        SoftFloatSpecialize.Default = SoftFloatSpecialize.X86Sse.Default;

        // Let's run some actual tests.
        const string? testFunctionName = null; // if non-null, then only a single test function will be tested; otherwise, all tests will be tested
        var testFunctions = testFunctionName != null ? Enumerable.Repeat(testFunctionName, 1) : FunctionInfos.Keys;
        var failureCount = 0;
        foreach (var testFunction in testFunctions)
        {
            var testFunctionHandler = SoftFloatTests.Functions[testFunction];
            if (!await RunTestsAsync(testRunner2, options, testFunction, testFunctionHandler))
            //if (!RunTests(testRunner, options, testFunction, testFunctionHandler))
            {
                failureCount++;
            }
        }

        return failureCount == 0 ? 0 : 1;
    }

    // This uses the functions info and input arguments to generate all "useful" test configurations for a given function (algorithm mostly copied from the "-all" argument that can be passed into "testsoftfloat").
    public static IEnumerable<(ExtFloat80RoundingPrecision? RoundingPrecision, RoundingMode? Rounding, bool? Exact, TininessMode? Tininess)> GetTestConfigurations(
        string functionName, ExtFloat80RoundingPrecision? roundingPrecision = null, RoundingMode? rounding = null, bool? exact = null, TininessMode? detectTininess = null)
    {
        // Get function attributes.
        var functionAttributes = FunctionInfos[functionName];

        var stringBuilder = new ValueStringBuilder(stackalloc char[128]);

        // Check for configuration overrides.
        var hasExplicitRoundingPrecision = roundingPrecision.HasValue;
        var hasExplicitRounding = rounding.HasValue;
        var hasExplicitExactness = exact.HasValue;
        var hasExplicitTininess = detectTininess.HasValue;

        // Determine which options are needed.
        // NOTE: Tininess may depend on the rounding precision, so it is calculated during precision enumeration.
        var needsRoundingPrecision = functionAttributes.AffectedByRoundingPrecision;
        var needsRounding = functionAttributes.HasArgumentRoundingMode || functionAttributes.AffectedByRoundingMode;
        var needsExact = functionAttributes.HasArgumentExact;

        static IEnumerable<ExtFloat80RoundingPrecision?> GetTestRoundingPrecisionValues(ExtFloat80RoundingPrecision? roundingPrecision, bool required) =>
            (!required || roundingPrecision.HasValue)
            ? Enumerable.Repeat(roundingPrecision, 1)
            : ExtFloat80RoundingPrecisions.Select(x => (ExtFloat80RoundingPrecision?)x);

        static IEnumerable<RoundingMode?> GetTestRoundingModes(RoundingMode? rounding, bool required) =>
            (!required || rounding.HasValue)
            ? Enumerable.Repeat(rounding, 1)
            : RoundingModes.Select(x => (RoundingMode?)x);

        static IEnumerable<bool?> GetTestExactModes(bool? exact, bool required) =>
            (!required || exact.HasValue)
            ? Enumerable.Repeat(exact, 1)
            : ExactModes.Select(x => (bool?)x);

        static IEnumerable<TininessMode?> GetTestTininessModes(TininessMode? detectTininess, bool required) =>
            (!required || detectTininess.HasValue)
            ? Enumerable.Repeat(detectTininess, 1)
            : TininessModes.Select(x => (TininessMode?)x);

        foreach (var roundingPrecisionValue in GetTestRoundingPrecisionValues(roundingPrecision, needsRoundingPrecision))
        {
            // Cache this sooner than later -- that way it doesn't need to be recalculated as many times.
            var needsTininess = functionAttributes.AffectedByTininessMode ||
                (functionAttributes.AffectedByTininessWithReducedPrecision && roundingPrecisionValue != ExtFloat80RoundingPrecision._80);

            foreach (var roundingValue in GetTestRoundingModes(rounding, needsRounding))
            {
                foreach (var exactValue in GetTestExactModes(exact, needsExact))
                {
                    foreach (var tininessValue in GetTestTininessModes(detectTininess, needsTininess))
                    {
                        // Return this test configuration options.
                        yield return (
                            roundingPrecisionValue,
                            roundingValue,
                            exactValue,
                            tininessValue
                        );
                    }
                }
            }
        }
    }

    private static bool RunTests(TestRunner runner, TestRunnerOptions options, string functionName, Func<TestRunnerState, TestRunnerArguments, TestRunnerResult>? functionHandler, bool stopOnFailure = false)
    {
        // Skip unimplemented functions.
        if (functionHandler == null)
            return true;

        // Enumerate all useful test configurations for given function.
        var failureCount = 0;
        foreach (var config in GetTestConfigurations(functionName, options.RoundingPrecisionExtFloat80, options.Rounding, options.Exact, options.DetectTininess))
        {
            // Set configuration options.
            (options.RoundingPrecisionExtFloat80, options.Rounding, options.Exact, options.DetectTininess) = config;

            PrintTestName(options, functionName);
            if (runner.Run(functionName, functionHandler, options) != 0)
            {
                failureCount++;
                if (stopOnFailure)
                    break;
            }
        }

        // Did all of the test configurations pass?
        return failureCount == 0;
    }

    // TODO: Add support for CancellationToken?
    private static async Task<bool> RunTestsAsync(TestRunner2 runner, TestRunnerOptions options, string functionName, Func<TestRunnerState, TestRunnerArguments, TestRunnerResult>? functionHandler, bool stopOnFailure = false)
    {
        // Skip unimplemented functions.
        if (functionHandler == null)
            return true;

        runner.Options = options;
        runner.TestFunction = functionName;
        runner.TestHandler = functionHandler;

        // Optimize the generator by generating the input operands only (skip calculating the result--it is ignored in this program and the
        // verifier will calculate it anyways). This seems to have a fairly noticable impact on heavier operations with LOTS of generated
        // test cases).
        (runner.GeneratorTypeOrFunction, runner.GeneratorTypeOperandCount) = GeneratorTypes[functionName];

        // Enumerate all useful test configurations for given function.
        var failureCount = 0;
        foreach (var config in GetTestConfigurations(functionName, options.RoundingPrecisionExtFloat80, options.Rounding, options.Exact, options.DetectTininess))
        {
            // Set configuration options.
            (options.RoundingPrecisionExtFloat80, options.Rounding, options.Exact, options.DetectTininess) = config;

            var startTime = DateTime.UtcNow;
            runner.TotalTestCount = 0;
            PrintTestName(options, functionName);
            if (!await runner.RunAsync(CancellationToken.None))
            {
                failureCount++;
                if (stopOnFailure)
                    break;
            }

            // Use DateTime instead of Stopwatch in case this finishes on a different thread. (The internal Stopwatch implementation may
            // not be guaranteed to have consistent timing between CPU cores. Though I think newer operating systems try to sync them.)
            var elapsedTime = DateTime.UtcNow - startTime;
            Console.WriteLine($"Ran a total of {runner.TotalTestCount:#,0} tests ({elapsedTime.TotalSeconds:f3} seconds).");
        }

        // Did all of the test configurations pass?
        return failureCount == 0;
    }

    // Attempts to reduce extra string allocations and formatting by using a low-level stack-allocated string builder instead.
    private static void PrintTestName(TestRunnerOptions options, string functionName)
    {
        var stringBuilder = new ValueStringBuilder(stackalloc char[256]);

        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append('=', 80);
        stringBuilder.Append(Environment.NewLine);
        stringBuilder.Append("Running tests: ");
        AppendTestDescription(ref stringBuilder, functionName, options.RoundingPrecisionExtFloat80, options.Rounding, options.Exact, options.DetectTininess);
        stringBuilder.Append(Environment.NewLine);
        Console.Out.Write(stringBuilder.AsSpan());
        Debug.Write(stringBuilder.AsSpan().ToString()); // Too bad this requires a string allocation.
    }

    private static void AppendTestDescription(ref ValueStringBuilder builder, string functionName, ExtFloat80RoundingPrecision? roundingPrecision, RoundingMode? rounding, bool? exact, TininessMode? tininess)
    {
        // This format should more-or-less match what TestFloat generates.

        builder.Append(functionName);

        if (roundingPrecision.HasValue)
        {
            builder.Append(roundingPrecision.Value switch
            {
                ExtFloat80RoundingPrecision._80 => ", precision 80",
                ExtFloat80RoundingPrecision._32 => ", precision 32",
                ExtFloat80RoundingPrecision._64 => ", precision 64",
                _ => $", precision {roundingPrecision.Value}"
            });
        }

        if (rounding.HasValue)
        {
            builder.Append(rounding.Value switch
            {
                RoundingMode.NearEven => ", rounding near_even",
                RoundingMode.NearMaxMag => ", rounding near_maxMag",
                RoundingMode.MinMag => ", rounding minMag",
                RoundingMode.Min => ", rounding min",
                RoundingMode.Max => ", rounding max",
                RoundingMode.Odd => ", rounding odd",
                _ => $", rounding {rounding.Value}"
            });
        }

        if (tininess.HasValue)
        {
            builder.Append(tininess.Value switch
            {
                TininessMode.BeforeRounding => ", tininess before rounding",
                TininessMode.AfterRounding => ", tininess after rounding",
                _ => $", tininess {tininess.Value}"
            });
        }

        if (exact.HasValue)
        {
            builder.Append(exact.Value
                ? ", exact"
                : ", not exact");
        }
    }

    // How to debug arguments inside test runs (requires hard-coding -- conditional breakpoints don't work with the ReadOnlySpan<char> arguments used for parsing):
    //if (arguments.Argument1 == TestRunnerArgument.ParseFloat16("+00.001") && arguments.Argument2 == TestRunnerArgument.ParseFloat16("-00.001"))
    //    Debugger.Break();
}
