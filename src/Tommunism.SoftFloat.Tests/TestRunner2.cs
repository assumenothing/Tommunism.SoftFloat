using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace Tommunism.SoftFloat.Tests;

// TODO: Use a single thread only when MaxVerifierProcesses is set to 1. This should help when debugging as there aren't multiple tasks
// getting in the way.

// TODO: Share the semaphore between multiple run operations? This could allow for greater concurrency when running multiple
// configurations. But it would be nice if there was a method for waiting until the previous generator finishes starting run/verify tasks
// before starting the next batch of test runs.

// TODO: Use System.Collections.Concurrent.Partitioner for scheduling the work? This might result in better throughput, possibly at the end
// when some of the longer running tasks are remaining (e.g., modulus takes more variable amounts of time than the others).

internal class TestRunner2
{
    #region Fields

    private TestRunnerOptions? _options;

    // Recycle results arrays so they can be reused for multiple verifier tasks (should reduce GC overhead for the large object heap).
    private readonly ConcurrentBag<TestRunnerArguments[]> _testArgsPool = new();

    #endregion

    #region Properties

    // If not set, then then Environment.ProcessorCount is used. Technically this could probably be one less (because the generator thread
    // may take a long time too), but this assumes that performing the test and verifying will take longer than the generator. The
    // generator will probably also wait for very large numbers of tests to complete before generating more tests (depends on the generator
    // process' output buffer size).
    public int MaxTestThreads { get; set; } = 0;

    // Used to split generated tests between multiple execution/verifier threads.
    public int MaxTestsPerProcess { get; set; } = Program.UseBuiltinGenerator ? 100_000 : 1_000_000; // increasing this adds significant memory costs, but should reduce the number of tasks/processes which have to be spawned

    public TestRunnerOptions Options
    {
        get => _options ??= new(); // lazy initialized
        set => _options = value;
    }

    // NOTE: If this is different than the TestFunction, then results will be appended (as it is assumed that this property is a type code instead).
    public string? GeneratorTypeOrFunction { get; set; } = null;

    public int GeneratorTypeOperandCount { get; set; } = 0;

    public string? VerifierFunction { get; set; } = null;

    public string TestFunction { get; set; } = string.Empty;

    public Func<TestRunnerState, TestRunnerArguments, TestRunnerResult>? TestHandler { get; set; }

    // Allow setter so it can be reset back to zero between test runs (if that's what the caller wants to do).
    public long TotalTestCount { get; set; } = 0;

    #endregion

    #region Methods

    public bool Run() => Run(CancellationToken.None);

    public bool Run(CancellationToken cancellationToken)
    {
        return Program.UseBuiltinGenerator
            ? RunGeneratorBuiltin(cancellationToken)
            : RunGeneratorProcess(cancellationToken);
    }

    public Task<bool> RunAsync() => RunAsync(CancellationToken.None);

    public Task<bool> RunAsync(CancellationToken cancellationToken)
    {
        return Program.UseBuiltinGenerator
            ? RunGeneratorBuiltinAsync(cancellationToken)
            : RunGeneratorProcessAsync(cancellationToken);
    }

