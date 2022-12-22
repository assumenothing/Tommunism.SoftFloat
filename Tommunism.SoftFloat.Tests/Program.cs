using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;

namespace Tommunism.SoftFloat.Tests;

internal static class Program
{
    public static string GeneratorCommandPath { get; private set; } = "testfloat_gen";

    public static string VerifierCommandPath { get; private set; } = "testfloat_ver";

    // The builtin generator is theoretically faster, due to being able to generate test cases in parallel. Note that the builtin generator
    // will generate different test cases than TestFloat's generator, because it uses a completely different random number generator.
    public static bool UseBuiltinGenerator { get; private set; } = true;

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

    /// <summary>
    /// Options for configuring test runs. Set common options here (individual test runs may change this slightly, especially
    /// state-specific properties).
    /// </summary>
    public static TestRunnerOptions Options { get; private set; } = new();

    public static uint GeneratorSeed => Options.GeneratorSeed ?? 1;

    private static readonly ulong[] _threefrySeed4x64 = new ulong[4];
    public static ReadOnlySpan<ulong> ThreefrySeed4x64 => _threefrySeed4x64;

    // NOTE: These must be function names (see FunctionInfos), not types (as there is no way to test them directly).
    // Using a hash set to avoid duplicate tests being run.
    public static HashSet<string> TestFunctions { get; private set; } = new();

    // NOTE: The higher this value is, the longer it will take to start verifying (as the generator has to generate this many tests before
    // starting executing tests and verification).
    public static int? MaxTestsPerThread { get; private set; } = null;

    // NOTE: If zero, then the max number of threads is the number of cores/threads on the CPU.
    public static int? MaxTestThreads { get; private set; } = null;

    // If true, then TestRunner (synchronous) will be used; otherwise, TestRunner2 (asynchronous) will be used. There may be some
    // variations between how they run, but it should be "okay".
    public static bool UseSynchronousTestRunner { get; private set; } = false;

    // If true, then the builtin SlowFloat implementation will be tested instead of the SoftFloat library.
    public static bool TestSlowFloat { get; private set; } = false;

    public static async Task<int> Main(string[] args)
    {
        // This is the original test runner. Single threaded and monolithic. Not great, but it does the job.
        var testRunner = new TestRunner
        {
            ConsoleDebug = 1,
        };

        // This is a parallel test runner. Capable of utilizing many threads for running tests and verifying. Test generator is still
        // single threaded though (which is probably one of the biggest reasons why these tests run so slowly).
        var testRunner2 = new TestRunner2
        {
        };

        // This technically depends on how TestFloat (and the associated SoftFloat library) was compiled. If running on an ARM processor,
        // then use the VFPv2 implementation by default. Otherwise, use the 8086-SSE implementation on 64-bit processors and the 8086
        // implementation on 32-bit processors by default (unfortunately there is no easy way of knowing what the verifier process is
        // using--that is why there is an argument to change it). This theoretically shouldn't matter if "-checkNaNs", "-checkInvInts", or
        // "-checkAll" arguments/options are not specified.
        SoftFloatSpecialize.Default = ArmBase.IsSupported
            ? SoftFloatSpecialize.ArmVfp2.Instance
            : Environment.Is64BitOperatingSystem ? SoftFloatSpecialize.X86Sse.Instance : SoftFloatSpecialize.X86.Instance;

        // Parse arguments.
        var argParserResult = ParseArguments(args);
        if (argParserResult != 0)
            return argParserResult;

        testRunner2.MaxTestThreads = MaxTestThreads ?? 0;
        testRunner2.MaxTestsPerProcess = MaxTestsPerThread ?? (UseBuiltinGenerator && Options.GeneratorLevel != 2 ? 200_000 : 1_000_000);

        // Generate the seed (key) to use for Threefry4x64 calls (if the builtin generator is going to be used). This is derived from the
        // options seed value. Try to get a good random bit distribution by using Random.NextBytes over the entire seed array's bytes.
        if (UseBuiltinGenerator)
        {
            var rng = new Random((int)GeneratorSeed);
            rng.NextBytes(MemoryMarshal.AsBytes(_threefrySeed4x64.AsSpan()));
        }

        // Let's run some actual tests.
        var failureCount = 0;
        foreach (var testFunction in TestFunctions)
        {
            var testFunctionHandler = TestSlowFloat
                ? SlowFloatTests.Functions[testFunction]
                : SoftFloatTests.Functions[testFunction];
            if (UseSynchronousTestRunner)
            {
                if (!RunTests(testRunner, Options, testFunction, testFunctionHandler))
                {
                    failureCount++;
                }
            }
            else
            {
                if (!await RunTestsAsync(testRunner2, Options, testFunction, testFunctionHandler))
                {
                    failureCount++;
                }
            }
        }

        // Report whether or not there were any failures.
        return failureCount == 0 ? 0 : 1;
    }

