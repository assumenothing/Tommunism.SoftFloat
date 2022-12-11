namespace Tommunism.SoftFloat.Tests;

// From all appearances, the generator will only ever use at most three "input" arguments, plus the expected output and exception flags (if a function is specified).
internal record struct TestRunnerArguments(
    TestRunnerArgument Argument1 = default,
    TestRunnerArgument Argument2 = default,
    TestRunnerArgument Argument3 = default,
    TestRunnerArgument Argument4 = default,
    TestRunnerArgument Argument5 = default)
{
    public const int MaxArgumentCount = 5;

    public TestRunnerArgument this[int index]
    {
        get
        {
            return index switch
            {
                0 => Argument1,
                1 => Argument2,
                2 => Argument3,
                3 => Argument4,
                4 => Argument5,
                _ => default
            };
        }

        set
        {
            switch (index)
            {
                case 0:
                    Argument1 = value;
                    break;
                case 1:
                    Argument2 = value;
                    break;
                case 2:
                    Argument3 = value;
                    break;
                case 3:
                    Argument4 = value;
                    break;
                case 4:
                    Argument5 = value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    public int Count =>
        Argument1.Kind is TestRunnerArgumentKind.None ? 0 :
        Argument2.Kind is TestRunnerArgumentKind.None ? 1 :
        Argument3.Kind is TestRunnerArgumentKind.None ? 2 :
        Argument4.Kind is TestRunnerArgumentKind.None ? 3 :
        Argument5.Kind is TestRunnerArgumentKind.None ? 4 :
                                                        5;

    // Replaces or adds the last two arguments this arguments set with the given results.
    public void SetResult(TestRunnerResult result, bool appendResult = false)
    {
        var count = Count;

        int resultIndex, exceptionFlagsIndex;
        if (appendResult)
        {
            if (count > MaxArgumentCount - 2)
                throw new InvalidOperationException("Input arguments has too many arguments assigned to store result.");

            resultIndex = count;
            exceptionFlagsIndex = count + 1;
        }
        else
        {
            if (count < 2)
                throw new InvalidOperationException("Input arguments does not have enough arguments assigned to store result.");

            resultIndex = count - 2;
            exceptionFlagsIndex = count - 1;
        }

        this[resultIndex] = result.Value;
        this[exceptionFlagsIndex] = new TestRunnerArgument(result.ExceptionFlags);
    }

    public static bool TryParse(ReadOnlySpan<char> span, out TestRunnerArguments value)
    {
        span = span.Trim();
        value = default;

        int argIndex = 0;
        while (!span.IsEmpty)
        {
            var lastSpaceIndex = span.IndexOf(' ');
            ReadOnlySpan<char> argSpan;
            if (lastSpaceIndex >= 0)
            {
                argSpan = span[..lastSpaceIndex];
                span = span[(lastSpaceIndex + 1)..].TrimStart();
            }
            else
            {
                argSpan = span;
                span = ReadOnlySpan<char>.Empty;
            }

            // Make sure arguments count does not exceed maximum number of supported arguments in this record.
            if (argIndex >= MaxArgumentCount)
            {
                value = default;
                return false;
            }

            // Try to parse the argument.
            if (!TestRunnerArgument.TryParse(argSpan, out var arg))
            {
                value = default;
                return false;
            }

            value[argIndex] = arg;
            argIndex++;
        }

        return true;
    }

    internal void WriteTo(ref ValueStringBuilder builder)
    {
        for (var i = 0; i < MaxArgumentCount; i++)
        {
            var arg = this[i];
            if (arg.Kind is TestRunnerArgumentKind.None)
                break;

            if (i != 0)
                builder.Append(' ');

            arg.WriteTo(ref builder);
        }
    }
}