    // TODO: Add support for cancellation? May not be complete...
    private async Task<bool> RunGeneratorProcessAsync(CancellationToken cancellationToken)
    {
        var options = Options;
        var testFunction = TestFunction;
        var generatorTypeOrFunction = GeneratorTypeOrFunction;
        var generatorTypeOperandCount = generatorTypeOrFunction != null ? GeneratorTypeOperandCount : 0;
        var verifierFunction = VerifierFunction ?? testFunction;
        var testHandler = TestHandler;

        if (testHandler == null)
            throw new InvalidOperationException("Test handler is not defined.");

        // Calculate the maximum number of verifier processes that can run at any given time for this test runner instance.
        var verifierProcessCount = MaxTestThreads;
        if (verifierProcessCount <= 0)
            verifierProcessCount = Environment.ProcessorCount;
        //Trace.TraceInformation($"VerifierProcessCount = {verifierProcessCount}");

        var maxTestsPerProcess = MaxTestsPerProcess;

        // Only a certain number of verifier tasks can run at any given time, so use a semaphore to limit them.
        using var semaphore = new SemaphoreSlim(verifierProcessCount, verifierProcessCount);

        // Create the generator instance.
        var generator = new TestGenerator()
        {
            Options = options,
            TestTypeOrFunction = generatorTypeOrFunction ?? testFunction,
            TypeOperandCount = generatorTypeOperandCount
        };

        // We only need to append the results to the arguments if the generator function is a type code (which can be detected if it doesn't match the test function).
        var state = new TestRunnerState(this, options, testFunction, verifierFunction, testHandler,
            AppendResultsToArguments: !string.Equals(testFunction, generator.TestTypeOrFunction, StringComparison.Ordinal));

        // Keep track of the number of test blocks that failed.
        // Also keep track of the exceptions that were thrown.
        var testProcessFailures = 0;
        var testProcessExceptions = new ConcurrentBag<Exception>();

        // Start generating and running tests.
        int testCount = 0;
        TestRunnerArguments[]? testArgsQueue = null;

        void RunTestsImpl(object? taskState)
        {
            Debug.Assert(taskState != null && taskState is ReadOnlyMemory<TestRunnerArguments>);
            Debug.Assert(semaphore != null);
            Debug.Assert(state is not null);

            var testArguments = (ReadOnlyMemory<TestRunnerArguments>)taskState;
            try
            {
                Trace.TraceInformation($"[{Task.CurrentId}] Started new test run/verify task.");
                var stopwatch = Stopwatch.StartNew();

                var result = RunTestsCore(state, testArguments, cancellationToken);
                if (result != 0)
                    Interlocked.Increment(ref testProcessFailures);

                stopwatch.Stop();
                Trace.TraceInformation($"[{Task.CurrentId}] Task finished in {stopwatch.Elapsed.TotalSeconds:f3} seconds with result {result} ({(result == 0 ? "PASS" : "FAIL")}).");
            }
            catch (Exception ex)
            {
                testProcessExceptions.Add(ex);

#if DEBUG
                // Make it a little bit easier to debug when something goes wrong?
                if (Debugger.IsAttached)
                    Debugger.Break();
#endif
            }
            finally
            {
                // Release the tests array back into the pool (if it was actually an array).
                if (MemoryMarshal.TryGetArray(testArguments, out var testArgumentsArray))
                {
                    Debug.Assert(testArgumentsArray.Array != null);
                    _testArgsPool.Add(testArgumentsArray.Array);
                }

                // Release the semaphore and allow more tasks to run in its place.
                semaphore.Release();
            }
        }

        foreach (var testArgs in generator.GenerateTestData(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (testArgsQueue == null)
            {
                // Get the next available test args data from the pool or create a new array.
                if (!_testArgsPool.TryTake(out testArgsQueue))
                {
                    testArgsQueue = new TestRunnerArguments[maxTestsPerProcess];
                }
#if DEBUG
                else
                {
                    Array.Clear(testArgsQueue);
                }
#endif
            }

            // Add the current test arguments to the queue.
            testArgsQueue[testCount++] = testArgs;
            TotalTestCount++;

            // If the queue is full, then try to run the tests.
            if (testCount >= maxTestsPerProcess)
            {
                Debug.Assert(testCount == maxTestsPerProcess);

                // Wait on the semaphore to be able to run the next task.
                await semaphore.WaitAsync(cancellationToken);

                // Get the subset of tests to run and clear the current args state.
                ReadOnlyMemory<TestRunnerArguments> tests = testArgsQueue.AsMemory(0, testCount);
                testCount = 0;
                testArgsQueue = null;

                // Run the tests asynchronously. We don't need to track it, because the semaphore handles it (mostly).
                _ = Task.Factory.StartNew(RunTestsImpl, tests, CancellationToken.None);
            }
        }

        if (testCount > 0 && testArgsQueue != null)
        {
            // Wait on the semaphore to be able to run the next task.
            await semaphore.WaitAsync(cancellationToken);

            // Get the subset of tests to run.
            ReadOnlyMemory<TestRunnerArguments> tests = testArgsQueue.AsMemory(0, testCount);

            // Run the tests asynchronously. We don't need to track it, because the semaphore handles it (mostly).
            _ = Task.Factory.StartNew(RunTestsImpl, tests, CancellationToken.None);
        }

        // Use the semaphore to wait for the max number of tasks. If they can all be consumed, then all tasks are complete.
        Trace.TraceInformation($"Generator finished generating tests. Waiting for tasks to finish ({verifierProcessCount - semaphore.CurrentCount} / {verifierProcessCount} threads currently running).");
        for (var i = 0; i < verifierProcessCount; i++)
            await semaphore.WaitAsync(cancellationToken);

        // Results are in an unknown state if cancelled. Just throw if that happens.
        cancellationToken.ThrowIfCancellationRequested();

        // Throw aggregate exception if any were thrown in the individual run tasks.
        if (!testProcessExceptions.IsEmpty)
            throw new AggregateException(testProcessExceptions);

        // Return false if the number of failed test run blocks is not zero.
        return testProcessFailures == 0;
    }

    private int RunTestsCore(TestRunnerState state, ReadOnlyMemory<TestRunnerArguments> tests, CancellationToken cancellationToken)
    {
        if (tests.Length == 0)
            return 0;

        var testsSpan = tests.Span;
        var testHandler = TestHandler;
        Debug.Assert(testHandler != null);

        // Make a copy of the state. This is because thread-specific contexts are required.
        state = new TestRunnerState(this, state.Options, state.TestFunction, state.VerifierFunction, state.TestFunctionHandler, state.AppendResultsToArguments);

        // Create the verifier instance.
        var verifier = new TestVerifier()
        {
            Options = state.Options,
            TestFunction = state.TestFunction,
            ResultsWriter = Console.Out // TODO: add property for choosing/generating the output streams
        };

        // Start the verifier process.
        verifier.Start();
        try
        {
            // Run tests against verifier.
            for (var i = 0; i < testsSpan.Length; i++)
            {
                // TODO: Only check for this every few tests to reduce overhead?
                cancellationToken.ThrowIfCancellationRequested();

                // Run test and get result.
                var testData = testsSpan[i];
                var result = testHandler(state, testData);
                testData.SetResult(result, state.AppendResultsToArguments);

                // Add test results to verifier queue.
                verifier.AddResult(testData);
            }

            // Stop the verifier process and return the result.
            var verifierResult = verifier.Stop();
            return verifierResult;
        }
        finally
        {
            var verifierProcess = verifier.Process;
            if (verifierProcess != null)
            {
                if (!verifierProcess.HasExited)
                    verifierProcess.Kill();
                verifierProcess.Dispose();
            }
        }
    }

    private bool RunGeneratorProcess(CancellationToken cancellationToken)
    {
        var options = Options;
        var testFunction = TestFunction;
        var generatorTypeOrFunction = GeneratorTypeOrFunction;
        var generatorTypeOperandCount = generatorTypeOrFunction != null ? GeneratorTypeOperandCount : 0;
        var verifierFunction = VerifierFunction ?? testFunction;
        var testHandler = TestHandler;

        if (testHandler == null)
            throw new InvalidOperationException("Test handler is not defined.");

        // Create the generator instance.
        var generator = new TestGenerator()
        {
            Options = options,
            TestTypeOrFunction = generatorTypeOrFunction ?? testFunction,
            TypeOperandCount = generatorTypeOperandCount
        };

        // We only need to append the results to the arguments if the generator function is a type code (which can be detected if it doesn't match the test function).
        var state = new TestRunnerState(this, options, testFunction, verifierFunction, testHandler,
            AppendResultsToArguments: !string.Equals(testFunction, generator.TestTypeOrFunction, StringComparison.Ordinal));

        // Create the verifier instance.
        var verifier = new TestVerifier()
        {
            Options = state.Options,
            TestFunction = state.VerifierFunction,
            ResultsWriter = Console.Out // TODO: add property for choosing/generating the output streams
        };

        // Start generating and running tests.
        Trace.TraceInformation($"[{"..."}] Started new test run/verify task.");
        var stopwatch = Stopwatch.StartNew();

        // Start the verifier process.
        int verifierResult;
        verifier.Start();
        try
        {
            foreach (var testArgs in generator.GenerateTestData(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Run test and get result.
                var result = testHandler(state, testArgs);
                testArgs.SetResult(result, state.AppendResultsToArguments);

                // Add test results to verifier queue.
                verifier.AddResult(testArgs);
                TotalTestCount++;
            }

            // Stop the verifier process and return the result.
            verifierResult = verifier.Stop();
        }
        finally
        {
            var verifierProcess = verifier.Process;
            if (verifierProcess != null)
            {
                if (!verifierProcess.HasExited)
                    verifierProcess.Kill();
                verifierProcess.Dispose();
            }
        }

        stopwatch.Stop();
        Trace.TraceInformation($"[{"..."}] Task finished in {stopwatch.Elapsed.TotalSeconds:f3} seconds with result {verifierResult} ({(verifierResult == 0 ? "PASS" : "FAIL")}).");

        // Return false if the verifier result is not zero.
        return verifierResult == 0;
    }

    private bool RunGeneratorBuiltin(CancellationToken cancellationToken)
    {
        var options = Options;
        var testFunction = TestFunction;
        var generatorTypeName = GeneratorTypeOrFunction;
        var generatorTypeOperandCount = generatorTypeName != null ? GeneratorTypeOperandCount : 0;
        var verifierFunction = VerifierFunction ?? testFunction;
        var testHandler = TestHandler;

        if (testHandler == null)
            throw new InvalidOperationException("Test handler is not defined.");

        // Try to get the generator type name from the TestFunction or GeneratorTypeOrFunction properties.
        if (generatorTypeName == null)
        {
            if (!FunctionInfo.GeneratorTypes.TryGetValue(testFunction, out var generatorType))
                throw new InvalidOperationException("Test function is either not implemented or has no known generator type.");

            generatorTypeName = generatorType.TypeName;
            generatorTypeOperandCount = generatorType.ArgCount;
        }
        else if (FunctionInfo.GeneratorTypes.TryGetValue(generatorTypeName, out var generatorType))
        {
            // For whatever reason, the user wants to generate arguments for a function that is possibly different from the verifier's test function.
            generatorTypeName = generatorType.TypeName;
            generatorTypeOperandCount = generatorType.ArgCount;
        }

        // Create the test case generator and get the total number of test cases.
        var generator = TestCaseGenerator.FromTypeName(generatorTypeName, generatorTypeOperandCount, options.GeneratorLevel ?? 1);
        var testCases = generator.TotalCount;
        Debug.Assert(testCases >= 0, "Test case generator wants a negative number of test cases.");

        // Did the user specify a custom number of test cases?
        // NOTE: TestFloat does not allow less than the default number of cases, but this generator will allow any number of tests.
        // TODO: This implementation currently does not support specifying an infinite number of tests.
        if (options.GeneratorCount.HasValue)
        {
            testCases = options.GeneratorCount.Value;
            if (testCases <= 0)
                throw new NotImplementedException("A finite number of test cases must be defined.");
        }

        // We always need to append the results to the arguments with the builtin test case generator.
        var state = new TestRunnerState(this, options, testFunction, verifierFunction, testHandler, AppendResultsToArguments: true);

        var testProcessFailures = 0;

        var range = Tuple.Create(0L, testCases);

        Trace.TraceInformation($"[{range}] Started new test run/verify task.");
        var stopwatch = Stopwatch.StartNew();

        var result = RunTestsCore(state, generator, Tuple.Create(0L, testCases), cancellationToken);
        if (result != 0)
            testProcessFailures = 1;

        stopwatch.Stop();
        Trace.TraceInformation($"[{range}] Task finished in {stopwatch.Elapsed.TotalSeconds:f3} seconds with result {result} ({(result == 0 ? "PASS" : "FAIL")}).");

        TotalTestCount += testCases;
        return testProcessFailures == 0;
    }

    private async Task<bool> RunGeneratorBuiltinAsync(CancellationToken cancellationToken)
    {
        var options = Options;
        var testFunction = TestFunction;
        var generatorTypeName = GeneratorTypeOrFunction;
        var generatorTypeOperandCount = generatorTypeName != null ? GeneratorTypeOperandCount : 0;
        var verifierFunction = VerifierFunction ?? testFunction;
        var testHandler = TestHandler;

        if (testHandler == null)
            throw new InvalidOperationException("Test handler is not defined.");

        // Try to get the generator type name from the TestFunction or GeneratorTypeOrFunction properties.
        if (generatorTypeName == null)
        {
            if (!FunctionInfo.GeneratorTypes.TryGetValue(testFunction, out var generatorType))
                throw new InvalidOperationException("Test function is either not implemented or has no known generator type.");

            generatorTypeName = generatorType.TypeName;
            generatorTypeOperandCount = generatorType.ArgCount;
        }
        else if (FunctionInfo.GeneratorTypes.TryGetValue(generatorTypeName, out var generatorType))
        {
            // For whatever reason, the user wants to generate arguments for a function that is possibly different from the verifier's test function.
            generatorTypeName = generatorType.TypeName;
            generatorTypeOperandCount = generatorType.ArgCount;
        }

        // Create the test case generator and get the total number of test cases.
        var generator = TestCaseGenerator.FromTypeName(generatorTypeName, generatorTypeOperandCount, options.GeneratorLevel ?? 1);
        var testCases = generator.TotalCount;
        Debug.Assert(testCases >= 0, "Test case generator wants a negative number of test cases.");

        // Did the user specify a custom number of test cases?
        // NOTE: TestFloat does not allow less than the default number of cases, but this generator will allow any number of tests.
        // TODO: This implementation currently does not support specifying an infinite number of tests.
        if (options.GeneratorCount.HasValue)
        {
            testCases = options.GeneratorCount.Value;
            if (testCases <= 0)
                throw new NotImplementedException("A finite number of test cases must be defined.");
        }

        // We always need to append the results to the arguments with the builtin test case generator.
        var state = new TestRunnerState(this, options, testFunction, verifierFunction, testHandler, AppendResultsToArguments: true);

        // Use a partitioner to figure out the distribution of tests for multithreaded testing.
        // Use the MaxTestsPerProcess property to determine the size of each partitioner range.
        var partitioner = Partitioner.Create(0, testCases, MaxTestsPerProcess);

        // Asynchronously run the test cases.
        var testProcessFailures = 0;
        var parallelOptions = new ParallelOptions()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = MaxTestThreads <= 0 ? -1 : MaxTestThreads,
        };
        await Parallel.ForEachAsync(partitioner.GetDynamicPartitions(), parallelOptions,
            (range, cancellationToken) =>
            {
#if DEBUG
                try
#endif
                {
                    // NOTE: Task.CurrentId is always undefined inside the parallel foreach context, use the range instead.
                    Trace.TraceInformation($"[{range}] Started new test run/verify task.");
                    var stopwatch = Stopwatch.StartNew();

                    var result = RunTestsCore(state, generator, range, cancellationToken);
                    if (result != 0)
                        Interlocked.Increment(ref testProcessFailures);

                    stopwatch.Stop();
                    Trace.TraceInformation($"[{range}] Task finished in {stopwatch.Elapsed.TotalSeconds:f3} seconds with result {result} ({(result == 0 ? "PASS" : "FAIL")}).");
                }
#if DEBUG
                catch
                {
                    // Make it a little bit easier to debug when something goes wrong?
                    if (Debugger.IsAttached)
                        Debugger.Break();

                    throw;
                }
#endif

                return ValueTask.CompletedTask;
            });

        TotalTestCount += testCases;
        return testProcessFailures == 0;
    }

    private int RunTestsCore(TestRunnerState state, TestCaseGenerator generator, Tuple<long, long> range, CancellationToken cancellationToken)
    {
        Debug.Assert(range.Item1 <= range.Item2, "Test range is invalid.");
        if (range.Item1 == range.Item2)
            return 0;

        var testHandler = TestHandler;
        Debug.Assert(testHandler != null);

        // Make a copy of the state. This is because thread-specific contexts are required.
        state = new TestRunnerState(this, state.Options, state.TestFunction, state.VerifierFunction, state.TestFunctionHandler, state.AppendResultsToArguments);

        // Create the verifier instance.
        var verifier = new TestVerifier()
        {
            Options = state.Options,
            TestFunction = state.VerifierFunction,
            ResultsWriter = Console.Out // TODO: add property for choosing/generating the output streams
        };

        // Start the verifier process.
        verifier.Start();
        try
        {
            // Run tests against verifier.
            for (var i = range.Item1; i < range.Item2; i++)
            {
                // TODO: Only check for this every few tests to reduce overhead?
                cancellationToken.ThrowIfCancellationRequested();

                // Generate the test data.
                var testData = generator.GenerateTestCase(i);

                // Run test and get result.
                var result = testHandler(state, testData);
                testData.SetResult(result, state.AppendResultsToArguments);

                // Add test results to verifier queue.
                verifier.AddResult(testData);
            }

            // Stop the verifier process and return the result.
            var verifierResult = verifier.Stop();
            return verifierResult;
        }
        finally
        {
            var verifierProcess = verifier.Process;
            if (verifierProcess != null)
            {
                if (!verifierProcess.HasExited)
                    verifierProcess.Kill();
                verifierProcess.Dispose();
            }
        }
    }

    #endregion
}