    private static int ParseArguments(string[] args)
    {
        int i; // keep external in case there was an unknown argument error
        for (i = 0; i < args.Length; i++)
        {
            var arg = args[i].AsSpan();
            if (arg.Length > 0 && arg[0] == '-')
            {
                arg = arg[1..];
                if (arg.Equals("seed", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (args.Length <= i)
                    {
                        Console.Error.WriteLine($"ERROR: Missing {"seed"} value argument.");
                        return 1;
                    }

                    if (!uint.TryParse(args[i], NumberStyles.Integer & ~NumberStyles.AllowLeadingSign, null, out var seedValue))
                    {
                        Console.Error.WriteLine($"ERROR: Could not parse {"seed"} value: {args[i]}");
                        return 1;
                    }

                    Options.GeneratorSeed = seedValue;
                }
                else if (arg.Equals("level", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (args.Length <= i)
                    {
                        Console.Error.WriteLine($"ERROR: Missing level value argument.");
                        return 1;
                    }

                    if (!int.TryParse(args[i], NumberStyles.Integer, null, out var levelValue))
                    {
                        Console.Error.WriteLine($"ERROR: Could not parse level value: {args[i]}");
                        return 1;
                    }

                    Options.GeneratorLevel = levelValue;
                }
                else if (arg.Equals("level1", StringComparison.OrdinalIgnoreCase))
                {
                    Options.GeneratorLevel = 1;
                }
                else if (arg.Equals("level2", StringComparison.OrdinalIgnoreCase))
                {
                    Options.GeneratorLevel = 2;
                }
                else if (arg.Equals("n", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (args.Length <= i)
                    {
                        Console.Error.WriteLine($"ERROR: Missing {"test count"} value argument.");
                        return 1;
                    }

                    if (!long.TryParse(args[i], NumberStyles.Integer, null, out var countValue))
                    {
                        Console.Error.WriteLine($"ERROR: Could not parse {"test count"} value: {args[i]}");
                        return 1;
                    }

                    Options.GeneratorCount = countValue;
                }
                else if (arg.Equals("forever", StringComparison.OrdinalIgnoreCase))
                {
                    Options.GeneratorCount = 0;
                }
                else if (arg.Equals("errors", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (args.Length <= i)
                    {
                        Console.Error.WriteLine($"ERROR: Missing {"error count"} value argument.");
                        return 1;
                    }

                    if (!int.TryParse(args[i], NumberStyles.Integer, null, out var errorCountValue))
                    {
                        Console.Error.WriteLine($"ERROR: Could not parse {"error count"} value: {args[i]}");
                        return 1;
                    }

                    Options.MaxErrors = errorCountValue;
                }
                else if (arg.Equals("checkNaNs", StringComparison.OrdinalIgnoreCase))
                {
                    Options.CheckNaNs = true;
                }
                else if (arg.Equals("checkInvInts", StringComparison.OrdinalIgnoreCase))
                {
                    Options.CheckInvalidIntegers = true;
                }
                else if (arg.Equals("checkAll", StringComparison.OrdinalIgnoreCase))
                {
                    Options.CheckNaNs = true;
                    Options.CheckInvalidIntegers = true;
                }
                else if (arg.StartsWith("precision", StringComparison.OrdinalIgnoreCase))
                {
                    arg = arg["precision".Length..];
                    if (arg.SequenceEqual("32"))
                    {
                        Options.RoundingPrecisionExtFloat80 = ExtFloat80RoundingPrecision._32;
                    }
                    else if (arg.SequenceEqual("64"))
                    {
                        Options.RoundingPrecisionExtFloat80 = ExtFloat80RoundingPrecision._64;
                    }
                    else if (arg.SequenceEqual("80"))
                    {
                        Options.RoundingPrecisionExtFloat80 = ExtFloat80RoundingPrecision._80;
                    }
                    else
                    {
                        goto UnknownArgument;
                    }
                }
                else if (arg.StartsWith("r", StringComparison.OrdinalIgnoreCase))
                {
                    arg = arg["r".Length..];
                    if (arg.Equals("near_even", StringComparison.OrdinalIgnoreCase) ||
                        arg.Equals("neareven", StringComparison.OrdinalIgnoreCase) ||
                        arg.Equals("nearest_even", StringComparison.OrdinalIgnoreCase))
                    {
                        Options.Rounding = RoundingMode.NearEven;
                    }
                    else if (arg.Equals("minmag", StringComparison.OrdinalIgnoreCase))
                    {
                        Options.Rounding = RoundingMode.MinMag;
                    }
                    else if (arg.Equals("min", StringComparison.OrdinalIgnoreCase))
                    {
                        Options.Rounding = RoundingMode.Min;
                    }
                    else if (arg.Equals("max", StringComparison.OrdinalIgnoreCase))
                    {
                        Options.Rounding = RoundingMode.Max;
                    }
                    else if (arg.Equals("near_maxmag", StringComparison.OrdinalIgnoreCase) ||
                        arg.Equals("nearmaxmag", StringComparison.OrdinalIgnoreCase) ||
                        arg.Equals("nearest_maxmag", StringComparison.OrdinalIgnoreCase))
                    {
                        Options.Rounding = RoundingMode.NearMaxMag;
                    }
                    else if (arg.Equals("odd", StringComparison.OrdinalIgnoreCase))
                    {
                        Options.Rounding = RoundingMode.Odd;
                    }
                    else
                    {
                        goto UnknownArgument;
                    }
                }
                else if (arg.StartsWith("tininess", StringComparison.OrdinalIgnoreCase))
                {
                    arg = arg["tininess".Length..];
                    if (arg.Equals("before", StringComparison.OrdinalIgnoreCase))
                    {
                        Options.DetectTininess = TininessMode.BeforeRounding;
                    }
                    else if (arg.Equals("after", StringComparison.OrdinalIgnoreCase))
                    {
                        Options.DetectTininess = TininessMode.AfterRounding;
                    }
                    else
                    {
                        goto UnknownArgument;
                    }
                }
                else if (arg.Equals("notexact", StringComparison.OrdinalIgnoreCase))
                {
                    Options.Exact = false;
                }
                else if (arg.Equals("exact", StringComparison.OrdinalIgnoreCase))
                {
                    Options.Exact = true;
                }
                else if (arg.Equals("testThreads", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (args.Length <= i)
                    {
                        Console.Error.WriteLine($"ERROR: Missing {"max test thread count"} value argument.");
                        return 1;
                    }

                    if (!int.TryParse(args[i], NumberStyles.Integer, null, out var countValue))
                    {
                        Console.Error.WriteLine($"ERROR: Could not parse {"max test thread count"} value: {args[i]}");
                        return 1;
                    }

                    MaxTestThreads = countValue;
                }
                else if (arg.Equals("testCountPerThread", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (args.Length <= i)
                    {
                        Console.Error.WriteLine($"ERROR: Missing {"max test count per thread"} value argument.");
                        return 1;
                    }

                    if (!int.TryParse(args[i], NumberStyles.Integer, null, out var countValue))
                    {
                        Console.Error.WriteLine($"ERROR: Could not parse {"max test count per thread"} value: {args[i]}");
                        return 1;
                    }

                    MaxTestThreads = countValue;
                }
                else if (arg.Equals("synchronous", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("sync", StringComparison.OrdinalIgnoreCase))
                {
                    UseSynchronousTestRunner = true;
                }
                else if (arg.StartsWith("specialize", StringComparison.OrdinalIgnoreCase))
                {
                    arg = arg["specialize".Length..];
                    if (arg.StartsWith("8086") || arg.StartsWith("X86", StringComparison.OrdinalIgnoreCase))
                    {
                        arg = arg[(arg[0] == '8' ? "8086".Length : "X86".Length)..];
                        if (arg.IsEmpty)
                        {
                            SoftFloatSpecialize.Default = SoftFloatSpecialize.X86.Instance;
                        }
                        else
                        {
                            // Hyphen between words is optional.
                            if (arg[0] == '-')
                                arg = arg[1..];

                            if (arg.Equals("SSE", StringComparison.OrdinalIgnoreCase))
                            {
                                SoftFloatSpecialize.Default = SoftFloatSpecialize.X86Sse.Instance;
                            }
                            else
                            {
                                goto UnknownArgument;
                            }
                        }
                    }
                    else if (arg.StartsWith("ARM", StringComparison.OrdinalIgnoreCase))
                    {
                        arg = arg["ARM".Length..];

                        // Hyphen between words is optional.
                        if (arg.Length > 0 && arg[0] == '-')
                            arg = arg[1..];

                        if (arg.StartsWith("VFPv2", StringComparison.OrdinalIgnoreCase) ||
                            arg.StartsWith("VFP2", StringComparison.OrdinalIgnoreCase))
                        {
                            arg = arg[(arg.StartsWith("VFPv2", StringComparison.OrdinalIgnoreCase) ? "VFPv2".Length : "VFP2".Length)..];

                            if (arg.IsEmpty)
                            {
                                SoftFloatSpecialize.Default = SoftFloatSpecialize.ArmVfp2.Instance;
                            }
                            else
                            {
                                // Hyphen between words is optional.
                                if (arg.Length > 0 && arg[0] == '-')
                                    arg = arg[1..];

                                if (arg.Equals("defaultNaN", StringComparison.OrdinalIgnoreCase))
                                {
                                    SoftFloatSpecialize.Default = SoftFloatSpecialize.ArmVfp2DefaultNaN.Instance;
                                }
                                else
                                {
                                    goto UnknownArgument;
                                }
                            }
                        }
                        else
                        {
                            goto UnknownArgument;
                        }
                    }
                    else
                    {
                        goto UnknownArgument;
                    }
                }
                else if (arg.Equals("slowfloat", StringComparison.OrdinalIgnoreCase))
                {
                    TestSlowFloat = true;
                }
                else if (arg.Equals("genBuiltin", StringComparison.OrdinalIgnoreCase))
                {
                    UseBuiltinGenerator = true;
                }
                else if (arg.Equals("genCmdPath", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("genCmd", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("generatorCommandPath", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("generatorCommand", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (args.Length <= i)
                    {
                        Console.Error.WriteLine($"ERROR: Missing {"generator command path"} value argument.");
                        return 1;
                    }

                    GeneratorCommandPath = args[i];
                }
                else if (arg.Equals("verCmdPath", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("verCmd", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("verifierCommandPath", StringComparison.OrdinalIgnoreCase) ||
                    arg.Equals("verifierCommand", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (args.Length <= i)
                    {
                        Console.Error.WriteLine($"ERROR: Missing {"verifier command path"} value argument.");
                        return 1;
                    }

                    VerifierCommandPath = args[i];
                }
                else
                {
                    goto UnknownArgument;
                }
            }
            else if (arg.StartsWith("all", StringComparison.OrdinalIgnoreCase))
            {
                arg = arg["all".Length..];
                if (arg.IsEmpty)
                {
                    // All functions.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly).Select(x => x.Key));
                }
                else if (arg.SequenceEqual("1"))
                {
                    // Only functions with a single argument.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.ArgumentCount == 1).Select(x => x.Key));
                }
                else if (arg.SequenceEqual("2"))
                {
                    // Only functions with two arguments.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.ArgumentCount == 2).Select(x => x.Key));
                }
                else if (arg.SequenceEqual("3"))
                {
                    // Only functions with three arguments.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.ArgumentCount == 3).Select(x => x.Key));
                }
                else if (arg.Equals("_ui32", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the UInt32 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsUInt32).Select(x => x.Key));
                }
                else if (arg.Equals("_ui64", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the UInt64 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsUInt64).Select(x => x.Key));
                }
                else if (arg.Equals("_i32", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the Int32 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsInt32).Select(x => x.Key));
                }
                else if (arg.Equals("_i64", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the Int64 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsInt64).Select(x => x.Key));
                }
                else if (arg.Equals("_f16", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the Float16 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsFloat16).Select(x => x.Key));
                }
                else if (arg.Equals("_f32", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the Float32 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsFloat32).Select(x => x.Key));
                }
                else if (arg.Equals("_f64", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the Float64 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsFloat64).Select(x => x.Key));
                }
                else if (arg.Equals("_extF80", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the Float16 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsExtFloat80).Select(x => x.Key));
                }
                else if (arg.Equals("_f128", StringComparison.OrdinalIgnoreCase))
                {
                    // Only functions which are related to the Float64 type.
                    TestFunctions.UnionWith(FunctionInfo.Functions.Where(x => !x.Value.TestSoftFloatOnly && x.Value.IsFloat128).Select(x => x.Key));
                }
                else
                {
                    goto UnknownArgument;
                }
            }
            else if (FunctionInfo.Functions.ContainsKey(args[i]))
            {
                // Add specific test function.
                TestFunctions.Add(args[i]);
            }
            else
            {
                goto UnknownArgument;
            }
        }

        // Make sure there is at least one test function defined.
        if (TestFunctions.Count == 0)
        {
            Console.Error.WriteLine($"ERROR: Missing required test function argument(s).");
            return 1;
        }

        // If any of the test functions is "all", then replace test functions with all known functions.

        return 0;

    UnknownArgument:
        Console.Error.WriteLine($"ERROR: Unknown argument: {args[i]}");
        return 1;
    }

    // This uses the functions info and input arguments to generate all "useful" test configurations for a given function (algorithm mostly copied from the "-all" argument that can be passed into "testsoftfloat").
    public static IEnumerable<(ExtFloat80RoundingPrecision? RoundingPrecision, RoundingMode? Rounding, bool? Exact, TininessMode? Tininess)> GetTestConfigurations(
        string functionName, ExtFloat80RoundingPrecision? roundingPrecision = null, RoundingMode? rounding = null, bool? exact = null, TininessMode? detectTininess = null)
    {
        // Get function attributes.
        var functionAttributes = FunctionInfo.Functions[functionName];

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

        // Make a copy of the test options to make sure the original options don't get mangled.
        options = options.Clone();

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

        // Make a copy of the test options to make sure the original options don't get mangled.
        options = options.Clone();

        runner.Options = options;
        runner.TestFunction = functionName;
        runner.TestHandler = functionHandler;

        // Optimize the generator by generating the input operands only (skip calculating the result--it is ignored in this program and the
        // verifier will calculate it anyways). This seems to have a fairly noticable impact on heavier operations with LOTS of generated
        // test cases).
        (runner.GeneratorTypeOrFunction, runner.GeneratorTypeOperandCount) = FunctionInfo.GeneratorTypes[functionName];

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

        // Release any allocated memory.
        stringBuilder.Dispose();
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
}
