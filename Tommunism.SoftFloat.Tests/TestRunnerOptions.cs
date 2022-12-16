namespace Tommunism.SoftFloat.Tests;

internal sealed class TestRunnerOptions
{
    /// <summary>
    /// Determines the pseudo-random number generator seed to use when generating test cases.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-seed &lt;num&gt;</c> generator argument.
    /// </remarks>
    public uint? GeneratorSeed { get; set; } = null;

    /// <summary>
    /// Determines the level of testing the generator creates.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-level &lt;num&gt;</c> generator argument.
    /// </remarks>
    public int? GeneratorLevel { get; set; } = null;

    /// <summary>
    /// Specifies the number of test cases to generate. If null, the default number for the specified test is used. If zero (or less than zero), then the generator will generate test cases indefinitely.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-n &lt;num&gt;</c> or <c>-forever</c> generator arguments.
    /// </remarks>
    public int? GeneratorCount { get; set; } = null;

    /// <summary>
    /// Indicate that no more than the given number of errors should be reported for a single test run. If zero (or less than zero), then any number of errors can be reported. If null, then the default number of errors is used.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-errors &lt;num&gt;</c> verifier argument.
    /// </remarks>
    public int? MaxErrors { get; set; } = null;

    /// <summary>
    /// Verify the bitwise correctness of NaN results. These should match the official SoftFloat implmentation being used by TestFloat.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-checkNaNs</c> verifier argument.
    /// </remarks>
    public bool CheckNaNs { get; set; } = false;

    /// <summary>
    /// Verify the bitwise correctness of integer results of invalid operations. These should match the official SoftFloat implmentation being used by TestFloat.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-checkInvInts</c> verifier argument.
    /// </remarks>
    public bool CheckInvalidIntegers { get; set; } = false;

    /// <summary>
    /// Only applies to 80-bit double-extended-precision operations. Determines the rounding precision to use.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-precision&lt;num&gt;</c> verifier argument.
    /// </remarks>
    public ExtFloat80RoundingPrecision? RoundingPrecisionExtFloat80 { get; set; } = null;

    /// <summary>
    /// Specifies which rounding mode to use for functions which require rounding.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-r&lt;mode&gt;</c> verifier argument where mode is one of: <c>near_even</c>, <c>near_maxMag</c>, <c>minMag</c>, <c>min</c>, <c>max</c>, or <c>odd</c>.
    /// </remarks>
    public RoundingMode? Rounding { get; set; } = null;

    /// <summary>
    /// Specifies which tininess check should be used for functions which require a tininess setting.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-tininess&lt;mode&gt;</c> verifier argument where mode is one of: <c>before</c> or <c>after</c>.
    /// </remarks>
    public TininessMode? DetectTininess { get; set; } = null;

    /// <summary>
    /// Specifies whether functions that require rounding should raise the inexact exceltpion flag.
    /// </summary>
    /// <remarks>
    /// Uses the <c>-notexact</c> or <c>-exact</c> verifier arguments.
    /// </remarks>
    public bool? Exact { get; set; } = null;

    public void SetupGeneratorArguments(ICollection<string> arguments)
    {
        if (GeneratorSeed.HasValue)
        {
            arguments.Add("-seed");
            arguments.Add(GeneratorSeed.Value.ToString());
        }

        if (GeneratorLevel.HasValue)
        {
            arguments.Add("-level");
            arguments.Add(GeneratorLevel.Value.ToString());
        }

        if (GeneratorCount.HasValue)
        {
            if (GeneratorCount.Value <= 0)
            {
                arguments.Add("-forever");
            }
            else
            {
                arguments.Add("-n");
                arguments.Add(GeneratorCount.Value.ToString());
            }
        }

        SetupCommonArguments(arguments);
    }

    public void SetupVerifierArguments(ICollection<string> arguments)
    {
        if (MaxErrors.HasValue)
        {
            arguments.Add("-errors");
            arguments.Add(MaxErrors.Value.ToString());
        }

        if (CheckNaNs)
        {
            arguments.Add("-checkNaNs");
        }

        if (CheckInvalidIntegers)
        {
            arguments.Add("-checkInvInts");
        }

        SetupCommonArguments(arguments);
    }

    private void SetupCommonArguments(ICollection<string> arguments)
    {
        if (RoundingPrecisionExtFloat80.HasValue)
        {
            arguments.Add(RoundingPrecisionExtFloat80.Value switch
            {
                ExtFloat80RoundingPrecision._32 => "-precision32",
                ExtFloat80RoundingPrecision._64 => "-precision64",
                ExtFloat80RoundingPrecision._80 => "-precision80",
                _ => throw new InvalidOperationException("Invalid ExtFloat80 rounding precision value.")
            });
        }

        if (Rounding.HasValue)
        {
            arguments.Add(Rounding.Value switch
            {
                RoundingMode.NearEven => "-rnear_even",
                RoundingMode.NearMaxMag => "-rnear_maxMag",
                RoundingMode.MinMag => "-rminMag",
                RoundingMode.Min => "-rmin",
                RoundingMode.Max => "-rmax",
                RoundingMode.Odd => "-rodd",
                _ => throw new InvalidOperationException("Invalid rounding mode value.")
            });
        }

        if (DetectTininess.HasValue)
        {
            arguments.Add(DetectTininess.Value switch
            {
                TininessMode.BeforeRounding => "-tininessbefore",
                TininessMode.AfterRounding => "-tininessafter",
                _ => throw new InvalidOperationException("Invalid detect tininess mode.")
            });
        }

        if (Exact.HasValue)
        {
            arguments.Add(Exact.Value ? "-exact" : "-notexact");
        }
    }
}
