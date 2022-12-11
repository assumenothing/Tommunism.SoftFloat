namespace Tommunism.SoftFloat.Tests;

// TestRunner is an object, because there are different object types which can use this.
internal record TestRunnerState(object TestRunner, TestRunnerOptions Options, string TestFunction,
    Func<TestRunnerState, TestRunnerArguments, TestRunnerResult> TestFunctionHandler, bool AppendResultsToArguments = false)
{
    private SlowFloatContext? _slowFloatContext;
    public SlowFloatContext SlowFloatContext => _slowFloatContext ??= new SlowFloatContext();

    private SoftFloatContext? _softFloatContext;
    public SoftFloatContext SoftFloatContext => _softFloatContext ??= new SoftFloatContext();

    public void ResetContext(SlowFloatContext context)
    {
        context.DetectTininess = Options.DetectTininess ?? 0;
        context.RoundingMode = Options.Rounding ?? 0;
        context.RoundingPrecisionExtFloat80 = Options.RoundingPrecisionExtFloat80 ?? 0;
        context.ExceptionFlags = 0;
    }

    public void ResetContext(SoftFloatContext context)
    {
        context.DetectTininess = Options.DetectTininess ?? 0;
        context.Rounding = Options.Rounding ?? 0;
        context.RoundingPrecisionExtFloat80 = Options.RoundingPrecisionExtFloat80 ?? 0;
    }
}
